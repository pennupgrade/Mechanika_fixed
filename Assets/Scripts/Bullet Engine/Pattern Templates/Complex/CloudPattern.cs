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

[CreateAssetMenu(menuName = "ScriptableObject/Patterns/Complex/Cloud", fileName = "CloudPattern")] 
public class CloudPattern : APattern
{

    [Min(0f)] public float Radius = 5f;
    [Min(1)] public int BulletCount;
    [Min(0.01f)] public float BulletSpawnFrequency;

    [Space(10f)]
    [Range(0f, 1f)] public float RandomWeighting;

    [Space(10f)]
    public float AddedBulletRadiusVariation;

    [Space(10f)]
    public float InitialVelocity = 0.1f;

    public override void Execute(BulletEngine engine, Transform bossTransform, Transform playerTransform, Action finishAction, float2? position = null)
    {
    
        GroupParameter groups = GroupParameter.CreateGroups(engine, Colors, Shader);

        float delayTime = 1f/BulletSpawnFrequency;

        float2 startPos = position == null ? bossTransform.position.xy() : (float2) position;

        IEnumerator Coro()
        {
            for(int i=0; i<BulletCount; i++)
            {
                float2 p = startPos + (float2) UnityEngine.Random.insideUnitCircle * Radius;
                engine.Add(groups.GetNext(), new BulletKinematic(p, UnityEngine.Random.insideUnitCircle * InitialVelocity, 0f, BulletRadius + UnityEngine.Random.Range(0f, 1f) * AddedBulletRadiusVariation, 10f, true));
                yield return new WaitForSeconds(delayTime);
            }

            float2 toPlayer = normalize(playerTransform.position.xy() - startPos);
            float2 toRandom = normalize(UnityEngine.Random.insideUnitCircle);

            float2 flingDir = normalize(toPlayer + (toRandom - toPlayer) * RandomWeighting);

            groups.TransformAllBullets(engine, b => 
            {
                BulletKinematic cb = (BulletKinematic) b;
                cb.v = Speed * flingDir;
                return cb;
            });
        }

        StartCommand(Coro(), finishAction);

    }
}
