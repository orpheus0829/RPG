using UnityEngine;
using UnityEngine.Playables;
using RootMotion.FinalIK;

public class TransformBehaviour : PlayableBehaviour
{
    public TransformTimelineClip clip;
    public Quaternion Rot;

    private Vector3 _startPos;
    private Vector3 _endPos;
    private Vector3 _obstacleTopPos;
    private bool _inited;
    private bool _isBlocked;

    private Vector3 _climbFinalPosition;

    public float checkRadius = 0.5f;
    public float groundOffset = 0.5f;

    private Rigidbody _rb;
    private Player _player;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        _inited = false;
        _isBlocked = false;
        _rb = null;
        _climbFinalPosition = Vector3.zero;
    }
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (clip.moveMode == MoveMode.ClimbOver && _player != null)
        {
            _player.transform.position = _climbFinalPosition;
            if (_rb != null)
            {
                _rb.position = _climbFinalPosition;
            }
        }
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (_isBlocked)
        {
            return;
        }

        Transform trans = playerData as Transform;
        if (trans == null || clip == null)
        {
            return;
        }

        if (_player == null)
        {
            _player = trans.GetComponent<Player>();
        }
        if (_rb == null && _player != null)
        {
            _rb = _player.rb;
        }

        float curTime = (float)playable.GetTime();
        float duration = (float)playable.GetDuration();
        if (duration <= 0) return;

        if (!_inited)
        {
            Rot = trans.rotation;
            _startPos = trans.position;

            if (clip.moveMode == MoveMode.ClimbOver)
            {
                _obstacleTopPos = _player.vaultObstacleTopPoint;
                if (_obstacleTopPos.magnitude < 0.01f)
                    _obstacleTopPos = _startPos;
            }

            if (clip.moveMode == MoveMode.ClimbOver)
            {
                if (clip.climbStage == ClimbStage.BeforeClimb)
                    _endPos = _obstacleTopPos;
                else
                {
                    Vector3 f = trans.forward;
                    f.y = 0; f.Normalize();
                    _endPos = _obstacleTopPos + f * clip.climbAfterExtraDistance;
                }
            }
            else
            {
                InitNormalMode(trans);
            }
            _inited = true;
        }

        //float progress = CalculateProgress(playable, curTime, duration);
        //Vector3 targetPos = Vector3.Lerp(_startPos, _endPos, progress);
        //Vector3 moveDelta = targetPos - trans.position;
        //float moveLen = moveDelta.magnitude;
        //Vector3 finalPos = targetPos;

        //if (clip.moveMode != MoveMode.ClimbOver && moveLen > 0.001f)
        //{
        //    Vector3 rayDir = moveDelta.normalized;
        //    Vector3 flatDir = new Vector3(rayDir.x, 0, rayDir.z).normalized;
        //    float rayDist = moveLen;
        //    float castR = _player.col.radius;
        //    Vector3 rayStart = trans.position + Vector3.up * castR;

        //    if (Physics.SphereCast(rayStart, checkRadius, flatDir, out RaycastHit hit, rayDist, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        //    {
        //        finalPos = hit.point - flatDir * 0.01f;
        //        _isBlocked = true;
        //    }
        //}
        float progress = CalculateProgress(playable, curTime, duration);
        Vector3 targetPos = Vector3.Lerp(_startPos, _endPos, progress);
        Vector3 finalPos = targetPos;

        if (clip.moveMode != MoveMode.ClimbOver)
        {
            Vector3 moveDelta = targetPos - trans.position;
            float moveLen = moveDelta.magnitude;

            if (moveLen > 0.001f)
            {
                Vector3 rayDir = moveDelta.normalized;
                Vector3 flatDir = new Vector3(rayDir.x, 0, rayDir.z).normalized;
                float rayDist = moveLen;
                float castR = _player.col.radius;
                Vector3 rayStart = trans.position + Vector3.up * castR;

                if (Physics.SphereCast(rayStart, checkRadius, flatDir, out RaycastHit hit, rayDist, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                {
                    finalPos = hit.point - flatDir * 0.01f;
                    _isBlocked = true;
                }
            }
        }
        else
        {
            _climbFinalPosition = finalPos;
        }

        if (_rb != null)
        {
            _rb.MovePosition(finalPos);
        }

        trans.rotation = Rot;
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
                return Mathf.Clamp01(curTime / duration);
            else
            {
                if (clip.climbUseVariableSpeed)
                {
                    float v0 = clip.climbStartSpeed;
                    float v1 = clip.climbEndSpeed;
                    float total = (v0 + v1) * 0.5f * duration;
                    if (total <= 0) return 0;
                    float covered = v0 * curTime + (v1 - v0) * curTime * curTime / 2;
                    return Mathf.Clamp01(covered / total);
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
            float total = (v0 + v1) * 0.5f * duration;
            if (total <= 0) return 0;
            float covered = v0 * curTime + (v1 - v0) * curTime * curTime / 2;
            return Mathf.Clamp01(covered / total);
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