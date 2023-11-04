using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorMissileScript : MonoBehaviour
{
    public GameObject explosionPrefab;
    private MikuMechControl player;
    private int damage, frameTimer;
    private float spd, duration, acc, max, homingStr, Cturn, turnTimer;
    private Rigidbody2D rb;
    private Vector2 TargetDirection;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        frameTimer = 3;
        turnTimer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        duration -=Time.deltaTime;
        spd += acc*Time.deltaTime; if (spd>max) spd = max;
        if (duration<0) Destruction();
        turnTimer=TimerF(turnTimer);
    }

    void FixedUpdate()
    {   
        frameTimer--; 
        if(frameTimer==0){ frameTimer = 2;
            TargetDirection = (player.MousePos-rb.position).normalized;
            if (homingStr != 0 && Vector2.Distance(player.MousePos,rb.position)<1) {
                homingStr = 0;
                Cturn = 0;
            }
            if(homingStr!=0 && turnTimer<0.001f){
                if (Vector3.Dot(transform.right, TargetDirection)>0){
                    Cturn = -homingStr;
                } else Cturn = homingStr;
                turnTimer = 0.16f;
            }
        }
        transform.eulerAngles += Cturn * Time.fixedDeltaTime * Vector3.forward;
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
        if(explosionPrefab!=null){
            GameObject expl = Instantiate(explosionPrefab, transform.position, Quaternion.Euler(new Vector3(0, 180, 0)));
            Destroy(expl, 2);
        }
        Destroy(transform.GetChild(0).gameObject, 2);
        transform.DetachChildren();
        
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

    private float TimerF( float val){
        if(val>=0){
            val-=Time.deltaTime;
            if (val<0) val = 0;
        }
        return val;
    }

}