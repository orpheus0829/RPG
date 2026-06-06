using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[CreateAssetMenu (fileName = "Item_Data",menuName = "Data/Item_Data")]
public class Item_Data : ScriptableObject
{
    //public GameObject Drop_Item;
    public Sprite Display_In_Backpacks;
    public GameObject Drop;
    //public Sprite Introduction_Image;
    public string item_name;
    public int item_id;
    public int Height;
    public int Width;
    public int PriceValue;
[TextArea(1,10)] public string Introduction;
    [Space]
    public Item_Kind item_Kind;
    public Weapon weapon;
}
public enum Item_Kind
{
    Weapon_Kind,
    Common_Kind,
}
[Serializable]
public class Weapon
{
    public float weapon_Damage;
}

