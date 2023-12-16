using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.Threading;

public class World3BossStartTrigger : MonoBehaviour
{
    public GameObject wall;
    public Boss3GMScript GM;
    private GameObject Player;
    void OnTriggerEnter2D(Collider2D c){
        if(c.gameObject.tag=="Player"){
            Player = GameObject.FindWithTag("Player");
            wall.SetActive(true);
            //start VN

            Action onVNFinish = () => 
            {
                GM.StartFight();
                Destroy(gameObject);
                SaveData.W3VNCompleted = true;
                Player.GetComponent<MikuMechControl>().UnFreeze();
            };
            
            Player.GetComponent<MikuMechControl>().Freeze();
            VNMain.Activate(VNMain.MikuVengeStory, onVNFinish, SaveData.W3VNCompleted);
            
        }
    }
}
