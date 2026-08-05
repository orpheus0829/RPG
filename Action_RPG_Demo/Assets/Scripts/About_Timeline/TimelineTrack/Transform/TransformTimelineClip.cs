using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
public enum MoveMode
{
    FixedEndPos,
    SpeedAndDistance,
    VariableSpeed,
    ClimbOver,
    CircleRotate,
}
public enum ClimbStage
{
    BeforeClimb,
    AfterClimb,
}
public class TransformTimelineClip : PlayableAsset, ITimelineClipAsset
{
    [Header("位移模式")]
    public MoveMode moveMode;

    [Header("通用参数")]
    public Vector3 direction = Vector3.forward;

    [Header("固定终点")]
    public Vector3 endPos;
    public Vector3 endEuler;

    [Header("速度+距离(匀速)")]
    public float moveSpeed;
    public float totalDistance;

    [Header("变速移动")]
    public float startSpeed;
    public float endSpeed;

    [Header("翻越模式")]
    public ClimbStage climbStage;
    public bool climbUseVariableSpeed;
    public float climbSpeed;
    public float climbStartSpeed;
    public float climbEndSpeed;
    public float climbAfterExtraDistance;

    [Header("绕圈旋转")]
    public Vector3 circleCenterLocal;
    public float circleRadius;
    public float circleTotalAngle;
    public bool circleClockwise = true;
    public bool circleVariableSpeed;
    public float circleConstantSpeed;
    public float circleStartAngSpeed;
    public float circleEndAngSpeed;

    public ClipCaps clipCaps => ClipCaps.Blending;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var behaviour = new TransformBehaviour();
        behaviour.clip = this;
        return ScriptPlayable<TransformBehaviour>.Create(graph, behaviour);
    }
}