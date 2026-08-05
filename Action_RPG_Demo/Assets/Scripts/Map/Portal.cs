using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Portal : MonoBehaviour
{
    public string TelePos;
    public SphereCollider col;
    public GameObject InteractPic;
    public void Awake()
    {
        col = GetComponent<SphereCollider>();
        col.isTrigger = true;
    }
    public void OnEnable()
    {
        InteractPic.SetActive(false);
        Game_Event.instance.PortalActive -= ReadyToTeleport;
    }
    public void OnDisable()
    {
        Game_Event.instance.PortalActive -= ReadyToTeleport;
    }
    public void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {
            InteractPic.SetActive(true);
            Game_Event.instance.PortalActive += ReadyToTeleport;
        }
    }
    public void OnTriggerExit(Collider col)
    {
        if (col.tag == "Player")
        {
            InteractPic.SetActive(false);
            Game_Event.instance.PortalActive -= ReadyToTeleport;
        }
    }
    public void ReadyToTeleport(Transform passenger)
    {
        if (TelePos == string.Empty)
        {
            PickNoticeMgr.instance.ShowDialogueTip("", "还不知道要去哪里呢", 3f);
            return;
        }
        Panel_Mgr.instance.ShowComfirmPanel($"确定前往{TelePos}?", false, () =>
        {
            Panel_Mgr.instance.HideAllPanel();
            LoadingMgr.instance.StartTransition(TelePos, true);
        });
    }
}
