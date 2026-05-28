using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// 相机移动执行逻辑
/// </summary>
public class CameraTimelineBehaviour : PlayableBehaviour
{
    public CameraTimelineClip clip;
    //public Vector3 targetLocalPos;
    private CameraMotion _cachedCamMotion;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        ActionControl ctrl = playerData as ActionControl;
        CameraMotion camMotion = ctrl?.GetCameraMotion();
        if (camMotion == null){
            return;
        }

        Vector3 targetPos = clip.data != null ? clip.data.cameraTargetLocalPos : clip.targetLocalPos;

        float total = (float)playable.GetDuration();
        float cur = (float)playable.GetTime();
        float p = total <= 0 ? 1 : cur / total;

        _cachedCamMotion = camMotion;
        camMotion.LerpMoveCamera(targetPos, p);
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        _cachedCamMotion?.ResetCameraOrigin(); // 只加这一行
    }
}