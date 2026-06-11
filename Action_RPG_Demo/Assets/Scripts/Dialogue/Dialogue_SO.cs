using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DialogueMode
{
    Quest,
    Chat,
}

public enum WayToNextDialogue
{
    Next,
    Choice,
    NoNext,
}
[System.Serializable]
public class SingleChoice
{
    public Dialogue_SO NextDialogue;
    public string ChoiceText;
}

[CreateAssetMenu(fileName = "Dialogue_Data", menuName = "Dialogue/Dialogue_Data")]
public class Dialogue_SO : QuestBase_SO
{
    public DialogueMode dialogueMode;
    [Header("发言人")]
    public string SpeakerName;
    [EntityId]
    public string SpeakerId;
    [Header("发言内容")]
    [TextArea(0, 5)] public string Single_Dialogue;

    [Header("配套演出")]
    public ActionSO Cut_Show;

    [Header("如何进入下一句对话")]
    public WayToNextDialogue ContinueWay;

    [Header("方式1:直接下一句")]
    public Dialogue_SO nextDialogue;

    [Header("方式2:分支选项")]
    public List<SingleChoice> choiceDialogues;

    [Header("方法3:结束")]
    [HideInInspector] public bool HasNext;
    private void OnEnable()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        HasNext = ContinueWay != WayToNextDialogue.NoNext;
    }
}