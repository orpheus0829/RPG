using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Choice_NextDialogue : MonoBehaviour
{
    public Button button;
    public TextMeshProUGUI ButtonText;
    public SingleChoice ChoiceDia;
    public void Awake()
    {
        button = GetComponent<Button>();
        ButtonText = GetComponentInChildren<TextMeshProUGUI>();
        ButtonText.text=string.Empty;
    }
    public void Start()
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(NextDialogue);
        ShowChoiceText(ChoiceDia);
    }
    public void OnEnable()
    {
        
    }
    public void OnDisable()
    {
        
    }
    public void Update()
    {

    }
    public void NextDialogue()
    {
        Debug.Log("обр╩╬Д");
        Game_Event.instance.PressNextChoice(ChoiceDia.NextDialogue, () =>
        {

        });
    }
    public void ShowChoiceText(SingleChoice dialogue)
    {
        ButtonText.text = dialogue.ChoiceText;
    }
}
