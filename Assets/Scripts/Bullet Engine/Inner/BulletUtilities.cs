using System;
using Unity.Mathematics;

using static Unity.Mathematics.math;

using Position = PositionParameter;
using Group = BulletEngine.GroupAccessor;
using System.Diagnostics;

namespace BulletUtilities
{


    public struct BulletExchange
    {

        BulletEngine engine; BulletEngine.GroupAccessor group; int i;
        public BulletExchange(BulletEngine engine, BulletEngine.GroupAccessor group, int i, ITBullet b)
        { this.engine = engine; this.group = group; this.i = i; this.b = b; }
        public BulletExchange(BulletEngine engine, BulletEngine.GroupAccessor group, int i) : this(engine, group, i, null) { }

        ITBullet b;

        public void Run() { if (b is null) throw new Exception("Null Bullet Attempted Exchange"); engine.Set(group, b, i); }
        public void Run(ITBullet b)
        { this.b = b; Run(); }

    }

    public struct KinematicBodyConstAcc : IKinematicBody
    {

        public float2 Position { get => p; set => p = value; }
        public float2 Velocity { get => v; set => v = value; }
        public float2 Acceleration => a;

        public float2 p;
        public float2 v;
        public float2 a;

        public KinematicBodyConstAcc(float2 p = new(), float2 v = new(), float2 a = new())
        { this.p = p; this.v = v; this.a = a; }

        public void Update(float dt)
        {
            p += v * dt + .5f * a * dt * dt;
            v += a * dt;
        }

    }

    public interface IKinematicBody
    {
        float2 Position { get; set; }
        float2 Velocity { get; set; }
        float2 Acceleration { get; }
        void Update(float dt);
    }

    public struct KinematicBodyPoint : IKinematicBody
    {
        
        public float2 Position { get => pos; set => pos = value; }
        public float2 Velocity { get => vel; set => vel = value; }
        public float2 Acceleration => accMag * normalize(targetPoint.Pos - pos);

        public float Magnetism { get => accMag; set => accMag = value; }

        public void Update(float dt)
        {
            float2 a = Acceleration;

            pos += vel * dt + 0.5f * dt * dt * a;
            vel += dt * a;

            if(boundSize != null)
            {
                float2? wallNormal = Utilities.Collision.PointCollideArena((float2) boundSize, pos - boundOrigin, 0.3f);
                if(wallNormal != null)
                {
                    float2 n = (float2) wallNormal;
                    vel -= 2f * n * dot(vel, n);
                }
            }
        }

        float2 pos;
        float2 vel;
        float accMag;

        Position targetPoint;

        float2? boundSize;
        float2 boundOrigin;

        public KinematicBodyPoint(float2 initialPos, float2 initialVelocity, float accMag, Position targetPoint, float2? boundSize = null, float2 boundOrigin = new())
        {
            pos = initialPos;
            vel = initialVelocity;
            this.accMag = accMag;

            this.targetPoint = targetPoint;

            this.boundSize = boundSize;
            this.boundOrigin = boundOrigin;
        }

    }
    public struct KinematicBodyConstSpeed //but not constant velocity
    {
        
    }
    public struct KinematicBodyRecursive
    {
        public float2 Position => pos;

        public void Update(float dt)
        {
            targetBody.Update(dt);

            float2 a = accMag * normalize(targetBody.Position - pos);

            pos += vel * dt + 0.5f * dt * dt * a;
            vel += dt * a;
        }

        float2 pos;
        float2 vel;
        float accMag;

        IKinematicBody targetBody;

        public KinematicBodyRecursive(float2 initialPos, float2 initialVelocity, float accMag, IKinematicBody targetBody)
        {
            pos = initialPos;
            vel = initialVelocity;
            this.accMag = accMag;

            this.targetBody = targetBody;
        }
    }

    public struct KinematicBodyStatic : IKinematicBody
    {
        public KinematicBodyStatic(float2 p) 
            => Position = p;

        public float2 Position { get; set; }
        public float2 Velocity { get => new(); set => UnityEngine.Debug.LogError("Setting velocity on static kinematic body."); }
        public float2 Acceleration => new();

        public void Update(float dt) {}
    }

    public struct Timer
    {

        public float2 time;
        public Timer(float maxTime) => time = new(maxTime);
        public bool Check(float dt) { time.x -= dt; if (time.x < 0) { time.x = time.y; return true; } return false; }

    }

}