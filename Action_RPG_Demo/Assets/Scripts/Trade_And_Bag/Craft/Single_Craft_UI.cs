using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Single_Craft_UI : MonoBehaviour
{
    [Header("单个按钮合成表数据")]
    public Crafting_SO crafting;
    [Header("单个物品展示")]
    public GameObject Singale_Display;
    [Header("位置调校")]
    //public Vector2 ToLeft;
    //public Vector2 ToRight;
    public float Space;
    public RectTransform btn_mid;
    public RectTransform Left;
    public RectTransform Right;
    public void Awake()
    {
        btn_mid = GetComponent<RectTransform>();
        //Left.anchoredPosition = btn_mid.anchoredPosition - ToLeft;
        //Right.anchoredPosition = btn_mid.anchoredPosition + ToRight;
    }
    public void Start()
    {
        Create_Single(crafting);
    }
    public void Create_Single(Crafting_SO crafting_SO)
    {
        int material_count = crafting_SO.crafting_Materials.Count;
        int product_count = crafting_SO.crafting_Results.Count;
        for(int i = 0; i < material_count; i++)
        {
            GameObject single_materail = ObjectPoolMgr.instance.GetObj(Singale_Display, Left);

            RectTransform rect_m = single_materail.GetComponent<RectTransform>();
            rect_m.anchoredPosition = new Vector2(Left.anchoredPosition.x + i * Space, Left.anchoredPosition.y);

            SingleDisplay_Control control_m = single_materail.GetComponent<SingleDisplay_Control>();
            control_m.Display.sprite = crafting_SO.crafting_Materials[i].Material.Display_In_Backpacks;
            control_m.Num.text = $"{crafting_SO.crafting_Materials[i].Number}";
        }
        for (int j = 0; j < product_count; j++)
        {
            GameObject single_product = ObjectPoolMgr.instance.GetObj(Singale_Display, Right);

            RectTransform rect_p = single_product.GetComponent<RectTransform>();
            rect_p.anchoredPosition = new Vector2(Right.anchoredPosition.x - j * Space, Right.anchoredPosition.y);

            SingleDisplay_Control control_p = single_product.GetComponent<SingleDisplay_Control>();
            control_p.Display.sprite = crafting_SO.crafting_Results[j].Product.Display_In_Backpacks;
            control_p.Num.text = $"{crafting_SO.crafting_Results[j].Res_Number}";
        }
        GetComponent<Craft_Operate>().craft = crafting_SO;
    }
}
