using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmojiList : MonoBehaviour
{
    public TMP_SpriteAsset mainSpriteAsset;
    public GameObject SingleEmoji;

    public void OnEnable()
    {
        CleanAllEmoji();
        SpawnAllEmoji();
    }
    public void OnDisable()
    {
        CleanAllEmoji();
    }

    public void SpawnAllEmoji()
    {
        List<TMP_SpriteAsset> emojiSubAssets = mainSpriteAsset.fallbackSpriteAssets;
        foreach (var subSa in emojiSubAssets)
        {
            if (!subSa)
            {
                continue;
            }
            if (subSa.spriteCharacterTable.Count == 0)
            {
                continue;
            }
            string emojiTag = subSa.spriteCharacterTable[0].name;
            if (string.IsNullOrEmpty(emojiTag))
            {
                continue;
            }
            GameObject emojiObj = ObjectPoolMgr.instance.GetObj(SingleEmoji, transform);
            RectTransform rect = emojiObj.GetComponent<RectTransform>();
            rect.localScale = new Vector3(1, 0.75f, 1);
            EmojiSelect emojiSelect = emojiObj.GetComponent<EmojiSelect>();
            emojiSelect.EmojiTag = emojiTag;
            Texture2D tex = subSa.spriteSheet as Texture2D;
            if (tex)
            {
                if (!tex.isReadable)
                {
                    Debug.Log($"纹理{tex.name}未开启Read和Write的Enabled");
                }
                Sprite previewSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                emojiSelect.ShowOut.sprite = previewSprite;
            }
        }
    }
    public void CleanAllEmoji()
    {
        for(int i = transform.childCount - 1; i >= 0; i--)
        {
            ObjectPoolMgr.instance.PushObj(transform.GetChild(i).gameObject);
        }
    }
}