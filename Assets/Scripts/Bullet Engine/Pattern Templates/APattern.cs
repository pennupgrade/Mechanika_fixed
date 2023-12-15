using System;
using UnityEngine;

using Unity.Mathematics;

public abstract class APattern : ScriptableObject 
{ 
    public abstract void Execute(BulletEngine engine, Transform bossTransform, Transform playerTransform, Action finishAction, float2? position = null); 
    public string Shader; public Color[] Colors;
    public float Duration; public float Speed; public float BulletRadius; public float Density;
    public int BulletDamage = 40;
}