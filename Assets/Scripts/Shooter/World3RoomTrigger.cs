using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World3RoomTrigger : MonoBehaviour
{
    [SerializeField] private GM3Script gm;
    [SerializeField] private int number;
    void OnTriggerEnter2D(Collider2D c){
        if(c.gameObject.tag == "Player"){
            gm.lockDown(number);
            Destroy(gameObject);
        }
    }
}
