using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class LaserController : MonoBehaviour
{

    static LaserController ins;
    void Awake() {
        ins = this;
        LaserParticleSystem = Instantiate(LaserParticleSystem); 
        ins.LaserParticleSystem.gameObject.SetActive(false);
    }

    [SerializeField] Material LaserMaterial;
    [SerializeField] ParticleSystem LaserParticleSystem;

    public static void DrawLaser(Vector2 start, Vector2 end)
    {

        // Laser Body
        ins.LaserMaterial.SetFloat("_LaserXScale", math.length(end - start));
        Matrix4x4 mat = Matrix4x4.TRS(start, Quaternion.FromToRotation(Vector3.right, ((Vector3)(end - start).xyz()).normalized), new Vector3(math.length(start - end), 1f, 1f));
        Graphics.DrawMesh(Utils.Quad, mat, ins.LaserMaterial, LayerMask.NameToLayer("Default"), Camera.main, 0, null, false, false);

        // Laser Particle
        ins.LaserParticleSystem.transform.position = end.xyz();
    }

    public static void EnableParticles() => ins.LaserParticleSystem.gameObject.SetActive(true);
    public static void DisableParticles() => ins.LaserParticleSystem.gameObject.SetActive(false);

}
