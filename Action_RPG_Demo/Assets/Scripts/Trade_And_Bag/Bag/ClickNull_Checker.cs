using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ClickNull_Checker : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        Introduction_Mrg.instance.StopTrack();
    }
}
