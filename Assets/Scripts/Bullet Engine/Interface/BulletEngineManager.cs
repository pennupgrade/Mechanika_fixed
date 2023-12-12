using Unity.Mathematics;
using UnityEngine;

public class BulletEngineManager : MonoBehaviour
{

    public static BulletEngine bossEngine;

    public static void InitializeBossManager()
    { if (bossEngine == null) bossEngine = new(new() { Ins.Player }, Ins.ArenaBoundSize, Ins.ArenaBoundOrigin); }

    public DebugMiku Player;
    public Transform Boss;
    public Camera ArenaCamera;
    public float2 ArenaBoundSize;
    public float2 ArenaBoundOrigin;

    public static BulletEngineManager Ins { get; private set; }

    private void Awake()
        => Ins = this;

    private void Update()
    {
        BulletEngine.CheckAll();
        BulletEngine.CheckAllWall();
        BulletEngine.DrawAll();
        BulletEngine.UpdateAll(Time.deltaTime);
    }

    private void OnDisable()
    {
        BulletEngine.DisposeAll();
        bossEngine = null;
    }
}
