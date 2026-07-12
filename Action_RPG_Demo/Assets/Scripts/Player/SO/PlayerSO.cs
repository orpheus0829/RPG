using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player_Data", menuName = "Character/Player_Data")]
public class PlayerSO : ScriptableObject
{
    [Header("ÊôĞÔ")]
    public string Player_Name;
    public float WalkSpeed;
    public float RunSpeed;
    public float PlayerMaxHP;
    public float Deadline;
    public float AFKInterval;

    public float DetectionRadius;
    public float LockDuration;
    [Header("ÌøÔ¾")]
    public float JumpScanRadius;
    public float VaultHeight;
    public float HighClimbHeight;
    [Header("ËÀÍö")]
    public bool EnableBornAnim;
    public Vector3 SpawnPoint;
    public Quaternion SpwanRotation;
    [Header("¼¼ÄÜ")]
    public float MaxPower;
    public float MaxCharge;
}
