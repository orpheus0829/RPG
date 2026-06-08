using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Story_Mgr : Base_mgr<Story_Mgr>
{
    [Header("故事总数据盒")]
    public Story_SO Story;
    protected override void Awake()
    {
        base.Awake();
        if (instance == this)
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }
    void Start()
    {
        if (Story == null)
        {
            Debug.Log("未赋值故事数据盒");
            return;
        }
        for (int a = 0; a < Story.Chapters.Count; a++)
        {
            Chapter_SO chapter = Story.Chapters[a];
            Debug.Log($"【第 {a + 1} 章】: {chapter.name}");
            for (int b = 0; b < chapter.Chapters.Count; b++)
            {
                Episode_SO episode = chapter.Chapters[b];
                Debug.Log($"  └─【第 {b + 1} 节】: {episode.name}");
                for (int c = 0; c < episode.Quests.Count; c++)
                {
                    QuestBase_SO quest = episode.Quests[c];
                    if (quest == null)
                    {
                        Debug.Log("  └─任务为空！");
                        continue;
                    }
                    if (quest is FightQuest_SO)
                    {
                        Debug.Log($"→ 战斗任务: {quest.name}");
                    }
                    else if (quest is CollectQuest_SO)
                    {
                        Debug.Log($"→ 收集任务: {quest.name}");
                    }
                    else if (quest is Dialogue_SO)
                    {
                        Debug.Log($"→ 对话任务: {quest.name}");
                    }
                    else
                    {
                        Debug.Log($"→ 未知任务: {quest.name}");
                    }
                }
            }
        }
        Debug.Log("======= 遍历完成 =======");
    }
}