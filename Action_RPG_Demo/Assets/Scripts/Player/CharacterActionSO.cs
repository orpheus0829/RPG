using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ParryStyle
{
    Block,
    Evade,
}
[System.Serializable]
public class Single_ATK
{
    public ActionSO ATK;
    public bool HasVariantATK;

    public ActionSO PerfectATK;
}
[System.Serializable]
public class Single_SpecialATK
{
    public ActionSO Special;
    public float Cost;
}
[System.Serializable]
public class RoleData
{
    public Sprite RoleIcom;
}
[CreateAssetMenu(fileName = "CharacterAction_Data", menuName = "Player/CharacterAction_Data")]
public class CharacterActionSO : ScriptableObject
{
    [Header("角色信息")]
    public RoleData Roledata;
    public ParryStyle RoleParry;
    [Header("待机")]
    public ActionSO Idle;
    public ActionSO AfkIdle;
    public ActionSO BornSet;
    public ActionSO Born;
    [Header("移动")]
    public ActionSO WalkStart;
    public ActionSO Walk;
    public ActionSO WalkEnd;
    public ActionSO Dodge;
    [Header("疾跑")]
    public ActionSO RunDodge;
    public ActionSO Run;
    public ActionSO RunEnd;

    [Header("攻击")]
    public List<Single_ATK> AtkList = new List<Single_ATK>();
    public ActionSO RushAttack;
    public ActionSO Block;
    [Header("特殊技")]
    public List<Single_SpecialATK> RelatedFullE;
    public List<Single_SpecialATK> FullE;
    public List<Single_SpecialATK> RelatedUnfilledE;
    public List<Single_SpecialATK> UnfilledE;
    [Header("终结技")]
    public ActionSO EndSkill;
    [Header("跳跃")]
    public ActionSO Jump;
    [Header("翻越")]
    public ActionSO PreVault;
    public ActionSO AftVault;
    [Header("滑铲")]
    public ActionSO Slide;
    [Header("受击")]
    public ActionSO GetHit;
    [Header("死亡")]
    public ActionSO Death;

    [Header("切换")]
    public ActionSO SwitchIn;
    public ActionSO SwitchOut;
}
