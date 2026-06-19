using UnityEngine;
using UnityEngine.Playables;
using RootMotion.FinalIK;

public class TransformBehaviour : PlayableBehaviour
{
    public TransformTimelineClip clip;
    public Quaternion Rot;
    private Vector3 _startPos;
    private Vector3 _curPos;
    private Vector3 _moveDir;
    private Vector3 _obstacleTopPos;
    private bool _inited;
    private bool _isBlocked;
    private Vector3 _climbFinalPosition;

    private bool _hasTeleported;
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
        _curPos = Vector3.zero;
        _moveDir = Vector3.zero;
        _hasTeleported = false;
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
        if (_isBlocked && !(clip.moveMode == MoveMode.ClimbOver && clip.climbStage == ClimbStage.AfterClimb))
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
        float deltaTime = (float)info.deltaTime;
        if (duration <= 0 || deltaTime <= 0)
        {
            return;
        }
        if (!_inited)
        {
            Rot = trans.rotation;
            _startPos = trans.position;
            _curPos = _startPos;
            if (clip.moveMode != MoveMode.ClimbOver)
            {
                ReadParams(out _, out Vector3 localDir, out _, out _);
                _moveDir = trans.TransformDirection(localDir);
                _moveDir.y = 0;
                _moveDir.Normalize();
            }
            else
            {
                _obstacleTopPos = _player.vaultObstacleTopPoint;
                if (_obstacleTopPos.magnitude < 0.01f)
                {
                    _obstacleTopPos = _startPos;
                }

                Vector3 flatForward = trans.forward;
                flatForward.y = 0;
                flatForward.Normalize();
                if (clip.climbStage == ClimbStage.BeforeClimb)
                {
                    _moveDir = (_obstacleTopPos - _startPos).normalized;
                }
                else
                {
                    _moveDir = flatForward;
                }
            }
            _inited = true;
        }

        if (clip.moveMode == MoveMode.FixedEndPos)
        {
            if (!_hasTeleported)
            {
                Vector3 targetTeleportPos = clip.endPos;
                if (_rb != null)
                {
                    _rb.position = targetTeleportPos;
                    _rb.velocity = Vector3.zero;
                    _rb.angularVelocity = Vector3.zero;
                }
                trans.position = targetTeleportPos;
                _curPos = targetTeleportPos;
                _hasTeleported = true;
            }
            trans.rotation = Rot;
            return;
        }

        float frameSpeed = GetCurrentFrameSpeed(curTime, duration);
        Vector3 frameDelta = _moveDir * frameSpeed * deltaTime;
        Vector3 targetNextPos = _curPos + frameDelta;

        bool shouldCheckCollision = clip.moveMode != MoveMode.ClimbOver || clip.climbStage == ClimbStage.BeforeClimb;
        if (shouldCheckCollision)
        {
            float castR = _player.col.radius;
            Vector3 rayStart = _curPos + Vector3.up * castR;
            Vector3 flatDir = frameDelta.normalized;
            float rayDist = frameDelta.magnitude;
            if (rayDist > 0.001f && Physics.SphereCast(rayStart, checkRadius, flatDir, out RaycastHit hit, rayDist, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                targetNextPos = hit.point - flatDir * 0.01f;
                _isBlocked = true;
            }
        }
        if (clip.moveMode == MoveMode.ClimbOver)
        {
            _climbFinalPosition = targetNextPos;
        }

        _curPos = targetNextPos;
        if (_rb != null)
        {
            _rb.MovePosition(_curPos);
        }
        trans.rotation = Rot;
    }
    private float GetCurrentFrameSpeed(float curTime, float duration)
    {
        float t = Mathf.Clamp01(curTime / duration);
        switch (clip.moveMode)
        {
            case MoveMode.SpeedAndDistance:
                return clip.moveSpeed;

            case MoveMode.VariableSpeed:
                return Mathf.Lerp(clip.startSpeed, clip.endSpeed, t);

            case MoveMode.ClimbOver:
                if (clip.climbUseVariableSpeed)
                {
                    return Mathf.Lerp(clip.climbStartSpeed, clip.climbEndSpeed, t);
                }
                else
                {
                    return clip.climbSpeed;
                }

            case MoveMode.FixedEndPos:
                return 0;

            default:
                return clip.moveSpeed;
        }
    }

    private void ReadParams(out MoveMode mode, out Vector3 dir, out Vector3 endPos, out float dist)
    {
        mode = clip.moveMode;
        dir = clip.direction;
        endPos = clip.endPos;
        dist = clip.totalDistance;
    }
}