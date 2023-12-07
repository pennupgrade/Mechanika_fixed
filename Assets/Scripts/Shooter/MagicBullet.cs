using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicBullet : MonoBehaviour, IMissile
{
    private GameObject player;
    public GameObject explosionPrefab;
    private int damage, frameTimer, targetNum;
    private float spd, duration, acc, max, homingStr, Cturn, turnTimer;
    private Rigidbody2D rb;
    private Vector3 Target, TargetDirection;
    private bool upgraded, destroyed;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        frameTimer = 3;
        turnTimer=0;
        targetNum = 3;
    }

    // Update is called once per frame
    void Update()
    {
        if(player==null) return;
        duration -= Time.deltaTime;
        spd += acc*Time.deltaTime;
        if (spd>max) spd = max;
        if (duration<0) {
            if(targetNum>1){
                duration = 3;
                targetNum--;
                Target = player.transform.position+7*(Vector3)UnityEngine.Random.insideUnitCircle;
            } else if (targetNum==1){
                duration = 5;
                max=max+2;
                targetNum--;
                homingStr += 40;
            } else {
                Destruction();
            }
        }
        turnTimer=TimerF(turnTimer);
    }

    void FixedUpdate()
    {   
        transform.eulerAngles += Cturn * Time.fixedDeltaTime * Vector3.forward;
        rb.MovePosition(rb.position+(Vector2)(Time.fixedDeltaTime*spd*transform.up));
        if(player==null) return;
        frameTimer--;
        if(frameTimer==0){
            frameTimer = 2;
            if(targetNum==0){
                if (MyMath.InterceptDirection(player.transform.position, transform.position, player.GetComponent<MikuMechControl>().Velocity, max, out Vector3 result)){
                    TargetDirection = (Vector3)result;
                } else TargetDirection = (Vector3)(player.transform.position - transform.position).normalized;
            } else {
                TargetDirection = (Target-(Vector3)rb.position).normalized;
            }
            if(homingStr!=0 && turnTimer<0.001f){
                if (Vector3.Dot(transform.right, TargetDirection)>0){
                    Cturn = -homingStr;
                } else Cturn = homingStr;
                turnTimer = 0.1f;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D c){
        if (c.gameObject.tag=="Environment"){
            Vector3 newDir;
            try{
                newDir=Vector3.Reflect(transform.up, c.contacts[0].normal+c.contacts[1].normal);
            }catch (Exception e) {
                newDir = Vector3.Reflect(transform.up, c.contacts[0].normal);
            } 
            var a = 1;
            if(newDir.y<0) a = -1;
            transform.eulerAngles = (a*Vector2.Angle(new Vector2(1,0), (Vector2)newDir)-90)* Vector3.forward;
        }else if (c.gameObject.TryGetComponent<MikuMechControl>(out MikuMechControl miku)){
            miku.Damage(damage, false);
            Destruction();
        }
    }

    private void Destruction(){
        if (destroyed) return;
        destroyed = true;
        if(explosionPrefab!=null){
            GameObject expl = Instantiate(explosionPrefab, transform.position, Quaternion.Euler(new Vector3(0, 180, 0)));
            Destroy(expl, 2);
        }
        
        Destroy(transform.GetChild(0).gameObject, 5);
        Destroy(transform.GetChild(1).gameObject);
        transform.DetachChildren();
        
        Destroy(gameObject);
    }

    public void SetSpeed (float initSpeed, float accel, float finalSpeed){
        spd = initSpeed;
        acc = accel;
        max = finalSpeed;
    }
    public void SetValues (int dmg, float timer, float homingStrength, bool stun, GameObject player){
        this.player = player;
        damage = dmg;
        duration = timer;
        homingStr = homingStrength;
        upgraded = stun;
        Target = player.transform.position+8*(Vector3)UnityEngine.Random.insideUnitCircle;
    }

    private float TimerF( float val){
        if(val>=0){
            val-=Time.deltaTime;
            if (val<0) val = 0;
        }
        return val;
    }
}

