using Unity.Mathematics;
using UnityEngine;

public class BulletEngineManager : MonoBehaviour
{

    public static BulletEngine bossEngine;

    public static void InitializeBossManager()
    { if (bossEngine == null) bossEngine = new(new() { Ins.Player }); }

    public DebugMiku Player;
    public Transform Boss;
    public Camera ArenaCamera;

    public static BulletEngineManager Ins { get; private set; }

    private void Awake()
        => Ins = this;

    private void Update()
    {
        BulletEngine.CheckAll();
        BulletEngine.DrawAll();
        BulletEngine.UpdateAll(Time.deltaTime);
    }

    private void OnDisable()
    {
        BulletEngine.DisposeAll();
        bossEngine = null;
    }
}
