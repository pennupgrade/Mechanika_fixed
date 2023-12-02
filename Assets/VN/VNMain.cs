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
using System.Runtime.InteropServices;

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

            string[] fullCommand = s.Split(' ');
            Func<int, string> getParam = i =>
            {
                try 
                {
                    return fullCommand[i+1];
                }
                catch (ArgumentOutOfRangeException)
                {
                    UnityEngine.Debug.LogWarning("You didn't give all the parameters the function needs.");
                    return "FAIL";
                }
            };
            string command = fullCommand[0];

            // Where then What
            switch(command)
            {
                case "setcharacter":
                SetCharacter(StringToSide(getParam(0)), getParam(1));
                    break;

                case "setactive":
                SetSpeaker(getParam(0), bool.Parse(getParam(1)), false);
                    break;

                case "setspeaker":
                SetSpeaker(getParam(0), true, true);
                    break;

                case "switchspeaker":
                SwitchSpeaker();
                    break;

                case "playsound":
                AudioPlayer.clip = ins.Sounds[getParam(0)];
                AudioPlayer.Play();
                    break;

                case "emotion":
                SetExpression(getParam(0), getParam(1));
                    break;

                case "setvisible":
                SetVisible(StringToSide(getParam(0)), bool.Parse(getParam(1)));
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

            textObj.text += c.text;
            choiceObj.transform.localPosition = (ChoiceVerticalSeparation * i - vertLen*.5f) * Vector3.down;

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
        else if(!ch)
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
    [SerializeField] SerializedDictionary<string, CharacterSpriteData> BossSpriteDictionary;
    [SerializeField] RectTransform CharacterHolder;

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

    //
    string lName;
    string rName;

    //
    bool lVisible = true;
    bool rVisible = true;
    
    void SetCharacter(bool isLeft, string character, string emotion = "happy") 
    {
        character = character.ToLower();

        var img = isLeft ? LeftImage : RightImage;
        var data = BossSpriteDictionary[character]; img.sprite = data.HappyCharacter; img.material = data.CharacterMaterial;

        if(isLeft) lName = character; else rName = character;

        SetExpression(isLeft, emotion);
    }

    bool StringToSide(string s) 
    {
        if(s == "left" || s == "right") return s == "left";
        else if(s == lName) return true;
        else if(s == rName) return false;
        else throw new Exception ("Invalid Side");
    }
    Image SideToImage(bool isLeft) => isLeft ? LeftImage : RightImage;
    string SideToName(bool isLeft) => isLeft ? lName : rName;

    void SetSpeaker(string s, bool active = true, bool otherOpp = true)
    {
        bool side = StringToSide(s);
        SetSpeaker(side, active); if(otherOpp) SetSpeaker(!side, !active);
    }
    void SwitchSpeaker() { isActiveLeft = !isActiveLeft; isActiveRight = !isActiveRight; }

    void SetExpression(bool isLeft, string emotion)
    {
        Image imageToEdit = SideToImage(isLeft);
        string name = SideToName(isLeft);

        CharacterSpriteData characterData = BossSpriteDictionary[name];
        Sprite spriteToUse = null;

        switch(emotion)
        {
            case "happy":
            spriteToUse = characterData.HappyCharacter;
            break;

            case "sad":
            spriteToUse = characterData.SadCharacter;
            break;

            case "evil":
            spriteToUse = characterData.EvilCharacter;
            break;

            case "scared":
            spriteToUse = characterData.ScaredCharacter;
            break;

        }

        imageToEdit.sprite = spriteToUse;
        
    }
    void SetExpression(string s, string emotion) => SetExpression(StringToSide(s), emotion);

    void SetVisible(bool isLeft, bool visible)
    { if(isLeft) lVisible = visible; else rVisible = visible; }

    
    void SetSpeaker(bool isLeft, bool activate = true)
    {
        if(isLeft) isActiveLeft = activate;
        else       isActiveRight = activate;
    }

    //
    float2 currLPos;
    float2 currRPos;

    void UpdatePortraits(float dt)
    {
        lState.Lerp(isActiveLeft ? ActivateLeftState : InactiveLeftState, CharacterAnimateSpeed*dt, lVisible ? -1 : 0); 
        rState.Lerp(isActiveRight ? ActivateRightState : InactiveRightState, CharacterAnimateSpeed*dt, rVisible ? -1 : 0);

        lState.SendToImage(LeftImage);
        rState.SendToImage(RightImage);
    }

    [System.Serializable]
    struct CharacterState
    {

        public float2 Position;
        public float Size;
        public float Fade;

        public void Lerp(CharacterState o, float t, float fadeOverride = -1) // could make ref
        {
            Position = math.lerp(Position, o.Position, t);
            Size = math.lerp(Size, o.Size, t);
            Fade = math.lerp(Fade, fadeOverride == -1 ? o.Fade : fadeOverride, t);
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
        lState = InactiveLeftState; rState = InactiveRightState;
    }

    [System.Serializable]
    struct CharacterSpriteData
    {
        public Sprite HappyCharacter;
        public Sprite SadCharacter;
        public Sprite EvilCharacter;
        public Sprite ScaredCharacter;
        public Material CharacterMaterial;
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