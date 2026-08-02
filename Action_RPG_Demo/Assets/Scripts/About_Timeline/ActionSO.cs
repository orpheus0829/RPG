using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
public enum HitBoxShape
{
    Sphere,
    Box,
}
[CreateAssetMenu(fileName = "NewAction", menuName = "动作数据/动作配置")]
public class ActionSO : ScriptableObject
{
    [Header("通用基础信息")]
    public int actionID;
    public string actionName;
    public TimelineAsset timeline;
    public ActionType actionType;

    [Header("无突发时 - 自动跳转")]
    public ActionSO nextAction;

    public bool Related;
}