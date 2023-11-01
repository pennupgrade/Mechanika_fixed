using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public GameObject gm;
    public int weapon;
    // Start is called before the first frame update

    void OnTriggerEnter2D(Collider2D c){
        IGameManager GM = gm.GetComponent<IGameManager>();
        if(c.gameObject.tag == "Player"){
            c.gameObject.GetComponent<MikuMechControl>().UnlockWeapon(weapon);
            if(weapon==3){
                GM.Dialogue("Unlocked: Senbonzakura", "Shotgun. Damage, range, and dispersion improve when charged.");
            } else if (weapon==4){
                GM.Dialogue("Unlocked: Nova", "Long charge and movement penalty compensated by high damage. Energy and damage linear to charge.");
            } else{
                GM.Dialogue("Unlocked: Meteor", "Missiles home in on cursor, homing stops within certain distance. High damage, high energy usage.");
            }
            Destroy(gameObject);
        }
    }
}
