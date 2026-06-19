using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;

public class Player : MonoBehaviour
{
    public PlayerSO playerSO;
    public ActionControl actionControl;
    public PlayerInput playerInput;
    public bool Is_Action_Playing;
    public float AFKTime;
    public float CurAFKCount;

    [Header("移动")]
    public Vector3 InputMove;
    public InputAction moveaction;
    public bool IsHoldingMove;
    private Vector3 MoveDir;
    public float Speed;
    public bool isWalking;
    public bool IsBlock = false;

    [Header("停下")]
    public bool isStopping;
    public float stopMoveLockTime;

    [Header("跳跃扇形检测配置")]
    public bool IsJump;
    public float scanAngle = 120f;
    public float horizontalScanRange;
    public float verticalMaxScanHeight = 3f;
    public int fanRayCount = 7;

    [Header("翻越检测")]
    public float vaultBackCheckDist = 1.2f;
    public float vaultBackCapsuleRadius = 0.3f;
    public Vector3 vaultObstacleTopPoint;
    public Vector3 vaultObstacleBottomPoint;
    public bool vaultFinishedFlag;

    [Header("翻越位移")]
    public float vaultMoveDuration = 0.4f;
    public float vaultForwardOffset = 0.6f;
    public bool isDoingVaultMove = false;
    public int vaultMoveMode = 0;
    public float vaultCurrentTime;
    public Vector3 vaultStartPos;
    public Vector3 vaultEndPos;

    public readonly float rayOriginOffset = 0.2f;

    [Header("攻击")]
    public bool IsAttacking;
    public LayerMask EnemyLayer;

    [Header("死亡")]
    public bool IsDead;
    public float DeadTime;

    [Header("特殊技")]
    public float Skill_PowerPool = 0;
    public float MaxPower = 100;

    [Header("相机")]
    public Vector3 moveDir;

    [Header("============背包相关============")]

    [Header("启动资产")]
    public int Start_Money;

    [Header("背包")]
    public Player_Bag bag;

    [Header("交互")]
    public bool Can_Trade;
    public bool Can_Chat;

    [Header("引用")]
    public Rigidbody rb;
    public Animator am;
    public CapsuleCollider col;
    public Interact_Trigger Interact_Trigger;
    public DamageReceiver damageReceiver;
    public Mouse mouse;
    //public CameraPivot cameraPivot;

    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
        am = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider>();
        playerInput = GetComponent<PlayerInput>();
        //cameraPivot = GameObject.FindGameObjectWithTag("Camera_Pivot").GetComponent<CameraPivot>();
        Speed = playerSO.WalkSpeed;
        bag = GetComponent<Player_Bag>();
        Interact_Trigger = GetComponentInChildren<Interact_Trigger>();
        damageReceiver = GetComponent<DamageReceiver>();
        DeadTime = playerSO.Deadline;
        AFKTime = playerSO.AFKInterval;
        mouse = Mouse.current;
        rb.useGravity = true;

        if (PlayerPrefs.GetInt("Money", 0) <= 0)
        {
            PlayerPrefs.SetInt("Money", Start_Money);
        }
        PlayerPrefs.Save();

        moveaction = playerInput.actions["Move"];
    }
    public void OnEnable()
    {
        Game_Event.instance.Init_Store += Set_StorePanel;
        Game_Event.instance.DeathState += BornSet;
        Game_Event.instance.DeathSecState += BornAction;
    }
    public void OnDisable()
    {
        Game_Event.instance.Init_Store -= Set_StorePanel;
        Game_Event.instance.DeathState -= BornSet;
        Game_Event.instance.DeathSecState -= BornAction;
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
        moveDir = moveaction.ReadValue<Vector3>();
        IsHoldingMove = moveDir.sqrMagnitude > 0.0001f;
        if (actionControl.currentAction == actionControl.Character.Walk)
        {
            Speed = playerSO.WalkSpeed;
        }
        else if (actionControl.currentAction == actionControl.Character.Run)
        {
            Speed = playerSO.RunSpeed;
        }

        Update_Vault();
        if (Panel_Mgr.instance.IsPanelOpen)
        {
            InputMove = Vector3.zero;
        }
        if (actionControl.canCombo)
        {
            AttackDectetcion();
        }
        if (!Is_Action_Playing && !IsHoldingMove)
        {
            if (actionControl.currentAction == actionControl.Character.WalkStart || actionControl.currentAction == actionControl.Character.Walk)
            {
                isWalking = false;
                isStopping = true;
                stopMoveLockTime = playerSO.LockDuration;
                StopCurrentAction();
                actionControl.PlayAction(actionControl.Character.WalkEnd);
            }
            else if (actionControl.currentAction == actionControl.Character.Run)
            {
                isWalking = false;
                isStopping = true;
                stopMoveLockTime = playerSO.LockDuration;
                StopCurrentAction();
                actionControl.PlayAction(actionControl.Character.RunEnd);
            }
        }
        if (mouse != null && !Panel_Mgr.instance.IsPanelOpen)
        {
            Vector2 mouseScroll = mouse.scroll.ReadValue();
            float verticalScroll = mouseScroll.y;
            if (Mathf.Abs(verticalScroll) > 0.01f)
            {
                CameraPivot.instance.AddZoomDelta(verticalScroll);
            }
        }
    }

    public void FixedUpdate()
    {
        Move_Follow_Camera();
        if (actionControl.currentAction != actionControl.Character.Idle
    /*&& actionControl.currentAction != actionControl.Character.AfkIdle*/)
        {
            CurAFKCount = 0;
        }
        if(actionControl.currentAction == actionControl.Character.Idle)
        {
            if (CurAFKCount < AFKTime)
            {
                CurAFKCount += Time.fixedDeltaTime;
            }
            else
            {
                Is_Action_Playing = true;
                StopCurrentAction();
                actionControl.PlayAction(actionControl.Character.AfkIdle);
            }
        }
        if (stopMoveLockTime > 0)
        {
            stopMoveLockTime -= Time.fixedDeltaTime;
            if ((actionControl.currentAction == actionControl.Character.WalkEnd || actionControl.currentAction == actionControl.Character.RunEnd) && isWalking)
            {
                actionControl.PlayAction(actionControl.Character.WalkStart);
            }
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
        if (IsDead)
        {
            DeadTime -= Time.fixedDeltaTime;
            if (DeadTime <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    public void StopCurrentAction()
    {
        Is_Action_Playing = false;
        if (actionControl.timelineDirector != null)
        {
            actionControl.timelineDirector.Stop();
        }
    }
    #region 移动
    public void Back_To_Move()
    {
        if (vaultFinishedFlag)
        {
            vaultFinishedFlag = false;
            if (IsHoldingMove)
            {
                StopCurrentAction();
                isStopping = false;
                isWalking = true;
                actionControl.AttackLevel = 0;
                actionControl.PlayAction(actionControl.Character.WalkStart);
                return;
            }
            else
            {
                StopCurrentAction();
                isWalking = false;
                isStopping = false;
                actionControl.PlayAction(actionControl.Character.Idle);
                return;
            }
        }
        if (isStopping && !IsHoldingMove)
        {
            return;
        }
        if (actionControl.currentAction == actionControl.Character.WalkStart || actionControl.currentAction == actionControl.Character.Walk)
        {
            if (!IsHoldingMove)
            {
                isWalking = false;
                isStopping = true;
                stopMoveLockTime = playerSO.LockDuration;
                actionControl.PlayAction(actionControl.Character.WalkEnd);
                return;
            }
            return;
        }
        if (actionControl.currentAction == actionControl.Character.Run)
        {
            if (!IsHoldingMove)
            {
                isWalking = false;
                isStopping = true;
                stopMoveLockTime = playerSO.LockDuration;
                actionControl.PlayAction(actionControl.Character.RunEnd);
                return;
            }
            return;
        }
        if (IsHoldingMove && actionControl.canInterrupt && actionControl.currentAction != actionControl.Character.Run)
        {
            StopCurrentAction();
            isStopping = false;
            isWalking = true;
            actionControl.AttackLevel = 0;
            actionControl.PlayAction(actionControl.Character.WalkStart);
            return;
        }
        if (!Is_Action_Playing && !isStopping)
        {
            if (actionControl.currentAction != actionControl.Character.Idle && actionControl.currentAction != actionControl.Character.AfkIdle && actionControl.currentAction != actionControl.Character.Walk && actionControl.currentAction != actionControl.Character.Run)
            {
                actionControl.PlayAction(actionControl.Character.Idle);
            }
        }
    }
    public void OnMove(InputValue value)
    {
        try
        {
            if (Panel_Mgr.instance.IsPanelOpen)
            {
                return;
            }
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
    public void StopMove()
    {
        InputMove = Vector3.zero;
        isWalking = false;
        isStopping = true;
    }
    #endregion
    #region 闪避
    public void OnDodge(InputValue value)
    {
        if (value.isPressed)
        {
            if (InputMove.magnitude > 0.1f)
            {
                StopCurrentAction();
                Is_Action_Playing = true;
                actionControl.PlayAction(actionControl.Character.RunDodge);
            }
            else
            {
                StopCurrentAction();
                Is_Action_Playing = true;
                actionControl.PlayAction(actionControl.Character.Dodge);
            }
        }
    }
    public void TurnRun()
    {
        if (actionControl.currentAction == actionControl.Character.RunDodge && InputMove.magnitude > 0.1f)
        {
            StopCurrentAction();
            actionControl.PlayAction(actionControl.Character.Run);
            Debug.Log("变为疾跑");
        }
    }
    #endregion
    #region 背包
    public void OnBackPack(InputValue value)
    {
        if (value.isPressed && !Panel_Mgr.instance.IsPanelVisible(Panel_Mgr.instance.TraderPanel) && !Panel_Mgr.instance.IsPanelVisible(Panel_Mgr.instance.CraftPanel))
        {
            if (!Panel_Mgr.instance.IsPanelVisible(Panel_Mgr.instance.BagPanel))
            {
                TimeMgr.instance.PauseGame();
                Cursor.lockState = CursorLockMode.None;
                Panel_Mgr.instance.OpenPanel(Panel_Mgr.instance.BagPanel);
                Introduction_Mrg.instance.gameObject.SetActive(false);
                bag.Load_Data("Bag_Data");
                bag.ReClean_Bag_Display();
                bag.Refresh_Bag_Display();
            }
            else
            {
                TimeMgr.instance.UnPauseGame();
                Cursor.lockState = CursorLockMode.Locked;
                bag.Save_Bag("Bag_Data");
                bag.ReClean_Bag_Display();
                Panel_Mgr.instance.HideAllPanel();
            }
        }
    }
    public void OnDrop_Item(InputValue value)
    {
        if (value.isPressed && bag.IsDragging)
        {
            bag.currentDraggingItem.Throw_Item();
            bag.ReClean_Bag_Display();
            bag.Refresh_Bag_Display();
        }
    }
    public void Set_StorePanel(bool Is_Ready)
    {
        Panel_Mgr.instance.TraderPanel.gameObject.SetActive(Is_Ready);
    }
    #endregion
    #region 制作
    public void OnCraft(InputValue value)
    {
        if (value.isPressed && !Panel_Mgr.instance.IsPanelVisible(Panel_Mgr.instance.TraderPanel) && !Panel_Mgr.instance.IsPanelVisible(Panel_Mgr.instance.BagPanel))
        {
            if (!Panel_Mgr.instance.IsPanelVisible(Panel_Mgr.instance.CraftPanel))
            {
                TimeMgr.instance.PauseGame();
                Cursor.lockState = CursorLockMode.None;
                Panel_Mgr.instance.OpenPanel(Panel_Mgr.instance.CraftPanel);
                Game_Event.instance.Init_Crafting();
            }
            else
            {
                TimeMgr.instance.UnPauseGame();
                Cursor.lockState = CursorLockMode.Locked;
                Panel_Mgr.instance.HideAllPanel();
            }
        }
    }
    #endregion
    #region 滑铲
    public void OnSlide(InputValue value)
    {
        if (Panel_Mgr.instance.IsPanelOpen)
        {
            return;
        }
        if (value.isPressed)
        {
            if (Is_Action_Playing)
            {
                return;
            }
            //InputMove = Vector3.zero;
            isWalking = false;
            StopCurrentAction();
            Is_Action_Playing = true;
            actionControl.PlayAction(actionControl.Character.Slide);
        }
    }
    #endregion
    #region 交易
    public void OnTrade(InputValue value)
    {
        if (value.isPressed && Can_Trade && !Panel_Mgr.instance.IsPanelVisible(Panel_Mgr.instance.BagPanel))
        {
            if (!Panel_Mgr.instance.IsPanelVisible(Panel_Mgr.instance.TraderPanel))
            {
                TimeMgr.instance.PauseGame();
                Cursor.lockState = CursorLockMode.None;
                Panel_Mgr.instance.OpenTraderBuyPanel();

                Game_Event.instance.Refresh_Buy_List();
                Game_Event.instance.Refresh_Sell_List();
                Game_Event.instance.Init_Store_Panel(true);
            }
            else
            {
                TimeMgr.instance.UnPauseGame();
                Cursor.lockState = CursorLockMode.Locked;
                Panel_Mgr.instance.HideAllPanel();
            }
        }
    }
    #endregion
    #region 交谈
    public void OnChat(InputValue value)
    {
        if (value.isPressed && Can_Chat && !Panel_Mgr.instance.IsPanelVisible(Panel_Mgr.instance.BagPanel))
        {
            DialogueWriter dialogueWriter = Panel_Mgr.instance.DialoguePanel.GetComponent<DialogueWriter>();
            if (!Panel_Mgr.instance.IsPanelVisible(Panel_Mgr.instance.DialoguePanel))
            {
                Cursor.lockState = CursorLockMode.None;
                Panel_Mgr.instance.OpenPanel(Panel_Mgr.instance.DialoguePanel);
                dialogueWriter.CurDialogue = Interact_Trigger.interactableChatNPCS[0].Cur_Dialogue;
                dialogueWriter.WriteDialogue();
                Panel_Mgr.instance.Control_InteractPanel(false, Panel_Mgr.instance.InteractChatPanel);
                Panel_Mgr.instance.Control_InteractPanel(false, Panel_Mgr.instance.TraderPanel);
            }
            else
            {
                if (dialogueWriter.IsTyping || dialogueWriter.CurDialogue.ContinueWay != WayToNextDialogue.NoNext)
                {
                    return;
                }
                Cursor.lockState = CursorLockMode.Locked;
                Panel_Mgr.instance.HideAllPanel();
                Interact_Trigger.ResetButton();
                if (dialogueWriter.typ != null)
                {
                    StopCoroutine(dialogueWriter.typ);
                }
            }
        }
    }
    #endregion
    #region 跳跃与翻越
    public void OnJump(InputValue value)
    {
        if (Panel_Mgr.instance.DialoguePanel.gameObject.activeSelf)
        {
            DialogueWriter writer = Panel_Mgr.instance.DialoguePanel.GetComponent<DialogueWriter>();
            if (writer.IsTyping)
            {
                return;
            }
            else
            {
                if (writer.CurDialogue.ContinueWay != WayToNextDialogue.Choice)
                {
                    Game_Event.instance.NextDialogue();
                }
            }
            return;
        }
        if (Panel_Mgr.instance.IsPanelOpen)
        {
            return;
        }
        //InputMove = Vector3.zero;
        int jumpResult = JumpScan();
        if (jumpResult > 1)
        {
            return;
        }
        if (value.isPressed)
        {
            if (Is_Action_Playing)
            {
                return;
            }
            isWalking = false;
            StopCurrentAction();
            if (jumpResult == 1)
            {
                actionControl.PlayAction(actionControl.Character.PreVault);
                Is_Action_Playing = true;
                Debug.Log("翻越");
            }
            else
            {
                actionControl.PlayAction(actionControl.Character.Jump);
                Is_Action_Playing = true;
                Debug.Log("跳跃");
            }
        }
    }

    public int JumpScan()
    {
        float CantJump = playerSO.HighClimbHeight;
        float vaultHeight = playerSO.VaultHeight;
        float scanRadius = playerSO.JumpScanRadius;

        float halfAngle = scanAngle / 2f;
        float angleStep = scanAngle / (fanRayCount - 1);
        if (SectorRaycast(vaultHeight, playerSO.HighClimbHeight, scanRadius, halfAngle, angleStep))
        {
            return 2;
        }

        if (SectorRaycast(0.05f, vaultHeight, scanRadius, halfAngle, angleStep))
        {
            return 1;
        }
        return 0;
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
                    if (hit.collider.gameObject == gameObject) continue;

                    float normalDot = Vector3.Dot(hit.normal, Vector3.up);
                    if (Mathf.Abs(normalDot) > 0.01f) continue;

                    Bounds b = hit.collider.bounds;
                    float obstacleHeight = b.max.y - hit.point.y;
                    if (obstacleHeight < 0.4f) continue;

                    vaultObstacleTopPoint = new Vector3(b.center.x, b.max.y, b.center.z);
                    vaultObstacleBottomPoint = new Vector3(b.center.x, b.min.y, b.center.z);
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
                vaultFinishedFlag = true;
                return;
            }

            Vector3 dir = vaultEndPos - transform.position;
            float currentDist = dir.magnitude;

            if (currentDist < 0.001f)
            {
                isDoingVaultMove = false;
                rb.isKinematic = false;
                vaultFinishedFlag = true;
                return;
            }

            dir.Normalize();
            float step = (currentDist / remainTime) * Time.deltaTime;
            transform.position += dir * step;
        }
        else
        {
            if (rb.isKinematic)
                rb.isKinematic = false;
        }
    }
    public void Vault_Aft()
    {
        StopCurrentAction();
        Is_Action_Playing = true;
        actionControl.PlayAction(actionControl.Character.AftVault);
        Debug.Log("后续翻越");
    }
    public void Climb_Scan() { }
    #endregion
    #region 特殊技
    public void OnSpecialSkill(InputValue value)
    {
        if (Panel_Mgr.instance.IsPanelOpen)
        {
            return;
        }
        if (!actionControl.canCombo && !actionControl.canInterrupt)
        {
            return;
        }
        if (value.isPressed)
        {
            actionControl.AttackLevel = 0;
            AttackDectetcion();
            StopCurrentAction();
            Is_Action_Playing = true;
            if (actionControl.canCombo)
            {
                ActionSO action = MaxPower == Skill_PowerPool ? actionControl.Character.RelatedFullE : actionControl.Character.RelatedUnfilledE;
                Debug.Log(action.actionName);
                actionControl.PlayAction(action);
            }
            else
            {
                ActionSO action = MaxPower == Skill_PowerPool ? actionControl.Character.FullE : actionControl.Character.UnfilledE;
                Debug.Log(action.actionName);
                actionControl.PlayAction(action);
            }
            actionControl.canCombo = false;
        }
    }
    #endregion
    #region 攻击
    public void OnAttack(InputValue value)
    {
        if (Panel_Mgr.instance.IsPanelOpen)
        {
            return;
        }
        if (actionControl.canCombo)
        {
            Is_Action_Playing = false;
        }
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
        IsBlock = !IsBlock;
        InputMove = IsBlock ? Vector3.zero : InputMove;
        rb.velocity = IsBlock ? Vector3.zero : rb.velocity;
    }
    public void AttackDectetcion()
    {
        Vector3 LookDir;
        float mindistance;
        int index = -1;
        //LayerMask enemyLayer = LayerMask.GetMask("Enemy");
        int mask = ~(1 << LayerMask.NameToLayer("Enemy"));
        Collider[] enemies = Physics.OverlapSphere(transform.position, playerSO.DetectionRadius, EnemyLayer);
        if (enemies.Length > 0)
        {
            //Transform cam = Camera.main.transform;
            //Vector3 camForward = cam.forward;
            //camForward.y = 0;
            //camForward.Normalize();
            mindistance = Mathf.Infinity;
            for (int i = 0; i < enemies.Length; i++)
            {
                bool BlockByObstacle = Physics.Linecast(transform.position, enemies[i].transform.position, mask);
                //bool AngleTooLarge = Vector3.Angle(camForward, enemies[i].transform.position - transform.position) > 90;
                if (BlockByObstacle/* || AngleTooLarge*/)
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
    #endregion
    #region 受伤
    public void GetHurt(float damage,Vector3 dir)
    {
        if (IsDead)
        {
            return;
        }
        StopCurrentAction();
        Is_Action_Playing = true;
        actionControl.PlayAction(actionControl.Character.GetHit);
        StartCoroutine(GetFly(dir));
        damageReceiver.TakeDamage(damage, dir);
    }
    public IEnumerator GetFly(Vector3 dir)
    {
        Vector3 start = transform.position;
        Vector3 end = start + dir * damageReceiver.knockForce;
        float t = 0;
        bool hitwall = false;
        while (t < 1)
        {
            RaycastHit[] hits = Physics.RaycastAll(rb.position - dir * 0.4f, dir, 0.8f);
            foreach (var hit in hits)
            {
                GameObject hitObj = hit.collider.gameObject;
                if (hitObj == gameObject || hitObj.CompareTag("Player") || hitObj.CompareTag("Enemy"))
                {
                    continue;
                }
                hitwall = true;
                break;
            }
            if (hitwall)
            {
                break;
            }
            t += damageReceiver.SmoothLerp + Time.fixedDeltaTime;
            Vector3 pos = Vector3.Lerp(start, end, t);
            rb.MovePosition(pos);
            yield return new WaitForFixedUpdate();
        }
        if (!hitwall)
        {
            rb.MovePosition(end);
        }
    }
    #endregion
    #region 死亡
    public void TurnDeath()
    {
        StopCurrentAction();
        Is_Action_Playing = true;
        actionControl.PlayAction(actionControl.Character.Death);
    }
    public void Dead()
    {
        DeathMgr.instance.DearhFade();
    }
    public void BornSet()
    {
        gameObject.tag = "Player";
        rb.position = playerSO.SpawnPoint;
        rb.rotation = playerSO.SpwanRotation;
        IsDead = false;
        Skill_PowerPool = 0;
        damageReceiver.currentHp = playerSO.PlayerMaxHP;
    }
    public void BornAction()
    {
        if (playerSO.EnableBornAnim)
        {
            CameraPivot.instance.PlayRevolveAroundPlayerAnim();
            StopCurrentAction();
            Is_Action_Playing = true;
            actionControl.PlayAction(actionControl.Character.Born);
        }
        else
        {
            StopCurrentAction();
            Is_Action_Playing = true;
            actionControl.PlayAction(actionControl.Character.AfkIdle);
        }
    }
    #endregion
    #region 相机
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
    #endregion
    #region 辅助调试显示
    //private void OnDrawGizmos()
    //{
    //    float vaultHeight = playerSO.VaultHeight;
    //    float scanRadius = playerSO.JumpScanRadius;

    //    float halfAngle = scanAngle / 2f;
    //    float angleStep = scanAngle / (fanRayCount - 1);
    //    int verticalRays = 5;

    //    DrawSectorGizmo(0.1f, vaultHeight, scanRadius, halfAngle, angleStep, verticalRays, Color.green);
    //    DrawSectorGizmo(vaultHeight, playerSO.HighClimbHeight, scanRadius, halfAngle, angleStep, verticalRays, Color.blue);
    //}

    //private void DrawSectorGizmo(float minY, float maxY, float radius, float halfAngle, float angleStep, int verticalRays, Color color)
    //{
    //    float yStep = (maxY - minY) / (verticalRays - 1);
    //    Gizmos.color = color;

    //    for (int h = 0; h < fanRayCount; h++)
    //    {
    //        float currentAngle = -halfAngle + angleStep * h;
    //        Vector3 dir = Quaternion.Euler(0, currentAngle, 0) * transform.forward;
    //        for (int v = 0; v < verticalRays; v++)
    //        {
    //            float yPos = rayOriginOffset + minY + yStep * v;
    //            Vector3 origin = transform.position + Vector3.up * yPos;
    //            Gizmos.DrawLine(origin, origin + dir * radius);
    //        }
    //    }
    //}
    #endregion
}