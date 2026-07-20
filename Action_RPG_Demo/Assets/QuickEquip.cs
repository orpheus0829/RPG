using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickEquip : MonoBehaviour
{
    public Image ToolPic;
    public Item_Data Tool;
    public Image ParentPic;
    public void Awake()
    {
        ToolPic = GetComponent<Image>();
        ParentPic = GetComponentInParent<Image>();
        EquipToHotbar(null);
    }
    public void OnEnable()
    {
        Game_Event.instance.Equip += EquipToHotbar;
        Game_Event.instance.AlreadyEquip += HaveEquip;
    }
    public void OnDisable()
    {
        Game_Event.instance.Equip -= EquipToHotbar;
        Game_Event.instance.AlreadyEquip -= HaveEquip;
    }
    public void EquipToHotbar(Item_Dragger d)
    {
        Item_Data item = d ? d.data : null;
        Tool = item;
        Debug.Log(Tool);
        if (item)
        {
            ToolPic.sprite = item.Display_In_Backpacks;
            Color color = ToolPic.color;
            color.a = 1f;
            ToolPic.color = color;
        }
        else
        {
            ToolPic.sprite = ParentPic.sprite;
            Color color = ToolPic.color;
            color.a = 0f;
            ToolPic.color = color;
        }
    }
    public bool HaveEquip(Item_Dragger dragger)
    {
        return dragger.data == Tool;
    }
}
