using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Change_StoreMode : MonoBehaviour
{
    public Button Change_Button;
    public GameObject Buy;
    public GameObject Sell;
    public void Awake()
    {
        Change_Button = GetComponent<Button>();

        Change_Button.onClick.RemoveAllListeners();
        Change_Button.onClick.AddListener(() => Click_Chnage());
    }
    public void Click_Chnage()
    {
        if (Buy.activeSelf && !Sell.activeSelf)
        {
            Buy.SetActive(false);
            Sell.SetActive(true);
            Game_Event.instance.Current_Trader.Refresh_BS();
        }
        else if(Sell.activeSelf && !Buy.activeSelf)
        {
            Sell.SetActive(false);
            Buy.SetActive(true);
            Game_Event.instance.Current_Trader.Refresh_B();
        }
    }
}
