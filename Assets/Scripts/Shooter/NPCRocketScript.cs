using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCRocketScript : MonoBehaviour, IMissile
{
    private GameObject player;
    public GameObject explosionPrefab;
    private int damage, frameTimer;
    private float spd, duration, acc, max, homingStr, Cturn;
    private bool stun, disabled, destroyed;
    private Rigidbody2D rb;
    private Vector3 TargetDirection;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        frameTimer = 3;
    }

    // Update is called once per frame
    void Update()
    {
        if(player==null) return;
        duration -=Time.deltaTime;
        if(duration<0){ spd += acc*Time.deltaTime;
            if (spd>max) spd = max;
        }
        if (duration<-3.2f) Destruction();
        if (!disabled&& Vector3.Distance(player.transform.position,(Vector3)rb.position)<4) {
            homingStr = 50; disabled = true;
        }
    }

    void FixedUpdate()
    {   
        transform.eulerAngles += Cturn * Time.fixedDeltaTime * Vector3.forward;
        rb.MovePosition(rb.position+(Vector2)(Time.fixedDeltaTime*spd*transform.up));
        if(player==null) return;
        frameTimer--;
        if(frameTimer==0){ 
            if (duration<0) frameTimer = 3;
            else frameTimer = 1;
            TargetDirection = (player.transform.position-(Vector3)rb.position).normalized;
            //if (homingStr != 0 && Vector2.Distance(player.MousePos,rb.position)<2) homingStr = 0;
            if(homingStr!=0){
                if (Vector3.Dot(transform.right, TargetDirection)>0){
                    Cturn = -homingStr;
                } else Cturn = homingStr;
            }
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
        if (destroyed) return;
        destroyed = true;
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
        homingStr = homingStrength;
        this.stun = stun;
    }
}
