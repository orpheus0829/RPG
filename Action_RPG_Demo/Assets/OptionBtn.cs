using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionBtn : MonoBehaviour
{
    public GameObject DetailOption;
    public Transform DetailContent;
    public string t;
    public TextMeshProUGUI BtnText;
    public Button Btn;

    public GameObject currentDetailPanel;

    public void Awake()
    {
        Btn = GetComponent<Button>();
        BtnText = GetComponentInChildren<TextMeshProUGUI>();
        currentDetailPanel = null;
    }

    public void OnEnable()
    {
        BtnText.text = t;
        Btn.onClick.RemoveAllListeners();
        Btn.onClick.AddListener(RebuildDetailOption);
    }

    public void OnDisable()
    {
        BtnText.text = string.Empty;
        Btn.onClick.RemoveAllListeners();
        ClearDetailContent();
    }
    private void ClearDetailContent()
    {
        Transform c = DetailContent.transform;
        for (int i = c.childCount - 1; i >= 0; i--)
        {
            GameObject j = c.GetChild(i).gameObject;
            if (j)
            {
                ObjectPoolMgr.instance.PushObj(j);
            }
        }
        if (currentDetailPanel)
        {
            ObjectPoolMgr.instance.PushObj(currentDetailPanel);
            currentDetailPanel = null;
        }
    }

    public void RebuildDetailOption()
    {
        ClearDetailContent();
        currentDetailPanel = ObjectPoolMgr.instance.GetObj(DetailOption, DetailContent);
        currentDetailPanel.GetComponent<RectTransform>().localScale = Vector3.one;
    }
}