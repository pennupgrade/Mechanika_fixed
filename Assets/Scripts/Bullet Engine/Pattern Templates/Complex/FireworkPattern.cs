using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static BulletCommandGradualAPI;
using static Utilities.MathUtils;
using Utilities;

using Unity.Mathematics;
using static Unity.Mathematics.math;

using Position = PositionParameter;
using BulletUtilities;

[CreateAssetMenu(menuName = "ScriptableObject/Patterns/Complex/Firework", fileName = "CirclePattern")] //also make homing circle
public class FireworkPattern : APattern
{

    public float InitialSpeed;
    public float InitialSpeedVariation;

    public float Magnetism;

    public override void Execute(BulletEngine engine, Transform bossTransform, Transform playerTransform, Action finishAction)
    {
        GroupParameter groups = GroupParameter.CreateGroups(engine, Colors, Shader);
        
        Position playerPoint = new (playerTransform);
        Position bossPoint = new (bossTransform);

        KinematicBodyPoint body = new KinematicBodyPoint(bossTransform.position.xy(), 
            (float2) (InitialSpeedVariation * UnityEngine.Random.insideUnitCircle) + InitialSpeed * normalize(bossPoint.Pos - playerPoint.Pos), 
            Magnetism, playerPoint);
        List<ITBullet> bullets = new();

        StartCommand(engine.CreateBulletCircleGradual(groups, new Position(bossTransform), -1f, -1f, -1f, (polar, t) => 
        {
            return new BulletKinematicPolar(); //IMPLEMENT
        }), null);
    }
}
