using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EpisodeRelation : MonoBehaviour
{
    public Episode_SO EpisodeLineData;
    public GameObject QuestBoard;
    public RectTransform QuestsParent;
    public TextMeshProUGUI EpisodeDescription;

    public void CreateEpisodeDropDown()
    {
        for (int i = QuestsParent.childCount - 1; i >= 0; i--)
        {
            Transform child = QuestsParent.GetChild(i);
            ObjectPoolMgr.instance.PushObj(child.gameObject);
        }
        Canvas.ForceUpdateCanvases();
        StartCoroutine(SpawnQuestDelay());
    }

    private IEnumerator SpawnQuestDelay()
    {
        yield return null;
        foreach (var i in EpisodeLineData.Quests)
        {
            GameObject quest = ObjectPoolMgr.instance.GetObj(QuestBoard, QuestsParent);
            QuestRelation relation = quest.GetComponent<QuestRelation>();
            relation.NowEpisode = EpisodeLineData;
            relation.quest = i;
            EpisodeDescription.text = EpisodeLineData.Episode_Introduction;
            relation.GetQuestDetail();
            relation.RefreshDoneMark();
        }
        Debug.Log($"生成第{EpisodeLineData.Episode_ID}幕任务");
    }
}