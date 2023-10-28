using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GM2Script : MonoBehaviour, IGameManager
{
    public TextMeshProUGUI DialogueName, DialogueText;
    public GameObject BlackPanel, DialogueBox;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SetPanelFalse());
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

    public void Restart(){
        StartCoroutine(RestartCor());
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

        SaveData.SceneNum = SceneManager.GetActiveScene().buildIndex;
        //replace with restart screen
        SceneManager.LoadSceneAsync("World2");
    }

    public void Dialogue(string n, string s){
        StartCoroutine(DialogueCor(n, s));
    }
    private IEnumerator DialogueCor(string n, string s){
        StartCoroutine(FadeInGUI(DialogueBox.GetComponent<Image>(), DialogueBox));
        DialogueName.text = n; 
        DialogueText.text = s;
        StartCoroutine(FadeInText(DialogueText));
        yield return FadeInText(DialogueName);
        yield return new WaitForSeconds(6);
        StartCoroutine(FadeOutText(DialogueText));
        StartCoroutine(FadeOutGUI(DialogueBox.GetComponent<Image>(), DialogueBox));
        yield return FadeOutText(DialogueName);
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
