using Unity.Mathematics;
using UnityEngine;

public class BulletEngineManager : MonoBehaviour
{

    public static BulletEngine bossEngine;

    public static void InitializeBossManager()
    { if (bossEngine == null) 
        bossEngine = Ins.DebugMode ? new(new() { Ins.DebugPlayer}, Ins.ArenaBoundSize, Ins.ArenaBoundOrigin ) : new(new() { Ins.Player }, Ins.ArenaBoundSize, Ins.ArenaBoundOrigin); 
    }

    public static Transform UsedPlayerTransform => Ins.DebugMode ? Ins.DebugPlayer.transform : Ins.Player.transform;

    public bool DebugMode;
    public DebugMiku DebugPlayer;
    public MikuMechControl Player;
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
