using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Sold_Control : MonoBehaviour
{
    public Item_Data Sell_Item;
    public Image Sell_Image;
    public TextMeshProUGUI Text;
    public Button Sell_Button;

    public int Max_Sold;
    public int Current_Sold;
    public void Awake()
    {
        Image[] all = GetComponentsInChildren<Image>();
        foreach (var i in all)
        {
            if (i.gameObject != this.gameObject)
            {
                Sell_Image = i;
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

        Sell_Button = GetComponent<Button>();
        Sell_Button.onClick.RemoveAllListeners();
        Sell_Button.onClick.AddListener(() => ClickItem());

        //Max_Sold=
        //Current_Sold = Game_Event.instance.Last_Item_For_Sell(Sell_Item);
    }
    public void Start()
    {
        Text.text = $"可出售:{Current_Sold}/{Max_Sold}\t${Sell_Item.PriceValue}";
        Sell_Image.sprite = Sell_Item.Display_In_Backpacks;
    }
    public void ClickItem()
    {
        if (Current_Sold <= 0 || Max_Sold <= 0)
        {
            Game_Event.instance.Current_Trader.PlayTraderShow(Game_Event.instance.Current_Trader.UnNod);
            PickNoticeMgr.instance.ShowFieldTip("未拥有相应货物，无法出售");
            PickNoticeMgr.instance.ShowDialogueTip(Game_Event.instance.Current_Trader.name, "你还没有这件物品噢", 3f);
            Debug.Log("无法出售");
            return;
        }
        Game_Event.instance.Real_Sell_Item -= Check_Sell;
        Game_Event.instance.Real_Sell_Item += Check_Sell;
        Game_Event.instance.Send_SellItem(Sell_Item);
    }
    public void Check_Sell(bool Is_Really)
    {
        Game_Event.instance.Real_Sell_Item -= Check_Sell;
        if (Is_Really)
        {
            Current_Sold--;
            Max_Sold--;
            Game_Event.instance.Remove_Sold_Item(Sell_Item);
            int money = PlayerPrefs.GetInt("Money", 0);
            money += Sell_Item.PriceValue;
            PlayerPrefs.SetInt("Money", money);
            Text.text = $"可出售:{Current_Sold}/{Max_Sold}\t${Sell_Item.PriceValue}";
            PickNoticeMgr.instance.ShowFieldTip($"已卖出{Sell_Item.item_name}");
            PickNoticeMgr.instance.ShowDialogueTip(Game_Event.instance.Current_Trader.name, "感谢惠顾!", 3f);
            Game_Event.instance.Current_Trader.PlayTraderShow(Game_Event.instance.Current_Trader.Nod);
        }
        Debug.Log("能卖吗" + Is_Really);
    }
}
