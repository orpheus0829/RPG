using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public PlayerSO playerSO;
    public ActionControl actionControl;
    public PlayerInput playerInput;
    public bool Is_Action_Playing;

    [Header("“∆∂Ø")]
    public Vector3 InputMove;
    public float Speed;
    public bool isWalking;
    public bool IsBlock = false;

    [Header("Õ£œ¬")]
    public bool isStopping;
    public float stopMoveLockTime;

    [Header("Ã¯‘æ…»–ŒºÏ≤‚≈‰÷√")]
    public bool IsJump;
    public float scanAngle = 120f;
    public float horizontalScanRange;
    public float verticalMaxScanHeight = 3f;
    public int fanRayCount = 7;

    [Header("∑≠‘ΩºÏ≤‚")]
    public float vaultBackCheckDist = 1.2f;
    public float vaultBackCapsuleRadius = 0.3f;
    public Vector3 vaultObstacleTopPoint;
    public Vector3 vaultObstacleBottomPoint;

    [Header("∑≠‘ΩŒª“∆")]
    public float vaultMoveDuration = 0.4f;
    public float vaultForwardOffset = 0.6f;
    public bool isDoingVaultMove = false;
    public int vaultMoveMode = 0;
    public float vaultCurrentTime;
    public Vector3 vaultStartPos;
    public Vector3 vaultEndPos;

    public readonly float rayOriginOffset = 0.2f;

    [Header("≈ ≈¿∂•—ÿÀÆ∆Ω…‰œﬂ")]
    public float rayUpOffset = 1.2f;
    public float rayForwardOffset = 0.3f;
    public float rayLength = 0.5f;

    [Header("≈ ≈¿ IK ƒø±Í")]
    public Transform leftHandIKTarget;
    public Transform rightHandIKTarget;

    [Header("π•ª˜")]
    public bool IsAttacking;

    [Header("œ‡ª˙")]
    public Vector3 moveDir;

    [Header("“˝”√")]
    public Rigidbody rb;
    public Animator am;
    public CapsuleCollider col;

    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
        am = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider>();
        playerInput = GetComponent<PlayerInput>();
        Speed = playerSO.WalkSpeed;
        rb.useGravity = true;
    }

    public void Start()
    {
        if (actionControl.timelineDirector != null)
        {
            actionControl.timelineDirector.stopped += (director) =>
            {
                Is_Action_Playing = false;
                rb.velocity = Vector3.zero;
                rb.isKinematic = false;
            };
        }
    }
    public void Update()
    {
        Update_Vault();
    }

    public void FixedUpdate()
    {
        Move_Follow_Camera();
        Climb_Scan();
        if (stopMoveLockTime > 0)
        {
            stopMoveLockTime -= Time.fixedDeltaTime;
            if (actionControl.currentAction == actionControl.WalkEnd && isWalking)
            {
                actionControl.PlayAction(actionControl.WalkStart);
            }
            //InputMove = Vector3.zero;
        }
        else
        {
            isStopping = false;
        }
        Back_To_Move();
        if (!Is_Action_Playing)
        {
            float VerticalVelocity = rb.velocity.y;
            Vector3 HorizontalVelocity = moveDir * Speed;
            rb.velocity = new Vector3(HorizontalVelocity.x, VerticalVelocity, HorizontalVelocity.z);
        }
    }

    public void StopCurrentAction()
    {
        if (actionControl.timelineDirector != null)
        {
            actionControl.timelineDirector.Stop();
        }
        rb.velocity = Vector3.zero;
        Is_Action_Playing = false;
    }

    public void Back_To_Move()
    {
        if (isStopping)
        {
            return;
        }
        if (actionControl.currentAction == actionControl.WalkStart || actionControl.currentAction == actionControl.walkAction)
        {
            if (InputMove.magnitude <= 0.1f)
            {
                isWalking = false;
                isStopping = true;
                stopMoveLockTime = playerSO.LockDuration;
                actionControl.PlayAction(actionControl.WalkEnd);
                return;
            }
            return;
        }
        if (isStopping && !Is_Action_Playing)
        {
            isStopping = false;
            actionControl.PlayAction(actionControl.idleAction);
            return;
        }
        if (InputMove.magnitude > 0.1f && actionControl.canInterrupt)
        {
            StopCurrentAction();
            isStopping = false;
            actionControl.AttackLevel = 0;
            actionControl.PlayAction(actionControl.WalkStart);
        }
        else if (!Is_Action_Playing && !isStopping)
        {
            actionControl.PlayAction(actionControl.idleAction);
        }
    }
    public void OnMove(InputValue value)
    {
        try
        {
            if (IsBlock)
            {
                IsBlock = false;
                return;
            }
            if (!actionControl.canInterrupt)
            {
                return;
            }
            isWalking = true;
            IsBlock = false;
            InputMove = value.Get<Vector3>();
            IsAttacking = false;
        }
        catch { }
    }

    public void OnRun(InputValue value)
    {
        if (value.isPressed)
        {
            StopCurrentAction();
            Is_Action_Playing = true;
            actionControl.PlayAction(actionControl.runAction);
        }
    }
    public void OnSlide(InputValue value)
    {
        if (value.isPressed)
        {
            if (Is_Action_Playing)
            {
                return;
            }
            isWalking = false;
            InputMove = Vector3.zero;
            StopCurrentAction();
            Is_Action_Playing = true;
            actionControl.PlayAction(actionControl.SlideAction);
        }
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            if (Is_Action_Playing)
            {
                return;
            }
            isWalking = false;
            InputMove = Vector3.zero;
            StopCurrentAction();

            int jumpResult = JumpScan();
            //jumpResult = 1;
            switch (jumpResult)
            {
                case 0:
                    actionControl.PlayAction(actionControl.JumpAction);
                    Is_Action_Playing = true;
                    Debug.Log("Ã¯‘æ");
                    break;
                case 1:
                    actionControl.PlayAction(actionControl.CrossAction);
                    Is_Action_Playing = true;
                    Debug.Log("∑≠‘Ω");
                    break;
                case 2:
                    actionControl.PlayAction(actionControl.WallUp_Start);
                    Is_Action_Playing = true;
                    Debug.Log("≈ ≈¿");
                    break;
                default:
                    Debug.Log("Ã¯‘æ±®¥Ì");
                    break;
            }
        }
    }
    public int JumpScan()
    {
        float vaultHeight = playerSO.VaultHeight;
        float highGrabHeight = playerSO.HighClimbHeight;
        float scanRadius = playerSO.JumpScanRadius;

        float halfAngle = scanAngle / 2f;
        float angleStep = scanAngle / (fanRayCount - 1);

        if (SectorRaycast(vaultHeight, highGrabHeight, scanRadius, halfAngle, angleStep))
        {
            return 2; // ≈ ≈¿
        }
        if (SectorRaycast(0.05f, vaultHeight, scanRadius, halfAngle, angleStep))
        {
            return 1; // ∑≠‘Ω
        }
        return 0; // Ã¯‘æ
    }
    public bool SectorRaycast(float minY, float maxY, float radius, float halfAngle, float angleStep)
    {
        int verticalRays = 5;
        float yStep = (maxY - minY) / (verticalRays - 1);

        for (int h = 0; h < fanRayCount; h++)
        {
            float currentAngle = -halfAngle + angleStep * h;
            Vector3 dir = Quaternion.Euler(0, currentAngle, 0) * transform.forward;

            for (int v = 0; v < verticalRays; v++)
            {
                float yPos = rayOriginOffset + minY + yStep * v;
                Vector3 origin = transform.position + Vector3.up * yPos;

                if (Physics.Raycast(origin, dir, out RaycastHit hit, radius))
                {
                    if (hit.collider.gameObject == gameObject)
                    {
                        continue;
                    }
                    float normalDot = Vector3.Dot(hit.normal, Vector3.up);
                    if (Mathf.Abs(normalDot) > 0.01f)
                    {
                        continue;
                    }

                    Bounds b = hit.collider.bounds;

                    float obstacleHeight = b.max.y - hit.point.y;
                    if (obstacleHeight < 0.4f)
                    {
                        continue;
                    }

                    vaultObstacleTopPoint = new Vector3(b.center.x, b.max.y, b.center.z);
                    vaultObstacleBottomPoint = new Vector3(b.center.x, b.min.y, b.center.z);
                    if (leftHandIKTarget != null && rightHandIKTarget != null)
                    {
                        Vector3 center = vaultObstacleTopPoint;
                        float handWidth = 0.6f;

                        Vector3 left = center - transform.right * handWidth;
                        Vector3 right = center + transform.right * handWidth;

                        leftHandIKTarget.position = left;
                        rightHandIKTarget.position = right;
                    }
                    return true;
                }
            }
        }

        vaultObstacleTopPoint = transform.position;
        return false;
    }
    public void Update_Vault()
    {
        if (isDoingVaultMove)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;

            vaultCurrentTime += Time.deltaTime;
            float remainTime = vaultMoveDuration - vaultCurrentTime;
            float totalDist = Vector3.Distance(vaultStartPos, vaultEndPos);

            if (remainTime <= 0f)
            {
                transform.position = vaultEndPos;
                isDoingVaultMove = false;
                rb.isKinematic = false;
                return;
            }

            Vector3 dir = vaultEndPos - transform.position;
            float currentDist = dir.magnitude;

            if (currentDist < 0.001f)
            {
                isDoingVaultMove = false;
                rb.isKinematic = false;
                return;
            }

            dir.Normalize();
            float step = (currentDist / remainTime) * Time.deltaTime;
            transform.position += dir * step;
        }
        else
        {
            if (rb.isKinematic)
            {
                rb.isKinematic = false;
            }
        }
    }
    public void Climb_Scan()
    {
        if (actionControl.isClimbing)
        {
            Vector3 origin = transform.position + Vector3.up * (col.height * 0.5f + rayUpOffset) + transform.forward * rayForwardOffset;
            Vector3 dir = transform.forward;
            if (!Physics.Raycast(origin, dir, rayLength) && !actionControl.currentAction==actionControl.Hang)
            {
                Debug.Log("µΩ¥Ô«ΩÃÂ∂•∂À");
                StopCurrentAction();
                actionControl.PlayAction(actionControl.Hang);
                Is_Action_Playing = true;
                //ÃÓµ«∂•∫Ûµƒ¬ﬂº≠
            }
        }
    }
    public void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            IsAttacking = true;
            StopCurrentAction();
            Is_Action_Playing = true;
            AttackDectetcion();
            actionControl.PlayAttackAction();
        }
    }
    public void AfterAttack()
    {
        IsBlock = IsBlock ? false : true;
        //Debug.Log(IsBlock);
        InputMove = IsBlock ? Vector3.zero : InputMove;
        rb.velocity = IsBlock ? Vector3.zero : rb.velocity;
    }
    public void AttackDectetcion()
    {
        Vector3 LookDir;
        float mindistance;
        int index = -1;
        Collider[] enemies = Physics.OverlapSphere(transform.position, playerSO.DetectionRadius, LayerMask.GetMask("Enemy"));
        if (enemies.Length > 0)
        {
            Transform cam = Camera.main.transform;
            Vector3 camForward = cam.forward;
            camForward.y = 0;
            camForward.Normalize();
            mindistance= Mathf.Infinity;
            for (int i = 0; i < enemies.Length; i++)
            {
                if (Vector3.Angle(camForward, enemies[i].transform.position - transform.position ) > 90)
                {
                    continue;
                }
                float distance = Vector3.Distance(enemies[i].transform.position, transform.position);
                if (distance < mindistance)
                {
                    mindistance = distance;
                    index = i;
                }
            }
            if (index != -1)
            {
                LookDir = enemies[index].transform.position - transform.position;
                LookDir.y = 0;
                LookDir.Normalize();
                transform.rotation = Quaternion.LookRotation(LookDir);
            }
        }
    }
    public void Move_Follow_Camera()
    {
        Transform cam = Camera.main.transform;
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        moveDir = camForward * InputMove.z + camRight * InputMove.x;

        if (moveDir.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(moveDir), 15f * Time.fixedDeltaTime);
        }
    }
    #region ∏®÷˙µ˜ ‘œ‘ æ
    private void OnDrawGizmos()
    {
        float vaultHeight = playerSO.VaultHeight;
        float highGrabHeight = playerSO.HighClimbHeight;
        float scanRadius = playerSO.JumpScanRadius;

        float halfAngle = scanAngle / 2f;
        float angleStep = scanAngle / (fanRayCount - 1);
        int verticalRays = 5;

        DrawSectorGizmo(0.1f, vaultHeight, scanRadius, halfAngle, angleStep, verticalRays, Color.green);
        DrawSectorGizmo(vaultHeight, highGrabHeight, scanRadius, halfAngle, angleStep, verticalRays, Color.red);
        if (actionControl.isClimbing)
        {
            Vector3 origin = transform.position + Vector3.up * (col.height * 0.5f + rayUpOffset) + transform.forward * rayForwardOffset;
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(origin, transform.forward * rayLength);
        }
    }
    private void DrawSectorGizmo(float minY, float maxY, float radius, float halfAngle, float angleStep, int verticalRays, Color color)
    {
        float yStep = (maxY - minY) / (verticalRays - 1);
        Gizmos.color = color;

        for (int h = 0; h < fanRayCount; h++)
        {
            float currentAngle = -halfAngle + angleStep * h;
            Vector3 dir = Quaternion.Euler(0, currentAngle, 0) * transform.forward;

            for (int v = 0; v < verticalRays; v++)
            {
                float yPos = rayOriginOffset + minY + yStep * v;
                Vector3 origin = transform.position + Vector3.up * yPos;
                Gizmos.DrawLine(origin, origin + dir * radius);
            }
        }
    }
    #endregion
}