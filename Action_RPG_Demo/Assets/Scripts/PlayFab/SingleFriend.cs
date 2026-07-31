using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SingleFriend : MonoBehaviour
{
    [Header("UI绑定")]
    public TextMeshProUGUI ID;
    //public TextMeshProUGUI IsOnline;
    public Button IntoChat;

    public string friendPlayFabId;

    public void Awake()
    {

    }
    public void OnEnable()
    {
        IntoChat.onClick.RemoveAllListeners();
        IntoChat.onClick.AddListener(OnInChat);
    }
    public void OnDisable()
    {
        IntoChat.onClick.RemoveAllListeners();
    }
    public void GetThisFriendInfo(string id,string displayname)
    {
        friendPlayFabId = id;
        if (!string.IsNullOrEmpty(displayname))
        {
            ID.text = displayname;
        }
        else
        {
            ID.text = id;
        }

        //if (isOnline)
        //{
        //    IsOnline.color = Color.green;
        //    IsOnline.text = "在线";
        //}
        //else
        //{
        //    IsOnline.color = Color.gray;
        //    IsOnline.text = "离线";
        //}
    }
    public void OnInChat()
    {
        Debug.Log("打开与好友的聊天:" + friendPlayFabId);
        Game_Event.instance.SwitchPeer(friendPlayFabId,ID.text);
    }
}