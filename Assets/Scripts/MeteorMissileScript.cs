using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorMissileScript : MonoBehaviour
{
    private MikuMechControl player;
    private int damage, frameTimer;
    private float spd, duration, acc, max, homingStr, Cturn;
    private Rigidbody2D rb;
    private Vector2 TargetDirection;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        frameTimer = 3;
    }

    // Update is called once per frame
    void Update()
    {
        duration -=Time.deltaTime;
        spd += acc*Time.deltaTime; if (spd>max) spd = max;
        if (duration<0) Destruction();
    }

    void FixedUpdate()
    {   
        frameTimer--;
        if(frameTimer==0){ frameTimer = 3;
            TargetDirection = (player.MousePos-rb.position).normalized;
            if (homingStr != 0 && Vector2.Distance(player.MousePos,rb.position)<2) homingStr = 0;
            if(homingStr!=0){
                if (Vector3.Dot(transform.right, TargetDirection)>0){
                    Cturn = -homingStr;
                } else Cturn = homingStr;
                transform.eulerAngles += Cturn * Time.deltaTime * Vector3.forward;
            }
        }
        rb.MovePosition(rb.position+(Vector2)(Time.fixedDeltaTime*spd*transform.up));
    }

    void OnCollisionEnter2D(Collision2D c){
        if (c.gameObject.tag=="Environment"){
            Destruction();
        }else if (c.gameObject.TryGetComponent<IEnemy>(out IEnemy enemy)){
            enemy.Damage(damage, true);
            Destruction();
        }
        
    }

    private void Destruction(){
        Destroy(gameObject);
    }

    public void SetValues (int dmg, float startSpeed, float accel, float maxSpeed, float timer, float homing, GameObject p){
        damage = dmg;
        spd = startSpeed;
        acc = accel;
        duration = timer;
        max = maxSpeed;
        homingStr = homing;
        player = p.GetComponent<MikuMechControl>();
    }

}