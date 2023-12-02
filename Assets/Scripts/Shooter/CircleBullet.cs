using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleBullet : MonoBehaviour
{
    private bool ready;
    private float speed, timer;
    private Vector3 target, targetDir;
    private Rigidbody2D rb;

    void Start() {
        ready = false;
        timer = 3;
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(ready){
            rb.MovePosition(rb.position+(Vector2)(Time.fixedDeltaTime*speed*-targetDir));
            timer -= Time.fixedDeltaTime;
            if (timer<0){
                Destroy(gameObject);
            }
        }else {
            if(Vector3.Distance(transform.position, target)>0.4){
                rb.MovePosition(rb.position+(Vector2)(Time.fixedDeltaTime*speed*targetDir));
            }
        }
    }
    void OnCollisionEnter2D(Collision2D c){
        if (c.gameObject.tag=="Environment"){
            Destroy(gameObject);
        }else if (c.gameObject.TryGetComponent<MikuMechControl>(out MikuMechControl miku)){
            miku.Damage(50, false);
            Destroy(gameObject);
        }
        
    }
    public void Ready(){
        GetComponent<CircleCollider2D>().enabled = true;
        var tmp = GetComponent<SpriteRenderer>().color;
        tmp.a = 1;
        GetComponent<SpriteRenderer>().color = tmp;
        ready = true;
    }
    public void SetSpeed(float s){
        speed = s;
    }
    public void SetTarget(Vector3 v){
        target = v;
        targetDir = (target-transform.position).normalized;
    }
}
