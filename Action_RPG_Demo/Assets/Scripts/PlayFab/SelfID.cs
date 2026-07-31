using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SelfID : MonoBehaviour
{
    public TextMeshProUGUI t;
    public string id;
    public string uniqueid;

    public void Awake()
    {
        t = GetComponent<TextMeshProUGUI>();
        t.text = "等待获取账号...";
    }

    void Start()
    {
        TimeMgr.instance.CreateTimer(TimeMgr.TimerMode.RealTimeUnscaled, 0, 2f, null, () =>
        {
            GetAccount();
        });
    }
    public void GetAccount()
    {
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(),
        res =>
        {
            id = res.AccountInfo.CustomIdInfo.CustomId;
            uniqueid = res.AccountInfo.PlayFabId;

            t.text = $"ID:{id}\nUID:{uniqueid}";
            Debug.Log($"已加载账户 {id} \\ {uniqueid}");
        },
        error =>
        {
            t.text = "获取失败";
            Debug.LogError(error.ErrorMessage);
        });
    }
}