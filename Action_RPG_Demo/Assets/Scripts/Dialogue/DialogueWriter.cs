using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueWriter : MonoBehaviour
{
    [Header("速度")]
    public float TypeSpeed = 0.5f;
    public float ButtonAppearInterval = 1f;
    [Header("是否正在打字")]
    public bool IsTyping;
    public string FullContent;
    [Header("引用")]
    public Dialogue_Set Actor;
    public GameObject ChoiceParent;
    public TextMeshProUGUI Speaker;
    public TextMeshProUGUI Content;
    public Dialogue_SO CurDialogue;
    public Coroutine typ;
    [Header("debug")]
    public bool Write;
    public void Awake()
    {
        Transform speaker = transform.GetChild(0).GetChild(0);
        Transform content = transform.GetChild(0).GetChild(1);
        if (!Speaker)
        {
            Speaker = speaker.GetComponent<TextMeshProUGUI>();
        }
        if (!Content)
        {
            Content = content.GetComponent<TextMeshProUGUI>();
        }
    }
    public void Start()
    {

    }
    public void Update()
    {
        if (Write)
        {
            WriteDialogue();
            Write = false;
        }
    }
    public void OnEnable()
    {
        Game_Event.instance.PressChoice2 += ReceiveChange;
        Game_Event.instance.PressChoice1 += WriteDialogue;
        Game_Event.instance.DirectNextDialogue += DirectNext;
    }
    public void OnDisable()
    {
        Game_Event.instance.PressChoice2 -= ReceiveChange;
        Game_Event.instance.PressChoice1 -= WriteDialogue;
        Game_Event.instance.DirectNextDialogue -= DirectNext;
    }
    public void WriteDialogue()
    {
        if (IsTyping)
        {
            StopCoroutine(typ);
        }
        FullContent = CurDialogue.Single_Dialogue;
        CameraPivot.instance.RestoreNormalCameraState();
        CameraPivot.instance.isPlayingCameraAnim = false;
        if (CurDialogue.Cut_Show)
        {
            Actor.PlayNpcAction(CurDialogue.Cut_Show);
        }
        typ = StartCoroutine(TypeCoroutine());
    }
    public IEnumerator TypeCoroutine()
    {
        ClearAllChoice();
        IsTyping = true;
        Speaker.text = CurDialogue.SpeakerName;
        Content.text = string.Empty;
        for(int i = 0; i < FullContent.Length; i++)
        {
            Content.text += FullContent[i];
            yield return new WaitForSeconds(TypeSpeed);
        }
        IsTyping = false;
        yield return new WaitForSeconds(ButtonAppearInterval);
        Game_Event.instance.LoadChoice(CurDialogue);
        typ = null;
    }
    public void SkipType()
    {
        if (!IsTyping)
        {
            return;
        }
        StopCoroutine(TypeCoroutine());
        IsTyping = false;
        Content.text = FullContent;
        ClearAllChoice();
        Game_Event.instance.LoadChoice(CurDialogue);
        typ = null;
    }
    public void ReceiveChange(Dialogue_SO dialogue)
    {
        CurDialogue = dialogue;
    }
    public void ClearAllChoice()
    {
        Transform parent = ChoiceParent.transform;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
        Cursor.visible = false;
    }
    public void DirectNext()
    {
        if (IsTyping)
        {
            return;
        }
        ClearAllChoice();
        if (CurDialogue.ContinueWay == WayToNextDialogue.Next)
        {
            CurDialogue = CurDialogue.nextDialogue;
            WriteDialogue();
        }
        if (CurDialogue.ContinueWay == WayToNextDialogue.NoNext)
        {
            Speaker.text = string.Empty;
            Content.text = string.Empty;
            CameraPivot.instance.isPlayingCameraAnim = false;
            Story_Mgr.instance.CurActor = null;
            Panel_Mgr.instance.HideAllPanel();
            if (CurDialogue.dialogueMode == DialogueMode.Quest)
            {
                Story_Mgr.instance.QuestAdvance();
                Story_Mgr.instance.Refresh_StoryProgress();
            }
        }
    }
}
