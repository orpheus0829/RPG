using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SingleInvitation : MonoBehaviour
{
    public Button Agree;
    public Button Disagree;
    public TextMeshProUGUI ID;
    public TextMeshProUGUI UID;

    public string _applicantPlayFabId;

    public void GetInfo(string nickName, string playfabId)
    {
        ID.text = nickName;
        UID.text = playfabId;
        _applicantPlayFabId = playfabId;
    }

    public void OnEnable()
    {
        Agree.onClick.RemoveAllListeners();
        Disagree.onClick.RemoveAllListeners();
        Agree.onClick.AddListener(OnAgreeClick);
        Disagree.onClick.AddListener(OnDisagreeClick);
    }
    public void OnDisable()
    {
        Agree.onClick.RemoveAllListeners();
        Disagree.onClick.RemoveAllListeners();
    }
    public void OnAgreeClick()
    {
        if (string.IsNullOrWhiteSpace(_applicantPlayFabId))
        {
            Destroy(gameObject);
            return;
        }

        var req = new ExecuteCloudScriptRequest
        {
            FunctionName = "AgreeFriendApply",
            FunctionParameter = new Dictionary<string, object>
        {
            { "ApplicantPlayFabId", _applicantPlayFabId }
        }
        };

        PlayFabClientAPI.ExecuteCloudScript(req, (suc) =>
        {
            Debug.Log("云脚本执行成功,申请已删除:" + JsonUtility.ToJson(suc));
            PickNoticeMgr.instance?.ShowFieldTip("已同意好友");
            Game_Event.instance?.RefrshInvitations();
            PlayFabClientAPI.AddFriend(new AddFriendRequest { FriendPlayFabId = _applicantPlayFabId },
            addRes =>
            {
                Debug.Log("我已添加对方为好友");
                PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest(), (res) =>
                {
                    Debug.Log("最终好友总数：" + res.Friends.Count);
                    foreach (var f in res.Friends)
                    {
                        Debug.Log($"好友：{f.TitleDisplayName} | {f.FriendPlayFabId}");
                    }
                    Destroy(gameObject);
                }, (e) => { Destroy(gameObject); });
            },
            addErr =>
            {
                Debug.LogError("添加好友失败：" + addErr.ErrorMessage);
                Destroy(gameObject);
            });
        },
        (err) =>
        {
            Debug.Log("失败：" + err.ErrorMessage);
            PickNoticeMgr.instance?.ShowFieldTip("同意失败");
            Destroy(gameObject);
        });
    }
    public void OnDisagreeClick()
    {
        if (string.IsNullOrWhiteSpace(_applicantPlayFabId))
        {
            Destroy(gameObject);
            return;
        }

        var req = new ExecuteCloudScriptRequest
        {
            FunctionName = "RejectFriendApply",
            FunctionParameter = new Dictionary<string, object>
        {
            { "ApplicantPlayFabId", _applicantPlayFabId }
        }
        };
        PlayFabClientAPI.ExecuteCloudScript(req, (suc) =>
        {
            PickNoticeMgr.instance?.ShowFieldTip("已拒绝");
            Game_Event.instance?.RefrshInvitations();
            Destroy(gameObject);
        },
        (err) =>
        {
            Debug.LogError("拒绝失败: " + err.ErrorMessage);
            PickNoticeMgr.instance?.ShowFieldTip("拒绝失败");
            Destroy(gameObject);
        });
    }

}