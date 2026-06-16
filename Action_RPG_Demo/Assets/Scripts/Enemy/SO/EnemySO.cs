using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy_Data", menuName = "Character/Enemy_Data")]
public class EnemySO : ScriptableObject
{
    public float Damage;
    public float MaxHP;
    public float WalkSpeed;
    public float RunSpeed;
    public float ChaseRadius;
    public float HitDetectLengeh;
    public float HitLength;
    public float HitRadius;
    public float HitHigh;
    public float HitCool;
    [Header("出生")]
    [Range(0,100)]public float IdlePer;
    [Range(0, 100)] public float WalkPer;
    [Header("进入追逐后从行走变为奔跑的概率")]
    [Range(0, 100)] public float RunFromWalk;
    public float ChangeInterval;
    [Header("死亡")]
    public float DisappearTime;
}
