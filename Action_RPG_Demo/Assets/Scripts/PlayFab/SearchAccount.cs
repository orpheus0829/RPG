using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;

public class SearchAccount : MonoBehaviour
{
    public TMP_InputField Search;

    public GameObject SingleResult;
    public GameObject Parent;

    public Button Find;

    public void Awake()
    {
        Search = GetComponent<TMP_InputField>();
    }

    public void OnEnable()
    {
        Find.onClick.RemoveAllListeners();
        Find.onClick.AddListener(OnFindClick);
    }

    public void OnDisable()
    {
        Find.onClick.RemoveAllListeners();
    }
    public void OnFindClick()
    {
        ExecuteRealSearch(Search.text);
    }

    public void ExecuteRealSearch(string TargetPlayFabId)
    {
        TargetPlayFabId = TargetPlayFabId.Trim();
        ClearAllResultItem();

        if (string.IsNullOrEmpty(TargetPlayFabId))
        {
            return;
        }

        PlayFabClientAPI.GetPlayerProfile(
            new GetPlayerProfileRequest
            {
                PlayFabId = TargetPlayFabId,
                ProfileConstraints = new PlayerProfileViewConstraints
                {
                    ShowDisplayName = true
                }
            },
            (SuccessResult) =>
            {
                var p = SuccessResult.PlayerProfile;
                if (p == null)
                {
                    PickNoticeMgr.instance.ShowFieldTip("没有找到该玩家");
                    return;
                }
                string PlayerId = p.PlayerId;
                string PlayerDisplayName = string.IsNullOrEmpty(p.DisplayName) ? "未设置昵称" : p.DisplayName;

                GameObject SpawnObj = ObjectPoolMgr.instance.GetObj(SingleResult, Parent.transform);
                FriendSearchItem TmpText = SpawnObj.GetComponent<FriendSearchItem>();
                RectTransform rect = SpawnObj.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(800, 150);
                rect.localScale = new Vector3(1, 1, 1);
                TmpText.GetSelfIdAndUid(PlayerDisplayName, PlayerId);
            },
            (ErrorResult) =>
            {
                ClearAllResultItem();
                PickNoticeMgr.instance.ShowFieldTip("找不到该玩家");
            });
    }

    public void ClearAllResultItem()
    {
        for (int i = Parent.transform.childCount - 1; i >= 0; i--)
        {
            ObjectPoolMgr.instance.PushObj(Parent.transform.GetChild(i).gameObject);
        }
    }
}