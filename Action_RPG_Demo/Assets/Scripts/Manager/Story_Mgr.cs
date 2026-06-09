using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class MainStoryData
{
    public int ChapterID;
    public int EpisodeID;
    public int QuestID;
}
public class Story_Mgr : Base_mgr<Story_Mgr>
{
    [Header("故事总数据盒")]
    public Story_SO Story;
    private string StoryPath = "WorldStory";
    public MainStoryData CurStory = new MainStoryData();
    [Header("数据盒debug")]
    public bool StoryDebug;
    public bool Advance;
    public bool Save;

    protected override void Awake()
    {
        base.Awake();
        if (instance == this)
        {
            DontDestroyOnLoad(this.gameObject);
        }
        Debug_Story();
        //Init_Story();
    }
    public void Start()
    {
        Load_WorldStory(StoryPath);
    }
    public void Update()
    {
        if (Advance)
        {
            QuestAdvance();
            Advance = false;
        }
        if (Save)
        {
            Save_WorldStory(StoryPath);
            Save = false;
        }
    }
    #region 设置数据
    public void Init_Story()
    {
        CurStory.ChapterID = 1;
        CurStory.EpisodeID = 1;
        CurStory.QuestID = 0;
        Save_WorldStory(StoryPath);
    }
    public void Save_WorldStory(string path)
    {
        string json_story_data = JsonUtility.ToJson(CurStory);
        File.WriteAllText(Application.persistentDataPath + "/" + path + ".json", json_story_data);
        Debug.Log("剧情进度保存成功，地址为" + Application.persistentDataPath + "/" + path + ".json");
    }
    public void Load_WorldStory(string path)
    {
        string json_story_data = Application.persistentDataPath + "/" + path + ".json";
        if (File.Exists(json_story_data))
        {
            string json = File.ReadAllText(json_story_data);
            CurStory = JsonUtility.FromJson<MainStoryData>(json);
            Debug.Log("已加载剧情，地址为" + Application.persistentDataPath + "/" + path + ".json");
        }
        else
        {
            Init_Story();
            Debug.Log("找不到剧情存档数据，新建立剧情节点");
        }
        Refresh_StoryProgress();
    }
    public void StoryAdvance()
    {
        bool isLastChapter = Story.Chapters.Find(t => t.Chapter_ID == CurStory.ChapterID + 1) == null;
        if (isLastChapter)
        {
            Debug.Log("故事已到达结局");
            return;
        }
        Chapter_SO curchapter = Story.Chapters.Find(t => t.Chapter_ID == CurStory.ChapterID);
        if (curchapter == null)
        {
            Debug.Log("找不到当前章节");
            return;
        }
        //Episode_SO curepisode = curchapter.Episodes.Find(t => t.Episode_ID == CurStory.EpisodeID);
        bool IsLastEpisodeInChapter = CurStory.EpisodeID >= curchapter.Episodes.Count;
        if (IsLastEpisodeInChapter)
        {
            CurStory.ChapterID++;
            CurStory.EpisodeID = 1;
            CurStory.QuestID = 0;
        }
        else
        {
            CurStory.EpisodeID++;
            CurStory.QuestID = 0;
        }
        Save_WorldStory(StoryPath);
        Refresh_StoryProgress();
    }
    public void QuestAdvance()
    {
        Chapter_SO curChap = Story.Chapters.Find(c => c.Chapter_ID == CurStory.ChapterID);
        if (curChap == null)
        {
            Debug.Log("当前章节不存在");
            return;
        }
        Episode_SO curEp = curChap.Episodes.Find(e => e.Episode_ID == CurStory.EpisodeID);
        if (curEp == null || curEp.Quests.Count == 0)
        {
            Debug.Log("当前剧目不存在或者当前剧目无任务,已前往下一剧目");
            StoryAdvance();
            return;
        }
        bool isLastQuest = CurStory.QuestID >= curEp.Quests.Count - 1;
        if (isLastQuest)
        {
            StoryAdvance();
        }
        else
        {
            CurStory.QuestID++;
            Save_WorldStory(StoryPath);
        }

        Refresh_StoryProgress();
    }
    public QuestBase_SO GetCurrentQuest()
    {
        Chapter_SO GetcurChap = Story.Chapters.Find(c => c.Chapter_ID == CurStory.ChapterID);
        if (GetcurChap == null)
        {
            return null;
        }
        Episode_SO GetcurEp = GetcurChap.Episodes.Find(e => e.Episode_ID == CurStory.EpisodeID);
        if (GetcurEp == null)
        {
            return null;
        }
        if (CurStory.QuestID < 0 || CurStory.QuestID >= GetcurEp.Quests.Count)
        {
            return null;
        }
        return GetcurEp.Quests[CurStory.QuestID];
    }
    public void Refresh_StoryProgress()
    {

    }
    public void Refresh_StoryUI()
    {

    }
    #endregion
    public void Debug_Story()
    {
        if (!StoryDebug)
        {
            return;
        }
        if (Story == null)
        {
            Debug.Log("未赋值故事数据盒");
            return;
        }
        for (int a = 0; a < Story.Chapters.Count; a++)
        {
            Chapter_SO chapter = Story.Chapters[a];
            Debug.Log($"【第 {a + 1} 章】: {chapter.Chapter_Title}");
            Debug.Log($"【第 {a + 1} 章】: {chapter.Chapter_Introduction}");
            for (int b = 0; b < chapter.Episodes.Count; b++)
            {
                Episode_SO episode = chapter.Episodes[b];
                Debug.Log($"【第 {b + 1} 节】: {episode.Episode_Title}");
                Debug.Log($"【第 {b + 1} 节】: {episode.Episode_Introduction}");
                for (int c = 0; c < episode.Quests.Count; c++)
                {
                    QuestBase_SO quest = episode.Quests[c];
                    if (quest == null)
                    {
                        Debug.Log("任务为空！");
                        continue;
                    }
                    if (quest is FightQuest_SO)
                    {
                        Debug.Log($"→ 战斗任务: {quest.Quest_Title}");
                    }
                    else if (quest is CollectQuest_SO)
                    {
                        Debug.Log($"→ 收集任务: {quest.Quest_Title}");
                    }
                    else if (quest is Dialogue_SO)
                    {
                        Debug.Log($"→ 对话任务: {quest.Quest_Title}");
                    }
                    else
                    {
                        Debug.Log($"→ 未知任务: {quest.Quest_Title}");
                    }
                }
            }
        }
        Debug.Log("======= 遍历完成 =======");
    }
}