using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Episode_Reward
{
    public int Count;
    public GameObject Reward;
}
[CreateAssetMenu(fileName = "Episode_Data", menuName = "Story/Episode_Data")]
public class Episode_SO : ScriptableObject
{
    public string Episode_Title;
    public int Episode_ID;
    [TextArea(0, 3)] public string Episode_Introduction;
    public List<QuestBase_SO> Quests = new List<QuestBase_SO>();
    public List<Episode_Reward> Episode_Rewards = new List<Episode_Reward>();
}
