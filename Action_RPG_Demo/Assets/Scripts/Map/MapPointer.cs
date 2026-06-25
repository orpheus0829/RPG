using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapPointer : MonoBehaviour, IPointerClickHandler
{
    public GameObject PointTo;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (Panel_Mgr.instance.CurMapStyle == MapStyle.Min || PointTo == null)
        {
            return;
        }
        MiniMapMgr mgr = MiniMapMgr.instance;
        if (mgr.trackingTarget == PointTo)
        {
            mgr.trackingTarget = null;
            NavPathMgr.instance.CloseNavPath();
        }
        else
        {
            mgr.trackingTarget = PointTo;
        }
    }
}
