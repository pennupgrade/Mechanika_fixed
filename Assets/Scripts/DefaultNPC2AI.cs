using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultNPC2AI : MonoBehaviour, IEnemy
{
    [Header("Prefabs")]
    public GameObject BulletPrefab;
    public GameObject MissilePrefab;
    [Header("Enemy Values")]
    [SerializeField] private int health, bulletDMG, bulletsLeft, maxBullets, missileDMG;
    private float moveSpeed, turnSpeed, bulletCD, bulletSpeed;
    private float bulletCDTimer, bulletReload=3, bulletReloadTimer, missileCD=10, missileCDTimer;
    private float Cturn, meleeTimer, stunTimer, searchTimer, aimTimer;
    private bool stunned;
    private Vector2 TargetDir;
    private Rigidbody2D rb;
    private Transform fp;
    public GameObject Player; private bool pfound;
    [SerializeField] private int enemyType, state;
    private int frameTimer;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        fp = gameObject.transform.GetChild(0);
        state = 0; frameTimer = 1;
        bulletDMG=80; maxBullets=10; missileDMG=160;
        moveSpeed=6; turnSpeed=60; bulletCD=0.7f; bulletSpeed = 10;
        bulletReload=3; missileCD=10;
        health = 200; bulletsLeft = maxBullets;
        pfound=false;
        if(Random.value>0.3f) enemyType = 1; else enemyType = 2;
        StartCoroutine(FindPlayer());
        bulletCDTimer = 0; meleeTimer = 0; stunTimer = 0; bulletReloadTimer = 0; missileCDTimer = 30;
        searchTimer = 3; aimTimer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(pfound&&state==0&&searchTimer<0.001) PlayerSearch();
        frameTimer--;
        if(frameTimer==0){ frameTimer = 5;
            if(state!=0) CheckRaycast();
        }
        if(state==2||(state==1&&aimTimer>0.01)){
            if(state==2) {
                aimTimer=5;
                if(Vector3.Dot(fp.up, TargetDir)>0.9f){
                    FireBullet();
                }
            }
            if(frameTimer==1){
                if (MyMath.InterceptDirection(Player.transform.position, transform.position, Player.GetComponent<MikuMechControl>().Velocity, bulletSpeed, out Vector3 result)){
                    TargetDir = result;
                } else TargetDir = (Player.transform.position - transform.position).normalized;
            }
        }else if(state==1){
            //lookdir is movement dir
        }
        if(Vector3.Dot(fp.up, TargetDir)>0.9994f) Cturn=0;
        else{
        if (Vector3.Dot(fp.right, TargetDir)>0){
            Cturn = -turnSpeed;
        } else Cturn = turnSpeed;
        }

        bulletCDTimer=TimerF(bulletCDTimer); meleeTimer=TimerF(meleeTimer); stunTimer=TimerF(stunTimer);
        bulletReloadTimer=TimerF(bulletReloadTimer); missileCDTimer=TimerF(missileCDTimer); searchTimer=TimerF(searchTimer);
        aimTimer=TimerF(aimTimer);
    }

    void FixedUpdate()
    {
       fp.eulerAngles += Cturn * Time.fixedDeltaTime * Vector3.forward; 
    }

    private void FireBullet(){
        if(bulletCDTimer>0.001||bulletReloadTimer>0.001) return;
        bulletCDTimer = bulletCD;
        bulletsLeft--;
        if(bulletsLeft==0){bulletsLeft=maxBullets;bulletReloadTimer=bulletReload;}
        GameObject bullet = Instantiate (BulletPrefab, fp.position, fp.rotation*Quaternion.Euler(0, 0, 8*(Random.value-0.5f)));
        bullet.GetComponent<IBullet>().SetValues (bulletDMG, bulletSpeed, 1.8f+0.4f*Random.value, 0, Vector2.zero);
    }
    private void FireMissile(){

    }
    private void CheckRaycast(){
        if (Physics2D.Raycast((Vector2)transform.position, (Vector2)(Player.transform.position-transform.position), Vector3.Distance(Player.transform.position,transform.position), 1<<11)){
            state = 1;
        }
        else{
            state = 2;
        }
    }
    private void PlayerSearch(){
        searchTimer=2;
        if(Vector3.Distance(Player.transform.position,transform.position)<18) state = 1;
    }
    public void SetState(int s){
        state = s;
    }

    public void Damage (int dmg, bool stun){
        health-=dmg; if (health<1) Destruction();
    }
    public void MeleeDamage (int dmg, bool stun){
        if (meleeTimer>0.001) return;
        health-=dmg; if (health<1) Destruction();
        meleeTimer = 0.5f;
    }

    private void Destruction(){
        Destroy(gameObject);
    }

    private IEnumerator FindPlayer(){
        yield return new WaitForSeconds(2*Random.value);
        Player = GameObject.FindWithTag("Player");
        pfound=true;
    }

    private float TimerF( float val){
        if(val>=0){
            val-=Time.deltaTime;
            if (val<0) val = 0;
        }
        return val;
    }
}
