using System;
using Unity.Mathematics;

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

    public struct KinematicBody
    {

        public float2 p;
        public float2 v;
        public float2 a;

        public KinematicBody(float2 p = new(), float2 v = new(), float2 a = new())
        { this.p = p; this.v = v; this.a = a; }

        public void Update(float dt)
        {
            p += v * dt + .5f * a * dt * dt;
            v += a * dt;
        }

    }

    public struct Timer
    {

        public float2 time;
        public Timer(float maxTime) => time = new(maxTime);
        public bool Check(float dt) { time.x -= dt; if (time.x < 0) { time.x = time.y; return true; } return false; }

    }

}