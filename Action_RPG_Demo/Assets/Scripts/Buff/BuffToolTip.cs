using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuffToolTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject Tip;
    public TextMeshProUGUI TipContent;
    public BuffSO buffData;

    public void Awake()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponentInChildren<TextMeshProUGUI>(true) != null)
            {
                Tip = child.gameObject;
                break;
            }
        }
        if (Tip != null)
        {
            TipContent = Tip.GetComponentInChildren<TextMeshProUGUI>(true);
            RectTransform tipRect = Tip.GetComponent<RectTransform>();
            tipRect.anchorMin = Vector2.zero;
            tipRect.anchorMax = Vector2.zero;
            tipRect.pivot = Vector2.zero;

            Tip.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!buffData || !Tip || !TipContent)
        {
            return;
        }
        TipContent.text = $"{buffData.BuffName}\n\t{buffData.BuffIntro}";
        RectTransform tipRect = Tip.GetComponent<RectTransform>();
        tipRect.anchoredPosition = (Vector2)Input.mousePosition;
        Tip.SetActive(true);
    }
    public void Update()
    {
        if (Tip && Tip.activeSelf)
        {
            RectTransform tipRect = Tip.GetComponent<RectTransform>();
            tipRect.anchoredPosition = (Vector2)Input.mousePosition - new Vector2(270, 75);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!Tip)
        {
            return;
        }
        Tip.SetActive(false);
    }
}