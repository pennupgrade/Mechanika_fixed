using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveMissile : MonoBehaviour, IMissile
{
    private GameObject player;
    public GameObject explosionPrefab, electricPrefab;
    private int damage, frameTimer;
    private float spd, duration, acc, max, homingStr, homingAccel, Cturn, turnTimer;
    private Rigidbody2D rb;
    private Vector3 Target, TargetDirection;
    private bool electric, destroyed;
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
        acc += homingAccel*Time.deltaTime;
        if(acc>180)acc=180;
        
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
                turnTimer = 0.24f;
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
        if (destroyed) return;
        destroyed = true;
        if (player!=null){
            if (electric){
                GameObject elect = Instantiate(electricPrefab, transform.position, Quaternion.identity);
                elect.GetComponent<ElectricScript>().SetPlayer(player, damage);
                Destroy(elect, 10+Random.value);
            } else {
                if(explosionPrefab!=null){
                    GameObject expl = Instantiate(explosionPrefab, transform.position, Quaternion.Euler(new Vector3(0, 180, 0)));
                    Destroy(expl, 2);
                }
                var d = Vector3.Distance(player.transform.position,transform.position);
                if (d<3) {
                    player.GetComponent<MikuMechControl>().MeleeDamage(50+(int)((3-d)*damage/3), false);
                }
            }
        }
        
        Destroy(transform.GetChild(0).gameObject, 2);
        transform.DetachChildren();
        
        Destroy(gameObject);
    }

    public void SetSpeed (float initSpeed, float accel, float finalSpeed){
        spd = initSpeed;
        acc = accel;
        max = finalSpeed;
    }
    public void SetTargetAndHomingAccel (Vector3 target, float hm) {
        Target = target;
        homingAccel = hm;
    }
    public void SetElectric(){
        electric = true;
    }
    public void SetValues (int dmg, float timer, float homingStrength, bool stun, GameObject player){
        this.player = player;
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
