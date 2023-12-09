using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Boss3GMScript : MonoBehaviour, IGameManager
{
    public TextMeshProUGUI DialogueName, DialogueText, MissionComplete;
    public TextMeshProUGUI CountDown;
    public GameObject BlackPanel, PausePanel;
    public GameObject Player;
    public GameObject heal;
    private float songPosition;
    public Boss3AI Boss;
    public float songPosInBeats;
    private float secPerBeat, dsptimesong, bpm;
    private bool started, hpDialogue, mikuSong;
    private bool[] commands;
    private float[] beats = {};
    private int nextIndex, minLeft, secLeft;
    private AudioSource AS;

    void Start(){
        SaveData.SceneNum = SceneManager.GetActiveScene().buildIndex;
        Player = GameObject.FindWithTag("Player");
        StartCoroutine(SetPanelFalse());
        started = false; hpDialogue = false; mikuSong = false;
        bpm = 85;
        secPerBeat = 60f / bpm;
        AS = GetComponent<AudioSource>();
        commands = new bool[24];
        nextIndex = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(!started) return;
        songPosition = (float) (AudioSettings.dspTime - dsptimesong);
        songPosInBeats = songPosition / secPerBeat;
        minLeft = (299-(int)songPosition)/60; if(minLeft<0) minLeft=0;
        secLeft = (299-(int)songPosition)%60; if(secLeft<0) secLeft=0;
        CountDown.text="Time Remaining:\n" + minLeft.ToString().PadLeft(1,'0')+":"+secLeft.ToString().PadLeft(2,'0');

        Commander();
        if (nextIndex < beats.Length && beats[nextIndex] < songPosInBeats){
            

            nextIndex++;
        }

        if(Boss.health==0&&!hpDialogue){
            Dialogue("Venge", "Core overloaded, huh. Reconfiguring energy circulation. Now this unit will last just... a bit... longer...");
            hpDialogue=true;
        }
    }

    public void StartFight(){
        started = true;
        dsptimesong = (float) AudioSettings.dspTime;
        AS.Play();
        CountDown.gameObject.SetActive(true);
    }

    private void SpawnHeal(){
        for(int i=0;i<1;i++){
            int v = Random.Range(0,2);
            Instantiate(heal, transform.GetChild(v).position, Quaternion.identity);
        }
    }

    private void Commander(){
        
    }

    private IEnumerator Win(){
        yield return new WaitForSeconds(2);
        //go to VN
        yield return StartCoroutine(SetPanelTrue());
        yield return new WaitForSeconds(2);
        yield return StartCoroutine(FadeInText(MissionComplete));
        SaveData.SceneNum = 4; // change to end screen later
        yield return new WaitForSeconds(5);
        SceneManager.LoadSceneAsync("MainMenu");
    }

    private IEnumerator SetPanelFalse(){
        if(SaveData.Weapons[0]) Player.GetComponent<MikuMechControl>().UnlockWeapon(3);
        if(SaveData.Weapons[1]) Player.GetComponent<MikuMechControl>().UnlockWeapon(4);
        if(SaveData.Weapons[2]) Player.GetComponent<MikuMechControl>().UnlockWeapon(5);

        Image img = BlackPanel.GetComponent<Image>();
        while (img.color.a>0){
            var temp = img.color.a;
            img.color = new Color(img.color.r, img.color.g, img.color.b, temp-Time.deltaTime);
            yield return null;
        }
        BlackPanel.SetActive(false);
    }
    private IEnumerator SetPanelTrue() {
        BlackPanel.SetActive(true);
        Image img = BlackPanel.GetComponent<Image>();
        while (img.color.a<1){
            var temp = img.color.a;
            img.color = new Color(img.color.r, img.color.g, img.color.b, temp+0.5f*Time.deltaTime);
            yield return null;
        }
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
            if (mikuSong) AudioListener.pause = true;
        } else {
            PausePanel.SetActive(false);
            Time.timeScale = 1;
            if (mikuSong) AudioListener.pause = false;
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

    public void Dialogue(string n, string s){
        StartCoroutine(DialogueCor(n, s));
    }
    private IEnumerator DialogueCor(string n, string s){
        DialogueName.gameObject.SetActive(true);
        DialogueText.gameObject.SetActive(true);
        DialogueName.text = n; 
        StartCoroutine(Typewriter(s));
        StartCoroutine(FadeInText(DialogueText));
        yield return FadeInText(DialogueName);
        yield return new WaitForSeconds(6.5f);
        StartCoroutine(FadeOutText(DialogueText));
        yield return FadeOutText(DialogueName);
        DialogueName.gameObject.SetActive(false);
        DialogueText.gameObject.SetActive(false);
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
    private IEnumerator Typewriter(string s){
        for(int i = 0; i<s.Length; i++){
            DialogueText.text = s.Substring(0,i+1);
            yield return new WaitForSeconds(0.02f);
        }
    }/* 
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
    } */

}