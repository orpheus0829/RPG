using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Buy_UI : MonoBehaviour
{
    public GameObject Buy_Button_Prefab;
    public RectTransform Content;
    public List<GameObject> btn_list = new List<GameObject>();

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
        Game_Event.instance.Spawn_Buy_Button += Spawn_Buy_Button;
        Game_Event.instance.Refresh_Buy += Refresh_Buy_Item;
    }
    public void OnDisable()
    {
        Game_Event.instance.Spawn_Buy_Button -= Spawn_Buy_Button;
        Game_Event.instance.Refresh_Buy -= Refresh_Buy_Item;
        //foreach(var i in btn_list)
        //{
        //    Destroy(i.gameObject);
        //}
        //btn_list.Clear();
    }
    public void Spawn_Buy_Button(Item_Data item,int max_store)
    {
        GameObject btn = Instantiate(Buy_Button_Prefab, Content);
        btn.transform.SetParent(Content);
        RectTransform btnRect = btn.GetComponent<RectTransform>();
        //Debug.Log("Éú³É" + btn);

        float yPos = -btn_list.Count * Space_Between;
        btnRect.anchoredPosition = new Vector2(orignal_offsetx, yPos - orignal_offsety);

        btn_list.Add(btn);
        Bought_Control control = btn.gameObject.GetComponent<Bought_Control>();
        control.Buy_Item = item;
        control.Max_Store = max_store;
        control.Current_Store = max_store;
    }
    public void Refresh_Buy_Item()
    {
        foreach(var i in btn_list)
        {
            Destroy(i.gameObject);
        }
        btn_list.Clear();
    }
}
