using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using Utilities;
using System.Linq;
using System.Collections;

using static Utilities.Utils;
using System;

using static Unity.Mathematics.math;

public partial class BulletEngine
{

    //
    public static readonly int MaxBulletCount = 4096;

    //
    bool doesUpdate = true;
    public static void DestroyAllEngines() => Engines.Clear();
    public void RemoveAllInteractables() => interactables.Clear();
    public void DisableUpdating() => doesUpdate = false;
    static List<BulletEngine> Engines = new();
    public static void UpdateAll(float dt) => Engines.ForEach(m => m.Update(dt));
    public static void CheckAll() => Engines.ForEach(m => m.Check());
    public static void CheckAllWall() => Engines.ForEach(m => m.CheckWall()); // could be way more optimized if iterate through group of the wall interactable bullets
    public static void DrawAll() => Engines.ForEach(m => m.Draw());
    public static void DisposeAll() { Engines.ForEach(m => m.Dispose()); Engines.Clear(); }

    //
    readonly Dictionary<string, List<ITBullet>> bullets = new();
    readonly Dictionary<string, EngineDrawer> drawers = new();
    readonly IList<IBulletEngineInteractable> interactables = new List<IBulletEngineInteractable>();

    //
    public void CreateGroup(string group, BulletMaterial? mat) { if (mat is null) CreateGroup(group); else CreateGroup(group, (BulletMaterial) mat); }
    public void CreateGroup(string group, BulletMaterial mat) { bullets.Add(group, new()); drawers.Add(group, new(mat)); }
    public void CreateGroup(string group) { bullets.Add(group, new()); drawers.Add(group, new()); }

    /// <summary>
    /// It'll create a new group if it doesn't exist, and if you specified a shader path, it'll make that group use your specified shader.
    /// </summary>
    /// <param name="group"></param>
    /// <param name="The bullet material"></param>
    /// <returns></returns>
    public GroupAccessor GetGroupAccessor(string group, BulletMaterial? bulletMat = null) { if (!bullets.ContainsKey(group)) CreateGroup(group, bulletMat); return new() { bullets = bullets[group], drawer = drawers[group] }; }
    public GroupAccessor GetGroupAccessor(string group, string localPath) => GetGroupAccessor(group, new BulletMaterial(localPath));

    //
    int uniqueGroupIndex;
    public string UniqueGroup { get { uniqueGroupIndex++; return "UNIQUEGROUP" + uniqueGroupIndex; } }

    public struct GroupAccessor { public List<ITBullet> bullets; public EngineDrawer drawer; }

    //
    public BulletEngine() => Engines.Add(this);
    public BulletEngine(List<IBulletEngineInteractable> interactables) : this() => this.interactables = interactables;
    public BulletEngine(List<IBulletEngineInteractable> interactables, float2 boundSize, float2 boundOrigin) : this(interactables) { this.boundSize = boundSize; this.boundOrigin = boundOrigin; }

    //
    public void Update(float dt)
    {
        if(!doesUpdate) return;
        bullets.ForEach((group, bullets) => { var g = GetGroupAccessor(group); bullets.ForEachIndex((b, i) => { if(b.Update(dt)) Set(g, b, i); else RemoveAt(g, i); });  });
    }

    public void Check()
    {
        GroupAccessor g;
        List<int> disposalIndices;
        foreach (var i in interactables.Where(i => i.CanBeHit))
            foreach (KeyValuePair<string, List<ITBullet>> kvp in bullets)
            {
                g = GetGroupAccessor(kvp.Key);
                disposalIndices = new();
                kvp.Value.ForEachIndex((b, ii) =>
                {
                    if (Utilities.Collision.CircleCircle(b.Position, b.Radius, i.Position, i.Radius)) { i.Hit(b.Damage); disposalIndices.Add(ii); }
                });
                int offset = 0;
                disposalIndices.ForEach(ind => { RemoveAt(g, ind + offset); offset--; });
            }
    }

    public void Draw()
        => drawers.ForEach(d => d.DrawLoop());

    //
    bool allowCreation = true;
    IEnumerator PauseCreationCoro(float timeSeconds) { allowCreation = false; yield return new WaitForSeconds(timeSeconds); allowCreation = true; }
    public void PauseCreation(float timeSeconds, MonoBehaviour coroutineRunner) => coroutineRunner.StartCoroutine(PauseCreationCoro(timeSeconds));

    public void Add(GroupAccessor group, ITBullet b)
    {
        if (!allowCreation) return;
        group.bullets.Add(b);
        group.drawer.Add(b);
    }
    public void RemoveAt(GroupAccessor group, int i)
    {
        group.bullets.RemoveAt(i);
        group.drawer.RemoveAt(i);
    }
    public void Set(GroupAccessor group, ITBullet b, int i)
    {
        group.bullets[i] = b;
        group.drawer.Set(b, i);
    }
    public void TransformBullets(GroupAccessor group, Func<ITBullet, ITBullet> transformFunc)
    {
        for(int i=0; i<group.bullets.Count; i++)
            group.bullets[i] = transformFunc(group.bullets[i]);
    }

    //
    public void Dispose()
        => drawers.ForEach(d => d.Dispose());

    public void Clear()
    {
        bullets.ForEach(bullets => bullets.Clear());
        drawers.ForEach(drawer => drawer.Clear());
    }

}

public partial class BulletEngine
{

    public float2 BoundSize => boundSize;
    public float2 BoundOrigin => boundOrigin;
    float2 boundSize;
    float2 boundOrigin;

    public void CheckWall() 
    {

        ITBullet b;
        float2? wallNormal;

        foreach(var kvp in bullets)
        {
            GroupAccessor g = GetGroupAccessor(kvp.Key);
            List<int> disposalIndices = new();

            for(int i=0; i<kvp.Value.Count; i++)
            {
                b = kvp.Value[i];

                float2 p = b.Position;
                float r = b.Radius;

                wallNormal = Utilities.Collision.PointCollideArena(this, p - boundOrigin, r);
                if(wallNormal != null) 
                {
                    if(b.OnHitWall((float2) wallNormal)) disposalIndices.Add(i);
                    kvp.Value[i] = b;
                }
            }

            int offset = 0;
            disposalIndices.ForEach(ind => { RemoveAt(g, ind + offset); offset--; });
        }
    }

}

public class EngineDrawer
{

    //Shaders
    public static readonly string Directory = "Unlit/BulletShaders/";

    //
    static readonly Bounds Bounds = new(Vector3.zero, 100f * Vector3.one);
    static readonly int Layer = LayerMask.NameToLayer("Bullet");

    //
    readonly List<Matrix4x4> transforms = new();
    readonly List<float> radiuses = new();
    readonly List<Vector4> directions = new(); //TODO: remove

    Matrix4x4[] transformArr;
    float[] radiusArr;
    Vector4[] directionArr;

    public BulletMaterial Material { get => bulletMat; set { bulletMat = value; material = bulletMat.GetMaterial(); material.enableInstancing = true; } }
    BulletMaterial bulletMat;
    Material material;

    public void Add(ITBullet b)
    { transforms.Add(Matrix4x4.Translate(b.Position.xyz(-1f))); radiuses.Add(b.Radius); directions.Add(b.Direction.xyz().xyzw()); }
    public void RemoveAt(int i)
    { transforms.RemoveAt(i); radiuses.RemoveAt(i); directions.RemoveAt(i); }
    public void Set(ITBullet b, int i)
    { transforms[i] = Matrix4x4.Translate(b.Position.xyz(-1f)); radiuses[i] = b.Radius; directions[i] = b.Direction.xyz().xyzw(); }

    void UpdateBlocks()
    {
        radiusArr = radiuses.ToArray();
        directionArr = directions.ToArray();
        BulletPropertyBlock.SetFloatArray("Radiuses", radiusArr);
        BulletPropertyBlock.SetVectorArray("Directions", directionArr);
    }

    InfiniteMaterialBlock BulletPropertyBlock = new();

    public EngineDrawer(string localPath = "Default", Color? color = null) : this(new BulletMaterial(localPath, color)) { }
    public EngineDrawer(BulletMaterial bulletMat)
    {

        //
        this.Material = bulletMat;

    }

    public void DrawLoop()
    {
        UpdateBlocks();
        transformArr = transforms.ToArray();
        BulletPropertyBlock.DrawMeshInstanced(MeshUtils.QuadMesh, 0, material, transformArr, Layer, BulletEngineManager.Ins.ArenaCamera);
    }

    public void Dispose() { }

    public void Clear()
    {
        transforms.Clear();
        radiuses.Clear();
        directions.Clear();
    }

}

public struct BulletMaterial
{

    public Color? color;
    public string shaderPath;

    public BulletMaterial(string shaderPath, Color? color = null)
    { this.shaderPath = shaderPath; this.color = color; }

    public Material GetMaterial()
    {
        Material m = new(Shader.Find(EngineDrawer.Directory + shaderPath));
        if (color is not null) m.SetColor("_Color", (Color) color);
        return m;
    }

}