using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static UnityEditor.Progress;

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
    public QuestBase_SO CurQuest;
    [Header("可对话角色缓存")]
    public List<Dialogue_Set> DialogActor = new List<Dialogue_Set>();
    public Dictionary<Dialogue_Set, string> DialogueActorToIdDict = new Dictionary<Dialogue_Set, string>();
    public Dictionary<string, Dialogue_Set> IdToDialogueActorDict = new Dictionary<string, Dialogue_Set>();
    [Header("任务相关缓存")]
    public List<GameObject> CurEnemys = new List<GameObject>();
    public List<GameObject> CurDrops = new List<GameObject>();
    public GameObject CurActor;
    public Vector3 CurQuestPos;
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
        DialogActor.Clear();
        CurEnemys.Clear();
        CurDrops.Clear();
        GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");
        foreach (var i in npcs)
        {
            if (i.TryGetComponent(out Dialogue_Set set))
            {
                DialogActor.Add(set);
            }
        }
        DialogueActorToIdDict.Clear();
        IdToDialogueActorDict.Clear();
        foreach (var actor in DialogActor)
        {
            string curActorId = actor.CharacterId;
            if (string.IsNullOrWhiteSpace(curActorId))
            {
                Debug.Log($"NPC {actor.gameObject.name}未配置角色ID");
                continue;
            }
            DialogueActorToIdDict.Add(actor, curActorId);
            if (!IdToDialogueActorDict.ContainsKey(curActorId))
            {
                IdToDialogueActorDict.Add(curActorId, actor);
            }
        }
        //CurQuest = Story.Chapters[CurStory.ChapterID].Episodes[CurStory.EpisodeID].Quests[CurStory.QuestID - 1];
        Refresh_StoryProgress();
        CurQuestPos = CalculateQuestPos();
        //NavPathMgr.instance.OpenNavPath(CurQuestPos);
    }
    public void OnEnable()
    {
        Game_Event.instance.ModifyEnemyDamage += ModifyD;
        Game_Event.instance.ModifyEnemyMaxHP += ModifyH;
    }
    public void OnDisable()
    {
        Game_Event.instance.ModifyEnemyDamage -= ModifyD;
        Game_Event.instance.ModifyEnemyMaxHP -= ModifyH;
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
        if (StoryDebug)
        {
            Debug_Story();
            StoryDebug = false;
        }
        CurQuestPos = CalculateQuestPos();
        //NavPathMgr.instance.targetPoint = CurQuestPos;
    }
    private void OnDestroy()
    {
        DialogueActorToIdDict.Clear();
        IdToDialogueActorDict.Clear();
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
        //Refresh_StoryProgress();
    }
    public void StoryAdvance()
    {
        //Debug.Log("storyadvance");
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
        //Debug.Log("questadvance");
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
            Debug.Log("增加任务");
            Save_WorldStory(StoryPath);
        }

        Refresh_StoryProgress();
        CurQuestPos = CalculateQuestPos();
        NavPathMgr.instance.OpenNavPath(CurQuestPos);
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
        //Debug.Log($"{GetcurEp.Quests[CurStory.QuestID]}");
        return GetcurEp.Quests[CurStory.QuestID];
    }
    public void Refresh_StoryProgress()
    {
        //foreach (var oldEnemy in CurEnemys)
        //{
        //    if (oldEnemy != null)
        //    {
        //        ObjectPoolMgr.instance.PushObj(oldEnemy);
        //    }
        //}
        //CurEnemys.Clear();
        //foreach (var oldDrop in CurDrops)
        //{
        //    if (oldDrop != null)
        //    {
        //        ObjectPoolMgr.instance.PushObj(oldDrop);
        //    }
        //}
        //CurDrops.Clear();
        //Debug.Log($"{CurStory.ChapterID},{CurStory.EpisodeID},{CurStory.QuestID}");
        QuestBase_SO curQuest = GetCurrentQuest();
        CurQuest = curQuest;
        if (curQuest == null)
        {
            return;
        }
        foreach (var i in DialogActor)
        {
            i.Story_Dialogue = null;
            i.Switch_DialogueSO();
        }
        if (curQuest is Dialogue_SO curDialogueQuest)
        {
            Dialogue_Set targetActor = GetDialogueActorByDialogueSO(curDialogueQuest);
            if (targetActor != null)
            {
                CurActor = targetActor.gameObject;
                targetActor.Story_Dialogue = curDialogueQuest;
                targetActor.Switch_DialogueSO();
                Debug.Log($"找到对话角色{targetActor.gameObject.name},对话SO为{curDialogueQuest.Single_Dialogue}");
            }
            else
            {
                CurActor = null;
                Debug.Log($"对话任务 {curDialogueQuest.Quest_Title} 未匹配到对应NPC SpeakerId:{curDialogueQuest.SpeakerId}");
            }
        }
        else if (curQuest is FightQuest_SO curFightQuest)
        {
            foreach (var i in curFightQuest.Quest_Enemys)
            {
                //GameObject e = Instantiate(i.Enemy, i.Location, Quaternion.identity);
                GameObject e = ObjectPoolMgr.instance.GetObj(i.Enemy, i.Location);
                if (e.TryGetComponent(out Enemy enemyprefab))
                {
                    enemyprefab.IsQuest = true;
                    Game_Event.instance.ModifyDamage(i.Damage, enemyprefab);
                    Game_Event.instance.ModifyHP(i.MaxHp, enemyprefab);
                }
                CurEnemys.Add(e);
            }
        }
        else if (curQuest is CollectQuest_SO curCollectQuest)
        {
            foreach (var i in curCollectQuest.single_QuestItems)
            {
                GameObject d = ObjectPoolMgr.instance.GetObj(i.QuestItem, i.Loaction);
                if(d.TryGetComponent(out Drop_gameObject dropprefab))
                {
                    dropprefab.IsQuestItem = true;
                    dropprefab.bindQuestItem = i;
                }
                CurDrops.Add(d);
            }
        }
    }
    public Vector3 CalculateQuestPos()
    {
        QuestBase_SO quest = GetCurrentQuest();
        if(quest is FightQuest_SO fight)
        {
            int count = CurEnemys.Count;
            Vector3 pos = Vector3.zero;
            foreach(var i in CurEnemys)
            {
                pos += i.gameObject.transform.position;
            }
            return pos / count;
        }
        else if(quest is CollectQuest_SO collect)
        {
            int count = CurDrops.Count;
            Vector3 pos = Vector3.zero;
            foreach (var i in CurDrops)
            {
                pos += i.gameObject.transform.position;
            }
            return pos / count;
        }
        else if(quest is Dialogue_SO dialogue)
        {
            if (CurActor)
            {
                return CurActor.gameObject.transform.position;
            }
        }
        return Vector3.zero;
    }
    public void CheckAllEnemyDead()
    {
        if (CurQuest is FightQuest_SO fight)
        {
            if (CurEnemys.Count <= 0)
            {
                DeliverReward(fight);
                PickNoticeMgr.instance.MissionDone();
                QuestAdvance();
                PickNoticeMgr.instance.MissionNext();
            }
        }
    }
    public void CheckAllDrop()
    {
        if (CurQuest is CollectQuest_SO collect)
        {
            if (CurDrops.Count <= 0)
            {
                DeliverReward(collect);
                PickNoticeMgr.instance.MissionDone();
                QuestAdvance();
                PickNoticeMgr.instance.MissionNext();
            }
        }
    }
    public void CheckDialogue()
    {
        PickNoticeMgr.instance.MissionDone();
        QuestAdvance();
        Refresh_StoryProgress();
        PickNoticeMgr.instance.MissionNext();
    }
    public void DeliverReward(QuestBase_SO quest)
    {
        GameObject[] pl = GameObject.FindGameObjectsWithTag("Player");
        foreach (var i in pl)
        {
            Player_Bag bag = i.GetComponent<Player_Bag>();
            if (quest is FightQuest_SO fight)
            {
                foreach (var j in fight.Rewards)
                {
                    Item_Data data = j.Reward.GetComponent<Drop_gameObject>().item_Data;
                    for (int n = 0; n < j.Count; n++)
                    {
                        bool get = bag.Pick_Up(data);
                        if (!get)
                        {
                            ObjectPoolMgr.instance.GetObj(data.Drop, bag.gameObject.transform.position + Vector3.up * 2);
                        }
                        else
                        {
                            PickNoticeMgr.instance.AddNote(data);
                            Debug.Log("发放奖励");
                        }
                    }
                }
            }
            else if (quest is CollectQuest_SO collect)
            {
                foreach (var j in collect.single_QuestItems)
                {
                    foreach (var m in j.ItemGets)
                    {
                        Item_Data data = m.ItemGet.GetComponent<Drop_gameObject>().item_Data;
                        for (int n = 0; n < m.CountGet; n++)
                        {
                            bool get = bag.Pick_Up(data);
                            if (!get)
                            {
                                ObjectPoolMgr.instance.GetObj(data.Drop, bag.gameObject.transform.position + Vector3.up * 2);
                            }
                            else
                            {
                                PickNoticeMgr.instance.AddNote(data);
                                Debug.Log("发放奖励");
                            }
                        }
                    }
                }
            }
        }
    }
    public void ModifyD(float property, Enemy e)
    {
        if (property == 0)
        {
            return;
        }
        e.damage = property;
    }
    public void ModifyH(float property, Enemy e)
    {
        if (property == 0)
        {
            return;
        }
        e.damageReceiver.currentHp = property;
    }
    public void Refresh_StoryUI()
    {

    }
    public Dialogue_Set GetDialogueActorByDialogueSO(Dialogue_SO curDialogueSO)
    {
        if (curDialogueSO == null)
        {
            return null;
        }
        string curSpeakerId = curDialogueSO.SpeakerId;
        if (string.IsNullOrEmpty(curSpeakerId))
        {
            Debug.Log("当前对话SO的SpeakerId为空");
            return null;
        }
        if (IdToDialogueActorDict.TryGetValue(curSpeakerId, out Dialogue_Set targetActor))
        {
            return targetActor;
        }
        Debug.Log($"未找到ID为{curSpeakerId}的对话角色");
        return null;
    }
    public Dialogue_Set GetDialogueActorById(string curActorId)
    {
        if (IdToDialogueActorDict.TryGetValue(curActorId, out Dialogue_Set targetActor))
        {
            return targetActor;
        }
        return null;
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