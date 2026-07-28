using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Drop_gameObject : MonoBehaviour
{
    [Header("数据盒")]
    public Item_Data item_Data;
    [Header("属性")]
    public Sprite Picture;
    //public Sprite Introduction_Pic;
    public string Drop_name;
    public int Drop_id;
    public int Drop_Height;
    public int Drop_Width;
    public int Drop_PriceValue;
    public string Drop_Description;
    public bool IsQuestItem;
    [Header("任务物品获得")]
    public Single_QuestItem bindQuestItem;
    public List<Item_Data> QuestGets = new List<Item_Data>();
    [Header("具体类别")]
    public Item_Kind Drop_Kind;
    public BoxCollider col;
    public void Awake()
    {
        col = GetComponent<BoxCollider>();
        Picture = item_Data.Display_In_Backpacks;
        //Introduction_Pic = item_Data.Introduction_Image;
        Drop_name = item_Data.item_name;
        Drop_id = item_Data.item_id;
        Drop_Height = item_Data.Height;
        Drop_Width = item_Data.Width;
        Drop_PriceValue = item_Data.PriceValue;
        Drop_Description = item_Data.Introduction;
        Drop_Kind = item_Data.item_Kind;
    }
    public void OnEnable()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(1f, 0.15f);
        col.enabled = true;
        transform.DOScale(1f, 0.2f);
    }
    public void OnDisable()
    {
        //bool isMatch = false;
        //foreach (var questItem in collectQuest.single_QuestItems)
        //{
        //    if (questItem == bindQuestItem)
        //    {
        //        isMatch = true;
        //        break;
        //    }
        //}
        //if (!isMatch)
        //{
        //    return;
        //}
    }
    public void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Player")
        {
            //写物品被吸收
            if (!IsQuestItem)
            {
                bool Pick = other.gameObject.GetComponent<Player_Bag>().Pick_Up(this.item_Data);
                if (Pick)
                {
                    PickNoticeMgr.instance.AddNote(item_Data);
                    other.gameObject.GetComponent<Player_Bag>().resort_list.Add(item_Data);
                    col.enabled = false;
                    transform.DOScale(0f, 0.3f).OnComplete(() =>
                    {
                        ObjectPoolMgr.instance.PushObj(gameObject);
                    });
                }
            }
            else
            {
                QuestBase_SO questBase = Story_Mgr.instance.GetCurrentQuest();
                if (!(questBase is CollectQuest_SO collectQuest))
                {
                    return;
                }
                if (bindQuestItem == null)
                {
                    return;
                }
                foreach (var pickResult in bindQuestItem.ItemGets)
                {
                    if (pickResult.ItemGet == null || pickResult.CountGet <= 0)
                    {
                        continue;
                    }
                    for (int i = 0; i < pickResult.CountGet; i++)
                    {
                        Item_Data data = pickResult.ItemGet.GetComponent<Drop_gameObject>().item_Data;
                        QuestGets.Add(data);
                    }
                }
                Debug.Log("match到了");
                IsQuestItem = false;
                //foreach (var item in QuestGets)
                //{
                //    bool Pick = other.gameObject.GetComponent<Player_Bag>().Pick_Up(item);
                //    if (Pick)
                //    {
                //        PickNoticeMgr.instance.AddNote(item);
                //        other.gameObject.GetComponent<Player_Bag>().resort_list.Add(item);
                //        Debug.Log($"加入{item.item_name}");
                //    }
                //}
                QuestGets.Clear();
                if (Story_Mgr.instance.CurDrops.Contains(this.gameObject))
                {
                    Story_Mgr.instance.CurDrops.Remove(this.gameObject);
                }
                if (Story_Mgr.instance.CurDrops.Count <= 0)
                {
                    Story_Mgr.instance.CurDrops.Clear();
                }

                Debug.Log("已消除任务物品");
                col.enabled = false;
                transform.DOScale(0f, 0.3f).OnComplete(() =>
                {
                    ObjectPoolMgr.instance.PushObj(gameObject);
                    Story_Mgr.instance.CheckAllDrop();
                });
            }
        }
    }
}
