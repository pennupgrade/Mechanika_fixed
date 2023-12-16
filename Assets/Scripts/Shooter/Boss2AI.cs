using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Unity.Mathematics;
using Random = UnityEngine.Random;

using Utilities;

public class Boss2AI : MonoBehaviour, IEnemy
{
    public GameObject cam;
    public Slider Bar;
    public TextMeshProUGUI BossName;
    [Header("Prefabs")]

    public GameObject BulletPrefab, explosionPrefab, RocketRefab, MissilePrefab,
        ChargedShotPrefab, BounceBulletPrefab;
    [Header("Enemy Values")]
    public int health;
    [SerializeField] private int maxHealth;
    [SerializeField] private float moveSpeed, moveSpeed2, turnSpeed, turnSpeed2, mspeed, tspeed;
    private int bulletDMG, rocketDMG, missileDMG, cqDMG, laserDMG, chargedShotDMG, bounceBulletDMG;
    private float trackingBspd, bulletSpeed, chargedShotSpeed, bounceBulletSpeed, bulletCD;
    [SerializeField]private float Cturn, meleeTimer, dashTimer, CQTimer;
    private Vector2 Waypoint, TargetDir, MoveDir, DashDir;
    private Rigidbody2D rb;
    private Vector3 endpt;
    private Transform fp;
    private bool dashing, tracking, laser, warning;
    public GameObject Player;
    [SerializeField] private int moveState, mode;
    private int frameTimer;

    [Header("Misc")]
    [SerializeField] Animator Animator;
    [SerializeField] SpriteRenderer sr;

    // Start is called before the first frame update
    void Awake(){
        rb = GetComponent<Rigidbody2D>();
        Player = GameObject.FindWithTag("Player");
        dashing = false; tracking = true; laser = false; warning = false;
    }
    void Start()
    {
        MoveDir=Vector2.zero;
        fp = gameObject.transform.GetChild(0);
        maxHealth = 12500; health=maxHealth;
        moveState = 0; mode = -1;
        frameTimer = 1;
        bulletDMG=60; bulletCD=0.24f; bulletSpeed = 10; rocketDMG = 150; missileDMG = 90;
        trackingBspd = bulletSpeed;
        cqDMG = 60; laserDMG = 360; chargedShotDMG = 320; chargedShotSpeed = 19; bounceBulletDMG = 240; 
        bounceBulletSpeed = 9;
        Cturn = 0;
        mspeed = 0; moveSpeed=5; moveSpeed2 = 8; turnSpeed=70; turnSpeed2 = 120; tspeed = turnSpeed;
        meleeTimer = 0; dashTimer = 0; CQTimer = 0;
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
            CQCheck();
        }
        var s = Vector3.Dot(fp.up, TargetDir);
        if(s>0.9994f||!tracking){
            if(!laser) Cturn=0;
        }
        else{
            if (Vector3.Dot(fp.right, TargetDir)>0){
                Cturn = -tspeed;
            } else Cturn = tspeed;
        }
        if(laser) LaserDamage();
        if(warning) Warning();
        if(dashing&&dashTimer<0.01f){
            dashing = false;
            if(moveState==0) mspeed=0;
            else if(moveState==1) mspeed = moveSpeed;
            else mspeed = moveSpeed2;
        }

        meleeTimer=TimerF(meleeTimer); dashTimer=TimerF(dashTimer); CQTimer=TimerF(CQTimer);
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + Time.fixedDeltaTime*mspeed*MoveDir);
        fp.eulerAngles += Cturn * Time.fixedDeltaTime * Vector3.forward; 

        //
        int2 ori = EnemyUtils.AngleDegreesToFourOrientation(fp.eulerAngles.z);

        Animator.SetInteger("Horizontal", ori.x);
        Animator.SetInteger("Vertical", ori.y);

    }

    private void FireBullet(){
        GameObject bullet = Instantiate (BulletPrefab, fp.position, fp.rotation*Quaternion.Euler(0, 0, 12*(Random.value-0.5f)));
        bullet.GetComponent<IBullet>().SetValues (bulletDMG, bulletSpeed, 4, -1.5f, Vector2.zero);
    }

    private IEnumerator Attack0(bool m){
        trackingBspd = bulletSpeed; tracking = true; laser = false; warning = false;
        LaserController.DisableParticles();
        yield return new WaitForSeconds(0.8f);
        for (int i = 0; i<20; i++){
            FireBullet();
            if (m && i==12) Attack0Missile();
            yield return new WaitForSeconds(bulletCD);
        }
        AttackSelect();
    }
    private IEnumerator Attack1(){
        if(Player==null) {StopAllCoroutines(); yield break;}
        tracking = false; laser = false; Cturn = 0;
        warning = true; LaserController.StartDrawTelegraph();
        yield return new WaitForSeconds(1.6f); laser = true; warning = false; LaserController.EnableParticles();
        if (Vector3.Dot(fp.right, (Player.transform.position-transform.position).normalized)>0){
                Cturn = -turnSpeed;
        } else Cturn = turnSpeed;
        yield return new WaitForSeconds(2);
        LaserController.DisableParticles();
        laser = false; tracking = true;
        AttackSelect();
    }
    private IEnumerator Attack2(int mType){
        trackingBspd = chargedShotSpeed; tracking = true; laser = false; warning = false;
        LaserController.DisableParticles();
        if(mType == 1){
            for(int i = 0; i<6; i++){
            GameObject missile = Instantiate (RocketRefab, fp.position+fp.right, fp.rotation);
            missile.GetComponent<IMissile>().SetSpeed(1,20,26);
            missile.GetComponent<IMissile>().SetValues (rocketDMG, 0.8f, 120, true, Player);
            yield return new WaitForSeconds(.08f);
            }
        }else if (mType==2){
            for(int i = 1; i<8; i++){
            GameObject missile = Instantiate (MissilePrefab, fp.position+fp.right, fp.rotation*Quaternion.Euler(0, 0, -10*i));
            missile.GetComponent<IMissile>().SetSpeed(4,8,18);
            missile.GetComponent<IMissile>().SetValues (missileDMG, 4, 110, false, Player);
            yield return new WaitForSeconds(.06f);
            GameObject missile2 = Instantiate (MissilePrefab, fp.position-fp.right, fp.rotation*Quaternion.Euler(0, 0, 10*i));
            missile2.GetComponent<IMissile>().SetSpeed(4,8,18);
            missile2.GetComponent<IMissile>().SetValues (missileDMG, 4, 110, false, Player);
            yield return new WaitForSeconds(.06f);
            }
        }else if (mType==3){
            for(int i = 0; i<6; i++){
            GameObject missile = Instantiate (MissilePrefab, fp.position+fp.right, fp.rotation*Quaternion.Euler(0, 0, -24*i));
            missile.GetComponent<IMissile>().SetSpeed(4,4,11);
            missile.GetComponent<IMissile>().SetValues (missileDMG, 6.4f, 75, false, Player);
            yield return new WaitForSeconds(.08f);
            GameObject missile2 = Instantiate (MissilePrefab, fp.position-fp.right, fp.rotation*Quaternion.Euler(0, 0, 24*i));
            missile2.GetComponent<IMissile>().SetSpeed(4,4,11);
            missile2.GetComponent<IMissile>().SetValues (missileDMG, 6.4f, 75, false, Player);
            yield return new WaitForSeconds(.08f);
            }
            yield return new WaitForSeconds(0.8f);
        }else if (mType==4){
            for(int i = 0; i<4; i++){
            GameObject missile = Instantiate (MissilePrefab, fp.position+fp.right, fp.rotation*Quaternion.Euler(0, 0, -100-20*i));
            missile.GetComponent<IMissile>().SetSpeed(20,-8,12);
            missile.GetComponent<IMissile>().SetValues (missileDMG, 3.6f, 110, false, Player);
            GameObject missile2 = Instantiate (MissilePrefab, fp.position-fp.right, fp.rotation*Quaternion.Euler(0, 0, 100+20*i));
            missile2.GetComponent<IMissile>().SetSpeed(20,-8,12);
            missile2.GetComponent<IMissile>().SetValues (missileDMG, 3.6f, 110, false, Player);
            yield return new WaitForSeconds(.32f);
            }
        }else if (mType==5){
            for(int i = 0; i<3; i++){
            GameObject missile = Instantiate (RocketRefab, fp.position+fp.right, fp.rotation*Quaternion.Euler(0, 0, -40-30*i));
            missile.GetComponent<IMissile>().SetSpeed(8,50,28);
            missile.GetComponent<IMissile>().SetValues (rocketDMG, 1.6f, 90, true, Player);
            GameObject missile2 = Instantiate (RocketRefab, fp.position-fp.right, fp.rotation*Quaternion.Euler(0, 0, 40+30*i));
            missile2.GetComponent<IMissile>().SetSpeed(8,50,28);
            missile2.GetComponent<IMissile>().SetValues (rocketDMG, 1.6f, 90, true, Player);
            yield return new WaitForSeconds(.08f);
            }
        }
        yield return new WaitForSeconds(1.6f);
        AttackSelect();
    }
    private IEnumerator Attack3(){
        trackingBspd = chargedShotSpeed; tracking = true; laser = false; LaserController.StartDrawTelegraph();
        LaserController.DisableParticles();
        for (int i = 0; i<2; i++){
            warning = true;
            yield return new WaitForSeconds(1.2f);
            GameObject bullet = Instantiate (ChargedShotPrefab, fp.position, fp.rotation);
            bullet.GetComponent<IBullet>().SetValues (chargedShotDMG, chargedShotSpeed, 3, 0, Vector2.zero);
            SFXPlayer.PlaySound("WP_3B");
        } Attack0Missile();
        warning = false;
        yield return new WaitForSeconds(0.4f);
        AttackSelect();
    }
    private IEnumerator Attack4(){
        trackingBspd = bounceBulletSpeed; tracking = true; laser = false; warning = false;
        LaserController.DisableParticles();
        for (int i = 0; i<5; i++){
            GameObject bullet = Instantiate (BounceBulletPrefab, fp.position, fp.rotation*Quaternion.Euler(0, 0, 8*(Random.value-0.5f)));
            bullet.GetComponent<IBullet>().SetValues (bounceBulletDMG, bounceBulletSpeed, 20, -0.1f, Vector2.zero);
            yield return new WaitForSeconds(1.6f);
        }
        AttackSelect();
    }
    private void Dash(){
        dashing = true; dashTimer= 0.5f;
        mspeed=3*moveSpeed2;
        int iter = 0;
        if (Vector3.Distance(new Vector3(0,36,0), transform.position)>10){
            do{
                iter++;//find center
                MoveDir = (Vector2)(Quaternion.AngleAxis(90*(Random.value-0.5f),Vector3.forward)
                *(Vector3)((new Vector3(0,36,0))-(Vector3)rb.position).normalized);
            }while(iter<10&&Vector2.Dot((Vector2)fp.up,MoveDir)>0.85f);
        } else if(Player!=null){
            Vector3 v;
            if(Random.value>0.5f) v = fp.forward; else v = -fp.forward;
            MoveDir = Vector3.Cross((Vector3)(Player.transform.position-(Vector3)rb.position).normalized, v);
        }
    }
    private void Attack0Missile(){
        if(Player==null) {StopAllCoroutines(); return;}
        for(int i = 0; i<2; i++){
            GameObject missile = Instantiate (MissilePrefab, fp.position, fp.rotation*Quaternion.Euler(0, 0, 40-80*i));
            missile.GetComponent<IMissile>().SetSpeed(4,5,16);
            missile.GetComponent<IMissile>().SetValues (missileDMG, 5, 130, true, Player);
        }
    }
    private void CQCheck(){
        if(Player==null) {StopAllCoroutines(); return;}
        if(CQTimer>0.01f || Vector3.Distance(Player.transform.position, (Vector3)rb.position)>5.5f) return;
        CQTimer=2;
        for (int i = 0; i<15;i++){
            GameObject bullet = Instantiate (BulletPrefab, fp.position, fp.rotation*Quaternion.Euler(0, 0, 120*(Random.value-0.5f)));
            bullet.GetComponent<IBullet>().SetValues (cqDMG, 10+3*Random.value, 0.45f+0.2f*Random.value, 5.5f, Vector2.zero);
        }
    }
    private void LaserDamage(){
        RaycastHit2D hit = Physics2D.Raycast((Vector2)fp.transform.position, (Vector2)fp.up, 70, 1<<11);
        endpt = fp.transform.position+hit.distance*fp.up;

        LaserController.DrawLaser(fp.transform.position, endpt);

        if(Physics2D.Raycast((Vector2)fp.transform.position, (Vector2)fp.up,
        50, 1<<6)){
            if (Player.TryGetComponent<MikuMechControl>(out MikuMechControl miku)){
                miku.MeleeDamage(laserDMG, false);
            }
        }
    }
    private void Warning(){
        RaycastHit2D hit = Physics2D.Raycast((Vector2)fp.transform.position, (Vector2)fp.up, 70, 1<<11);
        endpt = fp.transform.position+hit.distance*fp.up;
        LaserController.DrawTelegraph((Vector2)fp.transform.position, (Vector2)endpt);
    }


    private void AttackSelect(){
        if(mode<0) return;
        if(mode==0) StartCoroutine(Attack0(false));
        else if (mode==1){
            StartCoroutine(Attack0(true));
        }
        else if (mode==2){
            var r = Random.value;
            if(r>0.4f) StartCoroutine(Attack1());
            else StartCoroutine(Attack0(true));
        }
        else if (mode==3){
            var r = Random.value;
            if(r>0.75f) StartCoroutine(Attack2(2));
            else if (r>0.55f) StartCoroutine(Attack2(3));
            else if (r>0.38f) StartCoroutine(Attack2(4));
            else if (r>0.08f) StartCoroutine(Attack2(5));
            else StartCoroutine(Attack0(true));
        }
        else if (mode==4){
            if(Random.value>0.84f) StartCoroutine(Attack0(true));
            else StartCoroutine(Attack3());
        } else if (mode==5){
            if(Random.value>0.7f) StartCoroutine(Attack0(true));
            else StartCoroutine(Attack4());
        }
        else if (mode==6){
            var r  = Random.value;
            if (r<0.05f) StartCoroutine(Attack1());
            else if (r<0.3f) {StartCoroutine(Attack2(1)); Dash();}
            else if (r<0.72f) {
                var s = Random.value;
                if(s>0.7f) StartCoroutine(Attack2(2));
                else if (s>0.5f) StartCoroutine(Attack2(3));
                else if (s>0.3f) StartCoroutine(Attack2(4));
                else StartCoroutine(Attack2(5));
            }
            else if (r<0.95f) StartCoroutine(Attack3());
            else StartCoroutine(Attack4());
        }
    }
    public void SetMode(int m){
        mode = m;
        if (mode == -10){
            mode = 0 ; health = maxHealth; Waypoint = new Vector2(0,36);
            StartCoroutine(BarAnimation());
        }
        if (mode == -1 || mode == 0) moveState = 0;
        else if (mode == 4) moveState = 3;
        else if (mode == 3 || mode == 6) moveState = 2;
        else moveState = 1;

        if(moveState == 0) mspeed = 0;
        else if(moveState == 1) {mspeed = moveSpeed; tspeed = turnSpeed;}
        else {mspeed = moveSpeed2; tspeed = turnSpeed2;}

    }
    public void SetState(int s){}
    public void SetAttack(int a){
        if(a!=5) StopAllCoroutines();
        if (a==0) StartCoroutine(Attack0(false));
        else if (a==10) StartCoroutine(Attack0(true));
        else if (a==1) StartCoroutine(Attack1());
        else if (a==2) StartCoroutine(Attack2(1));
        else if (a==22) StartCoroutine(Attack2(2));
        else if (a==222) StartCoroutine(Attack2(3));
        else if (a==2222) StartCoroutine(Attack2(4));
        else if (a==22222) StartCoroutine(Attack2(5));
        else if (a==3) StartCoroutine(Attack3());
        else if (a==4) StartCoroutine(Attack4());
        else Dash();
    }
    private void SetWaypoint(){
        if(Player==null) {StopAllCoroutines(); return;}
        if (moveState==0) return;
        if ((Waypoint.x==0 && Waypoint.y==0)||Vector2.Distance(Waypoint, rb.position)<1){
            if(moveState==3) {Waypoint = GetValidPoint(14);}
            else {Waypoint = GetValidPoint(5);}
        }
        if(!dashing) MoveDir = (Waypoint-rb.position).normalized;
    }
    public bool CheckDefeated(){
        if (health == 0) {StartCoroutine(Destruction()); return true;}
        else{
            //shoot laser at miku
            StopAllCoroutines();
            StartCoroutine(EndingLaser());
            mspeed=0;
            return false;
        }
    }
    private IEnumerator EndingLaser(){
        laser = false; warning = false; tracking = false; LaserController.DisableParticles();
        RaycastHit2D hit = Physics2D.Raycast((Vector2)fp.transform.position, (Vector2)((Player.transform.position-transform.position).normalized), 70, 1<<11);
        endpt = fp.transform.position+hit.distance*((Player.transform.position-transform.position).normalized);
        LaserController.EnableParticles();
        for (int k = 0; k < 400; k++){
            LaserController.DrawLaser(fp.transform.position, endpt);
            if (k == 10) Player.GetComponent<MikuMechControl>().Death();
            yield return null;
        }
        LaserController.DisableParticles();
    }

    public void Damage (int dmg, bool stun){
        if(mode == 0 || mode == -1) dmg -= 10;
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
            point = new Vector2(Random.Range(-25.0f, 25.0f),20+Random.Range(0.0f, 31.0f));
            iter++;
            dist = Vector3.Distance(Player.transform.position,(Vector3)point);
        }while(iter < 11 && !(dist > d && ((d>12)?(true):(dist<20)) ));
        if (iter > 10) return (Vector2) Player.transform.position;
        else return point;
    }

    private IEnumerator Destruction(){
        LaserController.DisableParticles();
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

