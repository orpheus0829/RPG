using UnityEngine;
using UnityEngine.Playables;

// 相机总运动大类
public enum CamMoveMode
{
    SmoothLerp,
    Teleport,
    ResetOrigin
}
public enum ResetCamSubMode
{
    Teleport,
    SmoothLerp
}

public class CameraTimelineClip : PlayableAsset
{
    [Header("镜头移动总模式")]
    public CamMoveMode cameraMoveMode;

    [Header("=== 归位模式专用参数（cameraMoveMode = ResetOrigin 生效） ===")]
    public ResetCamSubMode resetSubMode;
    [Tooltip("归位平滑系数，仅SmoothLerp子模式生效")]
    public float resetLerpFactor = 10f;
    [Tooltip("归位是否启用变速")]
    public bool resetUseVariableSpeed = false;
    public float resetStartSpeed = 1f;
    public float resetEndSpeed = 3f;

    [Header("=== 环绕圆弧运镜 ===")]
    [Tooltip("开启后以固定半径绕角色旋转，高度全程不变")]
    public bool useSurroundMode = false;
    [Tooltip("固定环绕水平半径，只能面板修改，拖拽终点不改变圆弧大小")]
    public float surroundRadius = 3.2f;
    [Tooltip("总环绕旋转角度，180=右半圈，-180左半圈")]
    public float surroundTotalAngle = 180f;
    [Tooltip("环绕全程固定高度偏移（不升降）")]
    public float surroundFixedHeight = 1.2f;

    [Header("目标机位参数（直线/环绕模式生效；归位模式自动忽略此字段）")]
    public Vector3 cameraTargetLocalPos;
    public Vector3 cameraTargetEuler;

    [Header("朝向设置")]
    public bool lockLookAtPlayer = true;
    public bool lockMoveToCharacterForward = false;

    [Header("Smooth直线模式平滑插值系数")]
    public float smoothLerpFactor = 10f;

    [Header("直线/环绕变速插值设置")]
    public bool useVariableSpeed = false;
    public float startSpeed = 1f;
    public float endSpeed = 3f;

    [Header("以上一帧相机位置为本段起点")]
    public bool useLastFrameAsOrigin;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        CameraTimelineBehaviour behaviour = new CameraTimelineBehaviour();
        behaviour.clip = this;
        return ScriptPlayable<CameraTimelineBehaviour>.Create(graph, behaviour);
    }
}