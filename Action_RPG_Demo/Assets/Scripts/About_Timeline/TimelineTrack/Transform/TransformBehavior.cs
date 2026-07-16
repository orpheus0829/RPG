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

    private Vector3 _circleCenterWorld;
    private float _circleRotatedAngle;

    private bool _hasTeleported;
    public float checkRadius = 0.5f;
    public float groundOffset = 0.5f;

    private Rigidbody _rb;
    private CapsuleCollider _selfCol;
    private Player _player;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        _inited = false;
        _isBlocked = false;
        _rb = null;
        _selfCol = null;
        _player = null;
        _climbFinalPosition = Vector3.zero;
        _curPos = Vector3.zero;
        _moveDir = Vector3.zero;
        _hasTeleported = false;
        _circleRotatedAngle = 0f;
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
        if (clip.moveMode == MoveMode.CircleRotate)
        {
            if (_rb != null)
            {
                _rb.position = _curPos;
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
            _player = trans.GetComponentInParent<Player>();
        }
        if (_player && _player.CurAC.currentAction.actionType == ActionType.Attack && _player.AtkTo)
        {
            if(Vector3.Distance(trans.position, _player.AtkTo.transform.position) <= _player.AttackStopDistance)
            {
                trans.rotation = Rot;
                return;
            }
        }
        if (_rb == null)
        {
            _rb = trans.GetComponent<Rigidbody>();
        }
        if (_selfCol == null)
        {
            _selfCol = trans.GetComponent<CapsuleCollider>();
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
            if (clip.moveMode != MoveMode.ClimbOver && clip.moveMode != MoveMode.CircleRotate)
            {
                ReadParams(out _, out Vector3 localDir, out _, out _);
                _moveDir = trans.TransformDirection(localDir);
                _moveDir.y = 0;
                _moveDir.Normalize();
            }
            else if (clip.moveMode == MoveMode.CircleRotate)
            {
                _circleCenterWorld = trans.TransformPoint(clip.circleCenterLocal);
            }
            else
            {
                if (_player != null && _player.vaultObstacleTopPoint.magnitude > 0.01f)
                {
                    _obstacleTopPos = _player.vaultObstacleTopPoint;
                }
                else
                {
                    _obstacleTopPos = _startPos;
                }
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
            if (!_inited)
            {
                Rot = trans.rotation;
                _startPos = trans.position;
                _curPos = _startPos;
                _inited = true;
            }

            if (!_hasTeleported)
            {
                Vector3 targetTeleportPos = _startPos + trans.TransformVector(clip.endPos);
                Quaternion targetRot = Rot * Quaternion.Euler(clip.endEuler);

                if (_rb != null)
                {
                    _rb.position = targetTeleportPos;
                    _rb.rotation = targetRot;
                    _rb.velocity = Vector3.zero;
                    _rb.angularVelocity = Vector3.zero;
                }
                trans.position = targetTeleportPos;
                trans.rotation = targetRot;
                _curPos = targetTeleportPos;
                _hasTeleported = true;
            }
            trans.rotation = Rot * Quaternion.Euler(clip.endEuler);
            return;
        }

        else if (clip.moveMode == MoveMode.CircleRotate)
        {
            float t = Mathf.Clamp01(curTime / duration);
            float dirSign = clip.circleClockwise ? -1f : 1f;
            float currentTotalAngle = clip.circleTotalAngle * t * dirSign;
            Vector3 initRaw = (_startPos - _circleCenterWorld);
            initRaw.y = 0;
            Vector3 initDir = initRaw.magnitude < 0.001f ? Vector3.right : initRaw.normalized;
            Vector3 baseOffset = initDir * clip.circleRadius;
            Quaternion rotStep = Quaternion.Euler(0, currentTotalAngle, 0);
            Vector3 finalOffset = rotStep * baseOffset;
            Vector3 targetNextPos = _circleCenterWorld + finalOffset;
            _curPos = targetNextPos;
            trans.position = _curPos;
            trans.rotation = Rot;
        }
        else
        {
            float frameSpeed = GetCurrentFrameSpeed(curTime, duration);
            Vector3 frameDelta = _moveDir * frameSpeed * deltaTime;
            Vector3 targetNextPos = _curPos + frameDelta;
            bool shouldCheckCollision = clip.moveMode != MoveMode.ClimbOver || clip.climbStage == ClimbStage.AfterClimb;
            if (shouldCheckCollision && _selfCol != null)
            {
                float castR = _selfCol.radius;
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
            case MoveMode.CircleRotate:
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