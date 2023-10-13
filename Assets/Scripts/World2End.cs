using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World2End : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D c){
        if(c.gameObject.tag == "Player"){
            //collect data on weapons unlocked
            //fade out, change scene
            Debug.Log("End");
        }
    }
}
