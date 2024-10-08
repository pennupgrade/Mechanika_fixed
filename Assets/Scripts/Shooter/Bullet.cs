using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour, IBullet
{
    public GameObject explosionPrefab;
    private bool friendly;
    private int damage;
    private float spd, duration, acc;
    private Rigidbody2D rb;
    private Vector2 velocity;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        friendly = gameObject.tag == "PlayerBullet";
    }

    // Update is called once per frame
    void Update()
    {
        duration -=Time.deltaTime;
        spd -= acc*Time.deltaTime; if (spd<4) spd = 4;
        if (duration<0) Destruction();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position+(Vector2)(Time.fixedDeltaTime*(spd*transform.up+(Vector3)velocity)));
    }

    void OnCollisionEnter2D(Collision2D c){
        if (c.gameObject.tag=="Environment"){
            Destruction();
        }else if (friendly){
            if (c.gameObject.TryGetComponent<IEnemy>(out IEnemy enemy)){
                enemy.Damage(damage, false);
                Destruction();
            }
        }else{
            if(c.gameObject.tag == "Player"){
                c.gameObject.GetComponent<MikuMechControl>().Damage(damage,false);
                Destruction();
            }
        }
    }

    private void Destruction(){
        if(explosionPrefab!=null){
            GameObject expl = Instantiate(explosionPrefab, transform.position, Quaternion.Euler(new Vector3(0, 180, 0)));
            Destroy(expl, 2);
        }
        Destroy(gameObject);
    }

    public void SetValues (int dmg, float speed, float timer, float accel, Vector2 v){
        damage = dmg;
        spd = speed;
        duration = timer;
        acc = accel;
        velocity = v;
    }

}