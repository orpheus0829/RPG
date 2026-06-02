using UnityEngine;
using UnityEngine.Playables;

public class TransformBehaviour : PlayableBehaviour
{
    public TransformTimelineClip clip;
    public Quaternion Rot;

    private Vector3 _startPos;
    private Vector3 _endPos;
    private Vector3 _obstacleTopPos;
    private bool _inited;
    private bool _isBlocked;

    public float checkRadius = 0.5f;
    public float groundOffset = 0.5f;
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        _inited = false;
        _isBlocked = false;
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (_isBlocked)
        {
            return;
        }
        Transform trans = playerData as Transform;
        if (trans == null || clip == null || _isBlocked)
        {
            return;
        }

        Player player = trans.GetComponent<Player>();

        float curTime = (float)playable.GetTime();
        float duration = (float)playable.GetDuration();
        if (duration <= 0)
        {
            return;
        }

        if (!_inited)
        {
            Rot = trans.rotation;
            _startPos = trans.position;
            if (clip.moveMode == MoveMode.ClimbOver)
            {
                _obstacleTopPos = player.vaultObstacleTopPoint;
                if (_obstacleTopPos.magnitude < 0.01f)
                    _obstacleTopPos = _startPos;
            }

            if (clip.moveMode == MoveMode.ClimbOver)
            {
                if (clip.climbStage == ClimbStage.BeforeClimb)
                {
                    _endPos = _obstacleTopPos;
                }
                else
                {
                    Vector3 forward = trans.forward;
                    forward.y = 0;
                    forward.Normalize();
                    _endPos = _obstacleTopPos + forward * clip.climbAfterExtraDistance;
                }
            }
            else
            {
                InitNormalMode(trans);
            }

            _inited = true;
        }
        float progress = CalculateProgress(playable, curTime, duration);


        Vector3 targetPos = Vector3.Lerp(_startPos, _endPos, progress);
        Vector3 moveDelta = targetPos - trans.position;
        float moveLen = moveDelta.magnitude;

        if (moveLen > 0.001f)
        {
            Vector3 rayStart = trans.position + Vector3.up * groundOffset;
            Vector3 rayDir = moveDelta.normalized;
            float rayDist = moveLen;

            if (Physics.SphereCast(rayStart, checkRadius, rayDir, out RaycastHit hit, rayDist, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                trans.position = hit.point - rayDir * 0.01f;
                _isBlocked = true;
                return;
            }
        }
        trans.rotation = Rot;
        trans.position = targetPos;
    }

    private void InitNormalMode(Transform trans)
    {
        ReadParams(out var mode, out var dir, out var endPos, out var dist);
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
    }

    private float CalculateProgress(Playable playable, float curTime, float duration)
    {
        if (clip.moveMode == MoveMode.ClimbOver)
        {
            if (clip.climbStage == ClimbStage.BeforeClimb)
            {
                return Mathf.Clamp01(curTime / duration);
            }
            else
            {
                if (clip.climbUseVariableSpeed)
                {
                    float v0 = clip.climbStartSpeed;
                    float v1 = clip.climbEndSpeed;
                    float totalDist = (v0 + v1) * 0.5f * duration;
                    if (totalDist <= 0) return 0;

                    float covered = v0 * curTime + (v1 - v0) * curTime * curTime / (2 * duration);
                    return Mathf.Clamp01(covered / totalDist);
                }
                else
                {
                    return Mathf.Clamp01(curTime / duration);
                }
            }
        }
        else if (clip.moveMode == MoveMode.VariableSpeed)
        {
            float v0 = clip.startSpeed;
            float v1 = clip.endSpeed;
            float totalDist = (v0 + v1) * 0.5f * duration;
            if (totalDist <= 0) return 0;

            float covered = v0 * curTime + (v1 - v0) * curTime * curTime / (2 * duration);
            return Mathf.Clamp01(covered / totalDist);
        }
        else
        {
            return Mathf.Clamp01(curTime / duration);
        }
    }

    private void ReadParams(out MoveMode mode, out Vector3 dir, out Vector3 endPos, out float dist)
    {
        if (clip.data != null)
        {
            mode = clip.data.moveMode;
            dir = clip.data.direction;
            endPos = clip.data.endPos;
            dist = clip.data.totalDistance;
        }
        else
        {
            mode = clip.moveMode;
            dir = clip.direction;
            endPos = clip.endPos;
            dist = clip.totalDistance;
        }
    }
}