using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

public class Panel_Mgr : Base_mgr<Panel_Mgr>
{
    public GameObject PanelCollector;
    [Header("常规面板")]
    public BasePanel BagPanel;
    public BasePanel TraderPanel;
    public BasePanel CraftPanel;
    public BasePanel InteractTradePanel;
    public BasePanel InteractChatPanel;
    public BasePanel DialoguePanel;
    public BasePanel MapPanel;
    public BasePanel MissionPanel;
    public BasePanel EscPanel;
    public BasePanel PlayUiPanel;

    public BasePanel BuyPanel;
    public BasePanel SellPanel;

    [Header("确认面板")]
    public BasePanel ConfirmPanel;
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
        ActiveAllPanel(PanelCollector.transform);
        FindAllPanel();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    public void Start()
    {
        
    }
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
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
        if (MapPanel)
        {
            MapPanel.gameObject.SetActive(true);
        }
        InteractChatPanel.GetComponentInChildren<TextMeshProUGUI>().text = Game_Event.instance.Current_Chater ? "对话" : "交互";
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
    private void ActiveAllPanel(Transform root)
    {
        if (root.TryGetComponent(out BasePanel panel))
        {
            root.gameObject.SetActive(true);
        }
        foreach (Transform child in root)
        {
            ActiveAllPanel(child);
        }
    }
    public void AutoBindAllPanel()
    {
        PanelList.Clear();
        GameObject collector = GameObject.Find("Panel_Collector");
        if (!collector)
        {
            return;
        }
        BasePanel[] allPanels = collector.GetComponentsInChildren<BasePanel>(includeInactive: true);
        foreach (var panel in allPanels)
        {
            string tag = panel.BindTag;
            if (string.IsNullOrEmpty(tag))
            {
                continue;
            }
            FieldInfo field = GetType().GetField(tag, BindingFlags.Public | BindingFlags.Instance);
            if (field != null && field.FieldType == typeof(BasePanel))
            {
                field.SetValue(this, panel);
            }
            if (!PanelList.Contains(panel))
            {
                PanelList.Add(panel);
            }
        }
    }
    public void HideAllPanel()
    {
        IsPanelOpen = false;
        TimeMgr.instance.UnPauseGame();
        foreach (var panel in PanelList)
        {
            if (!panel || !panel.HideControl)
            {
                continue;
            }
            if(panel == BuyPanel || panel == SellPanel)
            {
                continue;
            }
            if (panel == InteractTradePanel || panel == InteractChatPanel || panel==MapPanel)
            {
                panel.HidePanel();
                continue;
            }
            if (panel == MapPanel)
            {
                CurMapStyle = MapStyle.Min;
            }
            if (panel.IsVisible())
            {
                panel.PlayHideAnim(() =>
                {
                    panel.HidePanel();
                });
            }
            else
            {
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
        panel.PlayShowAnim();
        IsPanelOpen = true;
        if (panel != DialoguePanel)
        {
            TimeMgr.instance.PauseGame();
        }
    }
    public void OpenTraderBuyPanel()
    {
        HideAllPanel();
        TraderPanel.PlayShowAnim();
        BuyPanel.PlayShowAnim();
        IsPanelOpen = true;
    }
    public void OpenTraderSellPanel()
    {
        HideAllPanel();
        TraderPanel.PlayShowAnim();
        SellPanel.PlayShowAnim();
        IsPanelOpen = true;
    }
    public void Control_InteractPanel(bool open, BasePanel panel)
    {
        if (panel)
        {
            panel.gameObject.SetActive(open);
        }
    }
    public bool IsPanelVisible(BasePanel panel)
    {
        if (panel == null)
        {
            return false;
        }
        return panel.IsVisible();
    }
    public void ShowComfirmPanel(string tip,bool IsWarning,Action action)
    {
        ConfirmPanel.ShowPanel();
        ConfirmPanleCtrl confirmPanle = ConfirmPanel.GetComponentInChildren<ConfirmPanleCtrl>();
        if (!confirmPanle)
        {
            return;
        }
        confirmPanle.BuildCfm(tip, IsWarning, action);
    }
}