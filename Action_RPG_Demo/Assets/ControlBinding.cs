using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class ControlBinding : MonoBehaviour
{
    public Transform bindingParents;
    public GameObject singleBinding;
    public PlayerInput playerInput;

    private InputActionAsset inputAsset;

    public void Awake()
    {
        if (playerInput == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerInput = playerObj.GetComponent<PlayerInput>();
            }
        }
        RefreshInputAsset();
    }
    public void Update()
    {
        transform.localScale = transform.localScale == Vector3.one ? transform.localScale : Vector3.one;
    }
    private void RefreshInputAsset()
    {
        if (playerInput != null)
        {
            inputAsset = playerInput.actions;
        }
        else
        {
            inputAsset = null;
        }
    }

    public void OnEnable()
    {
        RefreshInputAsset();
        Transform childContainer = bindingParents.transform;
        for (int i = childContainer.childCount - 1; i >= 0; i--)
        {
            GameObject childObj = childContainer.GetChild(i).gameObject;
            ObjectPoolMgr.instance.PushObj(childObj);
        }
        GenerateAllBindingItems();
    }

    public void OnDisable()
    {
        Transform childContainer = bindingParents.transform;
        for (int i = childContainer.childCount - 1; i >= 0; i--)
        {
            GameObject childObj = childContainer.GetChild(i).gameObject;
            ObjectPoolMgr.instance.PushObj(childObj);
        }
    }

    private void GenerateAllBindingItems()
    {
        if (inputAsset == null)
        {
            Debug.LogWarning("ControlBinding：inputAsset为空，无法生成键位列表");
            return;
        }
        if (singleBinding == null || bindingParents == null)
        {
            return;
        }

        foreach (InputActionMap actionMap in inputAsset.actionMaps)
        {
            foreach (InputAction targetAction in actionMap.actions)
            {
                for (int bindIndex = 0; bindIndex < targetAction.bindings.Count; bindIndex++)
                {
                    InputBinding targetBind = targetAction.bindings[bindIndex];
                    if (targetBind.isComposite || targetBind.isPartOfComposite)
                    {
                        continue;
                    }
                    if (string.IsNullOrEmpty(targetBind.path))
                    {
                        continue;
                    }
                    GameObject itemObj = ObjectPoolMgr.instance.GetObj(singleBinding, bindingParents);
                    SingleBindItem bindItem = itemObj.GetComponent<SingleBindItem>();
                    if (bindItem != null)
                    {
                        bindItem.Init(targetAction, bindIndex, this);
                    }
                }
            }
        }
    }
    public void RefreshAllBindingText()
    {
        foreach (Transform childTrans in bindingParents)
        {
            SingleBindItem bindItem = childTrans.GetComponent<SingleBindItem>();
            if (bindItem != null)
            {
                bindItem.UpdateKeyDisplayText();
            }
        }
    }
    public void SaveBindSetting()
    {
        string saveJson = inputAsset.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("InputBindData", saveJson);
        PlayerPrefs.Save();
    }
    public void LoadBindSetting()
    {
        if (PlayerPrefs.HasKey("InputBindData"))
        {
            string loadJson = PlayerPrefs.GetString("InputBindData");
            inputAsset.LoadBindingOverridesFromJson(loadJson);
            RefreshAllBindingText();
        }
    }
    public void ResetAllBind()
    {
        inputAsset.RemoveAllBindingOverrides();
        RefreshAllBindingText();
    }
}