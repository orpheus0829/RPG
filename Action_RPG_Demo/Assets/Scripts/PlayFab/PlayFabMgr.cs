using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayFabMgr : MonoBehaviour
{
    public TMP_InputField Id;
    public TMP_InputField Pwd;
    public TextMeshProUGUI Notice;
    public Button LoginBtn;

    public void Awake()
    {
        LoginBtn.onClick.RemoveAllListeners();
        LoginBtn.onClick.AddListener(Login);
        Notice.text = "";
    }

    public void OnDestroy()
    {
        LoginBtn.onClick.RemoveAllListeners();
    }
    public void ShowNotice(string msg)
    {
        StopAllCoroutines();
        Notice.text = msg;
        StartCoroutine(ClearNoticeAfterSeconds(3f));
    }

    public IEnumerator ClearNoticeAfterSeconds(float delay)
    {
        yield return new WaitForSeconds(delay);
        Notice.text = string.Empty;
    }

    public void Login()
    {
        string customId = Id.text.Trim();
        string inputPwd = Pwd.text;

        if (string.IsNullOrEmpty(customId) || string.IsNullOrEmpty(inputPwd))
        {
            ShowNotice("账号或密码不能为空");
            return;
        }

        if (inputPwd.Length < 6)
        {
            ShowNotice("密码长度至少6位");
            return;
        }

        var req = new LoginWithCustomIDRequest
        {
            CustomId = customId,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(req, (loginResult) =>
        {
            PlayFabClientAPI.GetUserData(new GetUserDataRequest(), (dataRes) =>
            {
                Dictionary<string, UserDataRecord> userData = dataRes.Data;

                bool hasPassword = false;
                string savedPwd = "";

                if (userData != null)
                {
                    if (userData.ContainsKey("Password"))
                    {
                        hasPassword = true;
                        savedPwd = userData["Password"].Value;
                    }
                }

                if (!hasPassword)
                {
                    var saveReq = new UpdateUserDataRequest();
                    saveReq.Data = new Dictionary<string, string>();
                    saveReq.Data["Password"] = inputPwd;
                    PlayFabClientAPI.UpdateUserData(saveReq, _ =>
                    {
                        ShowNotice($"账号 {customId} 不存在，已创建并登录");
                        OnEnterGame();
                    }, OnError);
                }
                else
                {
                    if (savedPwd == inputPwd)
                    {
                        ShowNotice($"账号密码匹配，登录成功 {customId}");
                        OnEnterGame();
                    }
                    else
                    {
                        ShowNotice($"账号{customId}已存在，但密码不匹配");
                        Pwd.text = string.Empty;
                        PlayFabClientAPI.ForgetAllCredentials();
                    }
                }
            }, OnError);

        }, OnError);
    }

    void OnEnterGame()
    {
        SceneManager.LoadSceneAsync("Start");
    }

    public void OnError(PlayFabError error)
    {
        string errMsg = error.ErrorMessage;
        ShowNotice("错误:" + errMsg);
        Debug.Log(error.GenerateErrorReport());
    }
}