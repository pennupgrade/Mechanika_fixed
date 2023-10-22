using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss2AI : MonoBehaviour, IEnemy
{
    [Header("Prefabs")]

    public GameObject BulletPrefab, explosionPrefab, MissilePrefab;
    [Header("Enemy Values")]
    [SerializeField] private int health, bulletDMG, bulletsLeft, maxBullets, missileDMG, shotgunDMG;
    private float moveSpeed, mspeed, turnSpeed, nextWaypointDistance, bulletCD, bulletSpeed;
    private float bulletCDTimer, bulletReload=3, bulletReloadTimer, missileCD=10, missileCDTimer;
    private float shotgunCDTimer, shotgunCD=5, spawnTime=10;
    private float Cturn, meleeTimer, searchTimer, aimTimer;
    private Vector2 TargetDir, MoveDir;
    private Rigidbody2D rb;
    private Transform fp;
    public GameObject Player; private bool pfound;
    [SerializeField] private int attackNum, state, mode;
    private int frameTimer;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        MoveDir=Vector2.zero;
        fp = gameObject.transform.GetChild(0);
        state = 0; frameTimer = 1;
        bulletDMG=80; missileDMG=240; shotgunDMG = 40;
        moveSpeed=6; mspeed=moveSpeed; turnSpeed=80;
        bulletCD=0.4f; bulletSpeed = 8; bulletReload=2; missileCD=12;
        health = 600;
        pfound=false;
        bulletCDTimer = 0; meleeTimer = 0; bulletReloadTimer = 0; missileCDTimer = 15;
    }

    // Update is called once per frame
    void Update()
    {
        frameTimer--;
        if(frameTimer==0){ frameTimer = 5;
            
        }
        var s = Vector3.Dot(fp.up, TargetDir);       
            if(s>0.9994f) Cturn=0;
            else{
                if (Vector3.Dot(fp.right, TargetDir)>0){
                    Cturn = -turnSpeed;
                } else Cturn = turnSpeed;
            }
        

        bulletCDTimer=TimerF(bulletCDTimer); meleeTimer=TimerF(meleeTimer);
        bulletReloadTimer=TimerF(bulletReloadTimer); missileCDTimer=TimerF(missileCDTimer);
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + Time.fixedDeltaTime*moveSpeed*MoveDir);
        fp.eulerAngles += Cturn * Time.fixedDeltaTime * Vector3.forward; 
    }

    private void FireBullet(){
        if(bulletCDTimer>0.001||bulletReloadTimer>0.001) return;
        bulletCDTimer = bulletCD;
        GameObject bullet = Instantiate (BulletPrefab, fp.position, fp.rotation*Quaternion.Euler(0, 0, 3*(Random.value-0.5f)));
        bullet.GetComponent<IBullet>().SetValues (bulletDMG, bulletSpeed, 3, -1, Vector2.zero);
    }
    private void FireMissile(){
        if(missileCDTimer>0.001) return;
        missileCDTimer = missileCD+8*(Random.value-0.5f);
        StartCoroutine(MissileCor());
    }
    private IEnumerator MissileCor(){
        for(int i = 0; i<2; i++){
            GameObject missile = Instantiate (MissilePrefab, fp.position, fp.rotation*Quaternion.Euler(0, 0, 40-80*i));
            missile.GetComponent<IMissile>().SetSpeed(5,5,10);
            missile.GetComponent<IMissile>().SetValues (missileDMG, 6, 110, true, Player);
            yield return new WaitForSeconds(.4f);
        }
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
        if(Vector3.Distance(Player.transform.position,transform.position)<18){
            state = 1;
        }
    }
    public void SetAttack(int a){
        attackNum = a;
    }

    public void Damage (int dmg, bool stun){
        health-=dmg; if (health<1) Destruction();
    }
    public void MeleeDamage (int dmg, bool stun){
        if (meleeTimer>0.001) return;
        health-=dmg; if (health<1) Destruction();
        meleeTimer = 0.5f;
    }

    private Vector2 GetValidPoint(){
        int iter = 0;
        Vector2 point;
        do{
        point = (Vector2)Player.transform.position + Random.insideUnitCircle*12;
        iter++;
        }while(iter<12&&Physics2D.Raycast(point, (Vector2)(Player.transform.position-(Vector3)point), Vector3.Distance(Player.transform.position,(Vector3)point), 1<<11)
        &&Vector3.Distance(Player.transform.position,(Vector3)point)<5f);
        if(iter>11) return (Vector2)Player.transform.position;
        else return point;
    }

    private void Destruction(){
        if(explosionPrefab!=null){
            GameObject expl = Instantiate(explosionPrefab, transform.position, Quaternion.Euler(new Vector3(0, 180, 0)));
            Destroy(expl, 2);
        }
        Destroy(gameObject);
        //end level
    }

    private void FindPlayer(){
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

