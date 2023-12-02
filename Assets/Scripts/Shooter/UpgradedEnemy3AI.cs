using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Unity.Mathematics;
using Random = UnityEngine.Random;

public class UpgradedEnemy3AI : MonoBehaviour, IEnemy
{
    public Healthbar Hbar;
    [Header("Prefabs")]
    public GameObject bulletPrefab, explosionPrefab, medkitPrefab; 
    public GameObject explodeRocketPrefab, hybridMissilePrefab, explodingBulletPrefab;
    public GameObject mageBulletPrefab, ringCenterPrefab;
    public GameObject[] trails;
    [Header("Enemy Values")]
    [SerializeField] private int health, maxHealth, bulletDMG;
    private float moveSpeed, mspeed, turnSpeed, nextWaypointDistance, minDistance, maxDistance, bulletCD, bulletSpeed;
    Path path;
    Seeker seeker;
    private int currentWaypoint;
    private float bulletCDTimer, specialCD, specialCDTimer, specialCD2, specialCD2Timer;
    private float Cturn, meleeTimer, stunTimer, aimTimer, bounceTimer, wayPointTimer, dashTimer, dashCDTimer;
    private float freezeTimer, uncloakTimer;
    private bool stunned, bounce, dashing, frozen, isDead, cloaked;
    private Vector2 TargetDir, MoveDir, bounceVector, dashVector, spawnPos;
    private Rigidbody2D rb;
    public Transform fp;
    private GameObject Player;
    [SerializeField] private int enemyType, state;
    private int frameTimer;

    // Start is called before the first frame update
    void Start()
    {
        spawnPos = (Vector2)transform.position;
        rb = GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>(); MoveDir=Vector2.zero;
        Player = GameObject.FindWithTag("Player");
        state = 1; frameTimer = 1;
        bulletDMG=80;
        bulletCD=1; bulletSpeed = 10;
        stunned=false; dashing = false; frozen = false; isDead = false; cloaked = false;
        dashTimer = 2; dashCDTimer = 0; freezeTimer = 0; uncloakTimer = 0;
        bulletCDTimer = 0; meleeTimer = 0; stunTimer = 0; 
        aimTimer = 0; wayPointTimer = 0;
        specialCDTimer = 5+4*Random.value; specialCD2Timer = 9+5*Random.value;
        bounceTimer = 0; bounce = false; bounceVector = Vector2.zero;
        nextWaypointDistance = 1;
    }
    public void SetState (int state){
        enemyType = state;
        GameObject trail;
        if (enemyType == 1) {
            maxHealth = 440;
            moveSpeed = 6.5f;
            turnSpeed = 90;
            specialCD = 17;
            specialCD2 = 15;
            minDistance = 5;
            maxDistance = 15;
        } else if (enemyType == 2) {
            maxHealth = 280;
            moveSpeed = 9.5f;
            turnSpeed = 100;
            specialCD = 12;
            minDistance = 2;
            maxDistance = 12;
        } else {
            maxHealth = 390;
            moveSpeed = 9;
            turnSpeed = 75;
            specialCD = 18;
            specialCD2 = 18;
            minDistance = 6;
            maxDistance = 18;
        }
        trail = Instantiate (trails[enemyType], transform.position, Quaternion.identity);
        trail.transform.SetParent(gameObject.transform);
        mspeed = moveSpeed;
        health = maxHealth;
        Hbar.SetHealth(health, maxHealth);
        StartCoroutine(StartFinding());
    }
    // Update is called once per frame
    void Update()
    {
        if(Player==null) return;
        frameTimer--;
        if(frameTimer==0){ frameTimer = 5;
            CheckRaycast();
        }
        var s = Vector3.Dot(fp.up, TargetDir);
        if(state==2||(state==1 && aimTimer>0.01)){
            FireSpecial2();
            if(state==2) {
                aimTimer=5;
                if(!stunned && s>0.85f){
                    FireBullet();
                    FireSpecial();
                }
            }
            if(frameTimer==1){
                if (!stunned) Dash();
                if (MyMath.InterceptDirection(Player.transform.position, transform.position, Player.GetComponent<MikuMechControl>().Velocity, bulletSpeed, out Vector3 result)){
                    TargetDir = result;
                } else TargetDir = (Player.transform.position - transform.position).normalized;
            }
        }else if(state==1){
            TargetDir = MoveDir;
        }

        if(s>0.9994f) Cturn=0;
        else{
            if (Vector3.Dot(fp.right, TargetDir)>0){
                Cturn = -turnSpeed;
            } else Cturn = turnSpeed;
        }

        if(bounce&&bounceTimer<0.01f){bounceVector=Vector2.zero; bounce = false;}
        if(dashing&&dashTimer<0.01f){dashVector=Vector2.zero; dashing = false;}
        if(frozen&&freezeTimer<0.01f){frozen = false;}
        if(enemyType==2 && !cloaked && uncloakTimer<0.1f && !dashing){
            Cloak();
        }

        //pathfinding
        if(path!=null){
            wayPointTimer += Time.deltaTime;
            if(currentWaypoint >= path.vectorPath.Count){
                UpdatePath();
                MoveDir=Vector2.zero;
            }else{
                if (wayPointTimer > 10){
                    UpdatePath();
                }
                MoveDir = ((Vector2)path.vectorPath[currentWaypoint]-rb.position).normalized;
                if (Vector2.Distance(rb.position,path.vectorPath[currentWaypoint])<nextWaypointDistance){
                    currentWaypoint++;
                }
            }
        }

        bulletCDTimer=TimerF(bulletCDTimer); meleeTimer=TimerF(meleeTimer); stunTimer=TimerF(stunTimer);
        specialCDTimer=TimerF(specialCDTimer); specialCD2Timer=TimerF(specialCD2Timer);
        dashTimer=TimerF(dashTimer); dashCDTimer=TimerF(dashCDTimer);
        aimTimer=TimerF(aimTimer); uncloakTimer=TimerF(uncloakTimer);
        bounceTimer=TimerF(bounceTimer); freezeTimer=TimerF(freezeTimer);
    }

    void FixedUpdate()
    {
        //stunned
        if (stunned){
            moveSpeed = mspeed*0.5f;
            if (stunTimer<0.001f) {moveSpeed = mspeed; stunned = false;}
        }
        if(!bounce && !dashing){
            if(!frozen)
                rb.MovePosition(rb.position + Time.fixedDeltaTime*moveSpeed*MoveDir);
        }else if (dashing){
            if (enemyType == 2 && dashTimer <0.4f){
                rb.MovePosition(rb.position + Time.fixedDeltaTime*20*dashVector);
                uncloakTimer = 0.8f;
            }else if (enemyType == 2){
                dashVector = (Vector2)(Player.transform.position-transform.position).normalized;
            } else {
                rb.MovePosition(rb.position + Time.fixedDeltaTime*18*dashVector);
            }
        } else {
            rb.MovePosition(rb.position - Time.fixedDeltaTime*moveSpeed*bounceVector);
        }
        fp.eulerAngles += Cturn * Time.fixedDeltaTime * Vector3.forward; 
    }

    private void FireBullet(){
        if(bulletCDTimer > 0.001) return;
        if(Vector3.Distance(Player.transform.position,transform.position) > 14) return;
        bulletCDTimer = bulletCD;
        GameObject bullet = Instantiate (bulletPrefab, fp.position, fp.rotation*Quaternion.Euler(0, 0, 3*(Random.value-0.5f)));
        bullet.GetComponent<IBullet>().SetValues (bulletDMG, bulletSpeed, 2, 0, Vector2.zero);
    }
    private void FireSpecial(){
        if(specialCDTimer>0.001) return;
        if (enemyType == 1){
            specialCDTimer = specialCD+1+4*(Random.value-0.5f);
            StartCoroutine(Bomber2());
        }
        if (enemyType == 2){
            bulletCDTimer = 2;
            float deg;
            int count;
            if (Random.value >0.5) {
                specialCDTimer = specialCD+1+4*(Random.value-0.5f)-4;
                count = 2;
                deg = 60;
            } else {
                specialCDTimer = specialCD+1+4*(Random.value-0.5f);
                count = 4;
                deg = 40;
            }
            for(int i = 0; i<count; i++){
            GameObject missile = Instantiate (hybridMissilePrefab, fp.position, fp.rotation*Quaternion.Euler(0, 0, -deg-deg*i));
            missile.GetComponent<IMissile>().SetSpeed(5,50,26);
            missile.GetComponent<IMissile>().SetValues (180, 0.85f, 90, true, Player);
            missile.GetComponent<HybridMissile>().SetVector (fp.up);
            GameObject missile2 = Instantiate (hybridMissilePrefab, fp.position, fp.rotation*Quaternion.Euler(0, 0, deg+deg*i));
            missile2.GetComponent<IMissile>().SetSpeed(5,50,26);
            missile2.GetComponent<IMissile>().SetValues (180, 0.85f, 90, true, Player);
            missile2.GetComponent<HybridMissile>().SetVector (fp.up);
            }
        }
        if (enemyType == 3){
            if (Random.value>0.6){
                specialCDTimer = specialCD-4+4*(Random.value-0.5f);
                GameObject missile = Instantiate (mageBulletPrefab, fp.position, fp.rotation*Quaternion.Euler(0, 0, 8*(Random.value-0.5f)));
                missile.GetComponent<IMissile>().SetSpeed(6,2,10);
                missile.GetComponent<IMissile>().SetValues (90, 5, 240, true, Player);
            } else {
                specialCDTimer = specialCD+4*(Random.value-0.5f);
                StartCoroutine(Mage2());
            }
        } 
    }
    private IEnumerator Mage2(){
        for(int i = 0; i<3; i++){
            GameObject missile = Instantiate (mageBulletPrefab, fp.position, fp.rotation*Quaternion.Euler(0, 0, 8*(Random.value-0.5f)));
            missile.GetComponent<IMissile>().SetSpeed(6,3,15);
            missile.GetComponent<IMissile>().SetValues (150, 10, 20, true, Player);
            yield return new WaitForSeconds(0.6f);
        }
    }
    private IEnumerator Bomber2(){
        for (int i = 0; i<3; i++){ 
                GameObject missile = Instantiate (explodeRocketPrefab, fp.position, fp.rotation*Quaternion.Euler(0, 0, -30-30*(3-i)));
                missile.GetComponent<IMissile>().SetSpeed(6,3,9);
                missile.GetComponent<IMissile>().SetValues (60, 5, 140, true, Player);
                missile.GetComponent<ExplosiveMissile>().SetElectric();
                Vector3 t = Player.transform.position + 4*(Player.transform.position-transform.position).normalized + 6*(Vector3)Random.insideUnitCircle;
                missile.GetComponent<ExplosiveMissile>().SetTargetAndHomingAccel(t, 60);
                GameObject missile2 = Instantiate (explodeRocketPrefab, fp.position, fp.rotation*Quaternion.Euler(0, 0, 30+30*(3-i)));
                missile2.GetComponent<IMissile>().SetSpeed(6,3,9);
                missile2.GetComponent<IMissile>().SetValues (60, 5, 140, true, Player);
                missile2.GetComponent<ExplosiveMissile>().SetElectric();
                t = Player.transform.position + 4*(Player.transform.position-transform.position).normalized + 6*(Vector3)Random.insideUnitCircle;
                missile2.GetComponent<ExplosiveMissile>().SetTargetAndHomingAccel(t, 60);
                yield return new WaitForSeconds(0.4f);
            }
    }
    private void FireSpecial2(){
        if((enemyType != 3 && enemyType != 1) || specialCD2Timer>0.001) return;
        if(enemyType == 1){
            specialCD2Timer = specialCD2+6*(Random.value-0.5f);
            GameObject missile = Instantiate (explodingBulletPrefab, fp.position, fp.rotation*Quaternion.Euler(0, 0, 4*(Random.value-0.5f)));
            missile.GetComponent<IMissile>().SetSpeed(6,3,9);
            missile.GetComponent<IMissile>().SetValues (70, 5, 140, true, Player);
            Vector3 t = Player.transform.position + 3*(Player.transform.position-transform.position).normalized + 5*(Vector3)Random.insideUnitCircle;
            missile.GetComponent<ExploderBullet>().SetTargetAndHomingAccel(t);
        } else{
            specialCD2Timer = specialCD2+4*(Random.value-0.5f);
            frozen = true; freezeTimer = 0.5f;
            GameObject center = Instantiate (ringCenterPrefab, transform.position, Quaternion.identity);
            center.GetComponent<BulletCenter>().initFields(8, Player);
        }
    }
    private void Dash (){
        if(enemyType != 2 && enemyType != 3) return;
        if(dashCDTimer > 0.001f) return;
        if(enemyType == 2 && Vector3.Distance(Player.transform.position, transform.position) > 5) return;
        if(enemyType == 3 && Vector3.Distance(Player.transform.position, transform.position) > 8) return;
        dashing = true;
        if (enemyType == 2) {
            Decloak();
            dashTimer = 1.2f;
            dashCDTimer = 7;
        } else {
            dashTimer = 0.2f;
            dashCDTimer = 3.6f+Random.value;
            Vector3 v;
            if(Random.value>0.5f) v = fp.forward; else v = -fp.forward;
            dashVector = (Vector2)Vector3.Cross((Vector3)(Player.transform.position-(Vector3)rb.position).normalized, v);
        }
    }
    private void CheckRaycast(){
        if (Physics2D.Raycast((Vector2)transform.position, (Vector2)(Player.transform.position-transform.position), Vector3.Distance(Player.transform.position,transform.position), 1<<11)
        || Vector3.Distance(Player.transform.position,transform.position) > 20){
            state = 1;
            if(currentWaypoint>6) UpdatePath();
        }
        else{
            state = 2;
        }
    }

    void OnCollisionEnter2D(Collision2D c){
        if(c.gameObject.tag == "Player"){
            if (dashing && enemyType == 2) {
                Destruction();
                return;
            }
            bounce = true; bounceTimer = 0.5f;
            bounceVector = (Vector2)(c.gameObject.transform.position-transform.position).normalized;
        } else if (c.gameObject.tag == "Enemy")
        {
            bounce = true; bounceTimer = 0.4f+0.4f*Random.value;
            bounceVector = (Vector2)(c.gameObject.transform.position-transform.position).normalized;
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
            wayPointTimer=0;
        }
    }
    private IEnumerator StartFinding(){
        yield return new WaitForSeconds(0.5f);
        UpdatePath();
    }

    public void Damage (int dmg, bool stun){
        if(enemyType==2||enemyType==1) dmg -= 10;
        health-=dmg; if (health<1) Destruction();
        if (stun){stunTimer += 1; stunned = true;}
        if(enemyType==2){
            uncloakTimer = 1;
            Decloak();
        }
        Hbar.SetHealth(health, maxHealth);
    }
    public void MeleeDamage (int dmg, bool stun){
        dmg = dmg/2;
        if (meleeTimer>0.001) return;
        health-=dmg/2; if (health<1) Destruction();
        meleeTimer = 0.5f;
        if (stun){stunTimer += 1; stunned = true;}
        if(enemyType==2){
            uncloakTimer = 1;
            Decloak();
        }
        Hbar.SetHealth(health, maxHealth);
    }

    private void Cloak(){
        cloaked = true;
        var temp = GetComponent<SpriteRenderer>().color;
        temp.a = 0;
        GetComponent<SpriteRenderer>().color = temp;
        transform.GetChild(2).gameObject.GetComponent<TrailRenderer>().emitting = false;
        transform.GetChild(1).gameObject.SetActive(false);
    }
    private void Decloak(){
        cloaked = false;
        var temp = GetComponent<SpriteRenderer>().color;
        temp.a = 1;
        GetComponent<SpriteRenderer>().color = temp;
        transform.GetChild(2).gameObject.GetComponent<TrailRenderer>().emitting = true;
        transform.GetChild(1).gameObject.SetActive(true);
    }
    private Vector2 GetValidPoint(){
        if(Player==null) return (Vector2)transform.position;
        int iter = 0;
        Vector2 point;
        do{
        point = (Vector2)Player.transform.position + Random.insideUnitCircle*maxDistance;
        iter++;
        }while(iter<17
        &&(Physics2D.Raycast(point, (Vector2)(Player.transform.position-(Vector3)point), Vector3.Distance(Player.transform.position,(Vector3)point), 1<<11)
        ||Vector3.Distance(Player.transform.position,(Vector3)point) < minDistance
        || Vector3.Dot((Player.transform.position-transform.position).normalized, ((Vector3)point-transform.position).normalized) > 0.8f));
        if(iter>16) {
            wayPointTimer = 5;
            if (Vector3.Distance(Player.transform.position, transform.position)>10){
                return (Vector2)Player.transform.position;
            }
            else {
                return spawnPos;
            }
        }
        else {return point;}
    }

    private void Destruction(){
        if(isDead) return;
        if(explosionPrefab!=null){
            GameObject expl = Instantiate(explosionPrefab, transform.position, Quaternion.Euler(new Vector3(0, 180, 0)));
            Destroy(expl, 2);
        }
        if (Vector3.Distance(Player.transform.position,transform.position)<2f) {
            if (enemyType==2) {
                Player.GetComponent<MikuMechControl>().MeleeDamage(410, false);
            } else {
                Player.GetComponent<MikuMechControl>().MeleeDamage(200, false);
            }
        }
        transform.parent.gameObject.GetComponent<GM3Script>().ReportDeath();
        isDead = true;
        Destroy(gameObject);
        
        if(Random.value>0.75) Instantiate (medkitPrefab, rb.position, Quaternion.identity);
    }

    private float TimerF( float val){
        if(val>=0){
            val-=Time.deltaTime;
            if (val<0) val = 0;
        }
        return val;
    }
}

