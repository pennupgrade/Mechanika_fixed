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

[CreateAssetMenu(menuName = "ScriptableObject/Patterns/Complex/Firework", fileName = "FireworkPattern")] //also make homing circle
public class FireworkPattern : APattern
{

    [Space(10f)]
    public float InitialVelocityVariation;

    [Space(10f)]
    public float MagnetismBeforeExplosion;
    public float MagnetismAfterExplosion;

    [Space(10f)]
    public float CircleRadius;
    public float FireworkThickness;
    
    [Space(10f)]
    public float AngularVelocity;

    [Space(10f)]
    public float TimeUntilFireworkExplosion;
    public float TimeAfterFireworkExplosion;
    public float ChaseTime;

    [Space(10f)]
    public float AdditionalExplosionVelocity;

    [Space(10f)]
    public float2 BulletRadiusOffsetTrig;

    [Space(10f)]
    public float StartOffsetAmount;
    public float StartOffsetVariation;
    public bool RandomOffPlayer = false;

    [Space(10f)]
    public bool DieOnWall = true;
    public bool BounceOffWall = false;

    public override void Execute(BulletEngine engine, Transform bossTransform, Transform playerTransform, Action finishAction, float2? position = null)
    {
        GroupParameter groups = GroupParameter.CreateGroups(engine, Colors, Shader);
        
        Position playerPoint = new (playerTransform);
        Position bossPoint = new (bossTransform);

        float2 startPos = position != null ? (float2) position :
            !RandomOffPlayer ?
            bossTransform.position.xy() + StartOffsetAmount * normalize(bossPoint.Pos - playerPoint.Pos) + StartOffsetVariation * normalize(UnityEngine.Random.insideUnitCircle)
            :
            playerTransform.position.xy() + (float2) StartOffsetAmount * normalize(UnityEngine.Random.insideUnitCircle)
                                          + (float2) (StartOffsetVariation * UnityEngine.Random.insideUnitCircle);

        float2 toPlayer = normalize(playerPoint.Pos - startPos);

        KinematicBodyPoint body = new KinematicBodyPoint(startPos, 
            Speed * normalize((float2) (InitialVelocityVariation * UnityEngine.Random.insideUnitCircle) - toPlayer), 
            MagnetismBeforeExplosion, playerPoint);

        StartCommand(engine.CreateBulletCircleGradual(groups, new Position(startPos), CircleRadius, Density, Duration, (polar, tt) => 
        {
            return new BulletPolarFunction(new KinematicBodyStatic(startPos), (float o, float t) => CircleRadius + 0.5f * FireworkThickness * sin(o*10f) - (CircleRadius * 0.6f*t/(TimeUntilFireworkExplosion+Duration)), 
                polar.x + AngularVelocity * tt, r => AngularVelocity, BulletRadius + BulletRadiusOffsetTrig.y*(.5f+.5f*sin(polar.x*BulletRadiusOffsetTrig.x)), tt, TimeUntilFireworkExplosion+10f, BulletDamage);
        }, new float3(), false, false, true), () =>
        {
            IEnumerator Coro() 
            {
                groups.TransformAllBullets(engine, b =>
                {
                    BulletPolarFunction cb = (BulletPolarFunction) b;
                    cb.origin = body;
                    return cb;
                });
                yield return new WaitForSeconds(TimeUntilFireworkExplosion);
                groups.TransformAllBullets(engine, b => 
                {
                    BulletPolarFunction cb = (BulletPolarFunction) b;
                    return new BulletKinematicBody(
                        new KinematicBodyPoint(cb.Position, cb.Velocity + AdditionalExplosionVelocity * normalize(cb.Position - cb.origin.Position), MagnetismAfterExplosion, playerPoint), 
                        TimeAfterFireworkExplosion, BulletRadius, BounceOffWall, DieOnWall, BulletDamage);
                });
                yield return new WaitForSeconds(ChaseTime);
                groups.TransformAllBullets(engine, b => 
                {
                    BulletKinematicBody cb = (BulletKinematicBody) b;
                    KinematicBodyPoint body = (KinematicBodyPoint) cb.body;
                    body.Magnetism = 0f;
                    cb.body = body;
                    return cb;
                });
            }
            StartCommand(Coro());
            finishAction?.Invoke();
        });

    }
}
