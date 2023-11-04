using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class LaserController : MonoBehaviour
{

    static LaserController ins;
    void Awake() => ins = this;

    [SerializeField] Material LaserMaterial;

    static readonly Mesh laserQuad = new ()
    {
        vertices = new Vector3[] 
            { new(0f, -1f), new(0f, 1f), new(1f, -1f), new(1f, 1f) },
        uv = new Vector2[]
            { new(0f, -1f), new(0f, 1f), new(1f, -1f), new(1f, 1f) },
        triangles = new int[]
            { 0, 1, 2, 1, 2, 3 }
    };

    public static void DrawLaser(Vector2 start, Vector2 end)
    {
        //Graphics.DrawMesh(laserQuad, Matrix4x4.TRS(start, Quaternion.FromToRotation(Vector3.right, (end - start).xyz()), new Vector3(math.length(start - end), 1f, 1f)), ins.LaserMaterial);
        ins.LaserMaterial.SetPass(0);
        Graphics.DrawMeshNow(laserQuad, Matrix4x4.TRS(start, Quaternion.FromToRotation(Vector3.right, (end - start).xyz()), new Vector3(math.length(start - end), 1f, 1f)));
        //Graphics.DrawMesh(laserQuad, start, Quaternion.FromToRotation(Vector3.right, (end - start).xyz()), new Vector3(math.length(start - end), 1f, 1f), ins.LaserMaterial);
    }

}
