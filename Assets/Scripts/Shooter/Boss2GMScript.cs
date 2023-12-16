using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Boss2GMScript : MonoBehaviour, IGameManager
{
    public TextMeshProUGUI DialogueName, DialogueText, MissionComplete;
    public TextMeshProUGUI CountDown;
    public GameObject DefaultSong;
    public GameObject BlackPanel, PausePanel;
    public GameObject Player;
    public GameObject heal;
    private float songPosition;
    public Boss2AI Boss;
    public float songPosInBeats;
    private float secPerBeat, dsptimesong, bpm;
    private bool started, hpDialogue, mikuSong;
    private bool[] commands;
    private float[] beats ={343, 351, 359, 367, 415, 423, 431, 439};
    private int nextIndex, minLeft, secLeft;
    private AudioSource AS;

    void Start(){
        SaveData.SceneNum = SceneManager.GetActiveScene().buildIndex;
        Player = GameObject.FindWithTag("Player");
        StartCoroutine(SetPanelFalse());
        started = false; hpDialogue = false; mikuSong = false;
        bpm = 150;
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
        minLeft = (298-(int)songPosition)/60; if(minLeft<0)minLeft=0;
        secLeft = (298-(int)songPosition)%60; if(secLeft<0)secLeft=0;
        CountDown.text="Time Remaining:\n" + minLeft.ToString().PadLeft(1,'0')+":"+secLeft.ToString().PadLeft(2,'0');

        Commander();
        if (nextIndex < beats.Length && beats[nextIndex] < songPosInBeats){
            Boss.SetAttack(5); Boss.SetAttack(2);
            nextIndex++;
        }

        if (!mikuSong && (SaveData.MikuSong || (Input.GetKeyUp(KeyCode.M) && Time.timeScale > 0.9f))){
            mikuSong = true;
            AS.mute = !AS.mute;
            DefaultSong.GetComponent<AudioSource>().Pause();
            SaveData.MikuSong = true;
        }
        if(Boss.health==0&&!hpDialogue){
            Dialogue("Charis", "Core compromised? I've miscalculated...But no matter. Systems are still functional. I'm not letting you win.");
            hpDialogue=true;
        }
    }

    public void StartFight(){
        started = true;
        dsptimesong = (float) AudioSettings.dspTime;
        DefaultSong.GetComponent<AudioSource>().Play();
        AS.Play();
        AS.mute = true;
        CountDown.gameObject.SetActive(true);
    }

    private void SpawnHeal(){
        for(int i=0;i<1;i++){
            int v = Random.Range(0,6);
            Instantiate(heal, transform.GetChild(v).position, Quaternion.identity);
        }
    }

    private void Commander(){
        if(!commands[0]&&songPosition>0){ commands[0] = true;
            Boss.SetMode(-10); Dialogue("Objectives:", "1. Deplete ER07's shield.\n2. Survive until its core overloads.");
        } else if (!commands[23]&&songPosition>3){ commands[23] = true;
            Boss.SetAttack(0);
        } else if (!commands[1]&&songPosition>34){ commands[1] = true;
            Boss.SetMode(-2);
            if(SaveData.Deaths[SceneManager.GetActiveScene().buildIndex]<3)
                Dialogue("Charis", "Redirecting energy from shielding to offensive capabilities. Let's see what you're made of.");
            else Dialogue("Charis", "You should know by now, but I don't go down easy.");
        } else if (!commands[2]&&songPosition>41){ commands[2] = true;
            Boss.SetAttack(1); Boss.SetMode(2);
        } else if (!commands[3]&&songPosition>50){ commands[3] = true;
            Boss.SetMode(-2);
        } else if (!commands[4]&&songPosition>54){ commands[4] = true;
            Boss.SetAttack(22); Boss.SetMode(3);
        } else if (!commands[5]&&songPosition>66){ commands[5] = true;
            Boss.SetAttack(1); Boss.SetMode(2);
        } else if (!commands[6]&&songPosition>90){ commands[6] = true;
            Boss.SetAttack(0); Boss.SetMode(0); SpawnHeal();
        } else if (!commands[7]&&songPosition>105){ commands[7] = true;
            Boss.SetAttack(10); Boss.SetMode(1);
        } else if (!commands[8]&&songPosition>116){ commands[8] = true;
            Boss.SetMode(-2);
        } else if (!commands[9]&&songPosition>123){ commands[9] = true;
            Boss.SetAttack(1); Boss.SetMode(-1);
        } else if (!commands[10]&&songPosition>130){ commands[10] = true;
            Boss.SetAttack(2222); 
            if(SaveData.Deaths[SceneManager.GetActiveScene().buildIndex]>4)
                Dialogue("Charis", "Let's dance.");
            else Dialogue("Charis", "Interesting. Disengaging autopilot. Assuming direct control.");
        } else if (!commands[11]&&songPosition>137){ commands[11] = true;
            Boss.SetMode(-2);
        } else if (!commands[12]&&songPosition>148){ commands[12] = true;
            Boss.SetAttack(22222); Boss.SetMode(3);
        } else if (!commands[13]&&songPosition>158){ commands[13] = true;
            Boss.SetAttack(4); Boss.SetMode(0); SpawnHeal();
        }  else if (!commands[14]&&songPosition>166){ commands[14] = true;
            Boss.SetMode(-2);
        }  else if (!commands[15]&&songPosition>177){ commands[15] = true;
            Boss.SetAttack(2222); Boss.SetMode(3);
        }  else if (!commands[16]&&songPosition>189){ commands[16] = true;
            Boss.SetAttack(10); Boss.SetMode(2);
            if(Boss.health>1000){ 
                if(SaveData.Deaths[SceneManager.GetActiveScene().buildIndex]<4)
                    Dialogue("Charis", "Impressive. But your journey ends here.");
                else Dialogue("Charis", "Impressive. But I shall prove your efforts futile.");
            }
        }  else if (!commands[17]&&songPosition>217){ commands[17] = true;
            Boss.SetAttack(4); Boss.SetMode(5);
        }  else if (!commands[18]&&songPosition>230){ commands[18] = true;
            Boss.SetAttack(3); Boss.SetMode(4);
        }  else if (!commands[19]&&songPosition>242){ commands[19] = true;
            Boss.SetAttack(4); Boss.SetMode(5);
        }  else if (!commands[20]&&songPosition>258){ commands[20] = true;
            Boss.SetAttack(3); Boss.SetMode(6);
        }  else if (!commands[21]&&songPosition>285){ commands[21] = true;
            Boss.SetAttack(10); Boss.SetMode(2);
            if(Boss.health>1000) Dialogue("Charis", "Laser guidance system nearing 95%. Your fate is sealed.");
        }  else if (!commands[22]&&songPosition>298){ commands[22] = true;
            bool a = Boss.CheckDefeated();
            if(a) {
                StartCoroutine(Win());
                Dialogue("Charis","Impossi-");
            }
            else Dialogue("Charis","Your damage output is pitiful.");
        }
    }

    private IEnumerator Win(){
        yield return new WaitForSeconds(2);
        //go to VN
        yield return StartCoroutine(SetPanelTrue());
        yield return new WaitForSeconds(2);
        yield return StartCoroutine(FadeInText(MissionComplete));
        SaveData.SceneNum = 3;
        yield return new WaitForSeconds(5);
        SceneManager.LoadSceneAsync("MainMenu");
    }

    public static void SkipFight() {
        var boss = GameObject.Find("Boss2GM").GetComponent<Boss2GMScript>();
        boss.StartCoroutine(boss.Win());
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
        } else {
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            AudioListener.pause = false;
        }
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