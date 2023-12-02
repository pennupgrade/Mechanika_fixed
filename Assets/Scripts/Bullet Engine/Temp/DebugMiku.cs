using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Utilities;

public class DebugMiku : MonoBehaviour, IBulletEngineInteractable
{

    void Start()
        => BulletEngineManager.InitializeBossManager();

    public float2 Position => transform.position.xy();
    public float Radius => 0.4f;

    public Transform Transform => this.transform;
    public bool CanBeHit => true;
    
    public void Hit()
        => Debug.Log("HIT!");
}
