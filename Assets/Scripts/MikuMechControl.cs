using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MikuMechControl : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject CepheidPrefab;
    public GameObject DISCPrefab;
    public GameObject BloomPrefab;
    public GameObject NOVAPrefab;
    private float moveSpeed=6, mspeed;
    [Header("Player Values")]
    [SerializeField] private int health, maxShield, shield, energy, weaponNum;
    private float shieldRegenTimer, meleeTimer, weaponCDTimer, chargeTimer;
    private bool shieldRegen, stunned;
    private float stunTimer;

    private bool dashing;
    private int dashDMG = 300, dashEnergy = 20; 
    private float dashTimer, dashCDTimer; private float dashCD = 0.7f;

    private int w1DMG = 25, w1Energy = 3, cepheidMode = 1; private float w1CD = 0.2f;
    private int w2DMG = 80, w2Energy = 18; private float w2CD = 0.7f;
    private int w3DMG = 30, w3Energy = 24; private float w3CD = 0.4f;
    private int w4DMG = 266, w4Energy = 27; private float w4CD = 0.1f;
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
        energy = 100; health = 400; maxShield = 600; shield = 0;
        shieldRegen = false; dashing = false;
        shieldRegenTimer = 0; weaponCDTimer = 0; dashCDTimer = 0;
        dashTimer = 0; chargeTimer = 0; meleeTimer = 0;
        stunTimer = 0; stunned = false;
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
        //stunned
        if (stunned && stunTimer>0.001f){
            moveSpeed = mspeed*0.5f;
            if (stunTimer<0.01f) {moveSpeed = mspeed; stunned = false;}
        }

        //dash
        if(Input.GetKeyDown(KeyCode.Space) && dashCDTimer<0.01f && energy-dashEnergy >= 0 &&(movement.x!=0||movement.y!=0)){
            dashCDTimer=dashCD; dashing = true; dashTimer = 0.2f; energy-=dashEnergy;
        }
        if (dashing&&dashTimer<0.001f) dashing = false;

        //weapon select
        if (Input.GetKeyDown(KeyCode.Alpha1)) {weaponNum = 1; chargeTimer=0;}
        if (Input.GetKeyDown(KeyCode.Alpha2)) {weaponNum = 2; chargeTimer=0;}
        if (Input.GetKeyDown(KeyCode.Alpha3)) {weaponNum = 3; chargeTimer=0;}
        if (Input.GetKeyDown(KeyCode.Alpha4)) {weaponNum = 4; chargeTimer=0;}
        if (Input.GetKeyDown(KeyCode.Q)) {weaponNum--; if(weaponNum==0) weaponNum=4; chargeTimer=0;}
        if (Input.GetKeyDown(KeyCode.E)) {weaponNum++; if(weaponNum==5) weaponNum=1; chargeTimer=0;}

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
                FireBloom(chargeTimer); chargeTimer = 0;
            }
            else chargeTimer = 0;
        }else if (weaponNum==4){
            if (Input.GetKey(KeyCode.Mouse0) && weaponCDTimer<0.001){
                moveSpeed = mspeed/2;
                chargeTimer+=Time.deltaTime;
                if (chargeTimer>3) chargeTimer=3;
                if(w4Energy*chargeTimer>energy){
                    weaponCDTimer=1; energy=0;
                    FireNOVA(chargeTimer); chargeTimer = 0; moveSpeed=mspeed;
                }
            } else if (Input.GetKeyUp(KeyCode.Mouse0) && weaponCDTimer<0.001){
                weaponCDTimer=w4CD; energy-=(int)(w4Energy*chargeTimer);
                FireNOVA(chargeTimer); chargeTimer = 0; moveSpeed=mspeed;
            }
            else {chargeTimer = 0; moveSpeed=mspeed;}
        }

        //update timers
        weaponCDTimer = TimerF(weaponCDTimer); dashCDTimer = TimerF(dashCDTimer); dashTimer = TimerF(dashTimer);
        shieldRegenTimer = TimerF(shieldRegenTimer); stunTimer = TimerF(stunTimer); meleeTimer = TimerF(meleeTimer);
    }

    void FixedUpdate(){
        lookDir = mousePos - rb.position;
        if(!dashing){
            velocity = moveSpeed*Time.fixedDeltaTime*movement.normalized;
            rb.MovePosition(rb.position + velocity);
        }else{
            rb.MovePosition(rb.position + 16*Time.fixedDeltaTime*movement.normalized);
        }
    }

    private void FireCepheid(){
        GameObject bullet = Instantiate (CepheidPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<IBullet>().SetValues (w1DMG+(int)(w1DMG*((100.0f - energy)/100)), 7+(4*(100.0f - energy)/100), 1.4f, 0);
        bullet.GetComponent<CepheidBulletScript>().SetMode(cepheidMode);
        cepheidMode++;
        if (cepheidMode == 5) cepheidMode = 1;
        var a = 1;
        if(lookDir.y<0) a = -1;
        bullet.transform.eulerAngles += (a*Vector2.Angle(new Vector2(1,0), lookDir)-90+6*(Random.value-0.5f))* Vector3.forward;
    }
    private void FireDISC(){
        GameObject bullet = Instantiate (DISCPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<IBullet>().SetValues (w2DMG, 7, 4, -1.5f);
        var a = 1;
        if(lookDir.y<0) a = -1;
        bullet.transform.eulerAngles += (a*Vector2.Angle(new Vector2(1,0), lookDir)-90+6*(Random.value-0.5f))* Vector3.forward;
    }
    private void FireBloom(float charge){
        var spread = 45-(30*charge);
        var range = 0.4f+0.3f*charge;
        for (int i = 0; i<15;i++){
            GameObject bullet = Instantiate (BloomPrefab, transform.position, Quaternion.identity);
            bullet.GetComponent<IBullet>().SetValues (w3DMG+(int)(charge*10), 9+4*charge+3*Random.value, range+0.2f*Random.value, 8-charge);
            var a = 1;
            if(lookDir.y<0) a = -1;
            bullet.transform.eulerAngles += (a*Vector2.Angle(new Vector2(1,0), lookDir)-90+spread*(Random.value-0.5f))* Vector3.forward;
        }
    }
    private void FireNOVA(float charge){
        if (charge<0.3f) return;
        GameObject bullet = Instantiate (NOVAPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<IBullet>().SetValues ((int)(w4DMG*charge), 5+0.5f*charge, 1.7f, -5);
        var a = 1;
        if(lookDir.y<0) a = -1;
        bullet.transform.eulerAngles += (a*Vector2.Angle(new Vector2(1,0), lookDir)-90+3*(Random.value-0.5f))* Vector3.forward;
    }

    private IEnumerator Regenerator(){
        shieldRegen = true;
        while(shield<maxShield){
            if (shieldRegenTimer>0.01f) {shieldRegen=false; yield break;}
            shield += 10;
            yield return new WaitForSeconds(.2f);
        }
        if (shield>maxShield) shield = maxShield;
    }

    private IEnumerator EnergyRegen(){
        while (true){
            if(energy<100){
                energy += 5;
                if (energy>100) energy = 100;
            }
            yield return new WaitForSeconds(.5f);
        }
    }

    private void Death(){
        // game over / retry screen
    }

    public void Damage(int dmg, bool stun){
        if (shield>0) {shield -= dmg; if (shield<0) shield = 0;}
        else {
            health -= dmg; moveSpeed = 6+2*((400-health)/400.0f);
        }

        if(health<0) Death();
        shieldRegenTimer = 24;
        if (stun){stunTimer += 0.5f; stunned = true;}
    }

    public void MeleeDamage(int dmg, bool stun){
        if (meleeTimer>0.001) return;
        if (shield>0) {shield -= dmg; if (shield<0) shield = 0;}
        else {
            health -= dmg; moveSpeed = 6+2*((400-health)/400.0f);
        }

        if(health<0) Death();
        shieldRegenTimer = 24;
        meleeTimer = 0.5f;
        if (stun){stunTimer += 0.5f; stunned = true;}
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
}