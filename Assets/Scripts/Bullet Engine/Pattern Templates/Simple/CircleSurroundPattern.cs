using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static BulletCommandGradualAPI;
using static Utilities.MathUtils;
using Utilities;

using static Unity.Mathematics.math;

using Position = PositionParameter;
using System.Collections;

[CreateAssetMenu(menuName = "ScriptableObject/Patterns/Simple/CircleSurround", fileName = "CircleSurroundPattern")]
public class CircleSurroundPattern : APattern
{

    public float FormingTime = 2f;

    public float StartRadius = 4f;
    public float CloseSpeed = 2f;
    
    public float AngularSpeed = 4f;
    public float AngularSpeedVariance = 1f;

    public float TimeUntilAlternate = -1f;

    public float BulletLifeTime = 10f;
    
    public override void Execute(BulletEngine engine, Transform bossTransform, Transform playerTransform, Action finishAction, float2? position = null)
    {

        GroupParameter groups = GroupParameter.CreateGroups(engine, Colors, Shader);

        float2 startPos = position == null ? playerTransform.position.xy() : (float2) position;

        //
        float s1 = step(UnityEngine.Random.Range(0f, 1f), 0.5f)*2f-1f;
        float s2 = step(UnityEngine.Random.Range(0f, 1f), 0.5f)*2f-1f;
        float m1 = AngularSpeed + UnityEngine.Random.Range(-1f, 1f) * AngularSpeedVariance;
        float m2 = AngularSpeed + UnityEngine.Random.Range(-1f, 1f) * AngularSpeedVariance;
        float w1 = s1*m1; float w2 = s2*m2;

        IEnumerator Coro()
        {
            yield return engine.CreateBulletCircleGradual(groups, new Position(startPos), StartRadius, Density, FormingTime, (polar, spawnTime) => 
            {
                return new BulletPolarFunction(startPos, 
                    (theta, time) => 
                    {
                        return StartRadius- time*CloseSpeed;
                    }, polar.x + w2*spawnTime, BulletRadius, w2, spawnTime, BulletLifeTime, BulletDamage);
            }, theta => 0, true, false);

            finishAction?.Invoke();

            if(TimeUntilAlternate < 0) yield break;
            yield return new WaitForSeconds(TimeUntilAlternate);

            groups.TransformAllBullets(engine, b =>
            {
                BulletPolarFunction cb = (BulletPolarFunction) b;
                cb.angularVelocityMultiplier *= -2f;
                return cb;
            });

            yield break;
        }

        StartCommand(Coro());
    }

}