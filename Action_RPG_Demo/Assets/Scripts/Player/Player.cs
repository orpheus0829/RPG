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
        Back_To_Move();

        //if (!isWalking)
        //{
        //    InputMove = Vector3.zero;
        //}
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
        if (actionControl.currentAction == actionControl.WalkStart || actionControl.currentAction == actionControl.walkAction)
        {
            if (InputMove.magnitude <= 0.1f)
            {
                isWalking = false;
                actionControl.PlayAction(actionControl.idleAction);
            }
            return;
        }
        if (InputMove.magnitude > 0.1f && actionControl.canInterrupt)
        {
            StopCurrentAction();
            actionControl.AttackLevel = 0;
            actionControl.PlayAction(actionControl.WalkStart);
        }
        else if (!Is_Action_Playing)
        {
            actionControl.PlayAction(actionControl.idleAction);
        }
    }

    public void OnMove(InputValue value)
    {
        try
        {
            //if (IsAttacking)
            //{
            //    IsAttacking = false;
            //    return;
            //}
            if (!actionControl.canInterrupt)
            {
                return;
            }
            isWalking = true;
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
            StopCurrentAction();
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
            actionControl.PlayAttackAction();
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