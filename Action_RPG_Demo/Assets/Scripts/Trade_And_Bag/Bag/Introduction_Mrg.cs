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
    public RectTransform rt;

    [Header("简介面板")]
    public TextMeshProUGUI Intro_Name;
    public Image Intro_Image;
    public TextMeshProUGUI Intro_Introduce;

    [Header("鼠标偏移")]
    public float mouseXOffset = 16f;
    public float mouseYOffset = 16f;

    public Canvas _parentCanvas;
    public RectTransform _canvasRect;
    public bool _isShow = false;
    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        rt = GetComponent<RectTransform>();
        rt.pivot = new Vector2(0, 0);
        _parentCanvas = rt.GetComponentInParent<Canvas>();
        _canvasRect = _parentCanvas.GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }
    public void SetContent(Item_Data _Data)
    {
        if (_Data.item_Kind == Item_Kind.Material)
        {
            string a = $"类型:制造材料";
            string content = $"价值:{_Data.PriceValue}\n{a}";
            Intro_Name.text = $"{_Data.item_name}\n\n\n{content}";
            Intro_Introduce.text = $"物品介绍:\n{_Data.Introduction}";
        }
        else if (_Data.item_Kind == Item_Kind.Consumable)
        {
            string a = $"类型:消耗品";
            string content = $"价值:{_Data.PriceValue}\n{a}";
            Intro_Name.text = $"{_Data.item_name}\n\n\n{content}";
            Intro_Introduce.text = $"物品介绍:\n{_Data.Introduction}\n{_Data.buff.BuffName}:{_Data.buff.BuffIntro}";
        }
        else if (_Data.item_Kind == Item_Kind.Weapon)
        {
            string a = $"类型:装备";
            string content = $"价值:{_Data.PriceValue}\n{a}";
            Intro_Name.text = $"{_Data.item_name}\n\n\n{content}";
            string EquipPos = string.Empty;
            switch (_Data.EquipmentSlot)
            {
                case WeaponKind.Head:
                    EquipPos = "头部";
                    break;
                case WeaponKind.Chest:
                    EquipPos = "胸部";
                    break;
                case WeaponKind.Hand:
                    EquipPos = "手部";
                    break;
                case WeaponKind.Foot:
                    EquipPos = "足部";
                    break;
                case WeaponKind.Armament:
                    EquipPos = "手持";
                    break;
                default:
                    break;
            }
            List<string> attrLines = new List<string>();
            if (_Data.MaxHP != 0)
            {
                attrLines.Add($"血量上限加成:{_Data.MaxHP}%");
            }
            if (_Data.Defense != 0)
            {
                attrLines.Add($"防御力加成:{_Data.Defense}%");
            }
            if (_Data.MoveSpeed != 0)
            {
                attrLines.Add($"移动速度加成:{_Data.MoveSpeed}%");
            }
            if (_Data.Attack != 0)
            {
                attrLines.Add($"攻击力加成:{_Data.Attack}%");
            }
            if (_Data.SpecialGain != 0)
            {
                attrLines.Add($"特殊技积攒速度加成:{_Data.SpecialGain}%");
            }
            if (_Data.EndGain != 0)
            {
                attrLines.Add($"终结技积攒速度加成:{_Data.EndGain}%");
            }

            string attrText = string.Join("\n", attrLines);
            string finalIntro = $"物品介绍:\n{_Data.Introduction}\n装备部位:{EquipPos}";
            if (!string.IsNullOrEmpty(attrText))
            {
                finalIntro += $"\n{attrText}";
            }
            Intro_Introduce.text = finalIntro;
        }
        Intro_Image.sprite = _Data.Display_In_Backpacks;
    }

    public void StartTrack()
    {
        Debug.Log("on");
        _isShow = true;
        gameObject.SetActive(true);
    }

    public void StopTrack()
    {
        Debug.Log("off");
        _isShow = false;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!_isShow)
        {
            return;
        }

        Vector2 screenMouse = Input.mousePosition;
        Vector2 targetScreenPos = screenMouse + new Vector2(mouseXOffset, mouseYOffset);

        rt.ForceUpdateRectTransforms();
        Vector2 size = rt.rect.size;
        float screenW = Screen.width;
        float screenH = Screen.height;
        if (targetScreenPos.x + size.x > screenW)
        {
            targetScreenPos.x = screenMouse.x - mouseXOffset - size.x;
        }
        if (targetScreenPos.y - size.y < 0)
        {
            targetScreenPos.y = screenMouse.y - mouseYOffset - size.y;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect,
            targetScreenPos,
            _parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _parentCanvas.worldCamera,
            out Vector2 localPos
        );
        rt.anchoredPosition = localPos;
    }
}