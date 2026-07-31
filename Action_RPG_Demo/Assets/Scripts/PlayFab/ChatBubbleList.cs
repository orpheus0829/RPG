using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class GetMessagesResult
{
    public List<ChatMessageData> Messages = new List<ChatMessageData>();
}

[System.Serializable]
public class ChatMessageData
{
    public string SenderId;
    public string Content;
    public string SenderName;
    public long Timestamp;
}

[System.Serializable]
public class ChatItemData
{
    public string senderUid;
    public string content;
    public long Timestamp;
}

public class ChatBubbleList : MonoBehaviour
{
    public Coroutine pollingCoroutine;
    public long lastTimestamp;
    [Header("会话信息")]
    public TextMeshProUGUI NowRoomName;
    public string PeerUid;
    private string SelfUid;

    private Dictionary<string, string> _nameCache = new Dictionary<string, string>();
    private string _selfDisplayName;
    private HashSet<long> shownTimestamps = new HashSet<long>();

    [Header("气泡预制体")]
    public GameObject MyBubble;
    public GameObject OtherBubble;

    [Header("父容器")]
    public RectTransform Parent;

    public List<ChatItemData> messageCache = new List<ChatItemData>();
    public Scrollbar scrollbar;
    public void Awake()
    {
        scrollbar = GetComponentInChildren<Scrollbar>();
    }

    public void OnEnable()
    {
        Game_Event.instance.DownContent += RefreshScroll;
        Game_Event.instance.SwitchNowChat += SetChatPeer;
        Game_Event.instance.SendMes += SpawnBubble;
        SelfUid = PlayFabSettings.staticPlayer.PlayFabId;
        CacheSelfDisplayName();
        if (string.IsNullOrEmpty(SelfUid))
        {
            Debug.Log("尚未登录PlayFab,无法获取自身UID");
        }

        if (pollingCoroutine != null)
        {
            StopCoroutine(pollingCoroutine);
            pollingCoroutine = null;
        }
        pollingCoroutine = StartCoroutine(PollingNewMessages());
    }

    public void OnDisable()
    {
        Game_Event.instance.DownContent -= RefreshScroll;
        Game_Event.instance.SwitchNowChat -= SetChatPeer;
        Game_Event.instance.SendMes -= SpawnBubble;
        if (pollingCoroutine != null)
        {
            StopCoroutine(pollingCoroutine);
            pollingCoroutine = null;
        }
    }

    public void SetChatPeer(string peerUid, string name)
    {
        Debug.Log($"SetChatPeer，peerUid={peerUid}name={name}");
        shownTimestamps.Clear();
        PeerUid = peerUid;
        NowRoomName.text = $"与{name}的聊天";
        ClearAllBubbles();
        messageCache.Clear();
        lastTimestamp = 0;
        Game_Event.instance.DownScroll();
        Game_Event.instance.CloseEmojiPanel();
        Debug.Log($"SetChatPeer完成，协程{(pollingCoroutine != null ? "存活" : "停了")}");
    }

    public void ClearAllBubbles()
    {
        for (int i = Parent.childCount - 1; i >= 0; i--)
        {
            ObjectPoolMgr.instance.PushObj(Parent.GetChild(i).gameObject);
        }
    }

    public void SpawnBubble(string content, string senderUid, string knownSenderName = null, long timestamp = 0)
    {
        if (string.IsNullOrEmpty(content))
        {
            Debug.Log("跳过，content空");
            return;
        }

        if (timestamp > 0 && shownTimestamps.Contains(timestamp))
        {
            Debug.Log($"去重timestamp={timestamp}");
            return;
        }
        if (timestamp > 0)
        {
            shownTimestamps.Add(timestamp);
        }

        GameObject prefab = senderUid == SelfUid ? MyBubble : OtherBubble;
        GameObject bubbleObj = ObjectPoolMgr.instance.GetObj(prefab, Parent);
        bubbleObj.name = $"Bubble_{senderUid}";

        TextMeshProUGUI contentTmp = bubbleObj.GetComponentInChildren<TextMeshProUGUI>(false);
        if (contentTmp)
        {
            contentTmp.text = content;
            RectTransform rect = contentTmp.GetComponent<RectTransform>();
            if (rect && content.Contains("<sprite"))
            {
                rect.anchoredPosition = new Vector2(-300, 130);
            }
        }
        TextMeshProUGUI nameTmp = FindNameTextInDirectChildren(bubbleObj.transform);
        if (nameTmp != null)
        {
            if (!string.IsNullOrEmpty(knownSenderName))
            {
                nameTmp.text = knownSenderName;
                _nameCache[senderUid] = knownSenderName;
            }
            else
            {
                string fallback = _nameCache.ContainsKey(senderUid) ? _nameCache[senderUid] : senderUid;
                nameTmp.text = fallback;
                GetDisplayName(senderUid, realName =>
                {
                    if (nameTmp != null && nameTmp.isActiveAndEnabled)
                    {
                        nameTmp.text = realName;
                    }
                });
            }
        }
        messageCache.Add(new ChatItemData
        {
            senderUid = senderUid,
            content = content,
            Timestamp = 0
        });
        scrollbar.value = 0f;
    }

    public IEnumerator PollingNewMessages()
    {
        Debug.Log("携程开始");
        while (true)
        {
            yield return new WaitForSecondsRealtime(2f);

            try
            {
                if (string.IsNullOrEmpty(PeerUid))
                {
                    Debug.Log("peerUid为空，跳过本次请求");
                    continue;
                }

                Debug.Log($"请求新消息,lastTimestamp={lastTimestamp}, PeerUid={PeerUid}");

                var request = new ExecuteCloudScriptRequest
                {
                    FunctionName = "GetNewMessages",
                    FunctionParameter = new Dictionary<string, object>
                    {
                        { "LastTimestamp", lastTimestamp }
                    }
                };

                PlayFabClientAPI.ExecuteCloudScript(request,
                    (result) =>
                    {
                        if (result.FunctionResult == null)
                        {
                            Debug.Log("FunctionResult为 null");
                            return;
                        }
                        string json;
                        try
                        {
                            json = PlayFab.PluginManager.GetPlugin<PlayFab.ISerializerPlugin>(PlayFab.PluginContract.PlayFab_Serializer)
                                          .SerializeObject(result.FunctionResult);
                        }
                        catch (System.Exception e)
                        {
                            Debug.Log($"序列化失败:{e.Message}");
                            return;
                        }

                        Debug.Log($"收到原来的json:{json}");
                        var wrapper = JsonUtility.FromJson<GetMessagesResult>(json);
                        if (wrapper == null)
                        {
                            Debug.LogError("反序列化失败");
                            return;
                        }
                        if (wrapper.Messages == null)
                        {
                            Debug.Log("Messages为null");
                            return;
                        }

                        Debug.Log($"[获取到{wrapper.Messages.Count}条新消息");
                        foreach (var msg in wrapper.Messages)
                        {
                            if (msg.Timestamp > lastTimestamp)
                            {
                                lastTimestamp = msg.Timestamp;
                            }
                            SpawnBubble(msg.Content, msg.SenderId, msg.SenderName, msg.Timestamp);
                            Game_Event.instance.DownScroll();
                        }
                    },
                    (error) =>
                    {
                        Debug.Log($"调用失败:{error.ErrorMessage}");
                    });
            }
            catch (System.Exception e)
            {
                Debug.Log($"协程循环异常:{e.Message}\n{e.StackTrace}");
            }
        }
    }

    public void CacheSelfDisplayName()
    {
        var req = new GetPlayerProfileRequest
        {
            PlayFabId = SelfUid,
            ProfileConstraints = new PlayerProfileViewConstraints { ShowDisplayName = true }
        };
        PlayFabClientAPI.GetPlayerProfile(req,
            res => { _selfDisplayName = res.PlayerProfile?.DisplayName ?? SelfUid; },
            err => { _selfDisplayName = SelfUid; });
    }

    public TextMeshProUGUI FindNameTextInDirectChildren(Transform bubbleRoot)
    {
        for (int i = 0; i < bubbleRoot.childCount; i++)
        {
            Transform child = bubbleRoot.GetChild(i);
            TextMeshProUGUI tmp = child.GetComponent<TextMeshProUGUI>();
            if (tmp != null && child.name.ToLower().Contains("name"))
                return tmp;
        }
        return null;
    }

    public void GetDisplayName(string uid, System.Action<string> onGot)
    {
        if (string.IsNullOrEmpty(uid)) { onGot?.Invoke("?"); return; }

        if (uid == SelfUid)
        {
            onGot?.Invoke(!string.IsNullOrEmpty(_selfDisplayName) ? _selfDisplayName : SelfUid);
            return;
        }

        if (_nameCache.TryGetValue(uid, out string cached) && !string.IsNullOrEmpty(cached))
        {
            onGot?.Invoke(cached);
            return;
        }

        var req = new GetPlayerProfileRequest
        {
            PlayFabId = uid,
            ProfileConstraints = new PlayerProfileViewConstraints { ShowDisplayName = true }
        };
        PlayFabClientAPI.GetPlayerProfile(req,
            res =>
            {
                string name = res.PlayerProfile?.DisplayName ?? uid;
                _nameCache[uid] = name;
                onGot?.Invoke(name);
            },
            err =>
            {
                _nameCache[uid] = uid;
                onGot?.Invoke(uid);
            });
    }
    public void RefreshScroll()
    {
        if (!scrollbar)
        {
            return;
        }
        scrollbar.value = 0;
    }
}