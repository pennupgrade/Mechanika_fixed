using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Unity.Mathematics;
using Random = UnityEngine.Random;

using Utilities;

public partial class Boss3AI : MonoBehaviour, IEnemy
{
    public GameObject cam;
    public Slider Bar;
    public TextMeshProUGUI BossName;
    [Header("Prefabs")]

    public GameObject explosionPrefab, RocketPrefab, BlueRocketPrefab, MissilePrefab,
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
    private bool dashing, started;
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
        maxHealth = 11666; health=maxHealth;
        moveState = 0;
        frameTimer = 1;
        trackingBspd = 16;
        bulletDMG = 72; rocketDMG = 150; missileDMG = 90;
        electricDMG = 60; magicDMG = 80;
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
        if (!started) return;
        rb.MovePosition(rb.position + Time.fixedDeltaTime*mspeed*MoveDir);
        fp.eulerAngles += Cturn * Time.fixedDeltaTime * Vector3.forward; 
        
        int2 ori = EnemyUtils.AngleDegreesToFourOrientation(fp.eulerAngles.z);
        Animator.SetInteger("Vertical", ori.y);

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
            started = true;
            Waypoint = new Vector2(0,36);
            StartCoroutine(BarAnimation());
        }

        if(moveState == 1) {mspeed = moveSpeed;}
        else {mspeed = moveSpeed/2;}
    }
    public void SetState(int s){}
    public void SetAttack(int a){
        if(Player == null) return;
        if (a == 0){
            SingleRocket();
        } else if (a == 1) {
            StartCoroutine(RocketBarrage(true));
        } else if (a == 11) {
        StartCoroutine(RocketBarrage(false));
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
            ExecuteHomingRing();
        }
    }
    
    // attacks
    private IEnumerator DashRockets(){
        if(Player==null) {StopAllCoroutines(); yield break;}
        for(int i = 0; i<10; i++){
            if (i != 4) {
                GameObject missile = Instantiate (RocketPrefab, transform.position, fp.rotation);
                missile.GetComponent<IMissile>().SetSpeed(2,80,40);
                missile.GetComponent<IMissile>().SetValues (rocketDMG, 0.70588f + 0.17647f*(i+1) - 0.058823f*i, 40, true, Player);
            }
            yield return new WaitForSeconds(0.058823f);
        }
    }
    private void SingleRocket(){
        if(Player==null) return;
        GameObject missile = Instantiate (RocketPrefab, transform.position, fp.rotation);
        missile.GetComponent<IMissile>().SetSpeed(4,30,24);
        missile.GetComponent<IMissile>().SetValues (rocketDMG, 0.70588f, 110, true, Player);
    }
    private IEnumerator RocketBarrage(bool regular){
        if(Player==null) {StopAllCoroutines(); yield break;}
        for(int i = 0; i<4; i++){
            GameObject missile, missile2;
            if (regular) {
                missile = Instantiate (RocketPrefab, fp.position+fp.right, fp.rotation*Quaternion.Euler(0, 0, -130+35*i));
                missile2 = Instantiate (RocketPrefab, fp.position-fp.right, fp.rotation*Quaternion.Euler(0, 0, 130-35*i));
            } else {
                missile = Instantiate (BlueRocketPrefab, fp.position+fp.right, fp.rotation*Quaternion.Euler(0, 0, -130+35*i));
                missile2 = Instantiate (BlueRocketPrefab, fp.position-fp.right, fp.rotation*Quaternion.Euler(0, 0, 130-35*i));
            }
            missile.GetComponent<IMissile>().SetSpeed(8,30,26);
            missile.GetComponent<IMissile>().SetValues (rocketDMG, 0.35294f * (2 - i) + 0.70588f, 90, true, Player);
            missile2.GetComponent<IMissile>().SetSpeed(8,30,26);
            missile2.GetComponent<IMissile>().SetValues (rocketDMG, 0.35294f * (2 - i) + 0.70588f, 90, true, Player);
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
        
        for (int i = 0; i<5; i++){ 
            if(Player==null) {StopAllCoroutines(); yield break;}
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
                missile.GetComponent<IMissile>().SetSpeed(10, 1, 14);
                missile.GetComponent<IMissile>().SetValues (missileDMG, 3.9f, 20, true, Player);
                missile.GetComponent<ExplosiveMissile>().SetHoming();
                missile.GetComponent<ExplosiveMissile>().SetTargetAndHomingAccel(transform.position, 40);
                GameObject missile2 = Instantiate (MissilePrefab, fp.position, fp.rotation*Quaternion.Euler(0, 0, 20+25*(5-i)));
                missile2.GetComponent<IMissile>().SetSpeed(10, 1, 14);
                missile2.GetComponent<IMissile>().SetValues (missileDMG, 3.9f, 20, true, Player);
                missile2.GetComponent<ExplosiveMissile>().SetHoming();
                missile2.GetComponent<ExplosiveMissile>().SetTargetAndHomingAccel(transform.position, 40);
                yield return new WaitForSeconds(0.117647f);
            }
        } else {
            for (int i = 0; i<20; i++){ 
                GameObject missile = Instantiate (MissilePrefab, fp.position, transform.rotation*Quaternion.Euler(0, 0, -30-25*(5-i)));
                missile.GetComponent<IMissile>().SetSpeed(8, 4, 16);
                missile.GetComponent<IMissile>().SetValues (400, 5, 85, true, Player);
                missile.GetComponent<ExplosiveMissile>().SetHoming();
                missile.GetComponent<ExplosiveMissile>().SetTargetAndHomingAccel(transform.position, 60);
                GameObject missile2 = Instantiate (MissilePrefab, fp.position, transform.rotation*Quaternion.Euler(0, 0, 30+25*(5-i)));
                missile2.GetComponent<IMissile>().SetSpeed(8, 4, 16);
                missile2.GetComponent<IMissile>().SetValues (400, 5, 85, true, Player);
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
        if (dmg > 400) SFXPlayer.PlaySound("HIT_SELF1");
        else SFXPlayer.PlaySound("HIT_BIG2");
        if (health<0) health = 0;
        Bar.value = health;
        if(health==0){
            Bar.gameObject.SetActive(false);
        }
    }
    public void MeleeDamage (int dmg, bool stun){
        if (meleeTimer>0.001) return;
        SFXPlayer.PlaySound("HIT_SELF1");
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
        SFXPlayer.PlaySound("MISC_4");
        for (int i = 0; i<8; i++){
            if(i==3){
                var temp = sr.color;
                temp.a = 0.7f;
                sr.color = temp;
                SFXPlayer.PlaySound("MISC_4");
            } else if (i==6){
                var temp = sr.color;
                temp.a = 0.4f;
                sr.color = temp;
                SFXPlayer.PlaySound("MISC_4");
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

public partial class Boss3AI : MonoBehaviour, IEnemy
{
    public void SetBulletEngine(ExecutionEnum a){
        if(Player == null) return;
        switch(a) 
        {
            case ExecutionEnum.CQ_RING:
            ExecuteCQRing(); break;
            case ExecutionEnum.EXPANDING_RING_1:
            ExecuteExpandingRing1(); break;
            case ExecutionEnum.EXPANDING_RING_2:
            ExecuteExpandingRing2(); break;
            case ExecutionEnum.EXPANDING_RING_3:
            ExecuteExpandingRing3(); break;
            case ExecutionEnum.ALT_RING_COLORS:
            AlternateRingColors(); break;
            case ExecutionEnum.HOMING_RING:
            ExecuteHomingRing(); break;
            case ExecutionEnum.FIRST_PIANO_BOXES:
            ExecuteFirstPianoBoxes(); break;
            case ExecutionEnum.SECOND_PIANO_BOXES:
            ExecuteSecondPianoBoxes(); break;
            case ExecutionEnum.INTIMIDATION_TRAIL_1:
            ExecuteIntimidationTrail1(); break;
            case ExecutionEnum.MINI_CIRCLE_EXPLODE_1:
            ExecuteMiniCircleExplode1(); break;
            case ExecutionEnum.MINI_CIRCLE_EXPLODE_2:
            ExecuteMiniCircleExplode2(); break;
            case ExecutionEnum.MINI_CIRCLE_EXPLODE_3:
            ExecuteMiniCircleExplode3(); break;
            case ExecutionEnum.FOLLOW_TRAIL_1:
            ExecuteFollowTrail1(); break;
            case ExecutionEnum.FIRE_BALL_1:
            ExecuteFireBall1(); break;
            case ExecutionEnum.FIRE_BALL_2:
            ExecuteFireBall2(); break;
            case ExecutionEnum.FIRE_BALL_3:
            ExecuteFireBall3(); break;
            case ExecutionEnum.FIRE_BALL_4:
            ExecuteFireBall4(); break;
            case ExecutionEnum.CHILL_TRAIL:
            ExecuteChillTrail(); break;
            case ExecutionEnum.CHILL_SURROUND:
            ExecuteChillSurround(); break;
            case ExecutionEnum.CHILL_SURROUND_POLY:
            ExecuteChillSurroundPoly(); break;
            case ExecutionEnum.FIREWORK_1:
            ExecuteFireworks1(); break;
            case ExecutionEnum.FIREWORK_2:
            ExecuteFireworks2(); break;
            case ExecutionEnum.PIANO_TRAIL_1:
            ExecutePianoTrail1(); break;
        }
    }    

    // Bullet Engine Patterns
    [Header("Patterns")]
    [SerializeField] APattern ExpandingRing1;
    [SerializeField] APattern ExpandingRing2;
    [SerializeField] APattern ExpandingRing3;
    [SerializeField] APattern HomingRing;
    [SerializeField] APattern FirstPianoBoxes;
    [SerializeField] APattern SecondPianoBoxes;
    [SerializeField] APattern IntimidationTrail1;
    [SerializeField] APattern MiniCircleExplode1;
    [SerializeField] APattern MiniCircleExplode2;
    [SerializeField] APattern MiniCircleExplode3;
    [SerializeField] APattern FollowTrail1;
    [SerializeField] APattern FireBall1;
    [SerializeField] APattern FireBall2;
    [SerializeField] APattern FireBall3;
    [SerializeField] APattern FireBall4;
    [SerializeField] APattern ChillTrail;
    [SerializeField] APattern ChillSurround;
    [SerializeField] APattern ChillSurroundPoly;
    [SerializeField] APattern Firework1;
    [SerializeField] APattern Firework2;
    [SerializeField] APattern PianoTrail1;
    
    void ExecuteCQRing() 
    {
        int count = 18;
        for (int i = 0; i<count; i++){
            GameObject bullet = Instantiate (BulletPrefab, transform.position, transform.rotation*Quaternion.Euler(0, 0, i*360/count));
            bullet.GetComponent<IBullet>().SetValues (bulletDMG, 9, 0.65f+0.2f*Random.value, 9, Vector2.zero);
        }
    }

    void ExecuteExpandingRing1(int count = 1, float delaySeconds = 2) 
    {
        if(count <= 0) return;
        ExpandingRing1.Execute(BulletEngineManager.bossEngine, BulletEngineManager.Ins.Boss, BulletEngineManager.UsedPlayerTransform, 
            () => StartCoroutine(Utils.WaitThenAction(4, () => ExecuteExpandingRing1(count-1))));
    }

    void ExecuteExpandingRing2() 
    {
        ExpandingRing2.Execute(BulletEngineManager.bossEngine, BulletEngineManager.Ins.Boss, BulletEngineManager.UsedPlayerTransform, null);
    }

    void ExecuteExpandingRing3() 
    {
        ExpandingRing3.Execute(BulletEngineManager.bossEngine, BulletEngineManager.Ins.Boss, BulletEngineManager.UsedPlayerTransform, null);
    }

    void AlternateRingColors() => ExpandingCirclePattern.ShiftAllGroupColors();

    void ExecuteHomingRing()
        => HomingRing.Execute(BulletEngineManager.bossEngine, BulletEngineManager.Ins.Boss, BulletEngineManager.UsedPlayerTransform, null);

    void ExecuteFirstPianoBoxes()
    {   
        //global uniform to set boxbullet shader to normal rot speed just in case
        FirstPianoBoxes.Execute(BulletEngineManager.bossEngine, BulletEngineManager.Ins.Boss, BulletEngineManager.UsedPlayerTransform, null);
    }

    void ExecuteSecondPianoBoxes()
    {
        //global uniform to speed up boxbullet shader
        SecondPianoBoxes.Execute(BulletEngineManager.bossEngine, BulletEngineManager.Ins.Boss, BulletEngineManager.UsedPlayerTransform, null);
    }

    void ExecuteIntimidationTrail1()
    {
        IntimidationTrail1.Execute(BulletEngineManager.bossEngine, BulletEngineManager.Ins.Boss, BulletEngineManager.UsedPlayerTransform, null);
    }

    void ExecuteMiniCircleExplode1()
    {
        MiniCircleExplode1.Execute(BulletEngineManager.bossEngine, BulletEngineManager.Ins.Boss, BulletEngineManager.UsedPlayerTransform, null);
    }

    void ExecuteMiniCircleExplode2()
    {
        MiniCircleExplode2.Execute(BulletEngineManager.bossEngine, BulletEngineManager.Ins.Boss, BulletEngineManager.UsedPlayerTransform, null);
    }

    void ExecuteMiniCircleExplode3()
    {
        MiniCircleExplode3.Execute(BulletEngineManager.bossEngine, BulletEngineManager.Ins.Boss, BulletEngineManager.UsedPlayerTransform, null);
    }
    
    void ExecuteFollowTrail1()
    {
        FollowTrail1.Execute(BulletEngineManager.bossEngine, BulletEngineManager.Ins.Boss, BulletEngineManager.UsedPlayerTransform, null);
    }

    void ExecuteFireBall1()
    {
        FireBall1.Execute(BulletEngineManager.bossEngine, BulletEngineManager.Ins.Boss, BulletEngineManager.UsedPlayerTransform, null);
    }

    void ExecuteFireBall2()
    {
        FireBall2.Execute(BulletEngineManager.bossEngine, BulletEngineManager.Ins.Boss, BulletEngineManager.UsedPlayerTransform, null);
    }

    void ExecuteFireBall3()
    {
        FireBall3.Execute(BulletEngineManager.bossEngine, BulletEngineManager.Ins.Boss, BulletEngineManager.UsedPlayerTransform, null);
    }

    void ExecuteFireBall4()
    {
        FireBall4.Execute(BulletEngineManager.bossEngine, BulletEngineManager.Ins.Boss, BulletEngineManager.UsedPlayerTransform, null);
    }

    void ExecuteChillTrail()
    {
        ChillTrail.Execute(BulletEngineManager.bossEngine, BulletEngineManager.Ins.Boss, BulletEngineManager.UsedPlayerTransform, null);
    }

    void ExecuteChillSurround()
    {
        ChillSurround.Execute(BulletEngineManager.bossEngine, BulletEngineManager.Ins.Boss, BulletEngineManager.UsedPlayerTransform, null);
    }

    void ExecuteChillSurroundPoly()
    {
        ChillSurroundPoly.Execute(BulletEngineManager.bossEngine, BulletEngineManager.Ins.Boss, BulletEngineManager.UsedPlayerTransform, null);
    }

    void ExecuteFireworks1()
    {
        Firework1.Execute(BulletEngineManager.bossEngine, BulletEngineManager.Ins.Boss, BulletEngineManager.UsedPlayerTransform, null);
    }

    void ExecuteFireworks2()
    {
        Firework2.Execute(BulletEngineManager.bossEngine, BulletEngineManager.Ins.Boss, BulletEngineManager.UsedPlayerTransform, null);
    }

    void ExecutePianoTrail1()
    {
        PianoTrail1.Execute(BulletEngineManager.bossEngine, BulletEngineManager.Ins.Boss, BulletEngineManager.UsedPlayerTransform, null);
    }

    public enum ExecutionEnum
    {
        CQ_RING,
        EXPANDING_RING_1,
        EXPANDING_RING_2,
        EXPANDING_RING_3,
        ALT_RING_COLORS,
        HOMING_RING,
        FIRST_PIANO_BOXES,
        SECOND_PIANO_BOXES,
        INTIMIDATION_TRAIL_1,
        MINI_CIRCLE_EXPLODE_1,
        MINI_CIRCLE_EXPLODE_2,
        MINI_CIRCLE_EXPLODE_3,
        FOLLOW_TRAIL_1,
        FIRE_BALL_1,
        FIRE_BALL_2,
        FIRE_BALL_3,
        FIRE_BALL_4,
        CHILL_TRAIL,
        CHILL_SURROUND,
        CHILL_SURROUND_POLY,
        FIREWORK_1,
        FIREWORK_2,
        PIANO_TRAIL_1
    }
}