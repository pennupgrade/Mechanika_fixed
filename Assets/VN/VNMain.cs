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

// Called Main because it interacts directly with Unity
//can split into pratial
public partial class VNMain : MonoBehaviour
{

    static VNMain ins;

    Story story;

    string toDisplay;

    private void Awake()
    { 
        bossSpriteDictionary = new()
        {
            {"miku", (MikuSprite, MikuMaterial)},
            {"charis", (CharisSprite, CharisMaterial)}
        };
        lState = InactiveLeftState; rState = InactiveRightState;

        ins = this;
        story = new Story(StoryData.text); 
        InitStates();
        Continue();

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
            string s = tag.Trim();

            var splitPnt = s.IndexOf(' '); //don't wanna use array cuz only two
            string command = s.Substring(0, splitPnt);
            string param = s.Substring(splitPnt+1);
            
            UnityEngine.Debug.Log(s);

            //Boss and Miku expressions can be changed since they may be on screen at same time later.
            switch(command)
            {
                case "setLeftCharacter":
                SetCharacter(true, param);
                    break;

                case "setRightCharacter":
                SetCharacter(false, param);
                    break;

                case "activateSpeaker":
                SetSpeaker(param, true, false);
                    break;

                case "deactivateSpeaker":
                SetSpeaker(param, false, false);
                    break;

                case "setSpeaker":
                SetSpeaker(param, true, true);
                    break;

                case "setLeftExpression":

                    break;

                case "setRightExpression":

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
        (bool cc, bool du, bool ch) = (story.canContinue, RunningState(State.DISPUPDATING), RunningState(State.CHOOSING));

        if (cc && !du)
            Continue();
        else if (du)
            ForceFinishDisplay();
        else if (!cc && !du && !ch)
            DoChoices();
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

    [Header(" -=- Choice Settings -=- ")]
    [SerializeField] GameObject ChoiceFolder;
    [SerializeField] GameObject ChoicePrefab;
    [Min(0.0f)] [SerializeField] float ChoiceVerticalSeparation;

    [Header(" -=- Misc -=- ")]
    [SerializeField] TMP_Text OutputTextbox;
    [SerializeField] TextAsset StoryData;

    [Header(" -=- Text Settings -=- ")]
    [SerializeField] float TextSpeed;

    [Header(" -=- VN Character Objects -=- ")]
    [SerializeField] Image LeftImage;
    [SerializeField] Image RightImage;
    
    [Header(" -=- VN Character Sprites -=- ")]
    [SerializeField] Sprite MikuSprite;
    [SerializeField] Sprite CharisSprite;
    [SerializeField] RectTransform CharacterHolder;

    [Header (" -=- VN Character Materials -=- ")]
    [SerializeField] Material MikuMaterial;
    [SerializeField] Material CharisMaterial;

    [Header (" -=- VN Character States -=- ")]
    [SerializeField] CharacterState ActivateRightState;
    [SerializeField] CharacterState InactiveRightState;
    [SerializeField] CharacterState ActivateLeftState;
    [SerializeField] CharacterState InactiveLeftState;

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

    Dictionary<string, (Sprite, Material)> bossSpriteDictionary;
    void SetCharacter(bool isLeft, String character) 
    {
        character = character.ToLower();

        var img = isLeft ? LeftImage : RightImage;
        var data = bossSpriteDictionary[character]; img.sprite = data.Item1; img.material = data.Item2;

        if(isLeft) lName = character; else rName = character;
    }

    string lName; string rName;
    void SetSpeaker(string s, bool active = true, bool otherOpp = true)
    {
        s = s.ToLower();
        if(s == "left" || s == "right") { SetSpeaker(s == "left", active); if(otherOpp) SetSpeaker(s != "left", !active); }
        else if(s == lName) { SetSpeaker(true, active); if(otherOpp) SetSpeaker(false, !active); }
        else if(s == rName) { SetSpeaker(false, active); if(otherOpp) SetSpeaker(true, !active); }
        else throw new Exception ("Invalid Speaker");
    }
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
        lState.Lerp(isActiveLeft ? ActivateLeftState : InactiveLeftState, 2f*dt); 
        rState.Lerp(isActiveRight ? ActivateRightState : InactiveRightState, 2f*dt);

        UnityEngine.Debug.Log(isActiveLeft + " " + isActiveRight);

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

}