using BulletUtilities;
using System;
using System.Diagnostics;
using Unity.Mathematics;
using UnityEngine;
using Utilities;

using static Unity.Mathematics.math;
using static Utilities.MathUtils;

public interface ITBullet
{

    float2 Position { get; set; }
    float Radius { get; }
    float2 Direction { get; }

    bool Update(float dt);

    void OnHitWall(float2 wallNormal) {}

}

public interface IBulletKinematic : ITBullet
{
    public float2 Velocity { get; set; }
    public float2 Acceleration { get; set; }
}

public struct BulletDeath<T> : ITBullet where T : struct, ITBullet
{

    T bullet;
    Action<BulletEngine, GroupParameter> deathAction; BulletEngine engine; GroupParameter group;

    public BulletDeath(T bullet, Action<BulletEngine, GroupParameter> deathAction, BulletEngine engine, GroupParameter group)
    { this.bullet = bullet; this.deathAction = deathAction; this.engine = engine; this.group = group; }

    public float2 Position { get => bullet.Position; set => bullet.Position = value; }
    public float Radius => bullet.Radius;
    public float2 Direction => bullet.Direction;

    public bool Update(float dt)
    {
        if (bullet.Update(dt)) return true;
        deathAction(engine, group); return false;
    }

}

public struct BulletKinematic : ITBullet, IBulletKinematic
{

    public BulletKinematic(float2 p = new(), float2 v = new(), float2 a = new(), float r = 1.0f, float lifeTime = 10f, bool wallInteract = false)
    {
        this.p = p; this.v = v; this.a = a; time = new float2(0f, lifeTime);
        this.r = r;
        this.wallInteract = wallInteract;
    }

    public float2 p;
    public float2 v;
    public float2 a;
    public float r;

    public float2 Position { get => p; set => p = value; }
    public float2 Velocity { get => v; set => v = value; }
    public float2 Acceleration { get => a; set => a = value; }
    public float Radius => r;
    public float2 Direction => normalize(v);

    float2 time;

    bool wallInteract;

    public bool Update(float dt)
    {

        p += v * dt + dt * dt * a * .5f;
        v += a * dt;
        time.x += dt;

        return time.x <= time.y;

    }

    public void OnHitWall(float2 wallNormal)
    {
        if(!wallInteract) return;
        v -= 2f * wallNormal * dot(wallNormal, v);
    }

}

public struct BulletKinematicPolar : ITBullet, IBulletKinematic
{

    public BulletKinematicPolar(float2 p = new(), float2 v = new(), float2 a = new(), float angularVelocity = 0f, float2 polarCoords = new(), float bulletRadius = 0.4f, float lifeTime = 10f)
    {
        origin = new(p, v, a);
        this.polarCoords = polarCoords;
        w = angularVelocity;

        Radius = bulletRadius;

        this.lifeTime = lifeTime;
        
    }

    KinematicBodyConstAcc origin;
    float2 polarCoords;
    float w;

    public float2 Position { get => origin.p + polarCoords.PolarToCartesian(); set => origin.p = value; } //weird but eh
    public float2 Velocity { get => origin.v; set => origin.v = value; }
    public float2 Acceleration { get => origin.a; set => origin.a = value; }
    public float Radius { get; private set; }
    public float2 Direction => new(5f, 5f);//PolarToCartesian(polarCoords.x);

    public float2 Origin => origin.p;

    float lifeTime;

    public bool Update(float dt)
    {
        origin.Update(dt);
        polarCoords.x += w * dt;

        lifeTime -= dt;
        return lifeTime > 0;
    }

}

public struct BulletPolarFunction : ITBullet //r(theta, time)
{

    //
    public BulletPolarFunction(float2 origin, Func<float, float, float> function, float theta, float r = 1.0f, float angularVelocity = 1f, float timeStart = 0f, float lifeTime = 10f)
        : this(new KinematicBodyConstAcc(origin), function, theta, f => angularVelocity, r, timeStart, lifeTime) { }

    public BulletPolarFunction(IKinematicBody origin, Func<float, float, float> function, float theta, Func<float, float> angularVelocity, float r = 1.0f, float timeStart = 0f, float lifeTime = 10f)
    {
        this.function = function; this.origin = origin; this.angularVelocity = angularVelocity;
        this.theta = theta; time = timeStart; this.lifeTime = lifeTime;
        Radius = r;
        Position = float2(0f); Update(0f);
    }

    public IKinematicBody origin;
    float lifeTime;

    Func<float, float, float> function;

    float time;
    float theta;

    Func<float, float> angularVelocity;

    //
    public float2 Position { get; set; }
    public float2 Direction => PolarToCartesian(theta);
    public float Radius { get; private set; }

    public float2 Velocity { get 
    {
        float r = function(theta, time);
        float w = angularVelocity(r);

        return origin.Velocity + Utils.toCartesian(new(w*r, theta+PI*.5f));
    }}

    public bool Update(float dt)
    {
        float r = function(theta, time);
        origin.Update(dt);

        Position = origin.Position + r * PolarToCartesian(theta);

        time += dt;
        theta += angularVelocity(r)*dt;

        return time <= lifeTime;
    }

}

public struct BulletKinematicBody : ITBullet
{

    public IKinematicBody body;
    Timer timer;
    float radius;

    public BulletKinematicBody(IKinematicBody body, float lifeTime, float bulletRadius)
    { this.body = body; timer = new(lifeTime); radius = bulletRadius; }

    public bool Update(float dt)
    {
        body.Update(dt);
        return !timer.Check(dt);
    }

    public float2 Position { get => body.Position; set => body.Position = value; }

    public float2 Direction => normalize(body.Velocity);
    public float Radius => radius;

}