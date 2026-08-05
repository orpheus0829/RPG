using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FriendApplyWrapper
{
    public List<FriendApplyItem> Items;
}

[System.Serializable]
public class FriendApplyItem
{
    public string PlayFabId;
    public string NickName;
}

public class CallList : MonoBehaviour
{
    public GameObject SingleSummon;
    public GameObject Parent;

    public void OnEnable()
    {
        Game_Event.instance.RefrshInvitationList += RefreshInviteList;
        RefreshInviteList();
    }

    public void OnDisable()
    {
        Game_Event.instance.RefrshInvitationList -= RefreshInviteList;
    }

    public void RefreshInviteList()
    {
        ClearAllItems();
        GetUserDataRequest request = new GetUserDataRequest();
        request.Keys = new List<string> { "FriendApply" };

        PlayFabClientAPI.GetUserData(request, res =>
        {
            if (res.Data == null || !res.Data.ContainsKey("FriendApply"))
            {
                Debug.Log("没有好友申请数据");
                return;
            }
            string json = res.Data["FriendApply"].Value;
            Debug.Log("原始申请数据:" + json);
            FriendApplyWrapper data = null;
            try
            {
                data = JsonUtility.FromJson<FriendApplyWrapper>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError("解析失败: " + e.Message);
                return;
            }

            if (data == null || data.Items == null || data.Items.Count == 0)
            {
                Debug.Log("解析后无有效申请");
                return;
            }

            foreach (var item in data.Items)
            {
                GameObject go = ObjectPoolMgr.instance.GetObj(SingleSummon, Parent.transform);
                SingleInvitation inviteItem = go.GetComponent<SingleInvitation>();
                RectTransform rect = go.GetComponent<RectTransform>();
                rect.localScale = Vector3.one;
                inviteItem.GetInfo(item.NickName, item.PlayFabId);
            }
        }, err =>
        {
            PickNoticeMgr.instance?.ShowFieldTip($"读取申请失败:{err.ErrorMessage}");
        });
    }


    public void ClearAllItems()
    {
        for (int i = Parent.transform.childCount - 1; i >= 0; i--)
        {
            ObjectPoolMgr.instance.PushObj(Parent.transform.GetChild(i).gameObject);
        }
    }
}