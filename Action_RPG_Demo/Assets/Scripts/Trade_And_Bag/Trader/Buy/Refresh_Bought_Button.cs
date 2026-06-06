using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Refresh_Bought_Button : MonoBehaviour
{
    public Button button;
    public void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => Game_Event.instance.Current_Trader.Refresh_B());
    }
}
