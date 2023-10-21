using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class DefaultNPC2AI : MonoBehaviour, IEnemy
{
    [Header("Prefabs")]

    public GameObject BulletPrefab, explosionPrefab, MissilePrefab;
    [Header("Enemy Values")]
    [SerializeField] private int health, bulletDMG, bulletsLeft, maxBullets, missileDMG;
    private float moveSpeed, mspeed, turnSpeed, nextWaypointDistance, bulletCD, bulletSpeed;
    Path path;
    Seeker seeker;
    private int currentWaypoint;
    private float bulletCDTimer, bulletReload=3, bulletReloadTimer, missileCD=10, missileCDTimer;
    private float Cturn, meleeTimer, stunTimer, searchTimer, aimTimer, bounceTimer;
    private bool stunned, bounce;
    private Vector2 TargetDir, MoveDir, bounceVector;
    private Rigidbody2D rb;
    private Transform fp;
    public GameObject Player; private bool pfound;
    [SerializeField] private int enemyType, state;
    private int frameTimer;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>(); MoveDir=Vector2.zero;
        fp = gameObject.transform.GetChild(0);
        state = 0; frameTimer = 1;
        bulletDMG=80; maxBullets=10; missileDMG=160;
        moveSpeed=8; mspeed=moveSpeed; turnSpeed=60;
        bulletCD=0.7f; bulletSpeed = 8.5f; bulletReload=3; missileCD=10;
        health = 240; bulletsLeft = maxBullets;
        pfound=false; stunned=false;
        if(Random.value>0.4f) enemyType = 1; else enemyType = 2;
        StartCoroutine(FindPlayer());
        bulletCDTimer = 0; meleeTimer = 0; stunTimer = 0; bulletReloadTimer = 0; missileCDTimer = 25;
        searchTimer = 3; aimTimer = 0; bounceTimer = 0; bounce = false; bounceVector = Vector2.zero;
        nextWaypointDistance = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if(pfound&&state==0&&searchTimer<0.001) PlayerSearch();
        frameTimer--;
        if(frameTimer==0){ frameTimer = 5;
            if(state!=0) CheckRaycast();
        }
        var s = Vector3.Dot(fp.up, TargetDir);
        if(state==2||(state==1&&aimTimer>0.01)){
            if(state==2) {
                aimTimer=5;
                if(!stunned && s>0.9f){
                    FireBullet();
                    if (s>0.97f) FireMissile();
                }
            }
            if(frameTimer==1){
                if (MyMath.InterceptDirection(Player.transform.position, transform.position, Player.GetComponent<MikuMechControl>().Velocity, bulletSpeed, out Vector3 result)){
                    TargetDir = result;
                } else TargetDir = (Player.transform.position - transform.position).normalized;
            }
        }else if(state==1){
            TargetDir = MoveDir;
        }
        if(state!=0){
            if(s>0.9994f) Cturn=0;
            else{
                if (Vector3.Dot(fp.right, TargetDir)>0){
                    Cturn = -turnSpeed;
                } else Cturn = turnSpeed;
            }
        }

        if (stunned && stunTimer>0.001f){
            moveSpeed = mspeed*0.5f;
        } if (stunned && stunTimer<0.01f) {moveSpeed = mspeed; stunned = false;}

        if(bounce&&bounceTimer<0.01f){bounceVector=Vector2.zero; bounce=false;}
        
        //pathfinding
        if(path!=null){
            if(currentWaypoint>=path.vectorPath.Count){
                MoveDir=Vector2.zero;
            }else{
                MoveDir = ((Vector2)path.vectorPath[currentWaypoint]-rb.position).normalized;
                if(Vector2.Distance(rb.position,path.vectorPath[currentWaypoint])<nextWaypointDistance){
                    currentWaypoint++;
                }
            }
        }


        bulletCDTimer=TimerF(bulletCDTimer); meleeTimer=TimerF(meleeTimer); stunTimer=TimerF(stunTimer);
        bulletReloadTimer=TimerF(bulletReloadTimer); missileCDTimer=TimerF(missileCDTimer); searchTimer=TimerF(searchTimer);
        aimTimer=TimerF(aimTimer); bounceTimer=TimerF(bounceTimer);
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + Time.fixedDeltaTime*(moveSpeed*MoveDir+4*bounceVector));
        fp.eulerAngles += Cturn * Time.fixedDeltaTime * Vector3.forward; 
    }

    private void FireBullet(){
        if(bulletCDTimer>0.001||bulletReloadTimer>0.001) return;
        bulletCDTimer = bulletCD;
        bulletsLeft--;
        if(bulletsLeft==0){bulletsLeft=maxBullets;bulletReloadTimer=bulletReload;}
        GameObject bullet = Instantiate (BulletPrefab, fp.position, fp.rotation*Quaternion.Euler(0, 0, 8*(Random.value-0.5f)));
        bullet.GetComponent<IBullet>().SetValues (bulletDMG, bulletSpeed+3*Vector3.Dot((Vector3)MoveDir, fp.up),
        1.8f, 0, Vector2.zero);
    }
    private void FireMissile(){
        if(enemyType==1||missileCDTimer>0.001) return;
        missileCDTimer = missileCD+10*(Random.value-0.5f);
        GameObject missile = Instantiate (MissilePrefab, fp.position, fp.rotation*Quaternion.Euler(0, 0, 8*(Random.value-0.5f)));
        missile.GetComponent<IMissile>().SetSpeed(5,24,24);
        missile.GetComponent<IMissile>().SetValues (missileDMG, 0.4f, 100, true, Player);
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
            InvokeRepeating("UpdatePath",0,2);
        }
    }
    private void UpdatePath(){
        if(seeker.IsDone())
            seeker.StartPath(rb.position, GetValidPoint(), OnPathComplete);
    }
    private void OnPathComplete(Path p){
        if(!p.error){
            path=p;
            currentWaypoint=0;
        }
    }
    public void SetState(int s){
        state = s;
    }
    public void SetType(int t){
        enemyType = t;
    }

    public void Damage (int dmg, bool stun){
        health-=dmg; if (health<1) Destruction();
        if (stun){stunTimer += 1; stunned = true;}
    }
    public void MeleeDamage (int dmg, bool stun){
        if (meleeTimer>0.001) return;
        health-=dmg; if (health<1) Destruction();
        meleeTimer = 0.5f;
        if (stun){stunTimer += 1; stunned = true;}
    }

    private Vector2 GetValidPoint(){
        int iter = 0;
        Vector2 point;
        do{
        point = (Vector2)Player.transform.position + Random.insideUnitCircle*12;
        iter++;
        }while(iter<11&&Physics2D.Raycast(point, (Vector2)(Player.transform.position-(Vector3)point), Vector3.Distance(Player.transform.position,(Vector3)point), 1<<11)
        &&Vector3.Distance(Player.transform.position,(Vector3)point)<3.5f);
        if(iter>10) return (Vector2)Player.transform.position;
        else return point;
    }

    void OnCollisionEnter2D(Collision2D c){
        if(c.gameObject.tag == "Player"){
            bounce = true; bounceTimer = 0.5f;
            bounceVector = (Vector2)(c.gameObject.transform.position-transform.position).normalized;
        }
    }

    private void Destruction(){
        if(explosionPrefab!=null){
            GameObject expl = Instantiate(explosionPrefab, transform.position, Quaternion.Euler(new Vector3(0, 180, 0)));
            Destroy(expl, 2);
        }
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
