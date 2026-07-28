using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[CreateAssetMenu (fileName = "Item_Data",menuName = "Data/Item_Data")]
public class Item_Data : ScriptableObject
{
    public Sprite Display_In_Backpacks;
    public GameObject Drop;
    public bool Stackable;
    public int StackMax = 1;
    public string item_name;
    public int item_id;
    public int Height;
    public int Width;
    public int PriceValue;
[TextArea(1,10)] public string Introduction;
    [Space]
    public Item_Kind item_Kind;
    public BuffSO buff;

    public WeaponKind EquipmentSlot;
    public float MaxHP;
    public float Defense;
    public float MoveSpeed;
    public float Attack;
    public float SpecialGain;
    public float EndGain;
}
public enum Item_Kind
{
    Material,
    Consumable,
    Weapon,
}
public enum WeaponKind
{
    Head,
    Chest,
    Hand,
    Foot,
    Armament,
}

