using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCenter : MonoBehaviour
{
    public GameObject bulletPrefab;
    private GameObject[] bullets;
    private GameObject player;
    private float timer;
    private bool isDone;
    private Vector3 spawnPos;
    // Start is called before the first frame update
    
    void Start(){
        spawnPos = transform.position;
        timer = -0.8f;
        isDone = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(player==null) return;
        timer += Time.deltaTime;
        if(timer>0 && timer <= 1){
            transform.position = Vector3.Lerp(spawnPos, player.transform.position, timer);
        } else if (!isDone && timer >1){
            StartCoroutine(Stuff());
            isDone = true;
        }
        
    }
    private IEnumerator Stuff(){
        foreach (var b in bullets)
        {
            b.GetComponent<CircleBullet>().Ready();
            yield return null;
        }
        Destroy(gameObject, 5);
    }

    public void initFields(int numBullets, GameObject player){
        this.player = player;
        bullets = new GameObject[numBullets];
        for (int i = 0; i<numBullets; i++){
            bullets[i] = Instantiate (bulletPrefab, transform.position, Quaternion.identity);
            bullets[i].GetComponent<CircleBullet>().SetSpeed(6);
            bullets[i].GetComponent<CircleBullet>().SetTarget(transform.position+5*(Vector3)(Random.insideUnitCircle.normalized));
            bullets[i].transform.SetParent(gameObject.transform);
        }
    }
}
