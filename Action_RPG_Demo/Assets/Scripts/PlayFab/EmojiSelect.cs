using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmojiSelect : MonoBehaviour
{
    public string EmojiTag;
    public Image ShowOut;
    public Button SelectBtn;
    public void Awake()
    {
        SelectBtn = GetComponent<Button>();
        ShowOut = GetComponent<Image>();
    }
    public void OnEnable()
    {
        SelectBtn.onClick.RemoveAllListeners();
        SelectBtn.onClick.AddListener(OnSelect);
    }
    public void OnDisable()
    {
        SelectBtn.onClick.RemoveAllListeners();
    }
    public void OnSelect()
    {
        Game_Event.instance.ShowEmoji(EmojiTag);
    }
}
