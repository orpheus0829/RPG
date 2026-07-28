using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSetting : MonoBehaviour
{
    public static MenuSetting instance { get; private set; }
    public float Vollume;
    public bool HaveBornAnim;
    public void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void RefreshToggle()
    {
        HaveBornAnim = !HaveBornAnim;
    }
}
