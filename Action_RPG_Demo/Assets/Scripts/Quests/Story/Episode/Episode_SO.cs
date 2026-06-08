using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Episode_Data", menuName = "Story/Episode_Data")]
public class Episode_SO : ScriptableObject
{
    public List<QuestBase_SO> Quests = new List<QuestBase_SO>();
}
