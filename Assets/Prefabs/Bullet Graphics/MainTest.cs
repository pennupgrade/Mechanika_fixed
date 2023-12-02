using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static UnityEngine.Mathf;

using static Utilities.Swizzle;

public class MainTest : MonoBehaviour
{

    LaserController laserController;

    void Start()
    {
        laserController = GetComponent<LaserController>();
        LaserController.EnableParticles();
    }

    // Update is called once per frame
    void Update()
    {
        float t = Time.time;
        LaserController.DrawLaser(transform.position.xy(), (Vector2) transform.position.xy() + (5f+Sin(Time.time)*4f) * new Vector2(Cos(Time.timeSinceLevelLoad), Sin(Time.timeSinceLevelLoad)));
    }
}
