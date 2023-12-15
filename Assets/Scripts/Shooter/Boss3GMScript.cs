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
    private bool started, hpDialogue, dialoguePlaying;
    private bool[] commands;
    private (float X, int Y) [] rAttacks;
    private (float X, int Y) [] bAttacks;
    private int nextIndexR, nextIndexB, minLeft, secLeft;
    private AudioSource AS;

    void Start(){
        SaveData.SceneNum = SceneManager.GetActiveScene().buildIndex;
        Player = GameObject.FindWithTag("Player");
        StartCoroutine(SetPanelFalse());
        started = false; hpDialogue = false; dialoguePlaying = false;
        bpm = 85;
        secPerBeat = 60f / bpm;
        AS = GetComponent<AudioSource>();
        rAttacks = SaveData.rAttacks3;
        bAttacks = SaveData.bAttacks3;
        commands = new bool[9];
        nextIndexB = 0; nextIndexR = 0; 
    }

    // Update is called once per frame
    void Update()
    {
        if(!started) return;
        songPosition = (float) (AudioSettings.dspTime - dsptimesong);
        songPosInBeats = songPosition / secPerBeat;
        minLeft = (293-(int)songPosition)/60; if(minLeft<0) minLeft=0;
        secLeft = (293-(int)songPosition)%60; if(secLeft<0) secLeft=0;
        CountDown.text="Time Remaining:\n" + minLeft.ToString().PadLeft(1,'0')+":"+secLeft.ToString().PadLeft(2,'0');

        Commander();

        if (nextIndexR < rAttacks.Length && rAttacks[nextIndexR].Item1 < songPosInBeats){
            Boss.SetAttack(rAttacks[nextIndexR].Item2);
            nextIndexR++;
        }
        if (nextIndexB < bAttacks.Length && bAttacks[nextIndexB].Item1 < songPosInBeats){
            Boss.SetBulletEngine(bAttacks[nextIndexB].Item2);
            nextIndexB++;
        }

        if(Boss.health==0&&!hpDialogue){
            Dialogue("Venge", "Core overloaded, huh. Reconfiguring energy circulation. Now this unit will last just a bit longer...");
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
        if(!commands[0]&&songPosition>0){ commands[0] = true;
            Boss.SetMode(-10); Dialogue("Objectives:", "1. Overload V53-A's core by depleting its shields\n2. Survive");
        } else if(!commands[1]&&songPosition>17){ commands[1] = true;
            if(SaveData.Deaths[SceneManager.GetActiveScene().buildIndex]<4)
                Dialogue("Venge", "Hear that ringin’ in your ears? These new engines have a nice kick to them.");
            else Dialogue("Venge", "Don't think I'm about to go easy on you.");
        } else if(!commands[2]&&songPosition>60){ commands[2] = true;
            Boss.SetMode(1); Dialogue("Venge", "R&D will have a field day with those weapons of yours, once I’ve scavenged what’s left.");
        } else if(!commands[3]&&songPosition>110){ commands[3] = true;
            Boss.SetMode(0); 
        } else if(!commands[4]&&songPosition>140){ commands[4] = true;
            Boss.SetMode(1); SpawnHeal();
        } else if(!commands[5]&&songPosition>165){ commands[5] = true;
            if(SaveData.Deaths[SceneManager.GetActiveScene().buildIndex]<3)
                Dialogue("Venge", "Well you're pretty good. I'm surprised.");
            else Dialogue("Venge", "Wow, you're really stretching my limits, huh. I'm not quite done yet, miss.");
        } else if(!commands[6]&&songPosition>208){ commands[6] = true;
            Boss.SetMode(1); Dialogue("Venge", "Six feet under is such a hassle. I’ll leave your ashes to the wind."); SpawnHeal();
        } else if (!commands[7]&&songPosition>292){ commands[7] = true;
            bool a = Boss.CheckDefeated();
            if(a) {
                StartCoroutine(Win());
                Dialogue("Venge","Damn it...");
            }
            else Dialogue("Venge","Try to impress me next time.");
        }
    }

    private IEnumerator Win(){
        yield return new WaitForSeconds(2);
        //go to VN?
        yield return StartCoroutine(SetPanelTrue());
        yield return new WaitForSeconds(2);
        yield return StartCoroutine(FadeInText(MissionComplete));
        yield return new WaitForSeconds(5);
        SaveData.SceneNum = -1;
        SceneManager.LoadSceneAsync("WinScreen");
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
            AudioListener.pause = true;
        } else {
            PausePanel.SetActive(false);
            Time.timeScale = 1;
            AudioListener.pause = false;
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
        if (!dialoguePlaying)
            StartCoroutine(DialogueCor(n, s));
    }
    private IEnumerator DialogueCor(string n, string s){
        dialoguePlaying = true;
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
        dialoguePlaying = false;
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