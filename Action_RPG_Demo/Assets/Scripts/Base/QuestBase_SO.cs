using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct QuestStartDia
{
    public string Speaker;
    public string Content;
}
public class QuestBase_SO : ScriptableObject
{
    public string Quest_Title;
    public string Quest_Description;
    [Header("任务开始喊话")]
    public List<QuestStartDia> QuestStarts = new List<QuestStartDia>();
}
