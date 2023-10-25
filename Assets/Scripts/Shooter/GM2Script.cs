using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GM2Script : MonoBehaviour, IGameManager
{
    public GameObject BlackPanel;

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
        SceneManager.LoadSceneAsync("World2");
    }


}
