using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Introduction_Mrg : MonoBehaviour
{
    public static Introduction_Mrg instance { private set; get; }
    public event Action<Item_Data> ClickOnItem;
    [Header("ºÚΩÈ√Ê∞Â")]
    public TextMeshProUGUI Intro_Name;
    public Image Intro_Image;
    public TextMeshProUGUI Intro_Kind;
    public TextMeshProUGUI Intro_Value;
    public TextMeshProUGUI Intro_Damage;
    public TextMeshProUGUI Intro_Introduce;
    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    public void OnItem(Item_Data item)
    {
        ClickOnItem?.Invoke(item);
    }
}
