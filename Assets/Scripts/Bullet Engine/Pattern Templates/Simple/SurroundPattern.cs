using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using static BulletCommandGradualAPI;
using static Utilities.MathUtils;
using static Unity.Mathematics.math;
using Unity.Mathematics;

[CreateAssetMenu(menuName = "ScriptableObject/Patterns/Simple/Surround", fileName = "SurroundPattern")]
public class SurroundPattern : APattern
{

    public float CircleRadius = 5.0f;
    public float FormingTime = 0.5f;
    public float BulletAcceleration = 0.1f;

    public override void Execute(BulletEngine engine, Transform bossTransform, Transform playerTransform, Action finishAction, float2? position = null)
    {
        List<(string, BulletMaterial?)> groups = new();
        foreach (Color c in Colors) groups.Add((engine.UniqueGroup, new BulletMaterial(Shader, c)));
        GroupParameter group = new(engine, groups);

        StartCommand(engine.CreateBulletCircleGradual(group, new PositionParameter(position == null ? playerTransform.position.xy() : (float2) position), CircleRadius, Density, FormingTime, (polar, time) => new BulletKinematic(0f, 0f, -PolarToCartesian(polar.x) * BulletAcceleration*(1f+0f*polar.x*.5f/math.PI), BulletRadius, Duration), new float3(2f, 0.5f, UnityEngine.Random.Range(0f, math.PI*2f)), false), finishAction);

    }

}