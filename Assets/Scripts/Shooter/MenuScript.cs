using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuScript : MonoBehaviour
{
    public GameObject PlayPanel, TipScreen1, TipScreen2;
    public TextMeshProUGUI StartText;

    public void PlayButton(){
        PlayPanel.SetActive(true);
        StartCoroutine(FadeInText(StartText));
    }
    public void StartGame(){
        if(SaveData.SceneNum == -1){
            SceneManager.LoadSceneAsync("World2");
        } else {
            SceneManager.LoadSceneAsync(SaveData.SceneNum);
        }
    }
    private IEnumerator FadeInText(TextMeshProUGUI t){
        t.color = new Color(t.color.r, t.color.g, t.color.b, 0);
        while (t.color.a<1){
            t.color = new Color(t.color.r, t.color.g, t.color.b, t.color.a+2*Time.deltaTime);
            yield return null;
        }
    }

    public void Tip1Button(){
        TipScreen1.SetActive(true);
        TipScreen2.SetActive(false);

    }
    public void Tip2Button(){
        TipScreen1.SetActive(false);
        TipScreen2.SetActive(true);
    }
    public void ReturnMenu(){
        TipScreen1.SetActive(false);
        TipScreen2.SetActive(false);
    }
    public void QuitGame(){
        Application.Quit();
    }
}
