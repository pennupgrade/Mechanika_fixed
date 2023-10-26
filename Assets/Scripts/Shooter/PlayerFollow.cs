using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    private GameObject Player;
    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Player==null) return;
        float interpolation = Time.fixedDeltaTime*8;
        
        Vector3 position = this.transform.position;
        position.y = Mathf.Lerp(this.transform.position.y, Player.transform.position.y, interpolation);
        position.x = Mathf.Lerp(this.transform.position.x, Player.transform.position.x, interpolation);
        
        this.transform.position = position;
    }
}
