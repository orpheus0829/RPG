using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeInvitedList : MonoBehaviour
{
    public GameObject BeInvitedPanel;
    public Button Btn;
    public void Awake()
    {
        Btn = GetComponent<Button>();
        BeInvitedPanel.SetActive(false);
    }
    public void OnEnable()
    {
        Btn.onClick.RemoveAllListeners();
        Btn.onClick.AddListener(() =>
        {
            BeInvitedPanel.SetActive(true);
        });
    }
    public void OnDisable()
    {
        Btn.onClick.RemoveAllListeners();
    }
}
