using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using static BulletCommandGradualAPI;
using static Utilities.MathUtils;
using static Unity.Mathematics.math;
using Unity.Mathematics;

[CreateAssetMenu(menuName = "ScriptableObject/Patterns/Simple/Spike", fileName = "SpikePattern")]
public class SpikePattern : APattern
{

    public float2 SpikeRadiusRange;
    public float SpikeRadiusVariation;
    public float FormingSpeed = 1.0f;
    public float ThrowSpeed = 5.0f;
    public float AngularVelocity = 0f;

    public override void Execute(BulletEngine engine, Transform bossTransform, Transform playerTransform, Action finishAction)
    {
        List<(string, BulletMaterial?)> groups = new();
        foreach (Color c in Colors) groups.Add((engine.UniqueGroup, new BulletMaterial(Shader, c)));
        GroupParameter group = new(engine, groups);
        StartCommand(engine.CreateBulletSpikeBall(group, new PositionParameter(bossTransform.position.xy()), 16, SpikeRadiusRange, SpikeRadiusVariation, Density, FormingSpeed, (polar, time) => new BulletKinematicPolar(new(), 0f, 0f, AngularVelocity, polar + float2(time*AngularVelocity, 0f), BulletRadius, Duration), UnityEngine.Random.Range(0f, 2f * math.PI)), () =>
        {
            engine.SetBulletVelocity(group, normalize((playerTransform.position - bossTransform.position).xy()) * ThrowSpeed);
            finishAction();
        });
    }

}