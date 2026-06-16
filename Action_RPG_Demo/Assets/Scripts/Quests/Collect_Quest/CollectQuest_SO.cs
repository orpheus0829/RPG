using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PickResult
{
    public int CountGet;
    public GameObject ItemGet;
}
[System.Serializable]
public class Single_QuestItem
{
    public Vector3 Loaction;
    public GameObject QuestItem;
    public List<PickResult> ItemGets;
}
[CreateAssetMenu(fileName = "CollectQuest_Data", menuName = "Quest/CollectQuest_Data")]
public class CollectQuest_SO : QuestBase_SO
{
    public List<Single_QuestItem> single_QuestItems = new List<Single_QuestItem>();
}
