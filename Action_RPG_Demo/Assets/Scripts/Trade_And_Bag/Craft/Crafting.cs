using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crafting : MonoBehaviour
{
    public AllCrafting_Maps all_maps;
    public int Map_Nums;
    public bool Have_Init = false;
    public void Awake()
    {
        Game_Event.instance.Init_Craft += All_Craft;
    }
    public void OnDisable()
    {
        Game_Event.instance.Init_Craft -= All_Craft;
    }
    public void All_Craft()
    {
        if (Have_Init)
        {
            return;
        }
        Map_Nums = all_maps.Crafting_Maps.Count;
        for (int i = 0; i < Map_Nums; i++)
        {
            Create_Crafting_Single_Map(all_maps.Crafting_Maps[i], i);
        }
        Have_Init = true;
    }
    public void Create_Crafting_Single_Map(Crafting_SO craft,int index)
    {
        Game_Event.instance.Spawn_Craft_Button(craft,index);
        Debug.Log("生成了" + craft.Map_Name + "合成表");
    }
}
