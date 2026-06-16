using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Panel_Mgr : Base_mgr<Panel_Mgr>
{
    [Header("面板")]
    public BasePanel BagPanel;
    public BasePanel TraderPanel;
    public BasePanel CraftPanel;
    public BasePanel InteractTradePanel;
    public BasePanel InteractChatPanel;
    public BasePanel DialoguePanel;

    public BasePanel BuyPanel;
    public BasePanel SellPanel;
    [Header("列表")]
    public List<BasePanel> PanelList;
    [Header("属性")]
    public bool _ispanelopen;
    public bool IsPanelOpen
    {
        get
        {
            return _ispanelopen;
        }
        set
        {
            if (_ispanelopen != value)
            {
                _ispanelopen = value;
                On_PanelChanged(value);
            }
        }
    }
    protected override void Awake()
    {
        base.Awake();
        if (instance == this)
        {
            DontDestroyOnLoad(this.gameObject);
        }
        FindAllPanel();
    }
    public void On_PanelChanged(bool v)
    {
        if (v)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        //Debug.Log($"当前面板为{v}");
    }
    public void FindAllPanel()
    {
        PanelList.Clear();
        FieldInfo[] fields = typeof(Panel_Mgr).GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            if (field.FieldType == typeof(BasePanel))
            {
                BasePanel basePanel = (BasePanel)field.GetValue(this);
                if (basePanel != null)
                {
                    PanelList.Add(basePanel);
                }
                else
                {
                    Debug.Log("为空值");
                }
            }
        }
    }
    public void HideAllPanel()
    {
        IsPanelOpen = false;
        foreach (var panel in PanelList)
        {
            panel?.HidePanel();
        }
    }
    public void OpenPanel(BasePanel panel)
    {
        HideAllPanel();
        panel.ShowPanel();
        IsPanelOpen = true;
    }
    public void OpenTraderBuyPanel()
    {
        HideAllPanel();
        TraderPanel.ShowPanel();
        BuyPanel.ShowPanel();
        IsPanelOpen = true;
    }
    public void OpenTraderSellPanel()
    {
        HideAllPanel();
        TraderPanel.ShowPanel();
        SellPanel.ShowPanel();
        IsPanelOpen = true;
    }
    public void Control_InteractPanel(bool open, BasePanel panel)
    {
        panel.gameObject.SetActive(open);
    }
    public bool IsPanelVisible(BasePanel panel)
    {
        if (panel == null)
        {
            return false;
        }
        return panel.IsVisible();
    }
}
