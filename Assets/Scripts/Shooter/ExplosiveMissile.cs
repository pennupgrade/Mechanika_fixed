using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveMissile : MonoBehaviour, IMissile
{
    private GameObject player;
    public GameObject explosionPrefab;
    private int damage, damage2, frameTimer;
    private float spd, duration, acc, max, homingStr, homingAccel, Cturn;
    private Rigidbody2D rb;
    private Vector3 Target, TargetDirection;
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
        spd += acc*Time.deltaTime;
        if (spd>max) spd = max;
        acc += homingAccel*Time.deltaTime;
        if(acc>180)acc=180;
        
        if (duration<0) Destruction();
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
            if(homingStr!=0){
                if (Vector3.Dot(transform.right, TargetDirection)>0){
                    Cturn = -homingStr;
                } else Cturn = homingStr;
            }
        }
        if(Vector3.Distance(Target,transform.position)<1) Destruction();
    }

    void OnCollisionEnter2D(Collision2D c){
        if (c.gameObject.tag=="Environment"){
            Destruction();
        }else if (c.gameObject.TryGetComponent<MikuMechControl>(out MikuMechControl miku)){
            miku.Damage(damage, false);
            Destruction();
        }
        
    }

    private void Destruction(){
        if(explosionPrefab!=null){
            GameObject expl = Instantiate(explosionPrefab, transform.position, Quaternion.Euler(new Vector3(0, 180, 0)));
            Destroy(expl, 2);
        }
        var d = Vector3.Distance(player.transform.position,transform.position);
        if (d<3) {
            player.GetComponent<MikuMechControl>().Damage(50+(int)((3-d)*damage/3), false);
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
    public void SetValues (int dmg, float timer, float homingStrength, bool stun, GameObject player){
        this.player = player;
        damage = dmg;
        duration = timer;
        homingStr = homingStrength;
    }
}
