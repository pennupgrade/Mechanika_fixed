using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CepheidBulletScript : MonoBehaviour, IBullet
{
    private bool friendly;
    private int damage;
    private float spd, duration, acc, Hspd, Hacc;
    private Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Hspd = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Hspd+=Hacc*Time.deltaTime;
        if(Hspd>2) Hacc = -8;
        else if(Hspd<-2) Hacc = 8;

        duration -=Time.deltaTime;
        spd -= acc*Time.deltaTime; if (spd<4) spd = 4;
        if (duration<0) Destruction();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position+(Vector2)(Time.fixedDeltaTime*(spd*transform.up+Hspd*transform.right)));

    }

    void OnCollisionEnter2D(Collision2D c){
        if (c.gameObject.tag=="Environment"){
            Destruction();
        }else if (c.gameObject.TryGetComponent<IEnemy>(out IEnemy enemy)){
            enemy.Damage(damage, false);
        }
    }

    private void Destruction(){
        Destroy(gameObject);
    }

    public void SetValues (int dmg, float speed, float timer, float accel){
        damage = dmg;
        spd = speed;
        duration = timer;
        acc = accel;
    }

    public void SetMode(int mode){
        if(mode==1||mode==3) Hacc = 8;
        else Hacc = -8;
    }

}