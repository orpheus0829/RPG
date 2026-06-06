using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SingleDisplay_Control : MonoBehaviour
{
    public TextMeshProUGUI Num;
    public Image Display;
    public void Awake()
    {
        Num = GetComponentInChildren<TextMeshProUGUI>();
        Display = GetComponentInChildren<Image>();
    }
}
