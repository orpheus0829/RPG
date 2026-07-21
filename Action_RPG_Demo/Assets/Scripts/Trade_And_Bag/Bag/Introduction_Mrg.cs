using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Introduction_Mrg : MonoBehaviour
{
    public static Introduction_Mrg instance { private set; get; }
    public event Action<Item_Data> ClickOnItem;
    public RectTransform rt;
    public bool CanShow;

    [Header("背包布局")]
    public RectTransform bagImagesRoot;
    public float cellSize;
    public float horizontalSpace;
    public int bagTotalCol;

    [Header("简介面板")]
    public TextMeshProUGUI Intro_Name;
    public Image Intro_Image;
    public TextMeshProUGUI Intro_Value;
    public TextMeshProUGUI Intro_Introduce;

    [Header("鼠标Y偏移")]
    public float mouseYOffset = 400f;
    public float mouseXOffset = 0f;
    public bool followActive;

    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        rt = GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }

    public void OnItem(Item_Data item)
    {
        ClickOnItem?.Invoke(item);
    }

    public void StartTrack()
    {
        if (!CanShow || bagImagesRoot == null) return;
        followActive = true;
        gameObject.SetActive(true);
    }

    public void StopTrack()
    {
        followActive = false;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            bagImagesRoot,
            Input.mousePosition,
            null,
            out Vector2 localMouse
        );

        float cellTotal = cellSize + horizontalSpace;
        float rowMaxX = bagTotalCol * cellTotal;
        float panelW = rt.sizeDelta.x;
        float targetX = rowMaxX;
        if (targetX + panelW > bagImagesRoot.sizeDelta.x)
        {
            targetX = -panelW;
        }

        float targetY = localMouse.y + mouseYOffset;
        rt.anchoredPosition = new Vector2(targetX + mouseXOffset, targetY);
    }
}