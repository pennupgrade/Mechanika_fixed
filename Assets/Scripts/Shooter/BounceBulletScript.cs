using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceBulletScript : MonoBehaviour, IBullet
{
    public GameObject explosionPrefab;
    private int damage, bounces;
    private float spd, duration, acc;
    private Rigidbody2D rb;
    private Vector2 velocity;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bounces = 0;
    }

    // Update is called once per frame
    void Update()
    {
        duration -=Time.deltaTime;
        spd -= acc*Time.deltaTime; if (spd<4) spd = 4;
        if (duration<0||bounces>6) Destruction();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position+(Vector2)(Time.fixedDeltaTime*(spd*transform.up+(Vector3)velocity)));
    }

    void OnCollisionEnter2D(Collision2D c){
        if (c.gameObject.tag=="Environment"){
            bounces++; spd +=0.6f;
            Vector3 newDir;
            GameObject expl = Instantiate(explosionPrefab, transform.position, Quaternion.Euler(new Vector3(0, 180, 0)));
            Destroy(expl, 2);
            try {
                newDir = Vector3.Reflect(transform.up, c.contacts[0].normal + c.contacts[1].normal);
            } catch (Exception e) {
                newDir = Vector3.Reflect(transform.up, c.contacts[0].normal);
            } 
            var a = 1;
            if(newDir.y<0) a = -1;
            transform.eulerAngles = (a*Vector2.Angle(new Vector2(1,0), (Vector2)newDir)-90)* Vector3.forward;
        }else if(c.gameObject.tag == "Player"){
            c.gameObject.GetComponent<MikuMechControl>().Damage(damage,false);
            Destruction();
        }
        
    }

    private void Destruction(){
        if(explosionPrefab!=null){
            GameObject expl = Instantiate(explosionPrefab, transform.position, Quaternion.Euler(new Vector3(0, 180, 0)));
            Destroy(expl, 2);
        }
        Destroy(gameObject);
    }

    public void SetValues (int dmg, float speed, float timer, float accel, Vector2 v){
        damage = dmg;
        spd = speed;
        duration = timer;
        acc = accel;
        velocity = v;
    }

}
