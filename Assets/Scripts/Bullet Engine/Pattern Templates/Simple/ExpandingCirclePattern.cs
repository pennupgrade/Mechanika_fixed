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

[CreateAssetMenu(menuName = "ScriptableObject/Patterns/Complex/ExpandingCircle", fileName = "ExpandingCirclePattern")] 
public class ExpandingCirclePattern : APattern
{

    [Space(10f)]
    public float StartRadius;
    public float FormingTime;

    [Space(10f)]
    [Tooltip("Velocity sign will be RNG")] public float AngularSpeed;
    public float AngularSpeedVariance;
    public float ExpansionSpeed;

    [Space(10f)]
    public int2 SpikeCountRange;
    public float SpikeOffset;
    public float SpikeSlope;

    [Space(10f)]
    public float TimeUntilAlternate = -1f;

    static List<GroupParameter> allGroupParams = new();
    public static void ShiftAllGroupColors(int amount = 1)
    {
        foreach(GroupParameter g in allGroupParams)
            g.ShiftColors(amount);
    }

    public override void Execute(BulletEngine engine, Transform bossTransform, Transform playerTransform, Action finishAction, float2? position = null)
    {
    
        float spikeCount = UnityEngine.Random.Range(SpikeCountRange.x, SpikeCountRange.y);

        GroupParameter groups = GroupParameter.CreateGroups(engine, Colors, Shader);
        allGroupParams.Add(groups);

        float2 startPos = position == null ? bossTransform.position.xy() : (float2) position;

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
                        float overallRadius = StartRadius + time*ExpansionSpeed;

                        float size = TAU/spikeCount;
                        float ltheta = Utils.amod(theta + w1*time, size) - size*.5f;
                        ltheta = abs(ltheta);
                        return overallRadius + SpikeOffset - SpikeSlope * ltheta;// * overallRadius;
                    }, polar.x + w2*spawnTime, BulletRadius, w2, spawnTime);
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
