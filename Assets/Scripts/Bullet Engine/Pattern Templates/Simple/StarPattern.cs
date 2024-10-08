﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using static BulletCommandGradualAPI;
using static Utilities.MathUtils;
using static Unity.Mathematics.math;
using Unity.Mathematics;

[CreateAssetMenu(menuName = "ScriptableObject/Patterns/Simple/Star", fileName = "StarPattern")]
public class StarPattern : APattern
{

    public float FormingSpeed;
    public float ThrowSpeed;
    public float StarSize = 4f;
    public float AngularVelocity = 0f;

    public override void Execute(BulletEngine engine, Transform bossTransform, Transform playerTransform, Action finishAction, float2? position = null)
    {
        List<(string, BulletMaterial?)> groups = new();
        foreach (Color c in Colors) groups.Add((engine.UniqueGroup, new BulletMaterial(Shader, c)));
        GroupParameter group = new(engine, groups);

        float2 startPos = position == null ? bossTransform.position.xy() : (float2) position;

        StartCommand(engine.CreateBulletSpikeBall(group, new PositionParameter(startPos), 10, new float2(StarSize*.5f, StarSize), 0f, Density, FormingSpeed, (polar, time) => new BulletKinematicPolar(new(), 0f, 0f, AngularVelocity, polar + float2(time*AngularVelocity, 0f), BulletRadius, Duration), UnityEngine.Random.Range(0f, 2f * math.PI)), () =>
        {
            engine.SetBulletVelocity(group, normalize(playerTransform.position.xy() - startPos) * ThrowSpeed);
            finishAction?.Invoke();
        });
    }

}