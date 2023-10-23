using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MikuMechControl : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject CepheidPrefab;
    public GameObject DISCPrefab;
    public GameObject SenbonzakuraPrefab;
    public GameObject NOVAPrefab;
    public GameObject MeteorPrefab;
    private float moveSpeed=7.5f, mspeed;
    [Header("Player Values")]
    [SerializeField] private int health, maxShield, shield, energy, weaponNum;
    private float shieldRegenTimer, meleeTimer, weaponCDTimer, chargeTimer;
    private bool shieldRegen, W3Locked, W4Locked, W5Locked;
    private float stunTimer;

    [SerializeField] private bool stunned, dashing;
    private int knockback, dashDMG = 250, dashEnergy = 20; 
    private float dashTimer, dashCDTimer; private float dashCD = 0.7f;

    private int w1DMG = 25, w1Energy = 3, cepheidMode = 1; private float w1CD = 0.2f;
    private int w2DMG = 56, w2Energy = 22; private float w2CD = 0.7f;
    private int w3DMG = 20, w3Energy = 24; private float w3CD = 0.4f;
    private int w4DMG = 200, w4Energy = 22; private float w4CD = 0.1f;
    private int w5DMG = 240, w5Energy = 30; private float w5CD = 6;
    [Header("Cam")]
    public Camera cam;
    private Rigidbody2D rb;
    private Vector2 movement, mousePos, lookDir;
    private Vector2 velocity;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mspeed = moveSpeed; weaponNum = 1; cepheidMode = 1;
        energy = 100; health = 390; maxShield = 400; shield = 0;
        shieldRegen = false; dashing = false; knockback = 0;
        shieldRegenTimer = 0; weaponCDTimer = 0; dashCDTimer = 0;
        dashTimer = 0; chargeTimer = 0; meleeTimer = 0;
        stunTimer = 0; stunned = false; W3Locked = true; W4Locked = true; W5Locked = true;
        StartCoroutine(EnergyRegen());
    }

    // Update is called once per frame
    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        //shield regeneration
        if (!shieldRegen && shield!=maxShield && shieldRegenTimer<0.001) StartCoroutine(Regenerator());

        //dash
        if((Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.LeftShift)) && dashCDTimer<0.01f && energy-dashEnergy >= 0 &&(movement.x!=0||movement.y!=0)){
            dashCDTimer=dashCD; dashing = true; dashTimer = 0.25f; energy-=dashEnergy;
        }
        if (dashing&&dashTimer<0.001f) dashing = false;

        //weapon select
        if (Input.GetKeyDown(KeyCode.Alpha1)) {weaponNum = 1; chargeTimer=0;}
        if (Input.GetKeyDown(KeyCode.Alpha2)) {weaponNum = 2; chargeTimer=0;}
        if (Input.GetKeyDown(KeyCode.Alpha3)&&!W3Locked) {weaponNum = 3; chargeTimer=0;}
        if (Input.GetKeyDown(KeyCode.Alpha4)&&!W4Locked) {weaponNum = 4; chargeTimer=0;}
        if (Input.GetKeyDown(KeyCode.Alpha5)&&!W5Locked) {weaponNum = 5; chargeTimer=0;}

        //weapon fire
        if (weaponNum==1){
            if (Input.GetKey(KeyCode.Mouse0) && weaponCDTimer<0.001 && energy-w1Energy >= 0){
                weaponCDTimer=w1CD; energy-=w1Energy;
                FireCepheid();
            }
        }else if (weaponNum==2){
            if (Input.GetKey(KeyCode.Mouse0) && weaponCDTimer<0.001 && energy-w2Energy >= 0){
                weaponCDTimer=w2CD; energy-=w2Energy;
                FireDISC();
            }
        }else if (weaponNum==3){
            if (Input.GetKey(KeyCode.Mouse0) && weaponCDTimer<0.001){
                chargeTimer+=Time.deltaTime;
                if (chargeTimer>1) chargeTimer=1;
            } else if (Input.GetKeyUp(KeyCode.Mouse0) && weaponCDTimer<0.001 && energy-w3Energy >= 0){
                weaponCDTimer=w3CD; energy-=w3Energy;
                FireSenbonzakura(chargeTimer); chargeTimer = 0;
            }
            else chargeTimer = 0;
        }else if (weaponNum==4){
            if (Input.GetKey(KeyCode.Mouse0) && weaponCDTimer<0.001){
                moveSpeed = mspeed/2;
                chargeTimer+=Time.deltaTime;
                if (chargeTimer>4) chargeTimer=4;
                if(w4Energy*chargeTimer>energy){
                    weaponCDTimer=1; energy=0;
                    FireNOVA(chargeTimer); chargeTimer = 0; moveSpeed=mspeed;
                }
            } else if (Input.GetKeyUp(KeyCode.Mouse0) && weaponCDTimer<0.001){
                weaponCDTimer=w4CD; energy-=(int)(w4Energy*chargeTimer);
                FireNOVA(chargeTimer); chargeTimer = 0; moveSpeed=mspeed;
            }
            else {chargeTimer = 0; moveSpeed=mspeed;}
        }else if(weaponNum==5){
            if (Input.GetKey(KeyCode.Mouse0) && weaponCDTimer<0.001 && energy-w5Energy >= 0){
                weaponCDTimer=w5CD;
                StartCoroutine(FireMeteor());
            }
        }

        //update timers
        weaponCDTimer = TimerF(weaponCDTimer); dashCDTimer = TimerF(dashCDTimer); dashTimer = TimerF(dashTimer);
        shieldRegenTimer = TimerF(shieldRegenTimer); stunTimer = TimerF(stunTimer); meleeTimer = TimerF(meleeTimer);
    }

    void FixedUpdate(){
        lookDir = mousePos - rb.position;
        //stunned
        if (stunned){
            moveSpeed = mspeed*0.5f;
            if (stunTimer<0.001f) {moveSpeed = mspeed+3*((400-health)/400.0f); stunned = false;}
        }
        velocity = moveSpeed*movement.normalized;
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
        GameObject bullet = Instantiate (CepheidPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<IBullet>().SetValues (w1DMG+(int)(w1DMG*((100.0f - energy)/100)), 5+(4*(100.0f - energy)/100), 1.5f, -4, velocity);
        bullet.GetComponent<CepheidBulletScript>().SetMode(cepheidMode);
        cepheidMode++;
        if (cepheidMode == 5) cepheidMode = 1;
        var a = 1;
        if(lookDir.y<0) a = -1;
        bullet.transform.eulerAngles = (a*Vector2.Angle(new Vector2(1,0), lookDir)-90+6*(Random.value-0.5f))* Vector3.forward;
    }
    private void FireDISC(){
        GameObject bullet = Instantiate (DISCPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<IBullet>().SetValues (w2DMG, 7, 4, -1.5f, velocity);
        var a = 1;
        if(lookDir.y<0) a = -1;
        bullet.transform.eulerAngles = (a*Vector2.Angle(new Vector2(1,0), lookDir)-90+6*(Random.value-0.5f))* Vector3.forward;
    }
    private void FireSenbonzakura(float charge){
        var spread = 45-(30*charge);
        var range = 0.4f+0.3f*charge;
        for (int i = 0; i<15;i++){
            GameObject bullet = Instantiate (SenbonzakuraPrefab, transform.position, Quaternion.identity);
            bullet.GetComponent<IBullet>().SetValues (w3DMG, 10+5*charge+3*Random.value, range+0.2f*Random.value, 8-charge, 0.5f*velocity);
            var a = 1;
            if(lookDir.y<0) a = -1;
            bullet.transform.eulerAngles = (a*Vector2.Angle(new Vector2(1,0), lookDir)-90+spread*(Random.value-0.5f))* Vector3.forward;
        }
        knockback = 6;
    }
    private void FireNOVA(float charge){
        if (charge<0.3f) return;
        GameObject bullet = Instantiate (NOVAPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<IBullet>().SetValues ((int)(w4DMG*charge), 5+0.4f*charge, 1.7f, -5, velocity);
        var a = 1;
        if(lookDir.y<0) a = -1;
        bullet.transform.eulerAngles = (a*Vector2.Angle(new Vector2(1,0), lookDir)-90)* Vector3.forward;
    }
    private IEnumerator FireMeteor(){
        var count = 0;
        var d = lookDir;
        while (energy>=w5Energy) {energy-=w5Energy; count++;}
        for(int i = 0; i<count; i++){
            GameObject bullet = Instantiate (MeteorPrefab, transform.position, Quaternion.identity);
            bullet.GetComponent<MeteorMissileScript>().SetValues (w5DMG, 3, 2, 9, 5, 100, gameObject);
            var a = 1;
            if(d.y<0) a = -1;
            bullet.transform.eulerAngles = (a*Vector2.Angle(new Vector2(1,0), d)-140+10*i)* Vector3.forward;
            yield return new WaitForSeconds(.12f);
        }
    }

    private IEnumerator Regenerator(){
        shieldRegen = true;
        while(shield<maxShield){
            if (shieldRegenTimer>0.01f) {shieldRegen=false; yield break;}
            shield += 10;
            yield return new WaitForSeconds(.2f);
        }
        if (shield>maxShield) shield = maxShield;
        shieldRegen = false;
    }

    private IEnumerator EnergyRegen(){
        while (true){
            if(energy<100){
                energy += 5;
                if (energy>100) energy = 100;
            }
            yield return new WaitForSeconds(.45f);
        }
    }

    private void Death(){
        Debug.Log("death");
        // game over / retry screen
    }

    public void Damage(int dmg, bool stun){
        if (dashing) dmg = (int)(0.25f*dmg);
        if (shield>0) {shield -= dmg; if (shield<0) shield = 0;}
        else {
            health -= dmg; moveSpeed = 6+2*((400-health)/400.0f);
        }

        if(health<1) Death();
        shieldRegenTimer = 18;
        if (stun){stunTimer = 0.5f; stunned = true;}
    }

    public void MeleeDamage(int dmg, bool stun){
        if (meleeTimer>0.001) return;
        if (dashing) dmg = (int)(0.25f*dmg);
        if (shield>0) {shield -= dmg; if (shield<0) shield = 0;}
        else {
            health -= dmg; moveSpeed = mspeed+3*((400-health)/400.0f);
        }

        if(health<1) Death();
        shieldRegenTimer = 18;
        meleeTimer = 0.5f;
        if (stun){stunTimer = 0.5f; stunned = true;}
    }

    public int UnlockWeapon(int weapon){
        if (weapon==3) W3Locked = false;
        else if (weapon==4) W4Locked = false;
        else if (weapon==5) W5Locked = false;

        if(W4Locked&&!W3Locked) return 3;
        else if(!W4Locked&&W3Locked) return 4;
        else if(!W4Locked&&!W3Locked) return 5;
        else return 2;
    }

    public void Heal(int hp){
        health+=hp;
        if (health>390) {health = 390; moveSpeed=10;}
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
                enemy.Damage(dashDMG, true);
            }
        }
    }

    public Vector2 Velocity { get {return velocity;} set{}}
    public Vector2 MousePos { get {return mousePos;} set{}}
}