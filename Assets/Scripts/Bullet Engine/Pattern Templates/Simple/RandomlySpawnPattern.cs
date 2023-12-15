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
using Unity.VisualScripting;

[CreateAssetMenu(menuName = "ScriptableObject/Patterns/Simple/RandomlySpawn", fileName = "RandomlySpawnPattern")] 
public class RandomlySpawnPattern : APattern
{

    [Space(10f)]
    public float BulletRadiusVariation;
    public float BulletVelocityMultiplier = 1f;
    public float LifeTime = 10f;

    [Space(10f)]
    public float SpawnDelay = 1f;
    public float MinimumDistanceToPlayer = 8f;
    public float MaximumDistanceFromPlayer = 26f;

    [Space(10f)]
    public bool DieOnWall = true;
    public bool BounceOffWall = false;

    public override void Execute(BulletEngine engine, Transform bossTransform, Transform playerTransform, Action finishAction = null, float2? position = null)
    {
    
        GroupParameter groups = GroupParameter.CreateGroups(engine, Colors, Shader);

        IEnumerator Coro()
        {
            float elapsedTime = 0f;
            while(elapsedTime <= Duration)
            {
                float2 p = engine.BoundOrigin + (float2) UnityEngine.Random.insideUnitCircle * MaximumDistanceFromPlayer; //new float2(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-.5f,.5f)) * engine.BoundSize;
                float2 fromPlayer = p - playerTransform.position.xy();
                p = playerTransform.position.xy() + normalize(fromPlayer) * max(length(fromPlayer), MinimumDistanceToPlayer);

                float2 v = UnityEngine.Random.insideUnitCircle * BulletVelocityMultiplier;

                engine.Add(groups.GetNext(), new BulletKinematic(
                    p, v, new(),
                    BulletRadius + UnityEngine.Random.Range(-0.5f, 0.5f) * BulletRadiusVariation,
                    LifeTime,
                    BounceOffWall, DieOnWall,
                    BulletDamage
                ));

                elapsedTime += SpawnDelay;
                yield return new WaitForSeconds(SpawnDelay);
            }
        }

        StartCommand(Coro(), finishAction);

    }
}
