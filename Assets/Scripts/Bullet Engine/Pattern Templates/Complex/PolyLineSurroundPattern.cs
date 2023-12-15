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

[CreateAssetMenu(menuName = "ScriptableObject/Patterns/Complex/PolyLineSurround", fileName = "PolyLineSurroundPattern")] 
public class PolyLineSurroundPattern : APattern
{

    public float Radius = 5f;
    public int SideCount = 3;

    public float OverExtendAmount = 0f;

    public float BulletSpeedMultiplier = 1f;

    public float BulletRadiusChangeToEnd = 0.2f;

    public float BulletLifeTime = 10f;

    public bool BounceOffWall = false;
    public bool DieOnWall = true;

    public override void Execute(BulletEngine engine, Transform bossTransform, Transform playerTransform, Action finishAction, float2? position = null)
    {
        GroupParameter groups = GroupParameter.CreateGroups(engine, Colors, Shader);

        float partSize = TAU/SideCount;

        int finishes = 0;

        float2 startPos = position == null ? playerTransform.position.xy() : (float2) position;
        
        for(int i=0; i<SideCount; i++)
        {
            float2 curr = startPos + Utils.toCartesian(float2(Radius, i*partSize));
            float2 next = startPos + Utils.toCartesian(float2(Radius, (i+1)*partSize));

            StartCommand(engine.CreateBulletLine(groups, new Position(curr), new Position(curr + (next - curr) * (OverExtendAmount + 1f)), Density, Speed, f => 
                new BulletKinematic(0f, BulletSpeedMultiplier * UnityEngine.Random.insideUnitCircle, 0f, BulletRadius + BulletRadiusChangeToEnd*f, BulletLifeTime, 
                BounceOffWall, DieOnWall, BulletDamage)), () =>
                {
                    finishes++;
                    if(finishes == SideCount)
                        finishAction?.Invoke();
                });
        }
        
    }
}
