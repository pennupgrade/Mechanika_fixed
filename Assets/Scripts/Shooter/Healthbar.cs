using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    public Slider s;
    public Vector3 Offset;
    // Start is called before the first frame update
    public void SetHealth(int health, int maxHealth){
        s.gameObject.SetActive(health<maxHealth);
        s.value = health;
        s.maxValue = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        s.transform.position = Camera.main.WorldToScreenPoint(transform.parent.position+Offset);
    }
}
