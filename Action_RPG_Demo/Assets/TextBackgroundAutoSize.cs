using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TextBackgroundAutoSize : MonoBehaviour
{
    public Image backgroundImage;
    [Header("所有文本")]
    public List<TextMeshProUGUI> targetTexts;
    public Vector2 padding = new Vector2(8, 4);

    [Header("动态边界")]
    public bool useLeft = true;
    public bool useRight = true;
    public bool useTop = true;
    public bool useBottom = true;

    private RectTransform ImgRt;

    void Awake()
    {
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }
        if (backgroundImage != null)
        {
            ImgRt = backgroundImage.rectTransform;
        }
    }

    void Update()
    {
        if (!ImgRt)
        {
            return;
        }

        List<TextMeshProUGUI> useTexts = new List<TextMeshProUGUI>();
        if (targetTexts != null && targetTexts.Count > 0)
        {
            useTexts.AddRange(targetTexts);
        }
        else
        {
            TextMeshProUGUI[] finds = GetComponentsInChildren<TextMeshProUGUI>(true);
            useTexts.AddRange(finds);
        }

        useTexts.RemoveAll(x => x == null);
        if (useTexts.Count == 0)
        {
            return;
        }

        float worldLeft = float.MaxValue;
        float worldRight = float.MinValue;
        float worldBottom = float.MaxValue;
        float worldTop = float.MinValue;

        foreach (var tmp in useTexts)
        {
            RectTransform textRt = tmp.rectTransform;
            Bounds bounds = tmp.textBounds;

            Vector3 wMin = textRt.TransformPoint(bounds.min);
            Vector3 wMax = textRt.TransformPoint(bounds.max);

            worldLeft = Mathf.Min(worldLeft, wMin.x);
            worldRight = Mathf.Max(worldRight, wMax.x);
            worldBottom = Mathf.Min(worldBottom, wMin.y);
            worldTop = Mathf.Max(worldTop, wMax.y);
        }

        Transform imgTrans = ImgRt.transform;
        Vector2 originalLocalMin = imgTrans.InverseTransformPoint(new Vector3(worldLeft, worldBottom));
        Vector2 originalLocalMax = imgTrans.InverseTransformPoint(new Vector3(worldRight, worldTop));

        Vector2 localMin = originalLocalMin;
        Vector2 localMax = originalLocalMax;
        if (!useLeft)
        {
            localMin.x = ImgRt.anchoredPosition.x - ImgRt.sizeDelta.x * 0.5f;
        }
        if (!useRight)
        {
            localMax.x = ImgRt.anchoredPosition.x + ImgRt.sizeDelta.x * 0.5f;
        }
        if (!useBottom)
        {
            localMin.y = ImgRt.anchoredPosition.y - ImgRt.sizeDelta.y * 0.5f;
        }
        if (!useTop)
        {
            localMax.y = ImgRt.anchoredPosition.y + ImgRt.sizeDelta.y * 0.5f;
        }

        Vector2 totalSize = localMax - localMin;
        ImgRt.sizeDelta = totalSize + padding * 2;
    }
}