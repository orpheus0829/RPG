using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargetStatus
{
    Health,
    Damage,
    SpecialPower,
    MoveSpeed,
}
[CreateAssetMenu(fileName = "Buff", menuName = "Effect/Buff")]
public class BuffSO : BaseBuff
{
    public bool IsInstant;
    public TargetStatus TargetValue;
    public float Val;
}
