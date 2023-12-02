using System;
using System.Collections.Generic;
using UnityEngine;
using static BulletCommandGradualAPI;
using static Utilities.MathUtils;

[CreateAssetMenu(menuName = "ScriptableObject/Patterns/Spiral", fileName = "SpiralPattern")]
public class SpiralPattern : APattern
{

    public override void Execute(BulletEngine engine, Transform bossTransform, Transform playerTransform, Action finishAction)
    {
        List<(string, BulletMaterial?)> groups = new();
        foreach (Color c in Color) groups.Add((engine.UniqueGroup, new BulletMaterial(Shader, c)));
        GroupParameter group = new(engine, groups);
        StartCommand(engine.CreateBulletSpiral(group, new PositionParameter(bossTransform), 33.3f * D2R / (.0075f * Density), .48f / Density, 2, Duration, Speed, BulletRadius), finishAction);
    }

}