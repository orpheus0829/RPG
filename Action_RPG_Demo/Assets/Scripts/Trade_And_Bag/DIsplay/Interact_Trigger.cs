using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interact_Trigger : MonoBehaviour
{
    public Player pl;
    public SphereCollider sc;
    [Header("可交互商人列表")]
    public List<Trader> interactableTraders = new List<Trader>();
    [Header("可互动对话NPC列表")]
    public List<Dialogue_Set> interactableChatNPCS = new List<Dialogue_Set>();
    [Header("我的位置")]
    public Transform playerTrans;

    public void Awake()
    {
        sc = GetComponent<SphereCollider>();
        pl = GetComponentInParent<Player>();
        playerTrans = pl.transform;

    }
    public void Start()
    {
        if (Panel_Mgr.instance.IsPanelVisible(Panel_Mgr.instance.InteractTradePanel))
        {
            Panel_Mgr.instance.Control_InteractPanel(false, Panel_Mgr.instance.InteractTradePanel);
        }
        if (Panel_Mgr.instance.IsPanelVisible(Panel_Mgr.instance.InteractChatPanel))
        {
            Panel_Mgr.instance.Control_InteractPanel(false, Panel_Mgr.instance.InteractChatPanel);
        }
    }
    public void Update()
    {
        if (pl.InputMove != Vector3.zero)
        {
            Refresh_Interact_State(interactableTraders);
            Refresh_Interact_State(interactableChatNPCS);
        }
        if (interactableTraders.Count <= 0)
        {
            Panel_Mgr.instance.TraderPanel?.HidePanel();
            //Panel_Mgr.instance.IsPanelOpen = false;
        }
        //if (interactableChatNPCS.Count <= 0)
        //{
        //    Panel_Mgr.instance.DialoguePanel?.HidePanel();
        //    //Panel_Mgr.instance.IsPanelOpen = false;
        //}
    }
    private void OnTriggerEnter(Collider other)
    {
        bool HasDialogue = other.TryGetComponent<Dialogue_Set>(out Dialogue_Set dialogue_Set1);
        bool HasTrade = other.TryGetComponent<Trader>(out Trader istrader1);
        bool IsNPC = other.CompareTag("NPC");
        //if (IsNPC && (HasDialogue || HasTrade))
        //{
        //    Trader trader = other.GetComponent<Trader>();
        //    if (trader != null && !interactableTraders.Contains(trader))
        //    {
        //        interactableTraders.Add(trader);
        //    }
        //}
        if (!IsNPC)
        {
            return;
        }
        if (HasTrade)
        {
            //Trader trader = other.GetComponent<Trader>();
            if (istrader1 != null && !interactableTraders.Contains(istrader1))
            {
                interactableTraders.Add(istrader1);
            }
        }
        if (HasDialogue)
        {
            //Dialogue_Set dialogue_set = other.GetComponent<Dialogue_Set>();
            if (dialogue_Set1 != null && !interactableChatNPCS.Contains(dialogue_Set1))
            {
                interactableChatNPCS.Add(dialogue_Set1);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        bool HasDialogue = other.TryGetComponent<Dialogue_Set>(out Dialogue_Set dialogue_Set2);
        bool HasTrade = other.TryGetComponent<Trader>(out Trader istrader2);
        bool IsNPC = other.CompareTag("NPC");
        //if (IsNPC && (HasDialogue || HasTrade))
        //{
        //    Trader trader = other.GetComponent<Trader>();
        //    if (trader != null && interactableTraders.Contains(trader))
        //    {
        //        Game_Event.instance.Init_Store -= trader.OnShopOpen;
        //        interactableTraders.Remove(trader);
        //    }
        //}
        if (!IsNPC)
        {
            return;
        }
        if (HasTrade)
        {
            //Trader trader = other.GetComponent<Trader>();
            if (istrader2 != null && interactableTraders.Contains(istrader2))
            {
                Game_Event.instance.Init_Store -= istrader2.OnShopOpen;
                interactableTraders.Remove(istrader2);
            }
        }
        if (HasDialogue)
        {
            //Dialogue_Set dialogue_set = other.GetComponent<Dialogue_Set>();
            if(dialogue_Set2 != null && interactableChatNPCS.Contains(dialogue_Set2)){
                interactableChatNPCS.Remove(dialogue_Set2);
            }
        }
    }

    private void SortList_By_Distance<T>(List<T> lst)where T:MonoBehaviour
    {
        lst.Sort((a, b) =>
        {
            float distA = Vector3.Distance(a.transform.position, playerTrans.position);
            float distB = Vector3.Distance(b.transform.position, playerTrans.position);
            return distA.CompareTo(distB);
        });
    }

    public void Refresh_Interact_State<T>(List<T> lst) where T : MonoBehaviour
    {
        SortList_By_Distance(lst);

        if (typeof(T) == typeof(Trader))
        {
            if (lst.Count > 0)
            {
                Trader nearest = lst[0] as Trader;
                Game_Event.instance.Current_Trader = nearest;
                Game_Event.instance.Init_Store -= nearest.OnShopOpen;
                Game_Event.instance.Init_Store += nearest.OnShopOpen;
                pl.Can_Trade = true;
                Panel_Mgr.instance.Control_InteractPanel(true, Panel_Mgr.instance.InteractTradePanel);
            }
            else
            {
                pl.Can_Trade = false;
                Panel_Mgr.instance.Control_InteractPanel(false, Panel_Mgr.instance.InteractTradePanel);
                Game_Event.instance.Current_Trader = null;
            }
        }
        else if (typeof(T) == typeof(Dialogue_Set))
        {
            if (!Panel_Mgr.instance.DialoguePanel)
            {
                return;
            }
            DialogueWriter writer = Panel_Mgr.instance.DialoguePanel.GetComponent<DialogueWriter>();
            if (lst.Count > 0)
            {
                Dialogue_Set nearest = lst[0] as Dialogue_Set;
                Game_Event.instance.Current_Chater = nearest;
                pl.Can_Chat = true;
                //writer.CurDialogue = nearest.Cur_Dialogue;
                Panel_Mgr.instance.Control_InteractPanel(true, Panel_Mgr.instance.InteractChatPanel);
            }
            else
            {
                pl.Can_Chat = false;
                Panel_Mgr.instance.Control_InteractPanel(false, Panel_Mgr.instance.InteractChatPanel);
                writer.CurDialogue = null;
                Game_Event.instance.Current_Chater = null;
            }
        }
    }
    public void ResetButton()
    {
        if (interactableTraders.Count > 0)
        {
            Panel_Mgr.instance.Control_InteractPanel(true, Panel_Mgr.instance.InteractTradePanel);
        }
        if (interactableChatNPCS.Count > 0)
        {
            Panel_Mgr.instance.Control_InteractPanel(true, Panel_Mgr.instance.InteractChatPanel);
        }
    }
}