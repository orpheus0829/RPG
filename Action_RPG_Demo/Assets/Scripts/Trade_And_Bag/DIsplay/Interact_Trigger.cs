using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interact_Trigger : MonoBehaviour
{
    public Player pl;
    public SphereCollider sc;
    [Header("可交互商人列表")]
    public List<Trader> interactableTraders = new List<Trader>();
    [Header("我的位置")]
    public Transform playerTrans;

    public void Awake()
    {
        sc = GetComponent<SphereCollider>();
        pl = GetComponentInParent<Player>();
        playerTrans = pl.transform;

        if (Panel_Mgr.instance.IsPanelVisible(Panel_Mgr.instance.InteractPanel))
        {
            Panel_Mgr.instance.Control_InteractPanel(false);
        }
    }
    public void Update()
    {
        if (pl.InputMove != Vector3.zero)
        {
            Refresh_Interact_State();
        }
        if (interactableTraders.Count <= 0)
        {
            Panel_Mgr.instance.TraderPanel?.HidePanel();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            Trader trader = other.GetComponent<Trader>();
            if (trader != null && !interactableTraders.Contains(trader))
            {
                interactableTraders.Add(trader);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            Trader trader = other.GetComponent<Trader>();
            if (trader != null && interactableTraders.Contains(trader))
            {
                Game_Event.instance.Init_Store -= trader.OnShopOpen;
                interactableTraders.Remove(trader);
            }
        }
    }

    private void SortTraders_By_Distance()
    {
        interactableTraders.Sort((a, b) =>
        {
            float distA = Vector3.Distance(a.transform.position, playerTrans.position);
            float distB = Vector3.Distance(b.transform.position, playerTrans.position);
            return distA.CompareTo(distB);
        });
    }

    public void Refresh_Interact_State()
    {
        SortTraders_By_Distance();

        if (interactableTraders.Count > 0)
        {
            Trader nearest = interactableTraders[0];
            Game_Event.instance.Current_Trader = nearest;
            Game_Event.instance.Init_Store -= nearest.OnShopOpen;
            Game_Event.instance.Init_Store += nearest.OnShopOpen;
            pl.Can_Interact = true;
            Panel_Mgr.instance.Control_InteractPanel(true);
        }
        else
        {
            pl.Can_Interact = false;
            Panel_Mgr.instance.Control_InteractPanel(false);
            Game_Event.instance.Current_Trader = null;
        }
    }
}