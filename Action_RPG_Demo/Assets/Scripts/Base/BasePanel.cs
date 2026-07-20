using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BasePanel : MonoBehaviour
{
    public string BindTag = "";
    public bool HideControl = true;
    public bool ConstantShow = false;
    public void Awake()
    {

    }
    public void Start()
    {
        BindTag = gameObject.name;
        Panel_Mgr.instance.AutoBindAllPanel();
        HidePanel();
        if (ConstantShow)
        {
            ShowPanel();
        }
    }
    public virtual void ShowPanel()
    {
        gameObject.SetActive(true);
    }
    public virtual void HidePanel()
    {
        if (this == null || !gameObject.activeInHierarchy)
        {
            return;
        }
        gameObject.SetActive(false);
    }
    public virtual  bool IsVisible()
    {
        return gameObject.activeSelf;
    }
}
