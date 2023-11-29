using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GM3Script : MonoBehaviour, IGameManager
{
    public TextMeshProUGUI DialogueName, DialogueText;
    public GameObject BlackPanel, DialogueBox, PausePanel;
    private bool dialogueCo;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SetPanelFalse());
        dialogueCo = false;
    }
    private IEnumerator SetPanelFalse(){
        Image img = BlackPanel.GetComponent<Image>();
        while (img.color.a>0){
            var temp = img.color.a;
            img.color = new Color(img.color.r, img.color.g, img.color.b, temp-Time.deltaTime);
            yield return null;
        }
        BlackPanel.SetActive(false);
    }

    public void Restart(bool death){
        if(death){
        SaveData.Deaths[SceneManager.GetActiveScene().buildIndex] += 1;
        StartCoroutine(RestartCor());
        } else SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }
    private IEnumerator RestartCor(){
        yield return new WaitForSeconds(2);
        Image img = BlackPanel.GetComponent<Image>();
        BlackPanel.SetActive(true);
        while (img.color.a<1){
            var temp = img.color.a;
            img.color = new Color(img.color.r, img.color.g, img.color.b, temp+Time.deltaTime);
            yield return null;
        }

        SceneManager.LoadSceneAsync("Restart");
    }
    public void PauseButton(){
        if(Time.timeScale > 0.9f){
            PausePanel.SetActive(true);
            Time.timeScale = 0;
        } else {
            PausePanel.SetActive(false);
            Time.timeScale = 1;
        }
    }
    public void RestartButton(){
        Time.timeScale = 1;
        PausePanel.SetActive(false);
        Restart(false);
    }
    public void MainMenu(){
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync("MainMenu");
    }
    public void WeaponDescription(int num){
        Dialogue("Description: " + SaveData.WeaponName[num], SaveData.WeaponDialogue[num]);
    }

    public void Dialogue(string n, string s){
        if(!dialogueCo) StartCoroutine(DialogueCor(n, s));
    }
    private IEnumerator DialogueCor(string n, string s){
        dialogueCo = true;
        StartCoroutine(FadeInGUI(DialogueBox.GetComponent<Image>(), DialogueBox));
        DialogueName.text = n; 
        DialogueText.text = s;
        StartCoroutine(FadeInText(DialogueText));
        yield return FadeInText(DialogueName);
        yield return new WaitForSeconds(6);
        StartCoroutine(FadeOutText(DialogueText));
        StartCoroutine(FadeOutGUI(DialogueBox.GetComponent<Image>(), DialogueBox));
        yield return FadeOutText(DialogueName);
        dialogueCo = false;
    }

    private IEnumerator FadeInText(TextMeshProUGUI t){
        t.color = new Color(t.color.r, t.color.g, t.color.b, 0);
        while (t.color.a<1){
            t.color = new Color(t.color.r, t.color.g, t.color.b, t.color.a+2*Time.deltaTime);
            yield return null;
        }
    }
    private IEnumerator FadeOutText(TextMeshProUGUI t){
        t.color = new Color(t.color.r, t.color.g, t.color.b, 1);
        while (t.color.a>0){
            t.color = new Color(t.color.r, t.color.g, t.color.b, t.color.a-2*Time.deltaTime);
            yield return null;
        }
    }
    private IEnumerator FadeInGUI(Image img, GameObject g){
        g.SetActive(true);
        while (img.color.a<1){
            var temp = img.color.a;
            img.color = new Color(img.color.r, img.color.g, img.color.b, temp+2*Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator FadeOutGUI(Image img, GameObject g){
        while (img.color.a>0){
            var temp = img.color.a;
            img.color = new Color(img.color.r, img.color.g, img.color.b, temp-2*Time.deltaTime);
            yield return null;
        }
        g.SetActive(false);
    }
}
