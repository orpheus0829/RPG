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

    public float hitForce;

    [Header("无突发时 → 自动跳转")]
    public ActionSO nextAction;

    [Header("镜头演出参数")]
    public MoveMode cameraMoveMode;
    public Vector3 cameraDirection = Vector3.forward;
    public float cameraTotalDistance;
    public Vector3 cameraTargetLocalPos;
    public float cameraMoveSpeed;
    public float cameraStartSpeed;
    public float cameraEndSpeed;

    [Header("特效音效参数")]
    public GameObject effectPrefab;
    public AudioClip soundClip;
    public float effectTriggerTime;
    public Vector3 effectSpawnOffset;

    [Header("位移")]
    public MoveMode moveMode;
    public Vector3 direction = Vector3.forward;

    public Vector3 endPos;

    public float moveSpeed;
    public float totalDistance;

    public float startSpeed;
    public float endSpeed;
    [Header("翻越")]
    public ClimbStage climbStage;
    public bool climbUseVariableSpeed;
    public float climbSpeed;
    public float climbStartSpeed;
    public float climbEndSpeed;
    public float climbAfterExtraDistance;
}