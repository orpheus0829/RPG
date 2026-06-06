using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trader : MonoBehaviour
{
    public Trader_SO SO;
    public Dictionary<Item_Data, int> Supply_Divide = new Dictionary<Item_Data, int>();
    public Dictionary<Item_Data, int> Sold_Divide = new Dictionary<Item_Data, int>();
    public bool already_Init = false;
    public void Awake()
    {
        foreach(var a in SO.Can_Buy_List)
        {
            Supply_Divide.Add(a, 0);
        }
        Game_Event.instance.Init_Store -= OnShopOpen;

        foreach (var b in SO.Can_Sell_List)
        {
            Sold_Divide.Add(b, 0);
        }
    }
    public void Start()
    {
        
    }
    #region 分配买
    public void Reset_Supply()
    {
        List<Item_Data> keys = new List<Item_Data>(Supply_Divide.Keys);
        foreach (var key in keys)
        {
            Supply_Divide[key] = 0;
            //Debug.Log("物品" + key + "的存货为" + Supply_Divide[key]);
        }
    }
    public void Random_Divide()
    {
        int Item_Style_Num = SO.Can_Buy_List.Count;
        int Count = SO.CanBuy_Count;
        Reset_Supply();
        Divide(Item_Style_Num, Count);
    }
    public void Divide(int Style_Num, int Count)
    {
        for (int i = 0; i < Count; i++)
        {
            int choose = Random.Range(0, Style_Num);
            var item = SO.Can_Buy_List[choose];
            if (Supply_Divide.ContainsKey(item))
            {
                Supply_Divide[item]++;
            }
        }
        if(Game_Event.instance.Current_Trader != this)
        {
            return;
        }
        foreach(var i in Supply_Divide)
        {
            //Debug.Log("已分配给" + i.Key + "物品" + i.Value + "个出售数量");
            Game_Event.instance.Send_Spawn_Buy_Button(i.Key, i.Value);
        }
    }
    #endregion
    #region 分配卖
    public void Reset_Sold()
    {
        List<Item_Data> keys = new List<Item_Data>(Sold_Divide.Keys);
        foreach (var key in keys)
        {
            Sold_Divide[key] = 0;
            //Debug.Log("物品" + key + "的可出售量为" + Sold_Divide[key]);
        }
    }
    public void Random_Divide_S()
    {
        int Item_Style_Num = SO.Can_Sell_List.Count;
        int Count = SO.CanSell_Count;
        Reset_Sold();
        Divide_S(Item_Style_Num, Count);
    }
    public void Divide_S(int Style_Num, int Count)
    {
        for (int i = 0; i < Count; i++)
        {
            int choose = Random.Range(0, Style_Num);
            var item = SO.Can_Sell_List[choose];
            if (Sold_Divide.ContainsKey(item))
            {
                Sold_Divide[item]++;
            }
        }
        if (Game_Event.instance.Current_Trader != this)
        {
            return;
        }
        foreach (var i in Sold_Divide)
        {
            //Debug.Log("已分配给" + i.Key + "物品" + i.Value + "个出售数量");
            Game_Event.instance.Send_Spawn_Sell_Button(i.Key, i.Value);
        }
    }
    #endregion
    public void OnShopOpen(bool isOpen)
    {
        if (!isOpen)
        {
            return;
        }
        if (Game_Event.instance.Current_Trader != this)
        {
            return;
        }
        Random_Divide_S();
        //if (already_Init)
        //{
        //    return;
        //}
        if (isOpen)
        {
            Random_Divide();
            already_Init = true;
        }
    }
    public void Reset_Shop_Init()
    {
        already_Init = false;
    }
    public void Refresh_B()
    {
        if (Game_Event.instance.Current_Trader != this)
        {
            return;
        }
        Game_Event.instance.Refresh_Buy_List();
        Reset_Supply();
        //already_Init = false;
        //OnShopOpen(true);
        Random_Divide();
    }
    public void Refresh_BS()
    {
        if (Game_Event.instance.Current_Trader != this)
        {
            return;
        }
        Game_Event.instance.Refresh_Sell_List();
        Reset_Sold();
        //already_Init = false;
        //OnShopOpen(true);
        Random_Divide_S();
    }
}
