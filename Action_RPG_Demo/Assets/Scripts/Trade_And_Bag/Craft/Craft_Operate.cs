using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Craft_Operate : MonoBehaviour
{
    public Button craft_button;
    public Crafting_SO craft;
    //public Single_Craft_UI ui;
    public void Awake()
    {
        //ui = GetComponent<Single_Craft_UI>();
        //craft = ui.crafting;

        craft_button = GetComponent<Button>();
        craft_button.onClick.RemoveAllListeners();
        craft_button.onClick.AddListener(() => Craft_Process());
    }
    public void Craft_Process()
    {
        if (Game_Event.instance.Craft_Process_Check(craft))
        {
            Game_Event.instance.Craft_Start(craft);
        }
    }
}
