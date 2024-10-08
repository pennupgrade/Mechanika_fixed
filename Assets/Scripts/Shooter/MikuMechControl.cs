using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class MikuMechControl : MonoBehaviour, IBulletEngineInteractable
{
    public GameObject[] WeaponUI;
    public Slider hBar, sBar, eBar, cBar, rBar;
    public GameObject GM;
    [Header("Prefabs")]
    public GameObject CepheidPrefab;
    public GameObject DISCPrefab;
    public GameObject SenbonzakuraPrefab;
    public GameObject NOVAPrefab;
    public GameObject MeteorPrefab, explosionPrefab;
    private TrailRenderer tr;
    private float moveSpeed, mspeed;
    private bool lerpingHealth, lerpingShield, lerpingEnergy;
    [Header("Player Values")]
    [SerializeField] private int health, maxShield, shield, energy, weaponNum;
    private float shieldRegenTimer, meleeTimer, weaponCDTimer, chargeTimer, hurtTimer;
    private bool frozen, shieldRegen, W3Locked, W4Locked, W5Locked, sussyBakaEngine;
    private float stunTimer;

    [SerializeField] private bool stunned, dashing;
    private int knockback, dashDMG = 200, dashEnergy = 20; 
    private float dashTimer, dashCDTimer; private float dashCD = 0.75f;

    private int w1DMG = 22, w1Energy = 3, cepheidMode = 1; private float w1CD = 0.18f;
    private int w2DMG = 92, w2Energy = 20; private float w2CD = 1f;
    private int w3DMG = 24, w3Energy = 22; private float w3CD = 0.4f;
    private int w4DMG = 200, w4Energy = 20; private float w4CD = 0.1f;
    private int w5DMG = 200, w5Energy = 26; private float w5CD = 2.6f;
    [Header("Cam")]
    public Camera cam;
    private Rigidbody2D rb;
    private Vector2 movement, mousePos, lookDir;
    private Vector2 velocity;

    private Animator animator;

    // Start is called before the first frame update
    void Awake()
    {
        moveSpeed = 8;
        mspeed = moveSpeed; weaponNum = 1; cepheidMode = 1;
        energy = 100; health = 390; maxShield = 410; shield = 0;
        frozen = false; shieldRegen = false; dashing = false; knockback = 0;
        shieldRegenTimer = 0; weaponCDTimer = 0; dashCDTimer = 0; hurtTimer = 0;
        dashTimer = 0; chargeTimer = 0; meleeTimer = 0;
        stunTimer = 0; stunned = false; W3Locked = true; W4Locked = true; W5Locked = true;
        lerpingEnergy = false; lerpingHealth=false; lerpingShield=false;
        WeaponUpdate(1);
        StartCoroutine(EnergyRegen());
    }

    void Start(){
        rb = GetComponent<Rigidbody2D>(); tr = GetComponent<TrailRenderer>(); animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        IconUpdate(Time.deltaTime);

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        //update animator
        animator.SetInteger("Horizontal", (int) Mathf.Round(movement.x));
        animator.SetInteger("Vertical", (int) Mathf.Round(movement.y));

        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        //shield regeneration
        if (!shieldRegen && shield!=maxShield && shieldRegenTimer<0.001) StartCoroutine(Regenerator());

        //dash
        if((Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.LeftShift)||Input.GetKeyDown(KeyCode.Mouse1)) && dashCDTimer<0.01f && energy-dashEnergy >= 0){
            dashCDTimer=dashCD; dashing = true; dashTimer = 0.25f; energy-=dashEnergy; tr.emitting = true;
            SFXPlayer.PlaySound("WP_4A", 0.3f);
        }
        if (dashing&&dashTimer<0.001f) {dashing = false; tr.emitting = false;}

        //weapon select
        if (Input.GetKeyDown(KeyCode.Alpha1)) {WeaponUpdate(1);}
        if (Input.GetKeyDown(KeyCode.Alpha2)) {WeaponUpdate(2);}
        if (Input.GetKeyDown(KeyCode.Alpha3)&&!W3Locked) {WeaponUpdate(3);}
        if (Input.GetKeyDown(KeyCode.Alpha4)&&!W4Locked) {WeaponUpdate(4);}
        if (Input.GetKeyDown(KeyCode.Alpha5)&&!W5Locked) {WeaponUpdate(5);}
        if(Input.mouseScrollDelta.y>0){
            weaponNum++;
            if (weaponNum==3 && W3Locked) weaponNum++;
            if (weaponNum==4 && W4Locked) weaponNum++;
            if (weaponNum==5 && W5Locked) weaponNum++;
            if(weaponNum>5) weaponNum = 1;
            WeaponUpdate(weaponNum);
        }
        if(Input.mouseScrollDelta.y<0){
            weaponNum--;
            if(weaponNum<1) weaponNum = 5;
            if (weaponNum==5 && W5Locked) weaponNum--;
            if (weaponNum==4 && W4Locked) weaponNum--;
            if (weaponNum==3 && W3Locked) weaponNum--;
            WeaponUpdate(weaponNum);
        }

        //weapon fire
        if(!frozen){
            if (weaponNum==1){
                if (Input.GetKey(KeyCode.Mouse0) && weaponCDTimer<0.001 && energy-w1Energy >= 0){
                    weaponCDTimer=w1CD; EnergyUpdate(-w1Energy);
                    if(energy<50) energy+=1;
                    FireCepheid();
                }
            }else if (weaponNum==2){
                if (Input.GetKey(KeyCode.Mouse0) && weaponCDTimer<0.001 && energy-w2Energy >= 0){
                    weaponCDTimer=w2CD; EnergyUpdate(-w2Energy);
                    FireDISC();
                }
            }else if (weaponNum==3){
                if (Input.GetKey(KeyCode.Mouse0) && weaponCDTimer<0.001){
                    chargeTimer+=Time.deltaTime; cBar.value = chargeTimer;
                    if (chargeTimer>1) chargeTimer=1;
                } else if (Input.GetKeyUp(KeyCode.Mouse0) && weaponCDTimer<0.001 && energy-w3Energy >= 0){
                    weaponCDTimer=w3CD; EnergyUpdate(-w3Energy);
                    FireSenbonzakura(chargeTimer);
                    chargeTimer = 0; cBar.value = chargeTimer;
                }
                else {chargeTimer = 0; cBar.value = chargeTimer;}
            }else if (weaponNum==4){
                if (Input.GetKey(KeyCode.Mouse0) && weaponCDTimer<0.001){
                    moveSpeed = mspeed/1.5f;
                    chargeTimer+=Time.deltaTime; cBar.value = chargeTimer;
                    if (chargeTimer>4) chargeTimer=4;
                    if(w4Energy*chargeTimer>energy){
                        weaponCDTimer=1; EnergyUpdate(-100);
                        FireNOVA(chargeTimer); chargeTimer = 0; cBar.value = chargeTimer;
                        moveSpeed = mspeed+4*((400-health)/400.0f);
                    }
                } else if (Input.GetKeyUp(KeyCode.Mouse0) && weaponCDTimer<0.001){
                    weaponCDTimer=w4CD; EnergyUpdate(-(int)(w4Energy*chargeTimer));
                    FireNOVA(chargeTimer); chargeTimer = 0; cBar.value = chargeTimer;
                    moveSpeed = mspeed+4*((400-health)/400.0f);
                }
                else {chargeTimer = 0; cBar.value = chargeTimer; moveSpeed = mspeed+4*((400-health)/400.0f);}
            }else if(weaponNum==5){
                if (Input.GetKey(KeyCode.Mouse0) && weaponCDTimer<0.001 && energy-w5Energy >= 0){
                    weaponCDTimer=w5CD;
                    StartCoroutine(FireMeteor());
                }
            }
        }

        rBar.value = 16 - shieldRegenTimer;

        //update timers
        weaponCDTimer = TimerF(weaponCDTimer); dashCDTimer = TimerF(dashCDTimer); dashTimer = TimerF(dashTimer);
        shieldRegenTimer = TimerF(shieldRegenTimer); stunTimer = TimerF(stunTimer); meleeTimer = TimerF(meleeTimer);
        hurtTimer = TimerF(hurtTimer);
    }

    void FixedUpdate(){
        lookDir = mousePos - rb.position;
        //stunned
        if (stunned){
            moveSpeed = mspeed*0.65f;
            if (stunTimer<0.001f) {moveSpeed = mspeed+4*((400-health)/400.0f); stunned = false;}
        }
        if (frozen){
            velocity = Vector2.zero;
        } else {
            velocity = moveSpeed*movement.normalized;
        }
        if(knockback>0){
            knockback--;
            velocity-=5*lookDir.normalized;
        }
        if(!dashing){
            rb.MovePosition(rb.position + Time.fixedDeltaTime*velocity);
        }else{
            rb.MovePosition(rb.position + 24*Time.fixedDeltaTime*movement.normalized);
        }
    }

    private void FireCepheid(){
        if (cepheidMode % 2 == 0) SFXPlayer.PlaySound("WP_1A", 0.4f);
        else  SFXPlayer.PlaySound("WP_1B", 0.5f);
        GameObject bullet = Instantiate (CepheidPrefab, transform.position+0.15f*Vector3.up, Quaternion.identity);
        bullet.GetComponent<IBullet>().SetValues (w1DMG+(int)(w1DMG*((100.0f - energy)/100)), 8+(4*(100.0f - energy)/100), 1.5f, -3, 0.6f*velocity);
        bullet.GetComponent<CepheidBulletScript>().SetMode(cepheidMode);
        cepheidMode++;
        if (cepheidMode == 5) cepheidMode = 1;
        var a = 1;
        if(lookDir.y<0) a = -1;
        bullet.transform.eulerAngles = (a*Vector2.Angle(new Vector2(1,0), lookDir)-90+6*(Random.value-0.5f))* Vector3.forward;
    }
    private void FireDISC(){
        SFXPlayer.PlaySound("WP_2A", 0.15f);
        GameObject bullet = Instantiate (DISCPrefab, transform.position+0.15f*Vector3.up, Quaternion.identity);
        bullet.GetComponent<IBullet>().SetValues (w2DMG, 6.4f, 4, -1.5f, velocity);
        var a = 1;
        if(lookDir.y<0) a = -1;
        bullet.transform.eulerAngles = (a*Vector2.Angle(new Vector2(1,0), lookDir)-90+6*(Random.value-0.5f))* Vector3.forward;
    }
    private void FireSenbonzakura(float charge){
        var spread = 45-(20*charge);
        var range = 0.4f+0.3f*charge;
        SFXPlayer.PlaySound("WP_3A", 0.3f);
        for (int i = 0; i<15;i++){
            GameObject bullet = Instantiate (SenbonzakuraPrefab, transform.position+0.15f*Vector3.up, Quaternion.identity);
            bullet.GetComponent<IBullet>().SetValues (w3DMG, 10+5*charge+3*Random.value, range+0.2f*Random.value, 8-4*charge, 0.5f*velocity);
            var a = 1;
            if(lookDir.y<0) a = -1;
            bullet.transform.eulerAngles = (a*Vector2.Angle(new Vector2(1,0), lookDir)-90+spread*(Random.value-0.5f))* Vector3.forward;
        }
        knockback = 6;
    }
    private void FireNOVA(float charge){
        if (charge<0.4f) return;
        SFXPlayer.PlaySound("WP_4A");
        GameObject bullet = Instantiate (NOVAPrefab, transform.position+0.15f*Vector3.up, Quaternion.identity);
        bullet.GetComponent<IBullet>().SetValues ((int)(w4DMG*(charge)*(charge/4)), 6+0.4f*charge, 2.2f, -5, 0.2f*velocity);
        var a = 1;
        if(lookDir.y<0) a = -1;
        bullet.transform.eulerAngles = (a*Vector2.Angle(new Vector2(1,0), lookDir)-90)* Vector3.forward;
        knockback = 8;
    }
    private IEnumerator FireMeteor(){
        var count = 0;
        var d = lookDir;
        if(energy > 90) {count = 3;}
        else if (energy > 60) {count = 2;}
        else count = 1;
        EnergyUpdate(-energy+10);
        for(int i = 0; i<count; i++){
            GameObject bullet = Instantiate (MeteorPrefab, transform.position+0.15f*Vector3.up, Quaternion.identity);
            bullet.GetComponent<MeteorMissileScript>().SetValues (w5DMG-20+20*count, 4, 3, 10, 5, 110, gameObject);
            var a = 1;
            if(d.y<0) a = -1;
            bullet.transform.eulerAngles = (a*Vector2.Angle(new Vector2(1,0), d)-140+10*i)* Vector3.forward;
            SFXPlayer.PlaySound("WP_5A", 0.5f);
            yield return new WaitForSeconds(.12f);
        }
    }
    private void WeaponUpdate(int w){
        weaponNum=w;
        moveSpeed = mspeed+3*((400-health)/400.0f);
        chargeTimer=0; cBar.value = 0;
        if(weaponNum==3){
            cBar.gameObject.SetActive(true);
            cBar.maxValue = 1;
        } else if (weaponNum==4){
            cBar.gameObject.SetActive(true);
            cBar.maxValue = 4;
        } else cBar.gameObject.SetActive(false);
        for (int i =0; i<5; i++){
            Image img = WeaponIconImages[i];//WeaponUI[i].GetComponent<Image>();
            if(i+1==weaponNum) img.color = new Color(img.color.r, img.color.g, img.color.b, 1);
            else img.color = new Color(img.color.r, img.color.g, img.color.b, 0.4f);
        }
        WeaponUI[2].gameObject.SetActive(!W3Locked);
        WeaponUI[3].gameObject.SetActive(!W4Locked);
        WeaponUI[4].gameObject.SetActive(!W5Locked);
    } private void HealthUpdate(int h) {
        health+=h;
        moveSpeed = mspeed+4*((400-health)/400.0f);
        if (health>390) {health = 390; moveSpeed=10;}
        else if (health<0) health = 0;
        if(!lerpingHealth) StartCoroutine(LerpHealth());
    } private void ShieldUpdate(int h) {
        shield+=h;
        if (shield<0) shield = 0;
        else if (shield>maxShield) shield = maxShield;
        if(!lerpingShield) StartCoroutine(LerpShield());
    } private void EnergyUpdate(int h) {
        energy += h;
        if (energy>100) energy = 100;
        else if (energy<0) energy = 0;
        if(!lerpingEnergy) StartCoroutine(LerpEnergy());
    } private void IconUpdate(float dt) {
        float s; bool isActive;
        RectTransform iT; Image iI;
        for(int i=0; i<5; i++)
        {
            iT = WeaponIconTransforms[i];
            iI = WeaponIconImages[i];

            isActive = i == weaponNum - 1;

            s = Unity.Mathematics.math.lerp(iT.localScale.x, isActive ? ActiveTargetScale : InactiveTargetScale, ApproachTargetSpeed*dt); 
            iT.localScale = s * Vector3.one;

        }
    }

    [Header(" Weapon Icon UI ")]
    [SerializeField] List<RectTransform> WeaponIconTransforms;
    [SerializeField] List<Image> WeaponIconImages;
    
    [SerializeField] [Min(0f)] float ActiveTargetScale = 1.1f;
    [SerializeField] [Min(0f)] float InactiveTargetScale = 0.95f;

    [SerializeField] [Min(0f)] float ApproachTargetSpeed = 2f;

    private IEnumerator Regenerator(){
        shieldRegen = true;
        while(shield<maxShield){
            if (shieldRegenTimer>0.01f) {shieldRegen=false; yield break;}
            ShieldUpdate(5);
            yield return new WaitForSeconds(.05f);
        }
        shieldRegen = false;
    }

    private IEnumerator EnergyRegen(){
        while (true){
            if(energy<100){
                EnergyUpdate(5);
            }
            yield return new WaitForSeconds(.45f);
        }
    }

    public void Death(){
        SFXPlayer.PlaySound("MISC_4");
        if (sussyBakaEngine) {
            BulletEngineManager.bossEngine?.RemoveAllInteractables();
            BulletEngineManager.EndAllCoroutines();
        }

        GameObject expl = Instantiate(explosionPrefab, transform.position, Quaternion.Euler(new Vector3(0, 180, 0)));
        Destroy(expl, 2);
        GM.GetComponent<IGameManager>().Restart(true);
        Destroy(gameObject);
    }

    public void Damage(int dmg, bool stun){
        if (dashing) return;
        if (hurtTimer < 0.001f) {
            hurtTimer = 0.4f;
            if (dmg > 120) SFXPlayer.PlaySound("MIKU_HURT_BIG");
            else SFXPlayer.PlaySound("MIKU_HURT_SMALL");
        }
        if (shield>0) {ShieldUpdate(-dmg);}
        else {
            HealthUpdate(-dmg);
        }

        shieldRegenTimer = 16;
        if (stun){stunTimer = 0.4f; stunned = true;}
    }

    public void MeleeDamage(int dmg, bool stun){
        if (meleeTimer>0.001 || dashing) return;
        SFXPlayer.PlaySound("MIKU_HURT_BIG");
        if (shield>0) {ShieldUpdate(-dmg);}
        else {
            HealthUpdate(-dmg);
        }

        shieldRegenTimer = 16;
        meleeTimer = 0.3f;
        if (stun){stunTimer = 0.4f; stunned = true;}
    }

    public bool[] UnlockWeapon(int weapon){
        if (weapon==3) W3Locked = false;
        else if (weapon==4) W4Locked = false;
        else if (weapon==5) W5Locked = false;
        WeaponUpdate(weaponNum);

        bool[] array = new bool[3];
        if(W3Locked) array[0]=false; else array[0]=true;
        if(W4Locked) array[1]=false; else array[1]=true;
        if(W5Locked) array[2]=false; else array[2]=true;
        return array;
    }

    public void Heal(int hp){
        HealthUpdate(hp);
    }

    private float TimerF( float val){
        if(val>=0){
            val-=Time.deltaTime;
            if (val<0) val = 0;
        }
        return val;
    }

    void OnCollisionEnter2D(Collision2D c){
        if(c.gameObject.tag == "Enemy" && dashing){
            if(c.gameObject.TryGetComponent<IEnemy>(out IEnemy enemy)){
                enemy.MeleeDamage(dashDMG, true);
                GameObject expl = Instantiate(explosionPrefab, transform.position, Quaternion.Euler(new Vector3(0, 180, 0)));
                Destroy(expl, 2);
            }
        }
    }

    private IEnumerator LerpHealth(){
        float startHealth = hBar.value;
        lerpingHealth = true;
        float timeScale = 0;

        while(timeScale < 1){
            timeScale += Time.deltaTime * 2;
            hBar.value = Mathf.Lerp(startHealth, health, timeScale);
            yield return null;
        }
        if (health==0) Death();
        else lerpingHealth = false;
    }private IEnumerator LerpShield(){
        float startShield = sBar.value;
        lerpingShield = true;
        float timeScale = 0;

        while(timeScale < 1){
            if(shield == 0 && health < 20){
                timeScale += Time.deltaTime*2.5f;
            } else {
                timeScale += Time.deltaTime/3;
            }
            sBar.value = Mathf.Lerp(startShield, shield, timeScale);
            yield return null;
        }
        lerpingShield = false;
    }private IEnumerator LerpEnergy(){
        float startEnergy = eBar.value;
        lerpingEnergy = true;
        float timeScale = 0;

        while(timeScale < 1){
            timeScale += Time.deltaTime;
            eBar.value = Mathf.Lerp(startEnergy, energy, timeScale);
            yield return null;
        }
        lerpingEnergy = false;
    }


    public void SusEngine() {
        sussyBakaEngine = true;
    }

    public void Freeze() {frozen = true;}
    public void UnFreeze() {frozen = false;}

    public Vector2 Velocity { get {return velocity;} set{}}
    public Vector2 MousePos { get {return mousePos;} set{}}

    // Bullet Engine Interface for Boss 2
    public void Hit(int damage = 40) => Damage(damage, false);
    public bool CanBeHit => !dashing;
    public Unity.Mathematics.float2 Position => transform.position.xy();
    public float Radius => 0.5f; //TEMP
    public Transform Transform => this.transform;
}