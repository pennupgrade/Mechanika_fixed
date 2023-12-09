using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static BulletCommandGradualAPI;
using static Utilities.MathUtils;
using Utilities;

using static Unity.Mathematics.math;

[CreateAssetMenu(menuName = "ScriptableObject/Patterns/Simple/Circle", fileName = "CirclePattern")]
public class CirclePattern : APattern
{

    //[Range(0f, 1f)]
    //public float Portion = 1f; If we can't do polar rotating around kinematic center it's not gonna even be worth it, and we can't do it cuz too much effort for little result

    public float CircleRadius = 2f;
    public float FormingTime = 2f;
    public float2 TrigSize = new (1f, 0f);

    public float AngularVelocity = 0f;

    public override void Execute(BulletEngine engine, Transform bossTransform, Transform playerTransform, Action finishAction, float2? position = null)
    {
        List<(string, BulletMaterial?)> groups = new();
        foreach (Color c in Colors) groups.Add((engine.UniqueGroup, new BulletMaterial(Shader, c)));
        GroupParameter group = new(engine, groups);

        float2 startPos = position == null ? bossTransform.position.xy() : (float2) position;

        StartCommand(engine.CreateBulletCircleGradual(group, position == null ? new PositionParameter(bossTransform) : new PositionParameter(startPos), CircleRadius, Density, FormingTime, (polar, time) => new BulletKinematicPolar(0f, 0f, new(), AngularVelocity, polar + new float2(AngularVelocity * time, 0f), BulletRadius, Duration), TrigSize.xyz(UnityEngine.Random.Range(0f, 2f * math.PI))), () =>
        {
            BulletCommandInstantAPI.SetBulletVelocity(engine, group, normalize(playerTransform.position.xy() - startPos) * Speed);
            finishAction();
        });
    }

}