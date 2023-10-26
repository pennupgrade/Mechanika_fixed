using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World2BossStartTrigger : MonoBehaviour
{
    public GameObject wall;
    public Boss2GMScript GM;
    void OnTriggerEnter2D(Collider2D c){
        if(c.gameObject.tag=="Player" && Input.GetKey(KeyCode.M)){
            wall.SetActive(true);
            //start VN

            //Start Bossfight - to be placed somewhere else
            GM.StartFight();

            Destroy(gameObject);
        }
    }
}
