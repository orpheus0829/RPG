using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ArrowUI : MonoBehaviour
{
    public static ArrowUI instance { get; private set; }
    public TextMeshProUGUI Left;
    public TextMeshProUGUI Right;
    public void Awake()
    {
        Left = transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        Right = transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void RefreshArrowUI()
    {
        HallShow h = HallShow.instance;
        int lft = h.CurIndex - 1;
        int rgt = h.CurIndex + 1;
        if (h.CurIndex == 0)
        {
            lft = h.lst.Count - 1;
        }
        else if (h.CurIndex == h.lst.Count - 1)
        {
            rgt = 0;
        }
        Left.text = h.lst[lft].CamName;
        Right.text = h.lst[rgt].CamName;
    }
}
