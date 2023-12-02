using System;
using UnityEngine;
using System.Collections.Generic;
using static BulletCommandGradualAPI;
using static Utilities.MathUtils;
using Utilities;
using Rand = UnityEngine.Random;
using Unity.Mathematics;

[CreateAssetMenu(menuName = "ScriptableObject/Patterns/Line", fileName = "LinePattern")]
public class LinePattern : APattern
{

    [Tooltip("From Boss")]
    public float StartDistance;
    [Tooltip("From Player")]
    public float EndDistance;

    public float RadiusEndDifference = 0f;

    public int Count = 1;

    public override void Execute(BulletEngine engine, Transform bossTransform, Transform playerTransform, Action finishAction)
    {
        List<(string, BulletMaterial?)> groups = new();
        foreach (Color c in Color) groups.Add((engine.UniqueGroup, new BulletMaterial(Shader, c)));
        GroupParameter groupParam = new(engine, groups);
        for(int i=0; i<Count; i++) StartCommand(engine.CreateBulletLine(groupParam, new PositionParameter(bossTransform.position.xy() + StartDistance * math.normalize(Rand.insideUnitCircle)), new PositionParameter(playerTransform.position.xy() + EndDistance * math.normalize(Rand.insideUnitCircle)), Density * 0.5f, Speed, d => new BulletKinematic(0f, math.normalize(Rand.insideUnitCircle), new(), BulletRadius + d*RadiusEndDifference, Duration)), i == Count - 1 ? finishAction : () => { }); //last one may not be the finisher but whatever
    }

}