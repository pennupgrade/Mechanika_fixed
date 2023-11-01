using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RestartScript : MonoBehaviour
{
    public GameObject BlackPanel;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FadeInPanel());
    }
    public void RestartLevel(){
        CanvasGroup CG = BlackPanel.GetComponent<CanvasGroup>();
        CG.alpha = 0;
        SceneManager.LoadSceneAsync(SaveData.SceneNum);
    }
    public void MainMenu(){
        CanvasGroup CG = BlackPanel.GetComponent<CanvasGroup>();
        CG.alpha = 0;
        SceneManager.LoadSceneAsync("MainMenu");
    }
    private IEnumerator FadeInPanel(){
        CanvasGroup CG = BlackPanel.GetComponent<CanvasGroup>();
        while (CG.alpha<1){
            CG.alpha+=Time.deltaTime/2;
            yield return null;
        }
    }
}
