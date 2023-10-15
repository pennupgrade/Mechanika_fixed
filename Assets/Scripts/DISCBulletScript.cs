using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DISCBulletScript : MonoBehaviour, IBullet
{
    private bool doesDamage;
    private int layermask = 1 << 9;
    private int damage, bounces, frameTimer;
    private float spd, duration, acc;
    private Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bounces = 0; doesDamage = true;
        frameTimer=4;
    }

    // Update is called once per frame
    void Update()
    {
        duration -=Time.deltaTime;
        spd -= acc*Time.deltaTime; if (spd>16) spd = 16;
        if (duration<0||bounces>3) Destruction();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position+(Vector2)(spd*Time.fixedDeltaTime*transform.up));
        frameTimer--; 
        if(frameTimer==0){
            frameTimer=4;
            DamageEnemy();
        }
    }

    void OnCollisionEnter2D(Collision2D c){
        if (c.gameObject.tag=="Environment"){
            bounces++; spd +=2;
            Vector3 newDir = Vector3.Reflect(transform.up, c.contacts[0].normal);
            var a = 1;
            if(newDir.y<0) a = -1;
            transform.eulerAngles = (a*Vector2.Angle(new Vector2(1,0), (Vector2)newDir)-90)* Vector3.forward;
        }
    }

    private void Destruction(){
        Destroy(gameObject);
    }

    private IEnumerator stopDamage(){
        doesDamage = false;
        yield return new WaitForSeconds(0.2f);
        doesDamage = true;
    }

    private void DamageEnemy(){
        if(doesDamage){
            Collider2D c = Physics2D.OverlapCircle(transform.position, 0.7f, layermask);
            if(c!=null && c.gameObject.TryGetComponent<IEnemy>(out IEnemy enemy)){
                enemy.Damage(damage, false);
                StartCoroutine(stopDamage());
            }
        }
    }

    public void SetValues (int dmg, float speed, float timer, float accel, Vector2 v){
        damage = dmg;
        spd = speed;
        duration = timer;
        acc = accel;
    }

}