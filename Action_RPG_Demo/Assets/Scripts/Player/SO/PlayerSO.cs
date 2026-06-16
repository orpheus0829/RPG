using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player_Data", menuName = "Character/Player_Data")]
public class PlayerSO : ScriptableObject
{
    [Header(" Ù–‘")]
    public string Player_Name;
    public float WalkSpeed;
    public float RunSpeed;
    public float PlayerMaxHP;
    public float Deadline;
    public float AFKInterval;

    public float DetectionRadius;
    public float LockDuration;
    [Header("Ã¯‘æ")]
    public float JumpScanRadius;
    public float VaultHeight;
    public float HighClimbHeight;
}
