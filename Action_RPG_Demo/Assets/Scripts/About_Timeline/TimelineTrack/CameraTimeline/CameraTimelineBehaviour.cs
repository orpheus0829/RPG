using UnityEngine;
using UnityEngine.Playables;

public class CameraTimelineBehaviour : PlayableBehaviour
{
    public CameraTimelineClip clip;
    private float _cacheOriginRotX;
    private float _cacheOriginRotY;
    private float _cacheOriginDist;
    private float _cacheOriginHeight;
    private Quaternion _cacheOriginWorldRot;
    private bool _isInit;
    private bool _oldCameraAnimState;
    public static int ActiveCameraClipCount = 0;
    public static bool IsLockMoveToCharForward = false;
    public bool isPlayer;

    // 归位模式缓存：片段启动瞬间相机原始坐标（归位目标）
    private Vector3 _resetTargetPos;

    public override void OnGraphStart(Playable playable)
    {
        _isInit = false;
        _resetTargetPos = Vector3.zero;
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        _isInit = false;
        ActiveCameraClipCount++;
        _resetTargetPos = Vector3.zero;
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        BaseActor actor = playerData as BaseActor;
        if (actor == null)
        {
            return;
        }
        Transform roleTrans = actor.transform;
        ActionControl ctrl = actor.GetComponent<ActionControl>();
        isPlayer = actor != null && actor.GetComponent<ActionControl>() != null;

        CameraPivot camPivot = CameraPivot.instance;
        if (camPivot == null)
        {
            return;
        }

        float totalDuration = (float)playable.GetDuration();
        float currentTime = (float)playable.GetTime();
        if (totalDuration <= 0.0001f)
        {
            return;
        }

        // 初始化
        if (_isInit == false)
        {
            // 归位模式：开播瞬间记录当前相机位置作为终点
            if (clip.cameraMoveMode == CamMoveMode.ResetOrigin)
            {
                _resetTargetPos = camPivot.transform.position;
                _isInit = true;
            }
            else if (!clip.useLastFrameAsOrigin)
            {
                // 原有直线/环绕初始化缓存
                _cacheOriginRotX = camPivot.rotX;
                _cacheOriginRotY = camPivot.rotY;
                _cacheOriginDist = camPivot.distance;
                _cacheOriginHeight = camPivot.height;
                _cacheOriginWorldRot = camPivot.transform.rotation;
                _oldCameraAnimState = camPivot.isPlayingCameraAnim;
                camPivot.isPlayingCameraAnim = true;
                IsLockMoveToCharForward = isPlayer ? clip.lockMoveToCharacterForward : false;
                _isInit = true;
            }
        }

        // ========== 归位模式独立分支 ==========
        if (clip.cameraMoveMode == CamMoveMode.ResetOrigin)
        {
            float progress = GetProgress(currentTime, totalDuration, clip.resetUseVariableSpeed, clip.resetStartSpeed, clip.resetEndSpeed);
            Vector3 curCamPos = camPivot.transform.position;

            if (clip.resetSubMode == ResetCamSubMode.Teleport)
            {
                // 瞬间归位
                camPivot.transform.position = _resetTargetPos;
            }
            else
            {
                // 平滑缓动归位
                Vector3 targetLerp = Vector3.Lerp(curCamPos, _resetTargetPos, clip.resetLerpFactor * Time.deltaTime);
                camPivot.transform.position = targetLerp;
            }

            // 朝向逻辑不变
            if (clip.lockLookAtPlayer)
                camPivot.transform.LookAt(roleTrans.position + Vector3.up * 1.2f);
            return;
        }
        // =====================================

        // 原有直线/环绕进度计算
        float baseProgress;
        if (clip.useVariableSpeed == true)
        {
            float vStart = clip.startSpeed;
            float vEnd = clip.endSpeed;
            float time = currentTime;
            float totalDisplacement = (vStart + vEnd) * 0.5f * totalDuration;
            if (totalDisplacement <= 0.0001f)
            {
                baseProgress = 0f;
            }
            else
            {
                float travel = vStart * time + (vEnd - vStart) * time * time / (2f * totalDuration);
                baseProgress = Mathf.Clamp01(travel / totalDisplacement);
            }
        }
        else
        {
            baseProgress = Mathf.Clamp01(currentTime / totalDuration);
        }

        Vector3 worldTargetPos = roleTrans.TransformPoint(clip.cameraTargetLocalPos);
        Quaternion worldTargetRot = roleTrans.rotation * Quaternion.Euler(clip.cameraTargetEuler);
        Vector3 originWorldPos;
        if (clip.useLastFrameAsOrigin)
        {
            originWorldPos = camPivot.transform.position;
        }
        else
        {
            originWorldPos = GetCameraOriginWorldPos(camPivot, roleTrans);
        }

        Vector3 finalTargetPos;
        if (!clip.useSurroundMode)
        {
            // 直线模式完全保留原版逻辑
            finalTargetPos = worldTargetPos;
        }
        else
        {
            Vector3 roleFoot = roleTrans.position;
            // 固定圆心高度，全程不变
            float circleY = roleFoot.y + clip.surroundFixedHeight;
            Vector3 circleCenter = new Vector3(roleFoot.x, circleY, roleFoot.z);

            // 仅读取起点初始方位角，半径固定为面板surroundRadius，不受终点拖拽影响
            Vector3 originFlat = originWorldPos - roleFoot;
            originFlat.y = 0;
            float startAngleDeg = Mathf.Atan2(originFlat.x, originFlat.z) * Mathf.Rad2Deg;

            // 进度叠加旋转角度
            float curAngleDeg = startAngleDeg + clip.surroundTotalAngle * baseProgress;
            float curRad = Mathf.Deg2Rad * curAngleDeg;

            // 固定半径生成XZ圆弧
            float x = Mathf.Sin(curRad) * clip.surroundRadius;
            float z = Mathf.Cos(curRad) * clip.surroundRadius;
            Vector3 circleXZ = new Vector3(x, 0, z);

            // 高度全程固定，不插值升降
            finalTargetPos = circleCenter + circleXZ;
        }

        if (clip.cameraMoveMode == CamMoveMode.Teleport)
        {
            if (baseProgress > 0)
            {
                camPivot.transform.position = finalTargetPos;
                SetCameraLook(camPivot, roleTrans, finalTargetPos, worldTargetRot);
            }
        }
        else
        {
            Vector3 lerpPos = Vector3.Lerp(originWorldPos, finalTargetPos, baseProgress);
            camPivot.transform.position = Vector3.Lerp(camPivot.transform.position, lerpPos, clip.smoothLerpFactor * Time.deltaTime);
            Quaternion lerpRot;
            if (clip.useLastFrameAsOrigin)
            {
                lerpRot = Quaternion.Lerp(camPivot.transform.rotation, worldTargetRot, baseProgress);
            }
            else
            {
                lerpRot = Quaternion.Lerp(_cacheOriginWorldRot, worldTargetRot, baseProgress);
            }
            SetCameraLook(camPivot, roleTrans, finalTargetPos, lerpRot);
        }
    }

    /// 统一进度计算函数（复用给归位变速）
    private float GetProgress(float curTime, float duration, bool useVarSpeed, float sSpd, float eSpd)
    {
        if (!useVarSpeed)
            return Mathf.Clamp01(curTime / duration);

        float totalDisplacement = (sSpd + eSpd) * 0.5f * duration;
        if (totalDisplacement <= 0.0001f)
            return 0f;
        float travel = sSpd * curTime + (eSpd - sSpd) * curTime * curTime / (2f * duration);
        return Mathf.Clamp01(travel / totalDisplacement);
    }

    private void SetCameraLook(CameraPivot camPivot, Transform roleTrans, Vector3 targetPos, Quaternion targetRot)
    {
        if (clip.lockLookAtPlayer == true)
        {
            camPivot.transform.LookAt(roleTrans.position + Vector3.up * 1.2f);
        }
        else
        {
            camPivot.transform.rotation = targetRot;
        }
    }

    private Vector3 GetCameraOriginWorldPos(CameraPivot camPivot, Transform roleTrans)
    {
        Quaternion originRot = Quaternion.Euler(_cacheOriginRotX, _cacheOriginRotY, 0);
        Vector3 dir = originRot * Vector3.back;
        Vector3 pos = roleTrans.position + dir * _cacheOriginDist;
        pos.y += _cacheOriginHeight;
        return pos;
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        ActiveCameraClipCount--;
        RestoreCameraOrigin();
    }

    public override void OnGraphStop(Playable playable)
    {
        RestoreCameraOrigin();
    }

    private void RestoreCameraOrigin()
    {
        CameraPivot camPivot = CameraPivot.instance;
        if (camPivot == null || _isInit == false)
        {
            return;
        }
        // 归位模式不执行原始缓存还原
        if (clip.cameraMoveMode != CamMoveMode.ResetOrigin && isPlayer && !clip.useLastFrameAsOrigin)
        {
            camPivot.rotX = _cacheOriginRotX;
            camPivot.rotY = _cacheOriginRotY;
            camPivot.distance = _cacheOriginDist;
            camPivot.height = _cacheOriginHeight;
            camPivot.TargetDistance = camPivot.distance;
            camPivot.transform.rotation = _cacheOriginWorldRot;
            if (ActiveCameraClipCount <= 0)
            {
                camPivot.isPlayingCameraAnim = _oldCameraAnimState;
            }
        }
        if (ActiveCameraClipCount <= 0)
        {
            IsLockMoveToCharForward = false;
        }
        _isInit = false;
    }
}