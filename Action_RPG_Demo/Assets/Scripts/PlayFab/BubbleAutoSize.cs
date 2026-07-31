using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum BubbleSide
{
    Left,
    Right,
}
public class BubbleAutoSize : MonoBehaviour
{

    [Header("气泡配置")]
    public BubbleSide fixedSide;
    public Vector2 padding = new Vector2(30f, 20f);

    [Header("宽度限制")]
    public float maxWidth = 1000f;

    public RectTransform bubbleRect;
    public TextMeshProUGUI textComponent;
    public Vector2 lastPreferredSize;

    public void Awake()
    {
        bubbleRect = GetComponent<RectTransform>();
        textComponent = GetComponentInChildren<TextMeshProUGUI>();
        if (!textComponent)
        {
            enabled = false;
            return;
        }
        UpdateAnchorsAndPivot();
    }

    public void LateUpdate()
    {
        Vector2 current = new Vector2(textComponent.preferredWidth, textComponent.preferredHeight);
        if (current != lastPreferredSize)
        {
            lastPreferredSize = current;
            UpdateSize();
        }
    }

    public void RefreshSize()
    {
        lastPreferredSize = Vector2.zero;
    }

    public void UpdateAnchorsAndPivot()
    {
        float ax = (fixedSide == BubbleSide.Left) ? 0f : 1f;
        Vector2 a = new Vector2(ax, 1f);
        bubbleRect.anchorMin = a;
        bubbleRect.anchorMax = a;
        bubbleRect.pivot = new Vector2(ax, 1f);
    }

    public void UpdateSize()
    {
        float prefW = textComponent.preferredWidth;
        float targetW = prefW + padding.x;
        if (maxWidth > 0 && targetW > maxWidth)
        {
            targetW = maxWidth;
        }

        bubbleRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetW);
        Canvas.ForceUpdateCanvases();
        float targetH = textComponent.preferredHeight + padding.y;
        bubbleRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetH);
    }
}
