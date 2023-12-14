using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Animations;

using Unity.Mathematics;
using Random = UnityEngine.Random;

using Utilities;

public class Boss3AI : MonoBehaviour, IEnemy
{
    public GameObject cam;
    public Slider Bar;
    public TextMeshProUGUI BossName;
    [Header("Prefabs")]

    public GameObject explosionPrefab, RocketPrefab, MissilePrefab,
        MagicBullet, HybridMissile, ExplodingBullet, BulletPrefab;
    [Header("Enemy Values")]
    public int health;
    [SerializeField] private int maxHealth;
    [SerializeField] private float moveSpeed, turnSpeed, mspeed, tspeed;
    private int bulletDMG, rocketDMG, missileDMG, electricDMG, magicDMG;
    private float trackingBspd;
    [SerializeField]private float Cturn, meleeTimer, dashTimer;
    private Vector2 Waypoint, TargetDir, MoveDir, DashDir;
    private Rigidbody2D rb;
    private Transform fp;
    private bool dashing;
    public GameObject Player;
    [SerializeField] private int moveState;
    private int frameTimer;

    [Header("Misc")]
    [SerializeField] Animator Animator;
    [SerializeField] SpriteRenderer sr;

    // Start is called before the first frame update
    void Awake(){
        rb = GetComponent<Rigidbody2D>();
        fp = transform.GetChild(0).gameObject.transform;
        Player = GameObject.FindWithTag("Player");
        dashing = false;
    }
    void Start()
    {
        MoveDir=Vector2.zero;
        maxHealth = 12666; health=maxHealth;
        moveState = 0;
        frameTimer = 1;
        trackingBspd = 16;
        bulletDMG = 72; rocketDMG = 180; missileDMG = 140;
        electricDMG = 60; magicDMG = 100;
        Cturn = 0;
        moveSpeed=6; mspeed = moveSpeed; turnSpeed=100; tspeed = turnSpeed;
        meleeTimer = 0; dashTimer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(Player==null) return;
        frameTimer--;
        if(frameTimer==0){ frameTimer = 4;
            if (MyMath.InterceptDirection(Player.transform.position, transform.position, Player.GetComponent<MikuMechControl>().Velocity, trackingBspd, out Vector3 result)){
                TargetDir = result;
            } else TargetDir = (Player.transform.position - transform.position).normalized;
            SetWaypoint();
        }
        var s = Vector3.Dot(fp.up, TargetDir);
        if (s>0.9994f){
            Cturn=0;
        } else {
            if (Vector3.Dot(fp.right, TargetDir)>0){
                Cturn = -tspeed;
            } else Cturn = tspeed;
        }
        if(dashing && dashTimer<0.001f){
            dashing = false;
            if (moveState==0) mspeed = moveSpeed/2;
            else mspeed = moveSpeed;
        }

        meleeTimer=TimerF(meleeTimer); dashTimer=TimerF(dashTimer);
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + Time.fixedDeltaTime*mspeed*MoveDir);
        fp.eulerAngles += Cturn * Time.fixedDeltaTime * Vector3.forward; 

        //
        //int2 ori = EnemyUtils.AngleDegreesToFourOrientation(fp.eulerAngles.z);

        //Animator.SetInteger("Horizontal", ori.x);
        //Animator.SetInteger("Vertical", ori.y);

    }
    private void Dash(){
        dashing = true; dashTimer= 0.668235f;
        mspeed = 5*moveSpeed;
        int iter = 0;
        if (Vector3.Distance(new Vector3(0,36,0), transform.position)>10){
            do{
                iter++;
                MoveDir = (Vector2)(Quaternion.AngleAxis(90*(Random.value-0.5f),Vector3.forward)
                *(Vector3)((new Vector3(0,36,0))-(Vector3)rb.position).normalized);
            }while(iter<10 && Vector2.Dot((Vector2)fp.up,MoveDir)>0.85f);
        } else if(Player!=null){
            Vector3 v;
            if(Random.value>0.5f) v = fp.forward; else v = -fp.forward;
            MoveDir = Vector3.Cross((Vector3)(Player.transform.position-(Vector3)rb.position).normalized, v);
        }
    }
       
    public void SetMode(int m){ // 0 or 1
        moveState = m;
        if (moveState == -10){
            moveState = 0 ; health = maxHealth;
            Waypoint = new Vector2(0,36);
            StartCoroutine(BarAnimation());
        }

        if(moveState == 1) {mspeed = moveSpeed;}
        else {mspeed = moveSpeed/2;}
    }
    public void SetState(int s){}
    public void SetAttack(int a){
        if (a == 0){
            SingleRocket();
        } else if (a == 1) {
            StartCoroutine(RocketBarrage());
        } else if (a == 2) {
            StartCoroutine(MagicBullets());
        } else if (a == 3) {
            FireHybridMissiles(true);
        } else if (a == 33) {
            FireHybridMissiles(false);
        } else if (a == 4) {
            FireExplodeBullet();
        } else if (a == 5) {
            StartCoroutine(ElectricBarrage());
        } else if (a == 6) {
            StartCoroutine(MissileBarrage(1));
        } else if (a == 66) {
            StartCoroutine(MissileBarrage(2));
        } else if (a == 7) {
            Dash();
            StartCoroutine(DashRockets());
            ExecuteRing();
        }
    }
    public void SetBulletEngine(int a){
        switch(a) 
        {
            case 0:
            ExecuteCQRing(); break;
            case 1:
            ExecuteRing(); break;
            case 2:
            ExecuteFireRing(); break;
            case 3:
            ExecuteBouncingTrail(); break;
            case 4:
            ExecuteBouncingCircle(); break;
        }
    }    

    // Bullet Engine Patterns
    [Header("Patterns")]
    [SerializeField] APattern CQRing;

    // Bullet Engine Attacks
    void ExecuteCQRing() 
    { //not bullet engine sry
        int count = 18;
        for (int i = 0; i<count; i++){
            GameObject bullet = Instantiate (BulletPrefab, transform.position, transform.rotation*Quaternion.Euler(0, 0, i*360/count));
            bullet.GetComponent<IBullet>().SetValues (bulletDMG, 9, 0.65f+0.2f*Random.value, 9, Vector2.zero);
        }
    }
    void ExecuteRing(int count = 1, float delaySeconds = 2) {
        if(count <= 0) return;
        CQRing.Execute(BulletEngineManager.bossEngine, BulletEngineManager.Ins.Boss, BulletEngineManager.UsedPlayerTransform, 
            () => StartCoroutine(Utils.WaitThenAction(4, () => ExecuteRing(count-1))));
    }
    void ExecuteFireRing() {}
    void ExecuteBouncingTrail() {}
    void ExecuteBouncingCircle() {}
    
    // attacks
    private IEnumerator DashRockets(){
        if(Player==null) {StopAllCoroutines(); yield break;}
        for(int i = 0; i<10; i++){
            if (i != 4) {
                GameObject missile = Instantiate (RocketPrefab, transform.position, fp.rotation);
                missile.GetComponent<IMissile>().SetSpeed(2,80,40);
                missile.GetComponent<IMissile>().SetValues (rocketDMG, 0.70588f + 0.17647f*(i+1) - 0.058823f*i, 36, true, Player);
            }
            yield return new WaitForSeconds(0.058823f);
        }
    }
    private void SingleRocket(){
        if(Player==null) return;
        GameObject missile = Instantiate (RocketPrefab, transform.position, fp.rotation);
        missile.GetComponent<IMissile>().SetSpeed(4,36,26);
        missile.GetComponent<IMissile>().SetValues (rocketDMG, 0.70588f, 110, true, Player);
    }
    private IEnumerator RocketBarrage(){
        if(Player==null) {StopAllCoroutines(); yield break;}
        for(int i = 0; i<4; i++){
            GameObject missile = Instantiate (RocketPrefab, fp.position+fp.right, fp.rotation*Quaternion.Euler(0, 0, -130+35*i));
            missile.GetComponent<IMissile>().SetSpeed(8,40,28);
            missile.GetComponent<IMissile>().SetValues (rocketDMG, 0.35294f * (2 - i) + 0.70588f, 92, true, Player);
            GameObject missile2 = Instantiate (RocketPrefab, fp.position-fp.right, fp.rotation*Quaternion.Euler(0, 0, 130-35*i));
            missile2.GetComponent<IMissile>().SetSpeed(8,40,28);
            missile2.GetComponent<IMissile>().SetValues (rocketDMG, 0.35294f * (2 - i) + 0.70588f, 92, true, Player);
            yield return new WaitForSeconds(0.35294f);
        }
    }
    private IEnumerator MagicBullets(){
        for (int i = 0; i<4; i++){
            FireMagicBullet( transform.rotation*Quaternion.Euler(0, 0, 120-20*(3-i)));
            FireMagicBullet( transform.rotation*Quaternion.Euler(0, 0, -120+20*(3-i)));
            yield return new WaitForSeconds(0.70588f);
        }

    }
    private void FireMagicBullet(Quaternion q){
        if(Player==null) return;
        GameObject missile = Instantiate (MagicBullet, fp.position, q);
        missile.GetComponent<IMissile>().SetSpeed(6,3,12);
        missile.GetComponent<IMissile>().SetValues (magicDMG, 4, 1, true, Player);
    }
    private void FireHybridMissiles(bool straight, int? c = null){
        if(Player==null) return;
        int count = (c == null) ? 4 : (int) c;
        for(int i = 1; i<count + 1; i++){
            GameObject missile = Instantiate (HybridMissile, fp.position, fp.rotation*Quaternion.Euler(0, 0, -12-42*i));
            missile.GetComponent<IMissile>().SetSpeed(5,50,26);
            missile.GetComponent<IMissile>().SetValues (rocketDMG, 0.70588f * i, 70, true, Player);
            GameObject missile2 = Instantiate (HybridMissile, fp.position, fp.rotation*Quaternion.Euler(0, 0, 12+42*i));
            missile2.GetComponent<IMissile>().SetSpeed(5,50,26);
            missile2.GetComponent<IMissile>().SetValues (rocketDMG, 0.70588f * i, 70, true, Player);
            if (straight) {
                missile.GetComponent<HybridMissile>().SetVector (fp.up);
                missile2.GetComponent<HybridMissile>().SetVector (fp.up);
            } else {
                float rnd = 60*Random.value;
                missile.GetComponent<HybridMissile>().SetVector ((Quaternion.AngleAxis(rnd,Vector3.forward)*fp.up));
                missile2.GetComponent<HybridMissile>().SetVector ((Quaternion.AngleAxis(-rnd,Vector3.forward)*fp.up));
            }
        }
    }
    private void FireExplodeBullet(){
        if(Player==null) return;
        GameObject missile = Instantiate (ExplodingBullet, fp.position, fp.rotation*Quaternion.Euler(0, 0, 4*(Random.value-0.5f)));
        missile.GetComponent<IMissile>().SetSpeed(8,2,10);
        missile.GetComponent<IMissile>().SetValues (bulletDMG, 5, 100, true, Player);
        Vector3 t = Player.transform.position + 2*(Player.transform.position-transform.position).normalized;
        missile.GetComponent<ExploderBullet>().SetTargetAndHomingAccel(t);
    }
    private IEnumerator ElectricBarrage(){
        if(Player==null) {StopAllCoroutines(); yield break;}
        for (int i = 0; i<5; i++){ 
            GameObject missile = Instantiate (MissilePrefab, fp.position, transform.rotation*Quaternion.Euler(0, 0, -40-24*(3-i)));
            missile.GetComponent<IMissile>().SetSpeed(6,3,9);
            missile.GetComponent<IMissile>().SetValues (electricDMG, 5, 60, true, Player);
            missile.GetComponent<ExplosiveMissile>().SetElectric();
            Vector3 t = Player.transform.position + 2*(Player.transform.position-transform.position).normalized + 9*(Vector3)Random.insideUnitCircle;
            missile.GetComponent<ExplosiveMissile>().SetTargetAndHomingAccel(t, 60);
            GameObject missile2 = Instantiate (MissilePrefab, fp.position, transform.rotation*Quaternion.Euler(0, 0, 40+24*(3-i)));
            missile2.GetComponent<IMissile>().SetSpeed(6,3,9);
            missile2.GetComponent<IMissile>().SetValues (electricDMG, 5, 60, true, Player);
            missile2.GetComponent<ExplosiveMissile>().SetElectric();
            t = Player.transform.position + 2*(Player.transform.position-transform.position).normalized + 9*(Vector3)Random.insideUnitCircle;
            missile2.GetComponent<ExplosiveMissile>().SetTargetAndHomingAccel(t, 60);
            yield return new WaitForSeconds(0.35294f);
        }
    }
    private IEnumerator MissileBarrage(int num){
        if(Player==null) {StopAllCoroutines(); yield break;}
        if (num == 1){
            for (int i = 0; i<5; i++){ 
                GameObject missile = Instantiate (MissilePrefab, fp.position, transform.rotation*Quaternion.Euler(0, 0, -30-25*(5-i)));
                missile.GetComponent<IMissile>().SetSpeed(8, 2.2f, 12);
                missile.GetComponent<IMissile>().SetValues (missileDMG, 4, 85, true, Player);
                missile.GetComponent<ExplosiveMissile>().SetHoming();
                missile.GetComponent<ExplosiveMissile>().SetTargetAndHomingAccel(transform.position, 8);
                GameObject missile2 = Instantiate (MissilePrefab, fp.position, transform.rotation*Quaternion.Euler(0, 0, 30+25*(5-i)));
                missile2.GetComponent<IMissile>().SetSpeed(8, 2.2f, 12);
                missile2.GetComponent<IMissile>().SetValues (missileDMG, 4, 85, true, Player);
                missile2.GetComponent<ExplosiveMissile>().SetHoming();
                missile2.GetComponent<ExplosiveMissile>().SetTargetAndHomingAccel(transform.position, 8);
                yield return new WaitForSeconds(0.117647f);
            }
        } else if (num == 2){
            for (int i = 0; i<5; i++){ 
                GameObject missile = Instantiate (MissilePrefab, fp.position, fp.rotation*Quaternion.Euler(0, 0, -20-25*(5-i)));
                missile.GetComponent<IMissile>().SetSpeed(11, 1, 14);
                missile.GetComponent<IMissile>().SetValues (missileDMG, 3.9f, 20, true, Player);
                missile.GetComponent<ExplosiveMissile>().SetHoming();
                missile.GetComponent<ExplosiveMissile>().SetTargetAndHomingAccel(transform.position, 40);
                GameObject missile2 = Instantiate (MissilePrefab, fp.position, fp.rotation*Quaternion.Euler(0, 0, 20+25*(5-i)));
                missile2.GetComponent<IMissile>().SetSpeed(11, 1, 14);
                missile2.GetComponent<IMissile>().SetValues (missileDMG, 3.9f, 20, true, Player);
                missile2.GetComponent<ExplosiveMissile>().SetHoming();
                missile2.GetComponent<ExplosiveMissile>().SetTargetAndHomingAccel(transform.position, 40);
                yield return new WaitForSeconds(0.117647f);
            }
        } else {
            for (int i = 0; i<10; i++){ 
                GameObject missile = Instantiate (MissilePrefab, fp.position, transform.rotation*Quaternion.Euler(0, 0, -30-25*(5-i)));
                missile.GetComponent<IMissile>().SetSpeed(8, 4, 16);
                missile.GetComponent<IMissile>().SetValues (400, 4, 85, true, Player);
                missile.GetComponent<ExplosiveMissile>().SetHoming();
                missile.GetComponent<ExplosiveMissile>().SetTargetAndHomingAccel(transform.position, 60);
                GameObject missile2 = Instantiate (MissilePrefab, fp.position, transform.rotation*Quaternion.Euler(0, 0, 30+25*(5-i)));
                missile2.GetComponent<IMissile>().SetSpeed(8, 4, 16);
                missile2.GetComponent<IMissile>().SetValues (400, 4, 85, true, Player);
                missile2.GetComponent<ExplosiveMissile>().SetHoming();
                missile2.GetComponent<ExplosiveMissile>().SetTargetAndHomingAccel(transform.position, 60);
                yield return new WaitForSeconds(0.117647f);
            }
        }
    }
    



    //end of attacks

    private void SetWaypoint(){
        if (Player==null) return;
        if ((Waypoint.x==0 && Waypoint.y==0) || Vector2.Distance(Waypoint, rb.position)<1){
            if(moveState==0) {
                Waypoint = new Vector2(0,36) + 6*Random.insideUnitCircle;
            }
            else {Waypoint = GetValidPoint(6);}
        }
        if(!dashing) MoveDir = (Waypoint-rb.position).normalized;
    }
    public bool CheckDefeated(){
        if (health == 0) {StartCoroutine(Destruction()); return true;}
        else{
            //shoot ??? at miku
            StartCoroutine(MissileBarrage(3));
            mspeed=0;
            return false;
        }
    }

    public void Damage (int dmg, bool stun){
        if(moveSpeed == 0) dmg -= 10;
        health-=dmg;
        if (health<0) health = 0;
        Bar.value = health;
        if(health==0){
            Bar.gameObject.SetActive(false);
        }
    }
    public void MeleeDamage (int dmg, bool stun){
        if (meleeTimer>0.001) return;
        health-=50;
        if (health<0) health = 0;
        meleeTimer = 0.5f;
        Bar.value = health;
        if(health==0){
            Bar.gameObject.SetActive(false);
        }
    }

    private Vector2 GetValidPoint(float d){
        int iter = 0;
        Vector2 point;
        float dist;
        do{
            point = new Vector2(Random.Range(-23.0f, 23.0f),20+Random.Range(1.0f, 30.0f));
            iter++;
            dist = Vector3.Distance(Player.transform.position,(Vector3)point);
        }while(iter < 13 && !(dist > d && dist < 20) );
        if (iter > 12) return new Vector2(0,36);
        else return point;
    }

    private IEnumerator Destruction(){
        cam.GetComponent<CamShake>().Shake();
        for (int i = 0; i<8; i++){
            if(i==3){
                var temp = sr.color;
                temp.a = 0.7f;
                sr.color = temp;
            } else if (i==6){
                var temp = sr.color;
                temp.a = 0.4f;
                sr.color = temp;
            }
            var a = Random.value*3-1.5f;
            var b = Random.value*3-1.5f;
            Instantiate(explosionPrefab, transform.position+transform.up*a+transform.right*b, Quaternion.Euler(new Vector3(0, 180, 0)));
            yield return new WaitForSeconds(0.1f);
        }
        BossName.gameObject.SetActive(false);
        Destroy(gameObject);
    }
    private IEnumerator BarAnimation(){
        yield return new WaitForSeconds(0.4f);
        BossName.gameObject.SetActive(true);
        StartCoroutine(FadeInGUI(Bar.gameObject.transform.GetChild(0).gameObject.GetComponent<Image>(), Bar.gameObject));
        Bar.maxValue = maxHealth;
        while(Bar.value<maxHealth){
            Bar.value+=(int)(5000*Time.deltaTime);
            if(Bar.value>maxHealth) Bar.value=maxHealth;
            yield return null;
        }
    }

    private float TimerF( float val){
        if(val>=0){
            val-=Time.deltaTime;
            if (val<0) val = 0;
        }
        return val;
    }

    private IEnumerator FadeInGUI(Image img, GameObject g){
        g.SetActive(true);
        while (img.color.a<1){
            var temp = img.color.a;
            img.color = new Color(img.color.r, img.color.g, img.color.b, temp+Time.deltaTime);
            yield return null;
        }
    }
}

