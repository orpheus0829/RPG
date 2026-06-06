using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Bought_Control : MonoBehaviour
{
    public Item_Data Buy_Item;
    public Image Buy_Image;
    public TextMeshProUGUI Text;
    public Button Buy_Button;

    public int Max_Store;
    public int Current_Store;
    public void Awake()
    {
        Image[] all = GetComponentsInChildren<Image>();
        foreach(var i in all)
        {
            if (i.gameObject != this.gameObject)
            {
                Buy_Image = i;
                break;
            }
        }
        TextMeshProUGUI[] alltext = GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var j in alltext)
        {
            if (j.gameObject != this.gameObject)
            {
                Text = j;
                break;
            }
        }

        Buy_Button = GetComponent<Button>();
        Buy_Button.onClick.RemoveAllListeners();
        Buy_Button.onClick.AddListener(() => ClickItem());

        Current_Store = Max_Store;
    }
    public void Start()
    {
        Text.text = $"可购入:{Current_Store}/{Max_Store}\t${Buy_Item.PriceValue}";
        Buy_Image.sprite = Buy_Item.Display_In_Backpacks;
    }
    public void ClickItem()
    {
        if (Current_Store <= 0)
        {
            Debug.Log("暂时缺货");
            return;
        }
        Debug.Log("查询是否有空间容纳" + Buy_Item.item_name + "中......");
        Game_Event.instance.Real_Buy_Item -= Check_Buy;
        Game_Event.instance.Real_Buy_Item += Check_Buy;
        Game_Event.instance.Send_BuyItem(Buy_Item);
    }
    public void Check_Buy(bool Is_Really)
    {
        Game_Event.instance.Real_Buy_Item -= Check_Buy;
        if (Is_Really)
        {
            Current_Store--;
            int money = PlayerPrefs.GetInt("Money", 0);
            money -= Buy_Item.PriceValue;
            PlayerPrefs.SetInt("Money", money);
            Text.text = $"可购入:{Current_Store}/{Max_Store}\t${Buy_Item.PriceValue}";
        }
    }
}
