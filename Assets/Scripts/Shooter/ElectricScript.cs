using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricScript : MonoBehaviour
{   
    private GameObject player;
    private int frameTimer, dmg;
    void Start(){
        frameTimer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null){
            if(frameTimer==0){
                if(Vector3.Distance(player.transform.position, transform.position)<1.8f){
                    player.GetComponent<MikuMechControl>().MeleeDamage(dmg, true);
                }
                frameTimer=5;
            }
            frameTimer--;
        }
    }

    public void SetPlayer(GameObject player, int dmg){
        this.player = player;
        this.dmg=dmg;
    }
}
