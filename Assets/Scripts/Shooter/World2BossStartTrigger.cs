using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World2BossStartTrigger : MonoBehaviour
{
    public GameObject wall;
    void OnTriggerEnter2D(Collider2D c){
        if(c.gameObject.tag=="Player"){
            wall.SetActive(true);
            //start VN
            Destroy(gameObject);
        }
    }
}
