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

[CreateAssetMenu(menuName = "ScriptableObject/Patterns/Complex/KinematicTrail", fileName = "KinematicTrailPattern")] 
public class KinematicTrailPattern : APattern
{

    [Min(0.001f)] [Tooltip("Lower parameter means more precise kinematic body simulation, that's all.")] public float TimeStep = 0.015f;
    [Tooltip("How much time between bullet spawns?")] public float SpawnRate = 0.1f;

    [Space(10f)]
    public float2 StartSpeedRandomRange;
    [Range(0f, 1f)] [Tooltip("0 means completely random velocity direction.  1 means velocity will be directed at player.")] public float VelocityRandomness;

    [Space(10f)]
    public float PlayerMagnetism;

    [Space(10f)]
    [Header("Assuming No Position Override")]
    float SpawnDistanceFromBoss;
    float SpawnPositionVariance;

    [Space(10f)]
    [Header("Extra Bullet Parameters")]
    public float BulletVelocityMultiplier = 1f;
    [Tooltip("Change in the radius of the bullets being spawned with respect to time.")] public float BulletRadiusTimeSlope = 0f;
    public float BulletLifeTime = 10f;
    public bool BounceOffWalls;

    public override void Execute(BulletEngine engine, Transform bossTransform, Transform playerTransform, Action finishAction, float2? position = null)
    {
    
        //
        GroupParameter groups = GroupParameter.CreateGroups(engine, Colors, Shader);

        // Start Position
        float2 bossStartPos = bossTransform.position.xy() + 
                              SpawnDistanceFromBoss * normalize(bossTransform.position.xy() - playerTransform.position.xy()) + 
                              SpawnPositionVariance*normalize(UnityEngine.Random.insideUnitCircle);

        float2 startPos = position == null ? bossStartPos : (float2) position;

        // Start velocity
        float2 initialVelocity = UnityEngine.Random.Range(StartSpeedRandomRange.x, StartSpeedRandomRange.y) * normalize(
                normalize(playerTransform.position.xy() - bossTransform.position.xy()) * (1f-VelocityRandomness) + 
                normalize(UnityEngine.Random.insideUnitCircle)                         * VelocityRandomness);

        KinematicBodyPoint body = new(startPos, initialVelocity, PlayerMagnetism, new Position(playerTransform), engine.BoundSize, engine.BoundOrigin);

        float timePassed = 0;
        float timeUntilBulletSpawn = 0f;

        IEnumerator Coro() 
        {

            while(timePassed <= Duration)
            {
                yield return new WaitForSeconds(TimeStep);
                body.Update(TimeStep);
                timePassed += TimeStep;

                if(timeUntilBulletSpawn <= 0f)
                {
                    engine.Add(groups.GetNext(), 
                        new BulletKinematic(
                            body.Position, 
                            UnityEngine.Random.insideUnitCircle * BulletVelocityMultiplier, 
                            0f, 
                            BulletRadius + BulletRadiusTimeSlope*timePassed, 
                            BulletLifeTime, 
                            BounceOffWalls));
                }
            }

            finishAction();
  
        }

        StartCommand(Coro());

    }
}
