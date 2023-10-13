using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedKit : MonoBehaviour
{
    private int healing;
    // Start is called before the first frame update
    void Start()
    {
        healing = Random.Range(6,13)*10;
    }

    void OnTriggerEnter2D(Collider2D c){
        if(c.gameObject.tag == "Player"){
            c.gameObject.GetComponent<MikuMechControl>().Heal(healing);
            Destroy(gameObject);
        }
    }

}
