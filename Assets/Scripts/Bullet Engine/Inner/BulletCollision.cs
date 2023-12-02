using Unity.Mathematics;

public interface IBulletEngineInteractable
{
    bool CanBeHit { get; }

    float2 Position { get; }
    float Radius { get; }
    
    void Hit();
}