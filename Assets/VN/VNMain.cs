using Ink.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static Utils;

// Called Main because it interacts directly with Unity
//can split into pratial
public partial class VNMain : MonoBehaviour
{

    Story story;

    string toDisplay;

    private void Awake()
    { 
        story = new Story(StoryData.text); 
        InitStates();
        Continue();
    }
    private void Update()
    {
        for (int i = 0; i < stateStatuses.Length; i++)
        {
            if (stateStatuses[i])
                stateLoopActions[i]();
        }
    }

    void Continue()
    {
        toDisplay = story.Continue();
        ResetTextbox();
    }
    void DoChoices()
    {
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
        story.ChooseChoiceIndex(choiceIndex);
        Continue();
    }

    public void OnInteract()
    {
        (bool cc, bool du) = (story.canContinue, RunningState(State.DISPUPDATING));

        if (cc && !du)
            Continue();
        else if (du)
            ForceFinishDisplay();
        else if (!cc && !du)
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
    void UpdateDisplay()
    {
        string newText = PartialStringSpaceTrim(toDisplay, (int)(textboxTimer.Elapsed.TotalSeconds * TextSpeed));
        if (newText != toDisplay)
            OutputTextbox.text = newText;
        else
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
        CHOOSING
    }
    bool[] stateStatuses = new bool[2];
    Action[] stateLoopActions;
    void InitStates()
    {
        stateLoopActions = new Action[2] { UpdateDisplay, null };
        SetState(State.DISPUPDATING, true);
    }

    bool RunningState(State s)
        => stateStatuses[(int) s];
    bool SetState(State s, bool active)
        => stateStatuses[(int)s] = active;
}