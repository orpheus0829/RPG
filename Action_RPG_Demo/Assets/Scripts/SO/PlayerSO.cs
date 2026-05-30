using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player_Data", menuName = "Player/Player_Data")]
public class PlayerSO : ScriptableObject
{
    [Header(" Ù–‘")]
    public string Player_Name;
    public float WalkSpeed;
    public float RunSpeed;

    public float DetectionRadius;
    public float LockDuration;
}
