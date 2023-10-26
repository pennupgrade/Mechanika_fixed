using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMissileScript : MonoBehaviour, IMissile
{
    private GameObject player;
    public GameObject explosionPrefab;
    private int damage, frameTimer;
    private float spd, duration, acc, max, homingStr, Cturn, turnTimer;
    private bool stun, disabled;
    private Rigidbody2D rb;
    private Vector3 TargetDirection;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        frameTimer = 3;
        disabled = false; turnTimer=0;
    }

    // Update is called once per frame
    void Update()
    {
        duration -= Time.deltaTime;
        spd += acc*Time.deltaTime;
        if (spd>max) spd = max;
        if (duration<0) Destruction();
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
            TargetDirection = (player.transform.position-(Vector3)rb.position).normalized;
            if(homingStr!=0&&turnTimer<0.001f){
                if (Vector3.Dot(transform.right, TargetDirection)>0){
                    Cturn = -homingStr;
                } else Cturn = homingStr;
                turnTimer = 0.16f;
            }
        }
        if (!disabled&& Vector3.Distance(player.transform.position,(Vector3)rb.position)<4) {
            homingStr = 30; disabled = true;
        }
    }

    void OnCollisionEnter2D(Collision2D c){
        if (c.gameObject.tag=="Environment"){
            Destruction();
        }else if (c.gameObject.TryGetComponent<MikuMechControl>(out MikuMechControl miku)){
            miku.Damage(damage, stun);
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

    public void SetSpeed (float initSpeed, float accel, float finalSpeed){
        spd = initSpeed;
        acc = accel;
        max = finalSpeed;
    }
    public void SetValues (int dmg, float timer, float homingStrength, bool stun, GameObject player){
        this.player = player;
        damage = dmg;
        duration = timer;
        homingStr = (0.9f+0.2f*Random.value)*homingStrength;
        this.stun = stun;
    }

    private float TimerF( float val){
        if(val>=0){
            val-=Time.deltaTime;
            if (val<0) val = 0;
        }
        return val;
    }
}

