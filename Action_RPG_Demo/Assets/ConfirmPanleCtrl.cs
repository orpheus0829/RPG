using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPanleCtrl : MonoBehaviour
{
    public TextMeshProUGUI TipText;
    public bool Warning;
    public Action AboutToDo;

    public RectTransform BtnParent;
    public GameObject CBtn;
    public GameObject WBtn;
    public void Awake()
    {
        foreach(Transform i in transform)
        {
            TipText = i.GetComponentInChildren<TextMeshProUGUI>();
            if (TipText)
            {
                break;
            }
        }
    }
    public void OnEnable()
    {
        
    }
    public void OnDisable()
    {
        ClearBtn();
    }
    public void BuildCfm(string tip, bool IsWarning, Action action)
    {
        AboutToDo = null;
        TipText.text = tip;
        Warning = IsWarning;
        AboutToDo = action;
        RebuildBtn();
    }
    public void ClearBtn()
    {
        for (var i = BtnParent.childCount - 1; i >= 0; i--)
        {
            GameObject obj = BtnParent.GetChild(i).gameObject;
            ObjectPoolMgr.instance.PushObj(obj);
        }
    }
    public void RebuildBtn()
    {
        ClearBtn();
        Panel_Mgr.instance.IsPanelOpen = true;
        if (Warning)
        {
            GameObject c = ObjectPoolMgr.instance.GetObj(CBtn, BtnParent);
            c.GetComponent<RectTransform>().localScale = Vector3.one;
            Button cb = c.GetComponent<Button>();
            cb.onClick.RemoveAllListeners();
            cb.onClick.AddListener(() =>
            {
                Panel_Mgr.instance.ConfirmPanel.HidePanel();
            });

        }
        else
        {
            GameObject c = ObjectPoolMgr.instance.GetObj(CBtn, BtnParent);
            c.GetComponent<RectTransform>().localScale = Vector3.one;
            GameObject w = ObjectPoolMgr.instance.GetObj(WBtn, BtnParent);
            w.GetComponent<RectTransform>().localScale = Vector3.one;
            Button cb = c.GetComponent<Button>();
            cb.onClick.RemoveAllListeners();
            Button wb = w.GetComponent<Button>();
            wb.onClick.RemoveAllListeners();
            cb.onClick.AddListener(() =>
            {
                if (AboutToDo == null)
                {
                    Debug.Log("Î´¼ì²âµ½ÊÂ¼þ");
                    return;
                }
                AboutToDo?.Invoke();
                Panel_Mgr.instance.ConfirmPanel.HidePanel();
                
            });
            wb.onClick.AddListener(() =>
            {
                Panel_Mgr.instance.ConfirmPanel.HidePanel();
            });

        }
    }
}
