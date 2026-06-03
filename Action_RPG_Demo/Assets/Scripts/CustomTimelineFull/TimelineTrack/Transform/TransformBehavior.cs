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

    public float checkRadius = 0.5f;
    public float groundOffset = 0.5f;

    private Rigidbody _rb;
    private Player _player;

    [Header("抓墙IK")]
    public Transform leftHandTarget;
    public Transform rightHandTarget;

    [Header("双手展开宽度")]
    public float handSpread = 0.1f;

    [Header("深度偏移")]
    public float forwardOffset = -0.1f;

    [Header("高度偏移")]
    public float heightOffset = 0.1f;

    private FullBodyBipedIK _fbIK;
    private float _leftHandIKWeight;
    private float _rightHandIKWeight;

    private RaycastHit _lastClimbWallHit;
    private bool _hasValidClimbWall;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        _inited = false;
        _isBlocked = false;
        _rb = null;
        _leftHandIKWeight = 0f;
        _rightHandIKWeight = 0f;

        //// 每次爬墙开始清空缓存
        //_hasValidClimbWall = false;
        //_lastClimbWallHit = new RaycastHit();
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
        if (_fbIK == null)
        {
            _fbIK = trans.GetComponent<FullBodyBipedIK>();
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

        if (clip.moveMode == MoveMode.ParkourClimb)
        {
            DoParkourClimbMove(trans, _player, info.deltaTime);
            trans.rotation = Rot;
            return;
        }

        float progress = CalculateProgress(playable, curTime, duration);
        Vector3 targetPos = Vector3.Lerp(_startPos, _endPos, progress);
        Vector3 moveDelta = targetPos - trans.position;
        float moveLen = moveDelta.magnitude;
        Vector3 finalPos = targetPos;

        if (clip.moveMode != MoveMode.ClimbOver && moveLen > 0.001f)
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

        if (_rb != null)
            _rb.MovePosition(finalPos);

        trans.rotation = Rot;
    }

    private void DoParkourClimbMove(Transform trans, Player player, float deltaTime)
    {
        var stage = clip.parkourClimbStage;
        if (player == null || _fbIK == null || _rb == null)
            return;

        _rb.useGravity = false;
        _rb.velocity = Vector3.zero;
        _rb.isKinematic = true;

        Transform leftTarget = player.leftHandIKTarget;
        Transform rightTarget = player.rightHandIKTarget;

        Vector3 rayOrigin = trans.position + Vector3.up * 1.2f;
        Vector3 forwardDir = trans.forward;
        const float detectRange = 1.2f;
        if (stage == ParkourClimbStage.RunToWall || stage == ParkourClimbStage.WallRunUp || stage == ParkourClimbStage.GrabEdge || stage == ParkourClimbStage.Hang)
        {
            if (Physics.Raycast(rayOrigin, forwardDir, out RaycastHit hit, detectRange))
            {
                if (hit.collider != null && hit.collider.transform != trans)
                {
                    _lastClimbWallHit = hit;
                    _hasValidClimbWall = true;
                }
            }
        }

        Vector3 climbTopPoint = trans.position;
        if (_hasValidClimbWall)
        {
            Bounds b = _lastClimbWallHit.collider.bounds;
            climbTopPoint = new Vector3(
                _lastClimbWallHit.point.x,
                b.max.y,
                _lastClimbWallHit.point.z
            );

            if (leftTarget != null && rightTarget != null)
            {
                float w = 0.25f;
                leftTarget.position = climbTopPoint - trans.right * w;
                rightTarget.position = climbTopPoint + trans.right * w;
            }
        }

        _leftHandIKWeight = Mathf.MoveTowards(_leftHandIKWeight, 1, 3 * deltaTime);
        _rightHandIKWeight = Mathf.MoveTowards(_rightHandIKWeight, 1, 3 * deltaTime);

        _fbIK.solver.leftHandEffector.positionWeight = _leftHandIKWeight;
        _fbIK.solver.rightHandEffector.positionWeight = _rightHandIKWeight;
        _fbIK.solver.SetEffectorWeights(FullBodyBipedEffector.LeftHand, 1, 1);
        _fbIK.solver.SetEffectorWeights(FullBodyBipedEffector.RightHand, 1, 1);

        if (leftTarget != null) _fbIK.solver.leftHandEffector.target = leftTarget;
        if (rightTarget != null) _fbIK.solver.rightHandEffector.target = rightTarget;

        Vector3 targetPos = trans.position;
        float speed = 3f;

        if (stage == ParkourClimbStage.RunToWall)
            targetPos = trans.position;
        else if (stage == ParkourClimbStage.WallRunUp)
            targetPos = climbTopPoint + trans.forward * 0.3f;
        else if (stage == ParkourClimbStage.GrabEdge)
            targetPos = climbTopPoint + trans.forward * 0.25f;
        else if (stage == ParkourClimbStage.Hang)
            targetPos = climbTopPoint + trans.forward * 0.2f;
        else if (stage == ParkourClimbStage.ClimbToTop)
            targetPos = climbTopPoint + new Vector3(0, 0.6f, 0);

        trans.position = Vector3.MoveTowards(trans.position, targetPos, speed * deltaTime);
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

    public void OnDrawGizmos()
    {
        if (_hasValidClimbWall)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(_lastClimbWallHit.collider.bounds.center, 0.1f);
        }
    }
}