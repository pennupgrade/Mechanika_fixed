using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static BulletCommandGradualAPI;
using static Utilities.MathUtils;
using Utilities;

using static Unity.Mathematics.math;
using BulletUtilities;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "ScriptableObject/Patterns/Simple/ExplodeCircle", fileName = "ExplodeCirclePattern")]
public class ExplodeCirclePattern : APattern
{

    [Space(10f)]
    public int BulletCount = 10;
    public float InitialVelocity = 1f;
    public float Friction = 0f;

    [Space(10f)]
    public float MinimumDistanceToPlayer = 5f;
    public float MaximumDistanceFromPlayer = 15f;

    [Space(10f)]
    public bool BounceOffWall = true;
    public bool DieOnWall = false;

    [Space(10f)]
    public float Magnetism = 0f;
    public bool SpawnOnBoss = false;

    public override void Execute(BulletEngine engine, Transform bossTransform, Transform playerTransform, Action finishAction, float2? position = null)
    {

        GroupParameter groups = GroupParameter.CreateGroups(engine, Colors, Shader);
        Debug.Log("MINI CIRCLE");
        float2 fromPlayer = (float2) UnityEngine.Random.insideUnitCircle * MaximumDistanceFromPlayer;
        fromPlayer = normalize(fromPlayer) * max(MinimumDistanceToPlayer, length(fromPlayer));
        float2 p = playerTransform.position.xy() + fromPlayer;

        float2 startPos = position != null ? (float2) position : SpawnOnBoss ? bossTransform.position.xy() : p;

        float2 v;
        for(int i=0; i<BulletCount; i++)
        {
            v = Utils.toCartesian(new(InitialVelocity, TAU * ((float)i)/BulletCount));
            if(Magnetism <= 0.001f) // soo jank
            {
                engine.Add(groups.GetNext(), new BulletKinematic(
                    startPos, v, new(), 
                    BulletRadius, Duration, 
                    BounceOffWall, DieOnWall,
                    BulletDamage, Friction
            ));
            }
            else
            {
                engine.Add(groups.GetNext(), new BulletKinematicBody(
                    new KinematicBodyPoint(startPos, v, Magnetism, new PositionParameter(playerTransform), engine.BoundSize, engine.BoundOrigin),
                    Duration, BulletRadius, 
                    BounceOffWall, DieOnWall,
                    BulletDamage
                ));
            }
        }

    }

}