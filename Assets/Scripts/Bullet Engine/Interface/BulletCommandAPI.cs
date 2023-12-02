using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Utilities;
using static Utilities.MathUtils;
using static Unity.Mathematics.math;
using Rand = UnityEngine.Random;

using Position = PositionParameter;
using Group = BulletEngine.GroupAccessor;
using System.Linq;

using static Utilities.Utils;
using BulletUtilities;

public struct GroupParameter
{

    public IReadOnlyList<Group> Groups => groups;
    List<Group> groups;
    static List<Group> StringsToGroups(BulletEngine engine, List<(string, BulletMaterial?)> tags) { List<Group> groups = new(); tags.ForEach(s => groups.Add(engine.GetGroupAccessor(s.Item1, s.Item2))); return groups; }
    public GroupParameter(BulletEngine engine, (string, BulletMaterial?) group) : this(engine, new List<(string, BulletMaterial?)>() { group }) { }
    public GroupParameter(BulletEngine engine, string group) : this(engine, (group, null)) { }
    public GroupParameter(BulletEngine engine, List<(string, BulletMaterial?)> groups) { i = groups.Count; this.groups = StringsToGroups(engine, groups); }
    public GroupParameter Merge(GroupParameter o) { foreach (var g in o.Groups) groups.Add(g); return this; }

    /// <summary>
    /// Iterate through the groups and do whatever with each group.
    /// </summary>
    /// <param name="Engine"></param>
    /// <param name="Iterator Action"></param>
    public void ForEach(BulletEngine engine, Action<Group> action)
    { foreach (var group in groups) action(group); }

    //
    int i;
    public Group Get(int i) => groups[i];
    public Group GetNext() { i = (i + 1) % groups.Count; return Get(i); }

}

public struct PositionParameter
{

    Transform transform;
    float2? position;

    public PositionParameter(Transform transform) { this.transform = transform; this.position = null; }
    public PositionParameter(float2 position) { this.transform = null; this.position = position; }

    public float2 Pos => position ?? transform.position.xy();

}

public static class BulletCommandInstantAPI
{
    public static void SetBulletVelocity(this BulletEngine engine, GroupParameter groups, float2 newVelocity)
    {
        groups.ForEach(engine, g =>
        {
            g.bullets.ForEachIndex((b, i) => { IBulletKinematic bb = (IBulletKinematic) b; bb.Velocity = newVelocity; engine.Set(g, bb, i); }); //HORRIBLE TEMP DOWNCAST.. GROUP BY TYPE BUT SHIT no
        });
    }

    //
    public static void CreateBulletStraight(this BulletEngine manager, GroupParameter group, float2 pos, float2 dir, Func<float2, float2> dirToVelocity, float radius = 0.4f, float duration = 10f)
        => manager.Add(group.GetNext(), new BulletKinematic(pos, dirToVelocity(dir), new(), radius, duration));

    //
    public static void CreateBulletBurst(this BulletEngine engine, GroupParameter group, float2 pos, int count, float2 velocityBounds, float duration = 10f, float radius = 0.4f)
        => Utils.ForLength(count, i => engine.Add(group.GetNext(), new BulletKinematic(pos, (velocityBounds.x + Rand.Range(0f, 1f) * (velocityBounds.y - velocityBounds.x))*normalize(Rand.insideUnitCircle), radius, duration)));
}

//public class CoroutineExecutor : MonoBehaviour
//{ public static CoroutineExecutor Executor { get; private set; } = new GameObject().AddComponent<CoroutineExecutor>(); void OnEnable() => Executor = Executor is not null ? Executor : new GameObject().AddComponent<CoroutineExecutor>(); }

public static class BulletCommandGradualAPI
{

    public static void StartCommand(IEnumerator coroutine, Action onCompletion = null)
    { BulletEngineManager.Ins.StartCoroutine(RunCommandWithEnd(coroutine, onCompletion)); } // :)

    static IEnumerator RunCommandWithEnd(IEnumerator coroutine, Action onCompletion)
    {
        yield return BulletEngineManager.Ins.StartCoroutine(coroutine);
        onCompletion?.Invoke();
    }
    
    /// <summary>
    /// GroupParameter can be seen as a list of materials, the list of materials will be iterated through for each 'leg'. 
    /// Each leg is like how many rays the spiral shoots out.
    /// The spiral you made was a 2 leg since there were 2 bullets per spawn.
    /// Angular velocity is how fast the spiral rotates.
    /// I think every other parameter is pretty self explanatory.
    /// The random bool may be used to toggle how random the materials, but I may generalize it further and I haven't even implemented it so just ignore it.
    /// Theta parameter at the end is just the starting angle.
    /// </summary>
    /// <param name="engine"></param>
    /// <param name="groups"></param>
    /// <param name="position"></param>
    /// <param name="deltaTheta"></param>
    /// <param name="timePerSpawn"></param>
    /// <param name="legCount"></param>
    /// <param name="totalTime"></param>
    /// <param name="random"></param>
    /// <param name="starting theta"></param>
    /// <returns></returns>
    public static IEnumerator CreateBulletSpiral(this BulletEngine engine, GroupParameter groups, Position position, float angularVelocity, float timePerSpawn, int legCount, float totalTime, float bulletSpeed = 1f, float bulletRadius = .4f, bool random = false, float theta = 0f) //add a way for radius to decrease or increase as we go away, would be a new type of bullet
    {
        float timePassed = 0f;
        float angle;

        while(timePassed <= totalTime)
        {
            for(int i=0; i<legCount; i++)
            {
                angle = theta + i * 2f * PI / (float)legCount;
                engine.Add(groups.GetNext(), new BulletKinematic(position.Pos, bulletSpeed * float2(cos(angle), sin(angle)), new(), bulletRadius));
            }

            theta += angularVelocity * timePerSpawn;
            timePassed += timePerSpawn; yield return new WaitForSeconds(timePerSpawn);
        }
    }

    /// <summary>
    /// Just like the Bullet Line method, the groups can be seen as a list of materials that the spawner will alternate through.
    /// </summary>
    /// <param name="engine"></param>
    /// <param name="groups"></param>
    /// <param name="pos"></param>
    /// <param name="radius"></param>
    /// <param name="distDensity"></param>
    /// <param name="time"></param>
    /// <param name="generatorFunction"></param>
    /// <returns></returns>
    public static IEnumerator CreateBulletCircleGradual(this BulletEngine engine, GroupParameter groups, Position pos, float radius, float distDensity, float time, Func<float2, float, ITBullet> generatorFunction, float3 trigParams = new(), bool posOrigin = true)
    {
        float theta = 0f;
        float dTheta = 1f / (distDensity * radius);
        float posOriginFloat = !posOrigin ? 1f : 0f;

        float startTime = Time.timeSinceLevelLoad;

        while(theta < 2f * PI)
        {
            float r = radius + trigParams.y * sin(theta * trigParams.x + trigParams.z);
            ITBullet b = generatorFunction(float2(theta, r), Time.timeSinceLevelLoad/*(theta/dTheta) * time / (distDensity * radius * 2f * PI)*/); b.Position = pos.Pos + posOriginFloat * r * new float2(cos(theta), sin(theta));
            engine.Add(groups.GetNext(), b);
            theta += dTheta;
            yield return new WaitForSeconds(time / (distDensity * radius * 2f * PI));
        }
    }
    public static IEnumerator CreateBulletCircleGradual(this BulletEngine engine, GroupParameter groups, Position pos, float radius, float distDensity, float time, Func<float2, ITBullet> generatorFunction, float3 trigParams = new())
        => CreateBulletCircleGradual(engine, groups, pos, radius, distDensity, time, (polar, time) => generatorFunction(polar), trigParams);

    /// <summary>
    /// Creates bullets conforming to a given generator function. 
    /// First parameter of the generator function is the 'dummy' variable, which has a specified start, end, and delta per step of the coroutine.
    /// Second parameter of the generator function is simply how much time has passed in the coroutine.
    /// Time step is how long the coroutine waits beteween each of its steps.
    /// </summary>
    /// <param name="engine"></param>
    /// <param name="groups"></param>
    /// <param name="dummyStart"></param>
    /// <param name="dummyEnd"></param>
    /// <param name="dummyStep"></param>
    /// <param name="timeStep"></param>
    /// <param name="generatorFunction"></param>
    /// <returns></returns>
    public static IEnumerator CreateBullets(this BulletEngine engine, GroupParameter groups, float dummyStart, float dummyEnd, float dummyStep, float timeStep, Func<float, float, ITBullet> generatorFunction)
    {
        float dummy = dummyStart;
        float timePassed = 0f;

        while(dummy <= dummyEnd)
        {
            engine.Add(groups.GetNext(), generatorFunction(dummy, timePassed));
            timePassed += timeStep;
            dummy += dummyStep;
            yield return new WaitForSeconds(timeStep);
        }
    }

    /// <summary>
    /// The Group Parameters parameter can be seen as a simple list of materials. This method will iterate over the list of materials, using the next material in the list for each bullet spawned in the line.
    /// </summary>
    /// <param name="engine"></param>
    /// <param name="groups"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="distDensity"></param>
    /// <param name="speed"></param>
    /// <param name="generatorFunction"></param>
    /// <returns></returns>
    public static IEnumerator CreateBulletLine(this BulletEngine engine, GroupParameter groups, Position start, Position end, float distDensity, float speed, Func<float, ITBullet> generatorFunction)
    {
        float d = 0f; float D = 0f;
        float distPerBullet = 1f / distDensity;
        Group g;
       
        while(d <= D)
        {
            D = length(end.Pos - start.Pos); //we only need this in case of varying Position, might not even make much sense tbh
            g = groups.GetNext();
            ITBullet b = generatorFunction(d/D); b.Position = start.Pos + normalize(end.Pos - start.Pos) * d;
            engine.Add(g, b);
            d += distPerBullet;
             
            yield return new WaitForSeconds(distPerBullet / speed);
        }
    }

    public static IEnumerator CreateBulletSlideIn(this BulletEngine engine, GroupParameter groups, Position pos, float2 radiusRange, float2 thetaRange, int repeatAmount, float fallTime, float timeStep, Action<BulletEngine, GroupParameter> deathAction, GroupParameter burstGroup, float radius = 0.4f)
    {

        float2 pEnd, pStart, space;

        for(int i =0; i<repeatAmount; i++)
        {
            space = Rand.Range(thetaRange.x, thetaRange.y);
            pStart = pos.Pos + (radiusRange.y + 5f) * space;
            pEnd = pos.Pos + Rand.Range(radiusRange.x, radiusRange.y) * space;

            engine.Add(groups.GetNext(), new BulletDeath<BulletKinematic>(new(pEnd, (pStart - pEnd) / fallTime, new(), radius, fallTime), deathAction, engine, burstGroup));

            yield return new WaitForSeconds(timeStep);

        }

    }

    public static IEnumerator CreateBulletSpikeBall(this BulletEngine engine, GroupParameter groups, Position pos, float partitionCount, float2 rRange, float variation, float bulletDensity, float formationSpeed, Func<float2, float, ITBullet> generatorFunction, float startTheta = 0f, bool posOrigin = true)
    {

        float theta = startTheta;
        float partitionSize = 2f * PI / partitionCount;
        float rtheta;
        float timePerBullet = 1f / (bulletDensity * formationSpeed); //without the annoying calcuations, it's db/dtheta but i can change that if i do the annoying theta in terms of intersection stuff to get correct length theta(length)
        float posOriginFloat = !posOrigin ? 1f : 0f;

        float startTime = Time.timeSinceLevelLoad;

        while (theta <= 2f * PI+startTheta)
        {

            rtheta = fmod(theta, partitionSize) - partitionSize * .5f;

            float stheta = theta - rtheta - partitionSize * .5f;
            float etheta = theta - rtheta + partitionSize * .5f;

            uint seedStart = (uint) (1000f*(stheta))+100u;
            uint seedEnd = (uint) (1000f*(etheta))+100u;

            float r1 = new Unity.Mathematics.Random(seedStart).NextFloat(-1f, 1f) * variation + rRange.x + (rRange.y - rRange.x) * step(0f, fmod(stheta + partitionSize * .5f, partitionSize * 2f) - partitionSize);
            float r2 = new Unity.Mathematics.Random(seedEnd).NextFloat(-1f, 1f) * variation +  rRange.x + (rRange.y - rRange.x) * step(0f, fmod(etheta + partitionSize * .5f, partitionSize * 2f) - partitionSize);

            float2 space = float2(sin(partitionSize * .5f), cos(partitionSize * .5f));

            float2 p1 = r1 * space;
            float2 p2 = r2 * space * float2(-1f, 1f);

            float m = tan(rtheta+PI*.5f);
            float s = (p2.y - p1.y) / (p2.x - p1.x);

            float x = (p1.y - s * p1.x) / (m - s); //sign(x) should == sign(cos(rtheta+PI*.5))
            float r = length(float2(x, x*m));


            float2 bulletPos = pos.Pos + posOriginFloat * r * PolarToCartesian(theta);// Rotate(stheta, new float2(x, x*m));
            ITBullet b = generatorFunction(float2(theta, r), Time.timeSinceLevelLoad - startTime/*bulletDensity * theta * timePerBullet*/); b.Position = bulletPos;
            engine.Add(groups.GetNext(), b);

            theta += 1f / bulletDensity;
            yield return new WaitForSeconds(timePerBullet);
        }

    }

    /// <summary>
    /// Creates a bullet spawner that will act kinematically.
    /// Every 'spawnTimeInterval' the spawner will spawn a bullet, if the spawner has a constant velocity it'll be like a bullet line.
    /// It'll kind of look like spawning a bullet in a curve manner if you set the parameters and accelerations right.
    /// The Kinematic Body will be able to be attracted/repulsed to/from a certain point, that certain point might also be a Kinematic Body so you can have bodies attracted to moving bodies
    /// attracted to moving bodies.
    /// Even if spawnTimeInterval is large, dt will be small which means updating will be precise.
    /// Can be attracted to player or something, transform instead of Kinematic Body.
    /// </summary>
    /// <param name="engine"></param>
    /// <param name="groups"></param>
    /// <param name="body"></param>
    /// <param name="dt"></param>
    /// <param name="spawnTimeInterval"></param>
    /// <param name="liveTimes"></param>
    /// <returns></returns>
    public static IEnumerator CreateBulletSpawner(this BulletEngine engine, GroupParameter groups, IKinematicBody body, float dt, float spawnTimeInterval, float liveTime, Func<float, ITBullet> generatorFunction)
    {
        
        float timeAccum = 0f;

        ITBullet b;
        while(timeAccum <= liveTime)
        {
            body.Update(dt);

            b = generatorFunction(timeAccum); b.Position = body.Position;
            engine.Add(groups.GetNext(), b);

            yield return new WaitForSeconds(dt);
        }

    }

}

public interface IKinematicBody
{
    float2 Position { get; }
    void Update(float dt);
}

public struct KinematicBodyPoint : IKinematicBody
{
    
    public float2 Position => pos;

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