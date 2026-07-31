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

    private bool CheckCustomIdValid(string customId)
    {
        if (customId.Length < 3 || customId.Length > 25)
        {
            ShowNotice("账号必须3-25个字符");
            return false;
        }
        foreach (char c in customId)
        {
            if (!char.IsLetterOrDigit(c) && c != '_')
            {
                ShowNotice("账号仅允许字母,数字,下划线，不能中文和特殊符号");
                return false;
            }
        }
        return true;
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
        if (!CheckCustomIdValid(customId))
        {
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

                if (userData != null && userData.ContainsKey("Password"))
                {
                    hasPassword = true;
                    savedPwd = userData["Password"].Value;
                }

                if (!hasPassword)
                {
                    var saveReq = new UpdateUserDataRequest();
                    saveReq.Data = new Dictionary<string, string>();
                    saveReq.Data["Password"] = inputPwd;

                    PlayFabClientAPI.UpdateUserData(saveReq, _ =>
                    {
                        ForceSetDisplayName(customId);
                    }, OnError);
                }
                else
                {
                    if (savedPwd == inputPwd)
                    {
                        ShowNotice($"账号密码匹配,登录成功 {customId}");
                        ForceSetDisplayName(customId);
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
    public void ForceSetDisplayName(string customId)
    {
        var setNameReq = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = customId
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(setNameReq,
        _ =>
        {
            OnEnterGame();
        },
        nameErr =>
        {
            Debug.LogWarning("设置DisplayName失败:" + nameErr.ErrorMessage);
            OnEnterGame();
        });
    }

    public void OnEnterGame()
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