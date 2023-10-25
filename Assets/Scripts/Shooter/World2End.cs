using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class World2End : MonoBehaviour
{
    public GameObject BlackPanel;
    void OnTriggerEnter2D(Collider2D c){
        if(c.gameObject.tag == "Player"){
            bool[] weapons = c.gameObject.GetComponent<MikuMechControl>().UnlockWeapon(0);
            for(int i =0; i<3; i++){
                SaveData.Weapons[i]=weapons[i];
            }
            StartCoroutine(LoadNewScene());
        }
    }

    private IEnumerator LoadNewScene(){
        Image img = BlackPanel.GetComponent<Image>();
        BlackPanel.SetActive(true);
        while (img.color.a<1){
            var temp = img.color.a;
            img.color = new Color(img.color.r, img.color.g, img.color.b, temp+Time.deltaTime);
            yield return null;
        }
        SceneManager.LoadSceneAsync("World2Boss");
    }
}
