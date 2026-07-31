using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class FriendList : MonoBehaviour
{
    public GameObject Parent;
    public GameObject SingleFriendPrefab;
    private void Start()
    {
        
    }
    public void OnEnable()
    {
        GetPlayFabFriendsList();
    }
    public void GetPlayFabFriendsList()
    {
        foreach (Transform child in Parent.transform)
        {
            ObjectPoolMgr.instance.PushObj(child.gameObject);
        }

        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest(),
        result =>
        {
            Debug.Log("获取好友成功：" + result.Friends.Count);
            foreach (FriendInfo friend in result.Friends)
            {
                GetFriendDisplayName(friend.FriendPlayFabId, (playFabId, displayName) =>
                {
                    GameObject item = ObjectPoolMgr.instance.GetObj(SingleFriendPrefab, Parent.transform);
                    RectTransform rect = item.GetComponent<RectTransform>();
                    rect.localScale = new Vector3(1, 1, 1);
                    SingleFriend single = item.GetComponent<SingleFriend>();
                    single.GetThisFriendInfo(playFabId, displayName);
                });
            }
        },
        error =>
        {
            Debug.LogError("获取好友列表失败：" + error.ErrorMessage);
        });
    }
    public void GetFriendDisplayName(string targetPlayFabId, System.Action<string, string> callback)
    {
        var req = new GetPlayerProfileRequest
        {
            PlayFabId = targetPlayFabId,
            ProfileConstraints = new PlayerProfileViewConstraints
            {
                ShowDisplayName = true
            }
        };

        PlayFabClientAPI.GetPlayerProfile(req, res =>
        {
            string name = res.PlayerProfile.DisplayName;
            if (string.IsNullOrEmpty(name))
            {
                name = targetPlayFabId;
            }

            callback.Invoke(targetPlayFabId, name);
        }, err =>
        {
            Debug.Log($"获取玩家档案失败 {targetPlayFabId}:{err.ErrorMessage}");
            callback.Invoke(targetPlayFabId, targetPlayFabId);
        });
    }
}