using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static BulletCommandGradualAPI;
using static Utilities.MathUtils;

[CreateAssetMenu(menuName = "ScriptableObject/Patterns/Simple/Spiral", fileName = "SpiralPattern")]
public class SpiralPattern : APattern
{

    public override void Execute(BulletEngine engine, Transform bossTransform, Transform playerTransform, Action finishAction, float2? position = null)
    {
        List<(string, BulletMaterial?)> groups = new();
        foreach (Color c in Colors) groups.Add((engine.UniqueGroup, new BulletMaterial(Shader, c)));
        GroupParameter group = new(engine, groups);

        PositionParameter positionParm = position == null ? new PositionParameter(bossTransform) : new PositionParameter((float2) position);

        StartCommand(engine.CreateBulletSpiral(group, positionParm, 33.3f * D2R / (.0075f * Density), .48f / Density, 2, Duration, Speed, BulletRadius), finishAction);
    }

}