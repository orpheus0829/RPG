using UnityEngine;
using UnityEngine.Playables;

public class TransformBehaviour : PlayableBehaviour
{
    public TransformTimelineClip clip;

    private Vector3 _startPos;
    private Vector3 _endPos;
    private bool _inited;
    private bool _isBlocked;


    // 检测半径，按角色体型调整
    public float checkRadius = 0.5f;
    public float groundOffset = 0.5f;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        _inited = false;
        _isBlocked = false;
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        Transform trans = playerData as Transform;
        if (trans == null || clip == null || _isBlocked) return;

        float curTime = (float)playable.GetTime();
        float duration = (float)playable.GetDuration();
        if (duration <= 0) return;

        if (!_inited)
        {
            _startPos = trans.position;
            ReadParams(out var mode, out var dir, out var endPos, out _, out var dist, out _, out _);

            Vector3 finalDir = trans.TransformDirection(dir);
            finalDir.Normalize();

            switch (mode)
            {
                case MoveMode.FixedEndPos:
                    _endPos = endPos;
                    break;
                case MoveMode.SpeedAndDistance:
                case MoveMode.VariableSpeed:
                    _endPos = _startPos + finalDir * dist;
                    break;
            }
            _inited = true;
        }

        ReadParams(out var currentMode, out _, out _, out _, out _, out var startSpeed, out var endSpeed);
        float progress;

        if (currentMode == MoveMode.VariableSpeed)
        {
            float v0 = startSpeed;
            float v1 = endSpeed;
            float t = curTime;
            float totalTime = duration;
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
            progress = Mathf.Clamp01(curTime / duration);
        }

        Vector3 targetPos = Vector3.Lerp(_startPos, _endPos, progress);
        Vector3 moveDelta = targetPos - trans.position;
        float moveLen = moveDelta.magnitude;

        // 检测所有碰撞体，忽略触发器
        if (moveLen > 0.001f)
        {
            if (Physics.SphereCast(trans.position + Vector3.up * groundOffset, checkRadius, moveDelta.normalized, out RaycastHit hit, moveLen,
                Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                trans.position = hit.point;
                _isBlocked = true;
                return;
            }
        }

        trans.position = targetPos;
    }

    private void ReadParams(out MoveMode mode, out Vector3 dir, out Vector3 endPos,
        out float speed, out float dist, out float startSpd, out float endSpd)
    {
        if (clip.data != null)
        {
            mode = clip.data.moveMode;
            dir = clip.data.direction;
            endPos = clip.data.endPos;
            speed = clip.data.moveSpeed;
            dist = clip.data.totalDistance;
            startSpd = clip.data.startSpeed;
            endSpd = clip.endSpeed;
        }
        else
        {
            mode = clip.moveMode;
            dir = clip.direction;
            endPos = clip.endPos;
            speed = clip.moveSpeed;
            dist = clip.totalDistance;
            startSpd = clip.startSpeed;
            endSpd = clip.endSpeed;
        }
    }
}