using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadChoice : MonoBehaviour
{
    public GameObject Parent;
    public GameObject ChoicePrefab;
    public void Awake()
    {
        
    }
    public void Start()
    {
        
    }
    public void OnEnable()
    {
        Game_Event.instance.LoadDialogueChoice += NextDialogue;
    }
    public void OnDisable()
    {
        Game_Event.instance.LoadDialogueChoice -= NextDialogue;
    }
    public void NextDialogue(Dialogue_SO dialogue)
    {
        if (dialogue.ContinueWay != WayToNextDialogue.Choice)
        {
            return;
        }
        Debug.Log("º”‘ÿ—°œÓ");
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        foreach (var i in dialogue.choiceDialogues)
        {
            GameObject btn = Instantiate(ChoicePrefab, Parent.transform);
            Choice_NextDialogue choice_Next = btn.GetComponent<Choice_NextDialogue>();
            choice_Next.ChoiceDia = i;
            choice_Next.ButtonText.text = i.ChoiceText;
        }
    }
}
