using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShowIntro : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Item_Data data;
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (data)
        {
            Introduction_Mrg.instance.SetContent(data);
            Introduction_Mrg.instance.StartTrack();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Introduction_Mrg.instance.StopTrack();
    }
}
