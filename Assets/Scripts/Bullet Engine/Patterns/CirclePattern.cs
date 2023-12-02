using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static BulletCommandGradualAPI;
using static Utilities.MathUtils;
using Utilities;

[CreateAssetMenu(menuName = "ScriptableObject/Patterns/Circle", fileName = "CirclePattern")]
public class CirclePattern : APattern
{

    //[Range(0f, 1f)]
    //public float Portion = 1f; If we can't do polar rotating around kinematic center it's not gonna even be worth it, and we can't do it cuz too much effort for little result

    public float CircleRadius = 2f;
    public float FormingTime = 2f;
    public float2 TrigSize = new (1f, 0f);

    public float AngularVelocity = 0f;

    public override void Execute(BulletEngine engine, Transform bossTransform, Transform playerTransform, Action finishAction)
    {
        List<(string, BulletMaterial?)> groups = new();
        foreach (Color c in Color) groups.Add((engine.UniqueGroup, new BulletMaterial(Shader, c)));
        GroupParameter group = new(engine, groups);
        StartCommand(engine.CreateBulletCircleGradual(group, new PositionParameter(bossTransform), CircleRadius, Density, FormingTime, (polar, time) => new BulletKinematicPolar(0f, 0f, new(), AngularVelocity, polar + new float2(AngularVelocity * time, 0f), BulletRadius, Duration), TrigSize.xyz(UnityEngine.Random.Range(0f, 2f * math.PI))), () =>
        {
            BulletCommandInstantAPI.SetBulletVelocity(engine, group, (Vector2) (playerTransform.position - bossTransform.position).normalized * Speed);
            finishAction();
        });
    }

}