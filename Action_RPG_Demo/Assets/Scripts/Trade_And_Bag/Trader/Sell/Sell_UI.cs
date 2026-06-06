using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sell_UI : MonoBehaviour
{
    public GameObject Sell_Button_Prefab;
    public RectTransform Content;
    public List<GameObject> btn_list_S = new List<GameObject>();

    public float Space_Between;
    public float orignal_offsetx;
    public float orignal_offsety;
    public void Awake()
    {

    }
    public void Start()
    {

    }
    public void OnEnable()
    {
        Game_Event.instance.Spawn_Sell_Button += Spawn_Sell_Button;
        Game_Event.instance.Refresh_Sell += Refresh_Sell_Item;

        Refresh_Sell_Item();
        //Game_Event.instance.Init_Store_Panel(true);
    }
    public void OnDisable()
    {
        Game_Event.instance.Spawn_Sell_Button -= Spawn_Sell_Button;
        Game_Event.instance.Refresh_Sell -= Refresh_Sell_Item;
        //foreach(var i in btn_list)
        //{
        //    Destroy(i.gameObject);
        //}
        //btn_list_S.Clear();
    }
    public void Spawn_Sell_Button(Item_Data item, int max_store)
    {
        GameObject btn = Instantiate(Sell_Button_Prefab, Content);
        btn.transform.SetParent(Content);
        RectTransform btnRect = btn.GetComponent<RectTransform>();
        //Debug.Log("生成" + btn);

        float yPos = -btn_list_S.Count * Space_Between;
        btnRect.anchoredPosition = new Vector2(orignal_offsetx, yPos - orignal_offsety);

        btn_list_S.Add(btn);
        Sold_Control control = btn.gameObject.GetComponent<Sold_Control>();
        control.Sell_Item = item;
        control.Max_Sold = max_store;
        control.Current_Sold = Game_Event.instance.Last_Item_For_Sell(item) < max_store ? Game_Event.instance.Last_Item_For_Sell(item) : max_store;
    }
    public void Refresh_Sell_Item()
    {
        foreach (var i in btn_list_S)
        {
            Destroy(i.gameObject);
        }
        btn_list_S.Clear();
        //Debug.Log("“btn_list大小:" + btn_list_S.Count);
    }
}
