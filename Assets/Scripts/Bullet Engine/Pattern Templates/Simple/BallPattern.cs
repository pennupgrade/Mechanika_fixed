using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static BulletCommandGradualAPI;
using static Utilities.MathUtils;
using Utilities;
using System.Linq;
using Unity.VisualScripting;

[CreateAssetMenu(menuName = "ScriptableObject/Patterns/Simple/Ball", fileName = "BallPattern")]
public class BallPattern : APattern
{

    public float BallRadius = 2.0f;
    public float RadialDensity = 4.0f;

    public float AngularVelocity = 2.0f;
    public float FormingTime = 4.0f;

    public float BeginOffset;

    public Color BeginColor;
    public Color OutlineColor;

    public bool BounceOffWall = true;

    public float MinimumRadiusesFromPlayer = 4.05f;

    public float AdditionalRadiusForOutline = 0f;

    public override void Execute(BulletEngine engine, Transform bossTransform, Transform playerTransform, Action finishAction, float2? position = null)
    {
        List<(string, BulletMaterial?)> groups = new();
        foreach (Color c in Colors) groups.Add((engine.UniqueGroup, new BulletMaterial(Shader, c)));
        GroupParameter group = new(engine, groups);
        float2 startPos = position == null ? bossTransform.position.xy() + (float2) (BeginOffset*math.normalize(UnityEngine.Random.insideUnitCircle)) : (float2) position;
        float2 toPlayer = startPos - playerTransform.position.xy();
        startPos = playerTransform.position.xy() + math.normalize(toPlayer) * math.max(math.length(toPlayer), BallRadius * MinimumRadiusesFromPlayer);

        GroupParameter beginGroup = new(engine, (engine.UniqueGroup, new BulletMaterial(Shader, BeginColor)));
        GroupParameter outlineGroup = new(engine, (engine.UniqueGroup, new BulletMaterial(Shader, OutlineColor)));

        //GroupParameter allGroup = group.Merge(outlineGroup).Merge(beginGroup);
        List<Action<int, float>> recursiveActions = new() { (i, d) => { BulletCommandInstantAPI.SetBulletVelocity(engine, group.Merge(outlineGroup).Merge(beginGroup), math.normalize(playerTransform.position.xy() - startPos) * Speed); finishAction?.Invoke(); } }; 
        for (float d = BallRadius; d > 0; d -= 1f / RadialDensity)
        {
            recursiveActions.Add((i, d) =>
            {
                float r = BulletRadius + (i == 1 ? AdditionalRadiusForOutline : 0);
                StartCommand(engine.CreateBulletCircleGradual(i == 1 ? outlineGroup : group, new PositionParameter(startPos), d, Density, FormingTime / (RadialDensity * BallRadius), (polar, time) => new BulletKinematicPolar(new(), new(), new(), AngularVelocity, polar + new float2(time * AngularVelocity, 0f), r, Duration, false, BulletDamage, BounceOffWall ?
                (normal, triggerBullet) => group.TransformAllBullets(engine, b =>
                {
                    BulletKinematicPolar cb = (BulletKinematicPolar) b;
                    float2 newV = triggerBullet.Velocity - 2f * normal * math.dot(triggerBullet.Velocity, normal);
                    cb.Velocity = newV;
                    //cb.Origin += normal*0.005f;
                    return cb;
                }) : null)),
                () => recursiveActions[i - 1](i - 1, d + 1f/RadialDensity));
            });
        }

        recursiveActions.Add((i, d) => { engine.Add(beginGroup.GetNext(), new BulletKinematicPolar(startPos, new(), new(), AngularVelocity, new(), BulletRadius, Duration, true, BulletDamage)); recursiveActions[i - 1](i - 1, d + 1f / RadialDensity); });

        recursiveActions.Last()(recursiveActions.Count-1, 0f);
    }

}