using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ActionBindingText : MonoBehaviour
{
    public PlayerInput playerInput;
    public string ActionCall;
    public string ActionName;
    public Button LinkButton;

    private TextMeshProUGUI t;
    private InputAction CurAction;

    public void Awake()
    {
        t = GetComponent<TextMeshProUGUI>();
        playerInput = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInput>();
        if (!LinkButton)
        {
            Transform parent = transform.parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                Button btn = parent.GetChild(i).GetComponent<Button>();
                if (btn != null && parent.GetChild(i) != transform)
                {
                    LinkButton = btn;
                    break;
                }
            }
        }
    }

    void OnEnable()
    {
        BindActionListen();
    }

    void OnDisable()
    {
        UnBindActionListen();
    }

    public void Update()
    {
        UpdateBinding();
    }
    public void UpdateBinding()
    {
        if (string.IsNullOrEmpty(ActionCall))
        {
            ActionCall = "未知键位";
        }
        if (!playerInput || string.IsNullOrEmpty(ActionName))
        {
            t.text = "未链接键位数据";
            return;
        }
        CurAction = playerInput.actions.FindAction(ActionName);
        if (CurAction == null || CurAction.bindings.Count == 0)
        {
            t.text = "未绑定按键";
            return;
        }
        string key = string.Empty;
        foreach (var i in CurAction.bindings)
        {
            if (i.isComposite || i.isPartOfComposite)
            {
                continue;
            }
            key = i.ToDisplayString();
            break;
        }
        t.text = $"{ActionCall}({key})";
    }
    void BindActionListen()
    {
        if (playerInput == null || string.IsNullOrEmpty(ActionName))
        {
            return;
        }
        CurAction = playerInput.actions.FindAction(ActionName);
        if (CurAction == null)
        {
            return;
        }
        CurAction.started += OnKeyDown;
        CurAction.Enable();
    }

    void UnBindActionListen()
    {
        if (CurAction == null)
        {
            return;
        }
        CurAction.started -= OnKeyDown;
        CurAction = null;
    }
    void OnKeyDown(InputAction.CallbackContext ctx)
    {
        if (LinkButton != null && LinkButton.interactable)
        {
            LinkButton.onClick.Invoke();
            StartCoroutine(ButtonPressEffect());
            Debug.Log($"按下了{ActionName}按键");
        }
    }
    IEnumerator ButtonPressEffect()
    {
        LinkButton.interactable = false;
        yield return new WaitForSeconds(0.03f);
        LinkButton.interactable = true;
    }
}