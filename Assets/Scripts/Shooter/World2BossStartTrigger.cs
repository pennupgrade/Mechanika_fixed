using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.Threading;

public class World2BossStartTrigger : MonoBehaviour
{
    public GameObject wall, falseBase;
    public Boss2GMScript GM;
    void OnTriggerEnter2D(Collider2D c){
        if(c.gameObject.tag=="Player"){
            wall.SetActive(true);
            falseBase.SetActive(false);
            //start VN

            Action onVNFinish = () => 
            {
                GM.StartFight();
                Destroy(gameObject);
                SaveData.W2VNCompleted = true;
            };

            VNMain.Activate(VNMain.MikuCharisStory, onVNFinish, SaveData.W2VNCompleted);
        }
    }
}
