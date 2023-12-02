using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GM3Script : MonoBehaviour
{
    public GameObject DefaultEnemy, UpgradedEnemy, SpawnParticles;
    [SerializeField] private GameObject[] Room1SpwnPts;
    [SerializeField] private GameObject[] Room2SpwnPts;
    [SerializeField] private GameObject[] Room3SpwnPts;
    [SerializeField] private GameObject lines;
    [SerializeField] private GameObject doors;
    private int[][] enemyComp1 = {
        new int[] {0, 0},
        new int[] {0, 1},
        new int[] {1, 1},
        new int[] {0, 1, 1},
        new int[] {1, 3},
        new int[] {0, 3, 3}
    };
    private int[][] enemyComp2 = {
        new int[] {0, 1, 1, 3},
        new int[] {2, 2, 0},
        new int[] {1, 1, 2, 2},
        new int[] {2, 2, 2, 3},
        new int[] {5},
        new int[] {2, 5, 2},
        new int[] {1, 4}
    };
    private int[][] enemyComp3 = {
        new int[] {5, 4},
        new int[] {5, 5, 5},
        new int[] {6, 6},
        new int[] {4, 6, 6},
        new int[] {5, 5, 4, 6},
        new int[] {4, 4, 5, 6, 6}
    };
    private int roomNum, waveNum;
    private AudioSource AS;
    private bool isSpawning;


    // Start is called before the first frame update
    void Start()
    {
        roomNum = 0; waveNum = 0;
        AS = GetComponent<AudioSource>();
        isSpawning = false;
    }

    public void lockDown (int room) {
        roomNum = room; waveNum = 0;
        lines.SetActive(false);
        doors.SetActive(true);
        AS.Play();
        StartCoroutine(SpawningCor());
    }

    private void roomCleared () {
        lines.SetActive(true);
        doors.SetActive(false);
        StartCoroutine(FadeOut(AS, 2));
    }
    public void ReportDeath () {
        SaveData.W3EnemyNum--;
        if (SaveData.W3EnemyNum == 0) {
            if ((roomNum == 1 && waveNum >= enemyComp1.Length) 
                || (roomNum == 2 && waveNum >= enemyComp2.Length)
                || (roomNum == 3 && waveNum >= enemyComp3.Length)
            ) {
                StopAllCoroutines();
                roomCleared();
                isSpawning = false;
            } else {
                if(!isSpawning){
                    StartCoroutine(SpawningCor());
                }
            }
        }
    }
    private IEnumerator SpawningCor() {
        isSpawning = true;
        int wNum = waveNum;
        waveNum++;
        int[][] enemyComp;
        if(roomNum == 1) {
            enemyComp = enemyComp1;
        } else if (roomNum == 2) {
            enemyComp = enemyComp2;
        } else {
            enemyComp = enemyComp3;
        }
        yield return new WaitForSeconds(4);
        Spawn(enemyComp[wNum][0]);
        for (int i = 1; i < enemyComp[wNum].Length; i++) {
            yield return new WaitForSeconds(3);
            Spawn(enemyComp[wNum][i]);
        }
        isSpawning = false;
    }

    private void Spawn (int type) {
        Vector3 spawnPos;
        if (roomNum == 1) {
            spawnPos = Room1SpwnPts[Random.Range(0, Room1SpwnPts.Length)].transform.position;
        } else if (roomNum == 2) {
            spawnPos = Room2SpwnPts[Random.Range(0, Room2SpwnPts.Length)].transform.position;
        } else {
            spawnPos = Room3SpwnPts[Random.Range(0, Room3SpwnPts.Length)].transform.position;
        }
        
        GameObject newEnemy;
        if (type == 0) {
            newEnemy = Instantiate (DefaultEnemy, spawnPos, Quaternion.identity);
            newEnemy.GetComponent<DefaultEnemy3AI>().SetState(0);
        } else if (type == 1) {
            newEnemy = Instantiate (DefaultEnemy, spawnPos, Quaternion.identity);
            newEnemy.GetComponent<DefaultEnemy3AI>().SetState(1);
        } else if (type == 2) {
            newEnemy = Instantiate (DefaultEnemy, spawnPos, Quaternion.identity);
            newEnemy.GetComponent<DefaultEnemy3AI>().SetState(2);
        } else if (type == 3) {
            newEnemy = Instantiate (DefaultEnemy, spawnPos, Quaternion.identity);
            newEnemy.GetComponent<DefaultEnemy3AI>().SetState(3);
        } else if (type == 4) {
            newEnemy = Instantiate (UpgradedEnemy, spawnPos, Quaternion.identity);
            newEnemy.GetComponent<UpgradedEnemy3AI>().SetState(1);
        } else if (type == 5) {
            newEnemy = Instantiate (UpgradedEnemy, spawnPos, Quaternion.identity);
            newEnemy.GetComponent<UpgradedEnemy3AI>().SetState(2);
        } else {
            newEnemy = Instantiate (UpgradedEnemy, spawnPos, Quaternion.identity);
            newEnemy.GetComponent<UpgradedEnemy3AI>().SetState(3);
        }
        GameObject ptcl = Instantiate (SpawnParticles, spawnPos, Quaternion.Euler(new Vector3(0, 180, 0)));
        Destroy(ptcl,5);
        newEnemy.transform.SetParent(gameObject.transform);
        SaveData.W3EnemyNum++;
    }

    private IEnumerator FadeOut (AudioSource audioSource, float FadeTime) {
        float startVolume = audioSource.volume;
 
        while (audioSource.volume > 0) {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;
 
            yield return null;
        }
 
        audioSource.Stop ();
        audioSource.volume = startVolume;
    }
    
}
