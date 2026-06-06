using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BasePanel : MonoBehaviour
{
    public virtual void ShowPanel()
    {
        gameObject.SetActive(true);
    }
    public virtual void HidePanel()
    {
        gameObject.SetActive(false);
    }
    public virtual  bool IsVisible()
    {
        return gameObject.activeSelf;
    }
}
