using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChapterRelation : MonoBehaviour
{
    public Chapter_SO ChapterLineData;
    public TMP_Dropdown ChapterDrop;
    public TextMeshProUGUI ChapterTitle;
    public EpisodeRelation EpisodeR;
    private bool IsInitLoadDone = false;

    public void Awake()
    {
        IsInitLoadDone = false;
        if (ChapterDrop)
        {
            ChapterDrop.options.Clear();
        }
    }
    public void OnEnable()
    {
        IsInitLoadDone = false;
    }
    public void CreateChapterDropDown()
    {
        if (ChapterDrop)
        {
            ChapterDrop.options.Clear();
        }
        ChapterDrop.options.Add(new TMP_Dropdown.OptionData("请选择剧集"));
        int num = 0;
        foreach (var i in ChapterLineData.Episodes)
        {
            num++;
            ChapterDrop.options.Add(new TMP_Dropdown.OptionData($"{num}.{i.Episode_Title}"));
        }
        ChapterDrop.onValueChanged.RemoveAllListeners();
        ChapterDrop.onValueChanged.AddListener(OnSelectChapter);
        ChapterDrop.SetValueWithoutNotify(0);
        ChapterDrop.RefreshShownValue();
        Debug.Log("章节下拉初始化完成");
    }

    public void OnSelectChapter(int index)
    {
        if (index == 0)
        {
            return;
        }
        if (ChapterLineData == null || EpisodeR == null)
        {
            return;
        }
        int realEpisodeID = index;
        Episode_SO episodedata = ChapterLineData.Episodes.Find(t => t.Episode_ID == realEpisodeID);
        if (episodedata == null)
        {
            return;
        }
        EpisodeR.EpisodeLineData = episodedata;
        EpisodeR.CreateEpisodeDropDown();
        Canvas.ForceUpdateCanvases();
        Debug.Log($"切换至第{episodedata.Episode_ID}幕");
    }

    public void OnDisable()
    {
        if (ChapterDrop)
        {
            ChapterDrop.onValueChanged.RemoveAllListeners();
        }
    }
}