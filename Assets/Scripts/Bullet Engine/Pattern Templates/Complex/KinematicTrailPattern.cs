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

    //kinematic body interacts with bullet wall bounds

    public override void Execute(BulletEngine engine, Transform bossTransform, Transform playerTransform, Action finishAction, float2? position = null)
    {
    


    }
}
