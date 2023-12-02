using System;
using UnityEngine;

public abstract class APattern : ScriptableObject 
{ 
    public abstract void Execute(BulletEngine engine, Transform bossTransform, Transform playerTransform, Action finishAction); 
    public string Shader; public Color[] Color;
    public float Duration; public float Speed; public float BulletRadius; public float Density;
}