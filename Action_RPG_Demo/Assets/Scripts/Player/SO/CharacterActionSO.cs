using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Single_ATK
{
    public ActionSO ATK;
    public float ChargeNor;
    public bool HasVariantATK;

    public ActionSO PerfectATK;
    public float ChargePer;
    [Range(0f, 100f)] public float Percentage;
}
[CreateAssetMenu(fileName = "CharacterAction_Data", menuName = "Player/CharacterAction_Data")]
public class CharacterActionSO : ScriptableObject
{
    [Header("´ý»ú")]
    public ActionSO Idle;
    public ActionSO AfkIdle;
    public ActionSO BornSet;
    public ActionSO Born;
    [Header("ÒÆ¶¯")]
    public ActionSO WalkStart;
    public ActionSO Walk;
    public ActionSO WalkEnd;
    public ActionSO Dodge;
    [Header("¼²ÅÜ")]
    public ActionSO RunDodge;
    public ActionSO Run;
    public ActionSO RunEnd;

    [Header("¹¥»÷")]
    public List<Single_ATK> AtkList = new List<Single_ATK>();
    [Header("ÌØÊâ¼¼")]
    public ActionSO RelatedFullE;
    public ActionSO FullE;
    public ActionSO RelatedUnfilledE;
    public ActionSO UnfilledE;
    [Header("ÌøÔ¾")]
    public ActionSO Jump;
    [Header("·­Ô½")]
    public ActionSO PreVault;
    public ActionSO AftVault;
    [Header("»¬²ù")]
    public ActionSO Slide;
    [Header("ÊÜ»÷")]
    public ActionSO GetHit;
    [Header("ËÀÍö")]
    public ActionSO Death;
}
