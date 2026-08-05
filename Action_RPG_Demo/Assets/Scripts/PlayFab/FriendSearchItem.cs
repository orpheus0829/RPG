using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendSearchItem : MonoBehaviour
{
    public Button Add;
    public TextMeshProUGUI ID;
    public TextMeshProUGUI UID;

    public string TargetPlayerId;

    public void Awake()
    {
        Add = GetComponentInChildren<Button>();
    }

    public void OnEnable()
    {
        ID.text = string.Empty;
        UID.text = string.Empty;
        TargetPlayerId = null;
        Add.onClick.RemoveAllListeners();
        Add.onClick.AddListener(OnAdd);
    }

    public void OnDisable()
    {
        Add.onClick.RemoveAllListeners();
    }

    public void GetSelfIdAndUid(string id, string uid)
    {
        TargetPlayerId = uid;
        ID.text = $"ID:{id}";
        UID.text = $"UID:{uid}";
    }

    public void OnAdd()
    {
        ExecuteCloudScriptRequest cloudReq = new ExecuteCloudScriptRequest()
        {
            FunctionName = "SendFriendApply",
            FunctionParameter = new Dictionary<string, object>()
            {
                { "TargetPlayFabId", TargetPlayerId }
            }
        };

        PlayFabClientAPI.ExecuteCloudScript(cloudReq, (success) =>
        {
            try
            {
                string tip = "∫√”—…Í«Î“—∑¢ÀÕ";
                if (PickNoticeMgr.instance != null)
                {
                    PickNoticeMgr.instance.ShowFieldTip(tip);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("∑¢ÀÕ…Í«Î“Ï≥££∫" + e);
            }
        }, (error) =>
        {
            Debug.Log($"‘∆∂À±®¥Ì:{error.ErrorMessage}");
            if (PickNoticeMgr.instance != null)
            {
                PickNoticeMgr.instance.ShowFieldTip($"∑¢ÀÕ…Í«Î ß∞‹£∫{error.ErrorMessage}");
            }
        });
    }
}