using Unity.Mathematics;
using UnityEngine;

public interface IBulletEngineInteractable
{
    Transform Transform {get;}

    bool CanBeHit { get; }

    float2 Position { get; }
    float Radius { get; }
    
    void Hit(int damage);
}