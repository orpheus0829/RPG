using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Drop_gameObject : MonoBehaviour
{
    [Header("数据盒")]
    public Item_Data item_Data;
    [Header("属性")]
    public Sprite Picture;
    //public Sprite Introduction_Pic;
    public string Drop_name;
    public int Drop_id;
    public int Drop_Height;
    public int Drop_Width;
    public int Drop_PriceValue;
    public string Drop_Description;
    [Header("具体类别")]
    public Item_Kind Drop_Kind;
    public Weapon Drop_weapon;
    public void Awake()
    {
        Picture = item_Data.Display_In_Backpacks;
        //Introduction_Pic = item_Data.Introduction_Image;
        Drop_name = item_Data.item_name;
        Drop_id = item_Data.item_id;
        Drop_Height = item_Data.Height;
        Drop_Width = item_Data.Width;
        Drop_PriceValue = item_Data.PriceValue;
        Drop_Description = item_Data.Introduction;
        Drop_Kind = item_Data.item_Kind;
        if (Drop_Kind == Item_Kind.Weapon_Kind)
        {
            Drop_weapon = new Weapon();
            Drop_weapon.weapon_Damage = item_Data.weapon.weapon_Damage;
        }
        else
        {
            Drop_weapon = null;
        }
    }
    public void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Player")
        {
            //写物品被吸收的代码

            bool Pick=other.gameObject.GetComponent<Player_Bag>().Pick_Up(this.item_Data);
            if (Pick)
            {
                other.gameObject.GetComponent<Player_Bag>().resort_list.Add(item_Data);
                Destroy(this.gameObject);
            }
        }
    }
}
