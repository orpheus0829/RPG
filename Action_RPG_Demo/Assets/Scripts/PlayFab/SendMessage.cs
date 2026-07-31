using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SendChatMessageResult
{
    public bool success;
    public long timestamp;
}
public class SendMessage : MonoBehaviour
{
    public TMP_InputField inputField;
    public string currentPeerId;
    public string MyDisplayName;

    public bool IsSending = false;

    public void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
    }

    public void Update()
    {
        if (inputField && inputField.text != string.Empty && !IsSending)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                SendOut(inputField.text);
            }
        }
    }

    public void OnEnable()
    {
        Game_Event.instance.SwitchNowChat += OnSwitchPeer;
        Game_Event.instance.ShowMood += SendEmoji;
        inputField.onSubmit.RemoveAllListeners();
        inputField.onSubmit.AddListener(SendOut);

        var req = new GetPlayerProfileRequest
        {
            PlayFabId = PlayFabSettings.staticPlayer.PlayFabId,
            ProfileConstraints = new PlayerProfileViewConstraints { ShowDisplayName = true }
        };
        PlayFabClientAPI.GetPlayerProfile(req,
            res => { MyDisplayName = res.PlayerProfile?.DisplayName ?? PlayFabSettings.staticPlayer.PlayFabId; },
            err => { MyDisplayName = PlayFabSettings.staticPlayer.PlayFabId; });
    }

    public void OnDisable()
    {
        Game_Event.instance.SwitchNowChat -= OnSwitchPeer;
        Game_Event.instance.ShowMood -= SendEmoji;
        inputField.onSubmit.RemoveAllListeners();
        IsSending = false;
    }

    public void OnSwitchPeer(string peerid,string name)
    {
        currentPeerId = peerid;
        if (inputField)
        {
            inputField.text = string.Empty;
        }
        IsSending = false;
    }

    public void SendOut(string msg)
    {
        if (IsSending)
        {
            return;
        }

        string content = msg.Trim();
        if (string.IsNullOrEmpty(content))
        {
            inputField.text = string.Empty;
            return;
        }

        string myUid = PlayFabSettings.staticPlayer.PlayFabId;
        if (string.IsNullOrEmpty(myUid))
        {
            Debug.Log("尚未登录PlayFab，不能发送消息");
            PickNoticeMgr.instance?.ShowFieldTip("请先登录");
            return;
        }

        if (string.IsNullOrEmpty(currentPeerId))
        {
            Debug.Log("没有选中聊天对象");
            PickNoticeMgr.instance?.ShowFieldTip("请选择聊天好友");
            return;
        }

        IsSending = true;

        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "SendChatMessage",
            FunctionParameter = new Dictionary<string, object>
            {
                { "ReceiverId", currentPeerId },
                { "Content", content }
            },
            GeneratePlayStreamEvent = false
        };

        PlayFabClientAPI.ExecuteCloudScript(request,
            (result) =>
            {
                long timestamp = 0;
                if (result.FunctionResult != null)
                {
                    var json = PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PlayFab.PluginContract.PlayFab_Serializer)
                                      .SerializeObject(result.FunctionResult);
                    var res = JsonUtility.FromJson<SendChatMessageResult>(json);
                    if (res != null) timestamp = res.timestamp;
                }
                Game_Event.instance.SendMessageToPeer(content, myUid, MyDisplayName, timestamp);
                inputField.text = "";
                IsSending = false;
            },
            (error) =>
            {
                Debug.LogError("发送失败: " + error.ErrorMessage);
                PickNoticeMgr.instance?.ShowFieldTip("发送失败");
                IsSending = false;
            });
        Game_Event.instance.DownScroll();
        Game_Event.instance.CloseEmojiPanel();
    }
    public void SendEmoji(string emojiName)
    {
        string richText = $"<size=300%><sprite name=\"{emojiName}\"></size>";
        SendOut(richText);
        Game_Event.instance.DownScroll();
    }
}