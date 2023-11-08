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
                GM.Dialogue("Unlocked: Senbonzakura", "Close range weapon. Damage, range, and dispersion improve when charged.");
            } else if (weapon==4){
                GM.Dialogue("Unlocked: Hibana", "Effective against larger enemies. High energy expenditure compensated by high damage.");
            } else{
                GM.Dialogue("Unlocked: Meteor", "Homing missiles track cursor. High damage, high energy usage.");
            }
            Destroy(gameObject);
        }
    }
}
