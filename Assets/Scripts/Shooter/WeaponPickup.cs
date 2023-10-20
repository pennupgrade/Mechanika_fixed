using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public int weapon;
    // Start is called before the first frame update

    void OnTriggerEnter2D(Collider2D c){
        if(c.gameObject.tag == "Player"){
            c.gameObject.GetComponent<MikuMechControl>().UnlockWeapon(weapon);
            Destroy(gameObject);
        }
    }
}
