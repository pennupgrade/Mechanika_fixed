using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

using Utilities;

public class LaserController : MonoBehaviour
{

    static LaserController ins;
    void Awake() {
        ins = this;
        LaserParticleSystem = Instantiate(LaserParticleSystem); 
        ins.LaserParticleSystem.gameObject.SetActive(false);
    }

    [SerializeField] Material LaserMaterial;
    [SerializeField] Material LaserTelegraphMaterial;
    [SerializeField] ParticleSystem LaserParticleSystem;

    public static void DrawLaser(Vector2 start, Vector2 end)
    {
        float quadHeight = 2f;

        // Laser Body
        ins.LaserMaterial.SetFloat("_UVYMult", quadHeight);
        ins.LaserMaterial.SetFloat("_LaserXScale", math.length(end - start));
        Matrix4x4 mat = Matrix4x4.TRS(start.xyz(-1f), Quaternion.FromToRotation(Vector3.right, ((Vector3)(end - start).xyz()).normalized), new Vector3(math.length(start - end), quadHeight, 1f));
        Graphics.DrawMesh(Utils.Quad, mat, ins.LaserMaterial, LayerMask.NameToLayer("Default"), Camera.main, 0, null, false, false);

        // Laser Particle
        ins.LaserParticleSystem.transform.position = end.xyz();
        ins.LaserParticleSystem.transform.rotation = Quaternion.LookRotation((end-start).xyz(-1f), Vector3.back);
    }

    public static void StartDrawTelegraph() 
        => ins.LaserTelegraphMaterial.SetFloat("_LastStartTime", Time.timeSinceLevelLoad); 
        
    public static void DrawTelegraph(Vector2 start, Vector2 end)
    {
        float quadHeight = 2f;

        // Laser Body
        ins.LaserTelegraphMaterial.SetFloat("_UVYMult", quadHeight);
        ins.LaserTelegraphMaterial.SetFloat("_LaserXScale", math.length(end - start));
        Matrix4x4 mat = Matrix4x4.TRS(start.xyz(2f), Quaternion.FromToRotation(Vector3.right, ((Vector3)(end - start).xyz()).normalized), new Vector3(math.length(start - end), quadHeight, 1f));
        Graphics.DrawMesh(Utils.Quad, mat, ins.LaserTelegraphMaterial, LayerMask.NameToLayer("Default"), Camera.main, 0, null, false, false);
    }

    public static void EnableParticles() { ins.LaserParticleSystem.gameObject.SetActive(true); ins.LaserMaterial.SetFloat("_LastStartTime", Time.timeSinceLevelLoad); }
    public static void DisableParticles() => ins.LaserParticleSystem.gameObject.SetActive(false);

}
