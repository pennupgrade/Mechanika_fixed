using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GM3Script : MonoBehaviour
{
    public GameObject DefaultEnemy, UpgradedEnemy;
    [SerializeField] private GameObject[] Room1SpwnPts;
    [SerializeField] private GameObject[] Room2SpwnPts;
    [SerializeField] private GameObject[] Room3SpwnPts;
    [SerializeField] private GameObject lines;
    [SerializeField] private GameObject doors;
    private int[][] enemyComp1 = {
        new int[] {0, 0, 1, 1},
        new int[] {0, 0, 1, 1, 3},
        new int[] {0, 1, 1, 3, 3}
    };
    private int[][] enemyComp2 = {
        new int[] {0, 1, 3, 3, 3},
        new int[] {0, 0, 1, 1, 2, 2},
        new int[] {2, 2, 2, 1, 1, 3},
        new int[] {2, 1, 1, 4, 4}
    };
    private int[][] enemyComp3 = {
        new int[] {2, 2, 2, 4, 4, 4},
        new int[] {2, 2, 3, 5, 5},
        new int[] {5, 5, 5, 5, 6, 6},
        new int[] {5, 5, 4, 4, 6, 6},
        new int[] {4, 4, 5, 5, 5, 6, 6, 6}
    };
    private int roomNum, waveNum;


    // Start is called before the first frame update
    void Start()
    {
        roomNum = 0; waveNum = 0;
    }

    public void lockDown (int room) {
        roomNum = room; waveNum = 0;
        lines.SetActive(false);
        doors.SetActive(true);
        StartCoroutine(SpawningCor());
    }

    private void roomCleared () {
        lines.SetActive(true);
        doors.SetActive(false);
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
            } else {
               StartCoroutine(SpawningCor()); 
            }
        }
    }
    private IEnumerator SpawningCor() {
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
            Spawn(enemyComp[wNum][i]);
            yield return new WaitForSeconds(3);
        }
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

        } else if (type == 1) {

        } else if (type == 2) {
            
        } else if (type == 3) {
            
        } else if (type == 4) {
            
        } else if (type == 5) {
            
        } else {

        }
        //newEnemy.transform.SetParent(gameObject.transform);
        SaveData.W3EnemyNum++;
    }

    
}
