using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crafting_UI : MonoBehaviour
{
    public GameObject Crafting_Button;
    public RectTransform content;
    public List<GameObject> CraftButton_List = new List<GameObject>();

    public int Space_Between;
    public int original_offsetx;
    public int original_offsety;
    public RectTransform original;
    public void OnEnable()
    {
        Game_Event.instance.Spawn_Crafting_Button -= Crafting_Spawner;
        Game_Event.instance.Spawn_Crafting_Button += Crafting_Spawner;
    }
    public void OnDisable()
    {
        Game_Event.instance.Spawn_Crafting_Button -= Crafting_Spawner;
    }
    public void Crafting_Spawner(Crafting_SO craft,int index)
    {
        //生成按钮
        GameObject btn = Instantiate(Crafting_Button, content);
        btn.transform.SetParent(content);
        RectTransform btnRect = btn.GetComponent<RectTransform>();

        btnRect.anchoredPosition = new Vector2(original_offsetx, original_offsety - index * Space_Between);
        CraftButton_List.Add(btn);
        Single_Craft_UI single_Craft = btn.GetComponent<Single_Craft_UI>();
        single_Craft.crafting = craft;
        Debug.Log("生成第" + index + "个按钮，按钮内容是" + craft.Map_Name + ",坐标为" + btnRect.anchoredPosition);
    }
}
