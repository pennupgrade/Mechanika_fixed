using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExploderBullet : MonoBehaviour, IMissile
{
    public GameObject bulletPrefab;
    private int damage, frameTimer;
    private float spd, duration, acc, max, homingStr, Cturn, turnTimer;
    private Rigidbody2D rb;
    private Vector3 Target, TargetDirection;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        frameTimer = 3;
        turnTimer=0;
    }

    // Update is called once per frame
    void Update()
    {
        duration -=Time.deltaTime;
        spd += acc*Time.deltaTime;
        if (spd>max) spd = max;
        
        if (duration<0) Destruction();
        turnTimer=TimerF(turnTimer);
    }

    void FixedUpdate()
    {   
        transform.eulerAngles += Cturn * Time.fixedDeltaTime * Vector3.forward;
        rb.MovePosition(rb.position+(Vector2)(Time.fixedDeltaTime*spd*transform.up));
        frameTimer--;
        if(frameTimer==0){ 
            frameTimer = 3;
            TargetDirection = (Target-(Vector3)rb.position).normalized;
            //if (homingStr != 0 && Vector2.Distance(player.MousePos,rb.position)<2) homingStr = 0;
            if(homingStr!=0 && turnTimer<0.001f){
                if (Vector3.Dot(transform.right, TargetDirection)>0){
                    Cturn = -homingStr;
                } else Cturn = homingStr;
                turnTimer = 0.5f;
            }
        }
        if(Vector3.Distance(Target,transform.position)<1) Destruction();
    }

    void OnCollisionEnter2D(Collision2D c){
        if (c.gameObject.tag=="Environment"){
            Destruction();
        }else if (c.gameObject.tag=="Player"){
            Destruction();
        }
        
    }

    private void Destruction(){
        for (int i = 0; i<20; i++){
            GameObject bullet = Instantiate (bulletPrefab, transform.position, transform.rotation*Quaternion.Euler(0, 0, 360*Random.value));
            bullet.GetComponent<IBullet>().SetValues (damage, 10+Random.value, 0.7f+0.2f*Random.value, 7, Vector2.zero);
        }
        Destroy(gameObject);
    }

    public void SetSpeed (float initSpeed, float accel, float finalSpeed){
        spd = initSpeed;
        acc = accel;
        max = finalSpeed;
    }
    public void SetTargetAndHomingAccel (Vector3 target) {
        Target = target;
    }
    public void SetValues (int dmg, float timer, float homingStrength, bool stun, GameObject player){
        damage = dmg;
        duration = timer;
        homingStr = homingStrength;
    }

    private float TimerF( float val){
        if(val>=0){
            val-=Time.deltaTime;
            if (val<0) val = 0;
        }
        return val;
    }
}