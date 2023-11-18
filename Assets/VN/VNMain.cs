using Ink.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

using static Utils;

using static Unity.Mathematics.math;
using AYellowpaper.SerializedCollections;

public partial class VNMain : MonoBehaviour
{

    [Header (" -=- Stories -=- ")]
    [SerializeField] TextAsset MikuCharisExchange;

    public static Story MikuCharisStory;

    void InitStories()
    {
        MikuCharisStory = new(MikuCharisExchange.text);
    }

}

public partial class VNMain : MonoBehaviour
{

    static Action currCallback;

    public static void Activate(Story story, Action callback, bool skippable)
    {
        //TODO: add backdrop blur post processing

        currCallback = callback;

        ins.VNFolder.SetActive(true);
        ins.SkipFolder.SetActive(skippable);

        ins.story = story;
        ins.ResetStates();
        ins.Continue();
    }

    public static void Deactivate()
    { currCallback?.Invoke(); currCallback = null; ins.VNFolder.SetActive(false); }

}

public partial class VNMain : MonoBehaviour
{

    static VNMain ins;

    Story story;

    string toDisplay;

    private void Awake()
    { 
        
        ins = this;

        InitVNSprites();
        InitStates();
        InitSkipFunctionality();
        InitStories();

        Deactivate();
    }
    private void Update()
    {
        for (int i = 0; i < stateStatuses.Length; i++)
        {
            if (stateStatuses[i])
                stateLoopActions[i]?.Invoke(Time.deltaTime);
        }


    }

    void DoTags()
    {

        foreach(var tag in story.currentTags)
        {
            string s = tag.Trim().ToLower();

            var splitPnt = s.IndexOf(' '); //don't wanna use array cuz only two
            string command = s.Substring(0, splitPnt);
            string param = s.Substring(splitPnt+1);
            
            UnityEngine.Debug.Log(s);

            //Boss and Miku expressions can be changed since they may be on screen at same time later.
            switch(command)
            {
                case "setleftcharacter":
                SetCharacter(true, param);
                    break;

                case "setrightcharacter":
                SetCharacter(false, param);
                    break;

                case "activatespeaker":
                SetSpeaker(param, true, false);
                    break;

                case "deactivatespeaker":
                SetSpeaker(param, false, false);
                    break;

                case "setspeaker":
                SetSpeaker(param, true, true);
                    break;

                case "switchspeaker":
                SwitchSpeaker();
                    break;

                case "playsound":
                AudioPlayer.clip = ins.Sounds[param];
                AudioPlayer.Play();
                    break;

                case "sethappy": //improve later make multiple param commands just use split
                SetExpression(param, true);
                    break; 

                case "setsad":
                SetExpression(param, false);
                    break;
            }
        }
    }

    void Continue()
    {
        toDisplay = story.Continue();
        DoTags();
        ResetTextbox();
    }
    void DoChoices()
    {
        SetState(State.CHOOSING, true);

        toDisplay = "";
        ResetTextbox();

        GameObject choiceObj; List<GameObject> choiceObjs = new();

        float vertLen = ChoiceVerticalSeparation * (story.currentChoices.Count-1);

        story.currentChoices.ForeachIndex((c, i) =>
        {
            Button buttonObj; TMP_Text textObj;

            choiceObjs.Add(choiceObj = Instantiate(ChoicePrefab, ChoiceFolder.transform));
            buttonObj = choiceObj.GetComponentInChildren<Button>(); textObj = buttonObj.GetComponentInChildren<TMP_Text>(); 

            choiceObj.transform.localPosition = (ChoiceVerticalSeparation * i - vertLen*.5f) * Vector3.down;
            textObj.text += c.text;

            buttonObj.onClick.AddListener(() => {
                foreach (GameObject g in choiceObjs) Destroy(g);
                SelectChoice(i);
                });

        });
    }
    void SelectChoice(int choiceIndex)
    {
        SetState(State.CHOOSING, false);
        story.ChooseChoiceIndex(choiceIndex);
        Continue();
    }

    public void OnInteract()
    {
        (bool cc, bool du, bool ch, bool csh) = (story.canContinue, RunningState(State.DISPUPDATING), RunningState(State.CHOOSING), story.currentChoices.Count > 0);

        if (cc && !du)
            Continue();
        else if (du)
            ForceFinishDisplay();
        else if (!cc && !du && !ch && csh)
            DoChoices();
        else 
            Deactivate();
    }
    public void OnExit()
    {

    }

    void OnDisplayFinish()
    {
        
    }

}

public partial class VNMain // Unity Refs 
{

    [Header(" -=- Skip Button -=- ")]
    [SerializeField] GameObject SkipFolder;
    [SerializeField] GameObject SkipObject;

    [Header(" -=- Choice Settings -=- ")]
    [SerializeField] GameObject ChoiceFolder;
    [SerializeField] GameObject ChoicePrefab;
    [Min(0.0f)] [SerializeField] float ChoiceVerticalSeparation;

    [Header(" -=- Misc -=- ")]
    [SerializeField] GameObject VNFolder;
    [SerializeField] TMP_Text OutputTextbox;
    [SerializeField] TextAsset StoryData;

    [Header(" -=- Speed Settings -=- ")]
    [SerializeField] float TextSpeed;
    [SerializeField] [Min(1f)] float CharacterAnimateSpeed = 2f;

    [Header(" -=- VN Character Objects -=- ")]
    [SerializeField] Image LeftImage;
    [SerializeField] Image RightImage;
    
    [Header(" -=- VN Character Sprites -=- ")]
    [SerializeField] CharacterSpriteData MikuSpriteData;
    [SerializeField] CharacterSpriteData CharisSpriteData;
    [SerializeField] RectTransform CharacterHolder;

    [Header (" -=- VN Character Materials -=- ")]
    [SerializeField] Material MikuMaterial;
    [SerializeField] Material CharisMaterial;

    [Header (" -=- VN Character States -=- ")]
    [SerializeField] CharacterState ActivateRightState;
    [SerializeField] CharacterState InactiveRightState;
    [SerializeField] CharacterState ActivateLeftState;
    [SerializeField] CharacterState InactiveLeftState;

    [Header(" -=- Audio -=- ")]
    [SerializeField] AudioSource AudioPlayer;
    [SerializeField] SerializedDictionary<string, AudioClip> Sounds;

}

public partial class VNMain // Display
{

    Stopwatch textboxTimer = new();
    void ResetTextbox()
    {
        OutputTextbox.text = "";
        textboxTimer.Restart();
        SetState(State.DISPUPDATING, true);
    }
    void UpdateDisplay(float dt)
    {
        string newText = PartialStringSpaceTrim(toDisplay, (int)(textboxTimer.Elapsed.TotalSeconds * TextSpeed));
        OutputTextbox.text = newText;
        if (newText == toDisplay)
            { SetState(State.DISPUPDATING, false); OnDisplayFinish(); }
    }
    void ForceFinishDisplay()
    {
        SetState(State.DISPUPDATING, false);
        OutputTextbox.text = toDisplay;
        OnDisplayFinish();
    }

}

public partial class VNMain // state machine
{
    enum State
    {
        DISPUPDATING,
        CHOOSING,
        PORTRAITMOVING
    }
    bool[] stateStatuses = new bool[3];
    Action<float>[] stateLoopActions;
    void InitStates()
    {
        stateLoopActions = new Action<float>[3] { UpdateDisplay, null, UpdatePortraits };
        ResetStates();
    }
    void ResetStates()
    {
        SetState(State.DISPUPDATING, true);
        SetState(State.PORTRAITMOVING, true);
    }

    bool RunningState(State s)
        => stateStatuses[(int) s];
    bool SetState(State s, bool active)
        => stateStatuses[(int)s] = active;
}

public partial class VNMain
{

    //
    CharacterState lState;
    CharacterState rState;

    //
    bool isActiveLeft;
    bool isActiveRight;

    Dictionary<string, (CharacterSpriteData, Material)> bossSpriteDictionary;
    void SetCharacter(bool isLeft, string character) 
    {
        character = character.ToLower();

        var img = isLeft ? LeftImage : RightImage;
        var data = bossSpriteDictionary[character]; img.sprite = data.Item1.HappyCharacter; img.material = data.Item2;

        if(isLeft) lName = character; else rName = character;
    }

    string lName; string rName;
    bool StringToSide(string s) 
    {
        if(s == "left" || s == "right") return s == "left";
        else if(s == lName) return true;
        else if(s == rName) return false;
        else throw new Exception ("Invalid Side");
    }
    void SetSpeaker(string s, bool active = true, bool otherOpp = true)
    {
        bool side = StringToSide(s);
        SetSpeaker(side, active); if(otherOpp) SetSpeaker(!side, !active);
    }
    void SetExpression(string s, bool happy)
    {
        if(StringToSide(s))
            LeftImage.sprite = happy ? bossSpriteDictionary[lName].Item1.HappyCharacter : bossSpriteDictionary[lName].Item1.SadCharacter;
        else
            RightImage.sprite = happy ? bossSpriteDictionary[rName].Item1.HappyCharacter : bossSpriteDictionary[rName].Item1.SadCharacter;
        
    }
    void SwitchSpeaker() { isActiveLeft = !isActiveLeft; isActiveRight = !isActiveRight; }
    void SetSpeaker(bool isLeft, bool activate = true) // miku will almost always be on the left
    {
        float2 far = new (240f, 0f);
        float2 close = new (100f, 0f);

        if(isLeft) isActiveLeft = activate;
        else       isActiveRight = activate;
    }

    //
    float2 currLPos;
    float2 currRPos;

    void UpdatePortraits(float dt)
    {
        lState.Lerp(isActiveLeft ? ActivateLeftState : InactiveLeftState, CharacterAnimateSpeed*dt); 
        rState.Lerp(isActiveRight ? ActivateRightState : InactiveRightState, CharacterAnimateSpeed*dt);

        lState.SendToImage(LeftImage);
        rState.SendToImage(RightImage);
    }

    [System.Serializable]
    struct CharacterState
    {

        public float2 Position;
        public float Size;
        public float Fade;

        public void Lerp(CharacterState o, float t) // could make ref
        {
            Position = math.lerp(Position, o.Position, t);
            Size = math.lerp(Size, o.Size, t);
            Fade = math.lerp(Fade, o.Fade, t);
        }

        public static CharacterState FlipX(CharacterState old)
             { old.Position *= float2(-1f, 1f); return old; }

        public void SendToImage(Image img)
        {
            img.transform.localPosition = Position.xyz();
            img.transform.localScale = Vector3.one * Size;
            img.material.SetFloat("_Fade", Fade);
            //setting aspect to reflect, but it's done automatically?
        }

    }

    void InitVNSprites()
    {
        bossSpriteDictionary = new()
        {
            {"miku", (MikuSpriteData, MikuMaterial)},
            {"charis", (CharisSpriteData, CharisMaterial)}
        };
        lState = InactiveLeftState; rState = InactiveRightState;
    }

    [System.Serializable]
    struct CharacterSpriteData
    {

        public Sprite HappyCharacter;
        public Sprite SadCharacter;
    }

}

public partial class VNMain
{

    void InitSkipFunctionality()
    {
        Button b = SkipObject.GetComponent<Button>();
        b.onClick.AddListener(() => Deactivate());
    }

}