using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Boss2GMScript : MonoBehaviour, IGameManager
{
    public GameObject DefaultSong;
    public GameObject BlackPanel;
    public GameObject Player;
    public GameObject heal;
    private float songPosition;
    public Boss2AI Boss;
    public float songPosInBeats;
    private float secPerBeat, dsptimesong, bpm;
    private bool started;
    private bool[] commands;
    private float[] beats ={343, 351, 359, 367, 415, 423, 431, 439};
    private int nextIndex;
    private AudioSource AS;

    void Start(){
        Player = GameObject.FindWithTag("Player");
        StartCoroutine(SetPanelFalse());
        started = false;
        bpm = 150;
        secPerBeat = 60f / bpm;
        AS = GetComponent<AudioSource>();
        commands= new bool[23];
        for(int i =0;i<23;i++){
            commands[i] = false;
        }
        nextIndex = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(!started) return;
        songPosition = (float) (AudioSettings.dspTime - dsptimesong);
        songPosInBeats = songPosition / secPerBeat;

        Commander();
        if (nextIndex < beats.Length && beats[nextIndex] < songPosInBeats){
            Boss.SetAttack(5); Boss.SetAttack(2);
            nextIndex++;
        }

        if (Input.GetKeyDown(KeyCode.M)){
            AS.mute = !AS.mute;
            DefaultSong.GetComponent<AudioSource>().Pause();
        }

    }

    public void StartFight(){
        started = true;
        dsptimesong = (float) AudioSettings.dspTime;
        DefaultSong.GetComponent<AudioSource>().Play();
        AS.Play();
        AS.mute = true;
    }

    private void SpawnHeal(){
        for(int i=0;i<3;i++){
            int v = Random.Range(0,6);
            Instantiate(heal, transform.GetChild(v).position, Quaternion.identity);
        }
    }

    private void Commander(){
        if(!commands[0]&&songPosition>0){ commands[0] = true;
            Boss.SetAttack(0); Boss.SetMode(-10);
        } else if (!commands[1]&&songPosition>34){ commands[1] = true;
            Boss.SetMode(-2);
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
            Boss.SetAttack(22);
        } else if (!commands[11]&&songPosition>137){ commands[11] = true;
            Boss.SetMode(-2);
        } else if (!commands[12]&&songPosition>148){ commands[12] = true;
            Boss.SetAttack(22); Boss.SetMode(3);
        } else if (!commands[13]&&songPosition>158){ commands[13] = true;
            Boss.SetAttack(0); Boss.SetMode(0); SpawnHeal();
        }  else if (!commands[14]&&songPosition>166){ commands[14] = true;
            Boss.SetMode(-2);
        }  else if (!commands[15]&&songPosition>177){ commands[15] = true;
            Boss.SetAttack(22); Boss.SetMode(3);
        }  else if (!commands[16]&&songPosition>189){ commands[16] = true;
            Boss.SetAttack(10); Boss.SetMode(2);
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
        }  else if (!commands[22]&&songPosition>298){ commands[22] = true;
            bool a = Boss.CheckDefeated();
            if(a){
                //win
            }
        }
    }

    private IEnumerator SetPanelFalse(){
        Image img = BlackPanel.GetComponent<Image>();
        while (img.color.a>0){
            var temp = img.color.a;
            img.color = new Color(img.color.r, img.color.g, img.color.b, temp-Time.deltaTime);
            yield return null;
        }
        BlackPanel.SetActive(false);

        if(SaveData.Weapons[0]) Player.GetComponent<MikuMechControl>().UnlockWeapon(3);
        if(SaveData.Weapons[1]) Player.GetComponent<MikuMechControl>().UnlockWeapon(4);
        if(SaveData.Weapons[2]) Player.GetComponent<MikuMechControl>().UnlockWeapon(5);
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
        SceneManager.LoadSceneAsync("World2Boss");
    }
}
