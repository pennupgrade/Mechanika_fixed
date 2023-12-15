using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Unity.Mathematics;
using Random = UnityEngine.Random;

using Utilities;

public class UpgradedNPC2AI : MonoBehaviour, IEnemy
{
    public Healthbar Hbar;
    [Header("Prefabs")]
    public GameObject BulletPrefab, explosionPrefab, 
    SpawnExplosionPrefab, MissilePrefab, ShotgunPrefab, DefaultNPCPrefab, MedkitPrefab;
    [Header("Enemy Values")]
    [SerializeField] private int health, maxHealth, bulletDMG, bulletsLeft, maxBullets, missileDMG, shotgunDMG;
    private float moveSpeed, mspeed, turnSpeed, nextWaypointDistance, bulletCD, bulletSpeed;
    Path path;
    Seeker seeker;
    private int currentWaypoint;
    private float bulletCDTimer, bulletReload=3, bulletReloadTimer, missileCD=10, missileCDTimer;
    private float shotgunCDTimer, shotgunCD=5, spawnTime=10;
    private float Cturn, meleeTimer, stunTimer, searchTimer, aimTimer, spawnTimer, bounceTimer, wayPointTimer;
    private bool stunned, bounce;
    private Vector2 TargetDir, MoveDir, bounceVector;
    private Rigidbody2D rb;
    private Transform fp;
    public GameObject Player; private bool pfound;
    [SerializeField] private int enemyType, state, spawnCounter;
    private int frameTimer;

    [Header("Misc")]
    [SerializeField] Animator Animator;

    [Header("Colliders")]
    [SerializeField] CircleCollider2D BigCollider;
    [SerializeField] BoxCollider2D SideSmallCollider;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>(); MoveDir=Vector2.zero;
        fp = gameObject.transform.GetChild(0);
        state = 0; frameTimer = 1;
        bulletDMG=80; maxBullets=25; missileDMG=180; shotgunDMG = 40;
        moveSpeed=6; turnSpeed=85;
        bulletCD=0.4f; bulletSpeed = 9; bulletReload=2; missileCD=12;
        maxHealth = 640; health=maxHealth; bulletsLeft = maxBullets;
        pfound=false; stunned=false;
        if(Random.value>0.4f) enemyType = 1; else {enemyType = 2; moveSpeed=8;}
        mspeed=moveSpeed;
        StartCoroutine(FindPlayer());
        bulletCDTimer = 0; meleeTimer = 0; stunTimer = 0; bulletReloadTimer = 0; missileCDTimer = 15;
        searchTimer = 3; aimTimer = 0; spawnTimer=0; shotgunCDTimer = 0; wayPointTimer = 0;
        bounceTimer = 0; bounce = false; bounceVector = Vector2.zero;
        nextWaypointDistance = 1;
        Hbar.SetHealth(health, maxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        if(pfound && Player==null) return;
        if(pfound && state==0 && searchTimer<0.001) PlayerSearch();
        frameTimer--;
        if(frameTimer==0){ frameTimer = 5;
            if(state!=0) {CheckRaycast(); if(enemyType==2) SpawnNPC();}
        }
        var s = Vector3.Dot(fp.up, TargetDir);
        if(state==2||(state==1 && aimTimer>0.01)){
            if(state==2) {
                aimTimer=5;
                if(!stunned && s>0.8f){
                    FireMissile(); if(!FireShotgun()) FireBullet();
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
            wayPointTimer += Time.deltaTime;
            if(s>0.9994f) Cturn=0;
            else{
                if (Vector3.Dot(fp.right, TargetDir)>0){
                    Cturn = -turnSpeed;
                } else Cturn = turnSpeed;
            }
        }

        if(bounce&&bounceTimer<0.01f){bounceVector=Vector2.zero; bounce = false;}
        
        //pathfinding
        if(path!=null){
            if(currentWaypoint>=path.vectorPath.Count){
                MoveDir=Vector2.zero;
                UpdatePath();
            }else{
                if (wayPointTimer > 8){
                    UpdatePath();
                    MoveDir=Vector2.zero;
                }
                MoveDir = ((Vector2)path.vectorPath[currentWaypoint]-rb.position).normalized;
                if (Vector2.Distance(rb.position,path.vectorPath[currentWaypoint])<nextWaypointDistance){
                    currentWaypoint++;
                }
            }
        }

        bulletCDTimer=TimerF(bulletCDTimer); meleeTimer=TimerF(meleeTimer); stunTimer=TimerF(stunTimer);
        bulletReloadTimer=TimerF(bulletReloadTimer); missileCDTimer=TimerF(missileCDTimer); searchTimer=TimerF(searchTimer);
        aimTimer=TimerF(aimTimer); spawnTimer=TimerF(spawnTimer); shotgunCDTimer=TimerF(shotgunCDTimer);
        bounceTimer=TimerF(bounceTimer);
    }

    void FixedUpdate()
    {
        //stunned
        if (stunned){
            moveSpeed = mspeed*0.5f;
            if (stunTimer<0.001f) {moveSpeed = mspeed; stunned = false;}
        }
        if(!bounce){
            rb.MovePosition(rb.position + Time.fixedDeltaTime*moveSpeed*MoveDir);
        }else {
            rb.MovePosition(rb.position - Time.fixedDeltaTime*moveSpeed*bounceVector);
        }
        fp.eulerAngles += Cturn * Time.fixedDeltaTime * Vector3.forward; 

        //
        int2 ori = EnemyUtils.AngleDegreesToFourOrientation(fp.eulerAngles.z);

        Animator.SetInteger("Horizontal", ori.x);
        Animator.SetInteger("Vertical", ori.y);

        //
        bool useBigCollider = ori.x == 0;

        BigCollider.enabled = useBigCollider;
        SideSmallCollider.enabled = !useBigCollider;

        //Animator.SetBool("IsMoving", (MoveDir.magnitude >= 0.0001f));
    }

    private void FireBullet(){
        if(bulletCDTimer>0.001||bulletReloadTimer>0.001) return;
        bulletCDTimer = bulletCD;
        bulletsLeft--;
        if(bulletsLeft==0){bulletsLeft=maxBullets;bulletReloadTimer=bulletReload;}
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
            missile.GetComponent<IMissile>().SetSpeed(6,6,16);
            missile.GetComponent<IMissile>().SetValues (missileDMG, 6, 130, false, Player);
            yield return new WaitForSeconds(.3f);
        }
    }
    private bool FireShotgun(){
        if(shotgunCDTimer>0.01||Vector3.Distance(Player.transform.position,(Vector3)rb.position)>6) return false;
        shotgunCDTimer=shotgunCD; bulletCDTimer=0.6f;
        for (int i = 0; i<9;i++){
            GameObject bullet = Instantiate (ShotgunPrefab, fp.position, fp.rotation*Quaternion.Euler(0, 0, 40*(Random.value-0.5f)));
            bullet.GetComponent<IBullet>().SetValues (shotgunDMG, 10+3*Random.value, 0.8f+0.2f*Random.value, 5, Vector2.zero);
        }
        return true;
    }
    private void SpawnNPC(){
        if(spawnCounter>2||spawnTimer>0.01) return;
        spawnTimer=spawnTime; spawnCounter++;
        GameObject ptcl = Instantiate (SpawnExplosionPrefab, rb.position-2*MoveDir, Quaternion.Euler(new Vector3(0, 180, 0)));
        Destroy(ptcl,5);
        GameObject npc = Instantiate (DefaultNPCPrefab, rb.position-2*MoveDir, Quaternion.identity);
        npc.GetComponent<DefaultNPC2AI>().SetType(2);
    }
    private void CheckRaycast(){
        if (Physics2D.Raycast((Vector2)transform.position, (Vector2)(Player.transform.position-transform.position), Vector3.Distance(Player.transform.position,transform.position), 1<<11)){
            state = 1;
            if(currentWaypoint>6) UpdatePath();
        }
        else{
            state = 2;
        }
    }

    void OnCollisionEnter2D(Collision2D c){
        if(c.gameObject.tag == "Player"){
            bounce = true; bounceTimer = 0.5f;
            bounceVector = (Vector2)(c.gameObject.transform.position-transform.position).normalized;
        } else if (c.gameObject.tag == "Enemy")
        {
            bounce = true; bounceTimer = 0.4f+0.3f*Random.value;
            bounceVector = (Vector2)(c.gameObject.transform.position-transform.position).normalized;
        }
    }
    private void PlayerSearch(){
        searchTimer=2;
        if(Vector3.Distance(Player.transform.position,transform.position)<18){
            state = 1;
            UpdatePath();
            if(enemyType==2) {spawnCounter=0; spawnTimer=16;}
            Collider2D c = Physics2D.OverlapCircle(transform.position, 10, 1<<9);
            if(c!=null && c.gameObject.TryGetComponent<IEnemy>(out IEnemy enemy)){
                enemy.SetState(1);
            }
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
    public void SetState(int s){
        state = s;
        UpdatePath();
        if(enemyType==2) {spawnCounter=0; spawnTimer=16;}
        Collider2D c = Physics2D.OverlapCircle(transform.position, 10, 1<<9);
        if(c!=null && c.gameObject!=this.gameObject && c.gameObject.TryGetComponent<IEnemy>(out IEnemy enemy)){
            enemy.SetState(1);
        }
    }

    public void Damage (int dmg, bool stun){
        if(state==0) SetState(1);
        if(enemyType==2) dmg -= 10;
        if (dmg > 400) SFXPlayer.PlaySound("HIT_SELF1");
        else SFXPlayer.PlaySound("HIT_BIG1");
        health-=dmg; if (health<1) Destruction();
        if (stun){stunTimer += 1; stunned = true;}
        Hbar.SetHealth(health, maxHealth);
    }
    public void MeleeDamage (int dmg, bool stun){
        if (meleeTimer>0.001) return;
        SFXPlayer.PlaySound("HIT_SELF1");
        health-=dmg/3; if (health<1) Destruction();
        meleeTimer = 0.5f;
        if (stun){stunTimer += 1; stunned = true;}
        Hbar.SetHealth(health, maxHealth);
    }

    private Vector2 GetValidPoint(){
        if(Player==null) return (Vector2)transform.position;
        int iter = 0;
        Vector2 point;
        do{
        point = (Vector2)Player.transform.position + Random.insideUnitCircle*12;
        iter++;
        //bad
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
        SFXPlayer.PlaySound("DIE");
        Destroy(gameObject);
        if(Random.value>0.5) Instantiate (MedkitPrefab, rb.position-2*MoveDir, Quaternion.identity);
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
