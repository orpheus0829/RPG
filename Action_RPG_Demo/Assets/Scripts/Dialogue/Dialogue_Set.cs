using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialogue_Set : MonoBehaviour
{
    public Dialogue_SO Story_Dialogue;
    public Dialogue_SO Chat_Dialogue;
    public Dialogue_SO Cur_Dialogue;
    [EntityId]
    public string CharacterId;
    public void Awake()
    {

    }
    public void Start()
    {
        Switch_DialogueSO();
    }
    public void Update()
    {
    }
    public void Switch_DialogueSO()
    {
        Panel_Mgr.instance.HideAllPanel();
        Cur_Dialogue = Story_Dialogue ? Story_Dialogue : Chat_Dialogue;
        DialogueWriter writer = Panel_Mgr.instance.DialoguePanel.GetComponent<DialogueWriter>();
        writer.ClearAllChoice();
    }
}
