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

    [Header("移动")]
    public Vector3 InputMove;
    public float Speed;
    public bool isWalking;
    public bool IsBlock = false;

    [Header("停下")]
    public bool isStopping;
    public float stopMoveLockTime;

    [Header("攻击")]
    public bool IsAttacking;

    [Header("相机")]
    public Vector3 moveDir;

    [Header("引用")]
    public Rigidbody rb;
    public Animator am;

    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
        am = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        Speed = playerSO.WalkSpeed;
    }

    public void Start()
    {
        if (actionControl.timelineDirector != null)
        {
            actionControl.timelineDirector.stopped += (director) =>
            {
                Is_Action_Playing = false;
            };
        }
    }

    public void FixedUpdate()
    {
        Move_Follow_Camera();

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
        float VerticalVelocity = rb.velocity.y;
        Vector3 HorizontalVelocity = moveDir * Speed;
        rb.velocity = new Vector3(HorizontalVelocity.x, VerticalVelocity, HorizontalVelocity.z);
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

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            isWalking = false;
            InputMove = Vector3.zero;
            StopCurrentAction();
            actionControl.PlayAction(actionControl.CrossActions[Random.Range(0, actionControl.CrossActions.Count)]);
            Is_Action_Playing = true;
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
        int index = 0;
        Collider[] enemies = Physics.OverlapSphere(transform.position, playerSO.DetectionRadius, LayerMask.GetMask("Enemy"));
        if (enemies.Length > 0)
        {
            mindistance = Vector3.Distance(enemies[0].transform.position, transform.position);
            for(int i = 0; i < enemies.Length; i++)
            {
                float distance = Vector3.Distance(enemies[i].transform.position, transform.position);
                if (distance < mindistance)
                {
                    mindistance = distance;
                    index = i;
                }
            }
            LookDir = enemies[index].transform.position - transform.position;
            LookDir.y = 0;
            LookDir.Normalize();
            transform.rotation = Quaternion.LookRotation(LookDir);
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
}