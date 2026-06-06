using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Show_Money : MonoBehaviour
{
    public TextMeshProUGUI Coins;
    public void Awake()
    {
        Coins = GetComponent<TextMeshProUGUI>();
    }
    public void Update()
    {
        Coins.text = $"×Ê²ú:\n{PlayerPrefs.GetInt("Money", 0)}";
    }
}
