using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class DropReward
{
    public int Count;
    public GameObject Reward;
}
[System.Serializable]
public class Single_Enemy
{
    public Vector3 Location;
    public GameObject Enemy;
    public float MaxHp;
    public float CurHp;
    public float Damage;
    public List<DropReward> Rewards;
}

[CreateAssetMenu(fileName = "FightQuest_Data", menuName = "Quest/FightQuest_Data")]
public class FightQuest_SO : QuestBase_SO
{
    public List<Single_Enemy> Quest_Enemys = new List<Single_Enemy>();
}
