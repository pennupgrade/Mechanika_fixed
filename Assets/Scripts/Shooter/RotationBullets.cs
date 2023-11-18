using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationBullets : MonoBehaviour
{
    [SerializeField] float rotationSpeed;
    private float rSpd;
    // Start is called before the first frame update
    void Start()
    {
        rSpd = rotationSpeed+30*(Random.value-0.5f);
        if(Random.value >0.5f){
            rSpd = -rSpd;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.eulerAngles += rSpd * Time.fixedDeltaTime * Vector3.forward; 
    }
}
