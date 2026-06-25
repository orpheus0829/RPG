using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestRelation : MonoBehaviour
{
    public Episode_SO NowEpisode;
    public QuestBase_SO quest;
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Description;
    public Button TrackBtn;
    public RectTransform RewardContent;
    public TextMeshProUGUI HasDone;

    public GameObject RewardImage;
    private HashSet<int> SpawnedItemIds = new HashSet<int>();

    public void Awake()
    {

    }

    public void Start()
    {
        RefreshDoneMark();
    }

    public void OnEnable()
    {
        TrackBtn.onClick.RemoveAllListeners();
        TrackBtn.onClick.AddListener(OnTrack);
        RefreshDoneMark();
    }

    public void OnDisable()
    {
        TrackBtn.onClick.RemoveAllListeners();
    }

    public void GetQuestDetail()
    {
        SpawnedItemIds.Clear();
        Title.text = quest.Quest_Title;
        Description.text = quest.Quest_Description;
        for (int i = RewardContent.childCount - 1; i >= 0; i--)
        {
            ObjectPoolMgr.instance.PushObj(RewardContent.GetChild(i).gameObject);
        }
        if (quest is FightQuest_SO fight)
        {
            foreach (var i in fight.Rewards)
            {
                Drop_gameObject drop = i.Reward.GetComponent<Drop_gameObject>();
                Item_Data item = drop.item_Data;
                if (SpawnedItemIds.Contains(item.item_id))
                {
                    continue;
                }
                GameObject iconObj = ObjectPoolMgr.instance.GetObj(RewardImage, RewardContent);
                Image img = iconObj.GetComponent<Image>();
                RectTransform rt = iconObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(40, 40);
                rt.localScale = new Vector2(1, 2);
                img.sprite = item.Display_In_Backpacks;
                SpawnedItemIds.Add(item.item_id);
            }
        }
        else if (quest is CollectQuest_SO collect)
        {
            foreach (var i in collect.single_QuestItems)
            {
                foreach (var j in i.ItemGets)
                {
                    Drop_gameObject drop = j.ItemGet.GetComponent<Drop_gameObject>();
                    Item_Data item = drop.item_Data;
                    if (SpawnedItemIds.Contains(item.item_id))
                    {
                        continue;
                    }
                    GameObject iconObj = ObjectPoolMgr.instance.GetObj(RewardImage, RewardContent);
                    Image img = iconObj.GetComponent<Image>();
                    RectTransform rt = iconObj.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(40, 40);
                    rt.localScale = new Vector2(1, 2);
                    img.sprite = item.Display_In_Backpacks;
                    SpawnedItemIds.Add(item.item_id);
                }
            }
        }
    }
    public void RefreshDoneMark()
    {
        if (Story_Mgr.instance == null)
        {
            HasDone.text = "加载中";
            HasDone.color = Color.gray;
            TrackBtn.interactable = false;
            return;
        }
        MainStoryData storyData = Story_Mgr.instance.CurStory;
        if (NowEpisode == null || quest == null || NowEpisode.Quests == null)
        {
            HasDone.text = "未加载任务数据";
            HasDone.color = Color.grey;
            TrackBtn.interactable = false;
            return;
        }
        Chapter_SO belongChapter = null;
        foreach (var ch in Story_Mgr.instance.Story.Chapters)
        {
            if (ch.Episodes.Contains(NowEpisode))
            {
                belongChapter = ch;
                break;
            }
        }
        if (belongChapter == null)
        {
            HasDone.text = "无效剧情";
            HasDone.color = Color.grey;
            TrackBtn.interactable = false;
            return;
        }
        int questindex = NowEpisode.Quests.FindIndex(t => t == quest);
        if (questindex < 0)
        {
            HasDone.text = "无效任务";
            HasDone.color = Color.grey;
            TrackBtn.interactable = false;
            return;
        }
        bool sameChapterEpisode = belongChapter.Chapter_ID == storyData.ChapterID && NowEpisode.Episode_ID == storyData.EpisodeID;
        if (sameChapterEpisode)
        {
            if (storyData.QuestID > questindex)
            {
                HasDone.text = "已完成";
                HasDone.color = Color.green;
                TrackBtn.interactable = false;
            }
            else if (storyData.QuestID == questindex)
            {
                HasDone.text = "进行中";
                HasDone.color = Color.yellow;
                TrackBtn.interactable = true;
            }
            else
            {
                HasDone.text = "未解锁";
                HasDone.color = Color.grey;
                TrackBtn.interactable = false;
            }
        }
        else
        {
            if (belongChapter.Chapter_ID < storyData.ChapterID)
            {
                HasDone.text = "已完成";
                HasDone.color = Color.green;
                TrackBtn.interactable = false;
            }
            else if (belongChapter.Chapter_ID == storyData.ChapterID)
            {
                if (NowEpisode.Episode_ID < storyData.EpisodeID)
                {
                    HasDone.text = "已完成";
                    HasDone.color = Color.green;
                    TrackBtn.interactable = false;
                }
                else
                {
                    HasDone.text = "未解锁";
                    HasDone.color = Color.grey;
                    TrackBtn.interactable = false;
                }
            }
            else
            {
                HasDone.text = "未解锁";
                HasDone.color = Color.grey;
                TrackBtn.interactable = false;
            }
        }
    }
    public void OnTrack()
    {
        Vector3 questPos = Story_Mgr.instance.CalculateQuestPos();
        MiniMapMgr.instance.trackingTarget = null;
        NavPathMgr.instance.SwitchNavTarget(questPos);
        NavPathMgr.instance.CloseNavPath();
        NavPathMgr.instance.OpenNavPath(questPos);
    }
}