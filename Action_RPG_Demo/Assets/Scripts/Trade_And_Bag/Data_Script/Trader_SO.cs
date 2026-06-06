using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Trader_Data", menuName = "Trader/Trader_Data")]
public class Trader_SO : ScriptableObject
{
    [Header("供货数量")]
    [Range(1, 100)] public int CanBuy_Count;
    [Header("可提供")]
    public List<Item_Data> Can_Buy_List;
    [Header("收购数量")]
    [Range(1, 100)] public int CanSell_Count;
    [Header("可收购")]
    public List<Item_Data> Can_Sell_List;
    [Space]
    [Header("供货刷新时间")]
    public float Rrstore_Time;
}
