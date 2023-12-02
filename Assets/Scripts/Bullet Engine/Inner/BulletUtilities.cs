using System;
using Unity.Mathematics;

using static Unity.Mathematics.math;

using Position = PositionParameter;
using Group = BulletEngine.GroupAccessor;

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

    public struct KinematicBodyConstAcc
    {

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
        void Update(float dt);
    }

    public struct KinematicBodyPoint : IKinematicBody
    {
        
        public float2 Position { get => pos; set => pos = value; }

        public void Update(float dt)
        {
            float2 a = accMag * normalize(targetPoint.Pos - pos);

            pos += vel * dt + 0.5f * dt * dt * a;
            vel += dt * a;
        }

        float2 pos;
        float2 vel;
        float accMag;

        Position targetPoint;

        public KinematicBodyPoint(float2 initialPos, float2 initialVelocity, float accMag, Position targetPoint)
        {
            pos = initialPos;
            vel = initialVelocity;
            this.accMag = accMag;

            this.targetPoint = targetPoint;
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

    public struct Timer
    {

        public float2 time;
        public Timer(float maxTime) => time = new(maxTime);
        public bool Check(float dt) { time.x -= dt; if (time.x < 0) { time.x = time.y; return true; } return false; }

    }

}