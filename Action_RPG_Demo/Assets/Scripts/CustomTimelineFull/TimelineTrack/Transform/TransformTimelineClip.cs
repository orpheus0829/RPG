using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public enum MoveMode
{
    FixedEndPos,
    SpeedAndDistance,
    VariableSpeed,
    ClimbOver,
    ParkourClimb,
}
public enum ClimbStage
{
    BeforeClimb,
    AfterClimb,
}
public class TransformTimelineClip : PlayableAsset, ITimelineClipAsset
{
    [Header("数据盒子")]
    public ActionSO data;

    [Header("位移模式")]
    public MoveMode moveMode;

    [Header("通用参数")]
    public Vector3 direction = Vector3.forward;

    [Header("模式1;固定终点")]
    public Vector3 endPos;

    [Header("模式2;速度+距离(匀速)")]
    public float moveSpeed;
    public float totalDistance;

    [Header("模式3;变速移动")]
    public float startSpeed;
    public float endSpeed;

    [Header("模式4;翻越模式")]
    public ClimbStage climbStage;
    public bool climbUseVariableSpeed;
    public float climbSpeed;
    public float climbStartSpeed;
    public float climbEndSpeed;
    public float climbAfterExtraDistance;

    [Header("模式5;攀爬")]
    public ParkourClimbStage parkourClimbStage;
    public bool parkourUseVariableSpeed;
    public float parkourSpeed;
    public float parkourStartSpeed;
    public float parkourEndSpeed;

    public ClipCaps clipCaps => ClipCaps.Blending;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var behaviour = new TransformBehaviour();
        behaviour.clip = this;
        return ScriptPlayable<TransformBehaviour>.Create(graph, behaviour);
    }
}