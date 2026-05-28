using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

/// <summary>
/// 动作配置数据表
/// 存储单套动作所有可调参数，资源复用
/// </summary>
[CreateAssetMenu(fileName = "NewAction", menuName = "动作数据/动作配置")]
public class ActionSO : ScriptableObject
{
    [Header("通用基础信息")]
    public int actionID;
    public string actionName;
    public TimelineAsset timeline;
    public ActionType actionType;

    [Header("攻击专属参数")]
    public float damageValue;
    public Vector3 hitBoxOffset;
    public float hitBoxRadius;
    public float hitStartTime;
    public float hitEndTime;

    [Header("无突发时 → 自动跳转")]
    public ActionSO nextAction;

    [Header("镜头演出参数")]
    public Vector3 cameraTargetLocalPos;

    [Header("特效音效参数")]
    public GameObject effectPrefab;
    public AudioClip soundClip;
    public float effectTriggerTime;
}