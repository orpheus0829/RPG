using UnityEngine;
using UnityEngine.Playables;

public class CameraTimelineBehaviour : PlayableBehaviour
{
    public CameraTimelineClip clip;
    private CameraMotion _cachedCamMotion;
    private Vector3 _cameraStartPos;
    private Vector3 _cameraEndPos;
    private bool _inited;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        _inited = false;
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        ActionControl ctrl = playerData as ActionControl;
        CameraMotion camMotion = ctrl?.GetCameraMotion();
        if (camMotion == null) return;

        MoveMode mode;
        Vector3 camDir;
        float camDist;
        Vector3 fixedTarget;
        float moveSpeed;
        float startSpeed;
        float endSpeed;

        mode = clip.cameraMoveMode;
        camDir = clip.cameraDirection;
        camDist = clip.cameraTotalDistance;
        fixedTarget = clip.cameraTargetLocalPos;
        moveSpeed = clip.cameraMoveSpeed;
        startSpeed = clip.cameraStartSpeed;
        endSpeed = clip.cameraEndSpeed;

        Transform roleTrans = ctrl.transform;
        float totalTime = (float)playable.GetDuration();
        float curTime = (float)playable.GetTime();
        if (totalTime <= 0) return;

        if (!_inited)
        {
            _cameraStartPos = camMotion.GetOriginPosition();
            Vector3 roleForward = roleTrans.forward;
            roleForward.y = 0;
            roleForward.Normalize();

            Vector3 roleRight = roleTrans.right;
            roleRight.y = 0;
            roleRight.Normalize();

            Vector3 finalDir = roleRight * camDir.x + roleForward * camDir.z + Vector3.up * camDir.y;
            if (finalDir.magnitude > 0)
                finalDir.Normalize();

            switch (mode)
            {
                case MoveMode.FixedEndPos:
                    _cameraEndPos = fixedTarget;
                    break;
                case MoveMode.SpeedAndDistance:
                case MoveMode.VariableSpeed:
                    _cameraEndPos = _cameraStartPos + finalDir * camDist;
                    break;
            }

            _inited = true;
            _cachedCamMotion = camMotion;
        }

        // 计算移动进度
        float progress;
        if (mode == MoveMode.VariableSpeed)
        {
            float v0 = startSpeed;
            float v1 = endSpeed;
            float t = curTime;
            float totalDist = (v0 + v1) * 0.5f * totalTime;

            if (totalDist <= 0)
                progress = 0;
            else
            {
                float covered = v0 * t + (v1 - v0) * t * t / (2 * totalTime);
                progress = Mathf.Clamp01(covered / totalDist);
            }
        }
        else
        {
            progress = Mathf.Clamp01(curTime / totalTime);
        }

        camMotion.LerpMoveCamera(_cameraEndPos, progress);
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        _cachedCamMotion?.ResetCameraOrigin();
    }
}