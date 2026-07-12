using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy_Data", menuName = "Character/Enemy_Data")]
public class EnemySO : ScriptableObject
{
    public enum EnemyType
    {
        Zombie,
        Raider
    }
    public EnemyType enemyType;
    public float Damage;
    public float MaxHP;
    public float WalkSpeed;
    public float RunSpeed;
    public float ChaseRadius;
    public float DisappearTime;

    //僵尸
    public float HitDetectLengeh;
    public float HitLength;
    public float HitRadius;
    public float HitHigh;
    public float HitCool;
    [Header("出生")]
    [Range(0f, 100f)] public float IdlePer;
    [Range(0f, 100f)] public float WalkPer;
    [Header("进入追逐后从行走变为奔跑的概率")]
    [Range(0f, 100f)] public float RunFromWalk;
    public float ChangeInterval;


    //匪
    public float MinHesitantWalk;
    public float HesitantDistance;
    public float HesitantInterval;
    [Range(0f, 100f)] public float AtkProbablity;
    [Range(0f, 100f)] public float DodgeProbablity;
    public float DangerDistance;
    [Range(0f, 100f)] public float DangerBackProbablity;
}
