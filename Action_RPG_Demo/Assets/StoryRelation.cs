using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryRelation : MonoBehaviour
{
    public TextMeshProUGUI StoryTitle;
    public RectTransform Content;
    public GameObject ChapterDropDown;
    public Story_SO StoryLineData;
    public EpisodeRelation episodeRelation;
    public bool HasInit;

    public void Awake()
    {
        StoryLineData = Story_Mgr.instance.Story;
        if (!episodeRelation)
        {
            Transform parent = transform.parent;
            foreach (Transform i in parent)
            {
                if (i.TryGetComponent(out EpisodeRelation relation))
                {
                    episodeRelation = relation;
                    break;
                }
            }
        }
    }

    public void OnEnable()
    {
        StoryLineData = Story_Mgr.instance.Story;
        CreateStoryDropDown();
    }

    public void CreateStoryDropDown()
    {
        for (int i = Content.childCount - 1; i >= 0; i--)
        {
            ObjectPoolMgr.instance.PushObj(Content.GetChild(i).gameObject);
        }

        StoryTitle.text = StoryLineData.Story_Title;
        ChapterRelation firstchapter = null;
        int count = 0;
        foreach (var i in StoryLineData.Chapters)
        {
            count++;
            GameObject ChapterD = ObjectPoolMgr.instance.GetObj(ChapterDropDown, Content);
            RectTransform rect = ChapterD.GetComponent<RectTransform>();
            rect.localScale = 4 * Vector3.one;
            ChapterRelation relation = ChapterD.GetComponent<ChapterRelation>();
            if (count == 1)
            {
                firstchapter = relation;
            }
            TMP_Dropdown dropdown = ChapterD.GetComponent<TMP_Dropdown>();
            relation.ChapterDrop = dropdown;
            relation.EpisodeR = episodeRelation;
            relation.ChapterLineData = i;
            relation.ChapterTitle.text = i.Chapter_Title;
            relation.CreateChapterDropDown();
        }
        if (!HasInit)
        {
            if (firstchapter)
            {
                firstchapter.OnSelectChapter(1);
            }
            HasInit = true;
        }
    }
}