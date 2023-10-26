using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamShake : MonoBehaviour
{
    public void Shake(){
        StartCoroutine(ShakeCor(0.5f, 0.8f));
    }
    private IEnumerator ShakeCor(float d, float m){
        float timer = 0;
        while (timer<d){
            float offsetX = Random.Range(-0.5f,0.5f)*m;
            float offsetY = Random.Range(-0.5f,0.5f)*m;
            transform.localPosition = new Vector3(offsetX,offsetY,0);
            timer+=Time.deltaTime;
            yield return null;
        }
        transform.localPosition = transform.localPosition = new Vector3(0,0,0);
    }
}
