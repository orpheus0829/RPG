using DG.Tweening;
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
    public BasePanel MapPanel;
    public BasePanel MissionPanel;

    public BasePanel BuyPanel;
    public BasePanel SellPanel;
    [Header("效果")]
    public float FadeInDuration;
    public float FadeOutDuration;
    [Header("列表")]
    public List<BasePanel> PanelList;
    [Header("属性")]
    public MapStyle CurMapStyle;
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
                On_PanelChanged();
            }
        }
    }
    public bool IsFullMapOpen
    {
        get
        {
            return CurMapStyle == MapStyle.Max;
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
    public void On_PanelChanged()
    {
        bool openState = IsPanelOpen || IsFullMapOpen;
        if (openState)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    public void Update()
    {
        MapPanel.gameObject.SetActive(true);
        //if (IsPanelOpen || IsFullMapOpen)
        //{
        //    NavPathMgr.instance.CloseNavPath();
        //}
        //else
        //{
        //    NavPathMgr.instance.OpenNavPath(NavPathMgr.instance.targetPoint);
        //}
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
            if (panel == InteractTradePanel || panel == InteractChatPanel || panel == BuyPanel || panel == SellPanel || panel==MapPanel)
            {
                panel.HidePanel();
                continue;
            }
            if (panel == MapPanel)
            {
                CurMapStyle = MapStyle.Min;
            }
            CanvasGroup cg = panel.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = panel.gameObject.AddComponent<CanvasGroup>();
            }
            if (panel.IsVisible())
            {
                cg.DOFade(0, FadeOutDuration).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    panel.HidePanel();
                });
            }
            else
            {
                cg.alpha = 0;
                panel.HidePanel();
            }
        }
    }
    public void SwitchMap(bool IsOpen)
    {
        CurMapStyle = IsOpen ? MapStyle.Max : MapStyle.Min;
        On_PanelChanged();
    }
    public void OpenPanel(BasePanel panel)
    {
        HideAllPanel();
        RectTransform rt = panel.GetComponent<RectTransform>();
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = panel.gameObject.AddComponent<CanvasGroup>();
        }
        cg.alpha = 0;
        panel.ShowPanel();
        cg.DOFade(1, FadeInDuration).SetEase(Ease.OutQuad);
        IsPanelOpen = true;
    }
    public void OpenTraderBuyPanel()
    {
        HideAllPanel();
        CanvasGroup traderCg = TraderPanel.GetComponent<CanvasGroup>();
        CanvasGroup buyCg = BuyPanel.GetComponent<CanvasGroup>();
        if (traderCg == null)
        {
            traderCg = TraderPanel.gameObject.AddComponent<CanvasGroup>();
        }
        if (buyCg == null)
        {
            buyCg = BuyPanel.gameObject.AddComponent<CanvasGroup>();
        }
        traderCg.alpha = 0;
        buyCg.alpha = 0;
        TraderPanel.ShowPanel();
        BuyPanel.ShowPanel();
        traderCg.DOFade(1, FadeInDuration).SetEase(Ease.OutQuad);
        buyCg.DOFade(1, FadeInDuration).SetEase(Ease.OutQuad);
        IsPanelOpen = true;
    }
    public void OpenTraderSellPanel()
    {
        HideAllPanel();

        CanvasGroup traderCg = TraderPanel.GetComponent<CanvasGroup>();
        CanvasGroup sellCg = SellPanel.GetComponent<CanvasGroup>();
        if (traderCg == null)
        {
            traderCg = TraderPanel.gameObject.AddComponent<CanvasGroup>();
        }
        if (sellCg == null)
        {
            sellCg = SellPanel.gameObject.AddComponent<CanvasGroup>();
        }
        traderCg.alpha = 0;
        sellCg.alpha = 0;
        TraderPanel.ShowPanel();
        SellPanel.ShowPanel();
        traderCg.DOFade(1, FadeInDuration).SetEase(Ease.OutQuad);
        sellCg.DOFade(1, FadeInDuration).SetEase(Ease.OutQuad);
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