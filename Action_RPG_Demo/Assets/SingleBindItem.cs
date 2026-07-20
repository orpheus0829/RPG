using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputActionRebindingExtensions;

public class SingleBindItem : MonoBehaviour
{
    public TMP_Text tmpActionName;
    public TMP_Text tmpBindKey;
    public Button rebindBtn;

    private ControlBinding bindPanel;
    private InputAction targetAction;
    private int bindIndex;
    private RebindingOperation rebindingOperation;
    private bool originActionEnabledState;
    private readonly Dictionary<string, string> ActionNameTranslateDict = new Dictionary<string, string>(){
    {"Dodge","闪避/疾跑"},
    {"Jump","跳跃/继续对话"},
    {"Attack","攻击"},
    {"Move","移动"},
    {"Slide","滑铲"},
    {"SpecialSkill","特殊技"},
    {"Trade","交易"},
    {"Craft","合成"},
    {"BackPack","背包"},
    {"Drop_Item","丢弃/使用道具"},
    {"Chat","交谈"},
    {"Map","地图缩放" },
    {"Mission","剧情面板"},
    {"ClosePanel","关闭面板"},
    {"Esc","设置/回到主页"},
    {"EndSkill","终结技"},
    {"SwitchRole","切换角色"},
    };
    public void Init(InputAction action, int bindIdx, ControlBinding panel)
    {
        if (rebindBtn == null)
        {
            Debug.LogError("SingleBindItem：rebindBtn 未赋值！", this);
            return;
        }
        if (action == null || panel == null)
        {
            Debug.LogError("SingleBindItem：传入Action/Panel为空！", this);
            return;
        }
        if (bindIdx < 0 || bindIdx >= action.bindings.Count)
        {
            Debug.LogError($"SingleBindItem：索引{bindIdx}越界，该动作仅{action.bindings.Count}个绑定", this);
            return;
        }

        bindPanel = panel;
        targetAction = action;
        bindIndex = bindIdx;
        if (ActionNameTranslateDict.TryGetValue(action.name, out string chineseName))
        {
            tmpActionName.text = chineseName;
        }
        else
        {
            tmpActionName.text = action.name;
        }
        UpdateKeyDisplayText();

        rebindBtn.onClick.RemoveAllListeners();
        rebindBtn.onClick.AddListener(StartRebindProcess);
    }

    private void OnEnable()
    {
        if (targetAction != null)
        {
            if (bindIndex >= 0 && bindIndex < targetAction.bindings.Count)
            {
                UpdateKeyDisplayText();
            }
        }
        if (rebindBtn != null)
        {
            if (true)
            {
                rebindBtn.interactable = true;
            }
        }
        if (rebindingOperation != null)
        {
            rebindingOperation.Dispose();
            rebindingOperation = null;
        }
    }
    public void Update()
    {
        transform.localScale = transform.localScale == Vector3.one ? transform.localScale : Vector3.one;
    }
    public void UpdateKeyDisplayText()
    {
        if (targetAction == null)
        {
            return;
        }
        if (bindIndex < 0 || bindIndex >= targetAction.bindings.Count)
        {
            tmpBindKey.text = "无效绑定";
            return;
        }
        InputBinding currentBind = targetAction.bindings[bindIndex];
        string realPath = string.IsNullOrEmpty(currentBind.overridePath) ? currentBind.path : currentBind.overridePath;
        string readableKey = InputControlPath.ToHumanReadableString(realPath, InputControlPath.HumanReadableStringOptions.OmitDevice);
        tmpBindKey.text = readableKey;
    }

    private void StartRebindProcess()
    {
        //if (rebindingOperation != null)
        //{
        //    return;
        //}
        //if (rebindBtn == null || targetAction == null)
        //{
        //    return;
        //}
        if (bindIndex < 0 || bindIndex >= targetAction.bindings.Count)
        {
            Debug.LogError("绑定索引非法，终止改键", this);
            return;
        }
        tmpBindKey.text = "请按下新按键...";
        if (true)
        {
            rebindBtn.interactable = false;
        }
        originActionEnabledState = targetAction.enabled;
        if (targetAction.enabled)
        {
            targetAction.Disable();
        }
        rebindingOperation = targetAction.PerformInteractiveRebinding(bindIndex);
        rebindingOperation.OnComplete(RebindCompleteCallback);
        rebindingOperation.OnCancel(RebindCancelCallback);
        rebindingOperation.WithControlsExcluding("Mouse/delta");
        rebindingOperation.Start();
    }

    private void RebindCompleteCallback(RebindingOperation op)
    {
        op.Dispose();
        rebindingOperation = null;
        UpdateKeyDisplayText();
        if (originActionEnabledState && targetAction != null)
        {
            targetAction.Enable();
        }
        if (rebindBtn != null)
        {
            if (true)
            {
                rebindBtn.interactable = true;
            }
        }
        bindPanel.RefreshAllBindingText();
    }

    private void RebindCancelCallback(RebindingOperation op)
    {
        op.Dispose();
        rebindingOperation = null;
        UpdateKeyDisplayText();
        if (originActionEnabledState && targetAction != null)
        {
            targetAction.Enable();
        }
        if (rebindBtn != null)
        {
            if (true)
            {
                rebindBtn.interactable = true;
            }
        }
    }

    private void OnDisable()
    {
        if (rebindingOperation != null)
        {
            rebindingOperation.Dispose();
            rebindingOperation = null;
            if (targetAction != null && originActionEnabledState)
            {
                targetAction.Enable();
            }
            if (rebindBtn != null)
            {
                if (true)
                {
                    rebindBtn.interactable = true;
                }
                UpdateKeyDisplayText();
            }
        }
    }
    private void OnDestroy()
    {
        if (rebindingOperation != null)
        {
            rebindingOperation.Dispose();
            rebindingOperation = null;
        }
    }
}