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

    [Header("相机")]
    public Vector3 moveDir;

    [Header("动作")]
    public ActionSO idleAction;
    public ActionSO runAction;
    public ActionSO attack1Action;

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
        Is_Action_Playing = false;
    }
    public void OnMove(InputValue value)
    {
        try
        {
            Vector3 moveInput = value.Get<Vector3>();
            InputMove = moveInput;
            if (Is_Action_Playing && moveInput.magnitude > 0.1f)
            {
                StopCurrentAction();
                Is_Action_Playing = true;
                actionControl.PlayAction(idleAction);
            }
        }
        catch
        {

        }
    }
    public void OnRun(InputValue value)
    {
        if (value.isPressed)
        {
            StopCurrentAction();
            Is_Action_Playing = true;
            actionControl.PlayAction(runAction);
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
            StopCurrentAction();
            Is_Action_Playing = true;
            actionControl.PlayAction(attack1Action);
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
