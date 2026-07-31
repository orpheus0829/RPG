using DG.Tweening.Plugins.Core.PathCore;
using MMD4MecanimInternal;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.TextCore.Text;
using UnityEngine.Timeline;
[System.Serializable]
public class EquipWeaponData
{
    public int HeadData;
    public int ChestData;
    public int HandData;
    public int FootData;
    public int OnHandData;
}
[System.Serializable]
public class RoleList
{
    public string RoleID;
    public GameObject RoleObj;
    public ActionControl RoleAC;
}
public class Player : MonoBehaviour
{
    public static Player instance { get; private set; }
    public PlayerSO playerSO;
    public PlayerInput playerInput;
    public bool Is_Action_Playing;
    public float AFKTime;
    public float CurAFKCount;
    public bool HaveBornAnim;
    [Header("装备配置")]
    public EquipWeaponData EquipData;
    private string EquipDataPath = "ArmData";
    [Header("装备提升属性")]
    public float SpeedFac;
    public float DamageFac;
    public float MaxhpFac;
    public float DefenseFac;
    public float SpecialFac;
    public float EndFac;
    [Header("角色切换")]
    public List<RoleList> allrole;
    public ActionControl CurAC;
    public int CurRoleIndex;
    public Dictionary<string, (float hp, float skillpool, float charge)> RoleStatusCache = new Dictionary<string, (float hp, float skillpool, float charge)>();
    public bool IsSwitchingRole = false;
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
    public GameObject AtkTo;
    public float AttackStopDistance = 1.2f;
    public bool IsAttacking;
    public LayerMask EnemyLayer;
    public InputAction AtkAction;
    public float HoldJudgeTime = 0.3f;
    public Coroutine HoldATK;
    public bool IsHoldAtk;

    [Header("死亡")]
    public bool IsDead;
    public float DeadTime;

    [Header("闪避")]
    public bool IsDodging;
    public float DownSpeed;
    public float UpSpeed;
    public float BulletScale;
    public float BulletDuration;
    public float DodgeAlpha = 0.3f;

    [Header("特殊技")]
    public float Skill_PowerPool = 0;
    public float PowerFactor;
    public float TriggerPower = 100;
    public float MaxPower = 500;

    [Header("终结技")]
    public bool IsInvincible;
    public float ChargeFactor;
    public float Charge;
    public float MaxCharge;

    [Header("相机")]
    public Vector3 moveDir;
    public float HitShakePower;
    public float HitShakeDuration;
    public float HitShakeFade;

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
    public BuffReceiver buffReceiver;
    public QuickEquip equip;
    public AudioSource au;
    public Mouse mouse;
    public List<SkinnedMeshRenderer> AllRenderers = new List<SkinnedMeshRenderer>();
    public List<Material> MatCopies = new List<Material>();
    private const string volumeSaveKey = "GameSoundVolume";
    public void Awake()
    {
        if (!instance)
        {
            instance = this as Player;
        }
        else
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(gameObject);
        rb = GetComponent<Rigidbody>();
        am = GetComponentInChildren<Animator>();
        col = GetComponent<CapsuleCollider>();
        playerInput = GetComponent<PlayerInput>();
        Speed = playerSO.WalkSpeed;
        bag = GetComponent<Player_Bag>();
        Interact_Trigger = GetComponentInChildren<Interact_Trigger>();
        damageReceiver = GetComponent<DamageReceiver>();
        buffReceiver = GetComponent<BuffReceiver>();
        DeadTime = playerSO.Deadline;
        AFKTime = playerSO.AFKInterval;
        au = GetComponent<AudioSource>();
        mouse = Mouse.current;
        rb.useGravity = true;
        if (PlayerPrefs.GetInt("Money", 0) <= 0)
        {
            PlayerPrefs.SetInt("Money", Start_Money);
        }
        PlayerPrefs.Save();

        moveaction = playerInput.actions["Move"];
        GetComponentsInChildren<SkinnedMeshRenderer>(true, AllRenderers);
        foreach (Renderer rd in AllRenderers)
        {
            foreach (Material mat in rd.materials)
            {
                Material instMat = new Material(mat);
                MatCopies.Add(instMat);
            }
            int matCount = rd.materials.Length;
            rd.materials = MatCopies.GetRange(MatCopies.Count - matCount, matCount).ToArray();
        }
        InitRole();
        EquipData = LoadWeaponData();
        Game_Event.instance.BroadcastRefreshAllArmEquip(EquipData, bag.allData_Item);
        RefrshArmAttribute();
    }
    public void OnEnable()
    {
        AtkAction = playerInput.actions["Attack"];
        AtkAction.started += AtkDown;
        AtkAction.canceled += AtkUp;
        Game_Event.instance.Init_Store += Set_StorePanel;
        Game_Event.instance.DeathState += BornSet;
        Game_Event.instance.DeathSecState += BornAction;
        Game_Event.instance.SetDodgeAlpha += SetDodgeA;
        Game_Event.instance.SetNormalAlpha += ResetA;
        Game_Event.instance.SendArmToPlayer += GetWeaponData;
        CameraPivot.instance.target = this.gameObject.transform;
    }
    public void OnDisable()
    {
        AtkAction.started -= AtkDown;
        AtkAction.canceled -= AtkUp;
        Game_Event.instance.Init_Store -= Set_StorePanel;
        Game_Event.instance.DeathState -= BornSet;
        Game_Event.instance.DeathSecState -= BornAction;
        Game_Event.instance.SetDodgeAlpha -= SetDodgeA;
        Game_Event.instance.SetNormalAlpha -= ResetA;
        Game_Event.instance.SendArmToPlayer -= GetWeaponData;
    }

    public void Start()
    {


        NavPathMgr.instance.OpenNavPath(new Vector3(2, 2, 2));
        if (MenuSetting.instance)
        {
            this.HaveBornAnim = MenuSetting.instance.HaveBornAnim;
        }
        if (CurAC.timelineDirector != null)
        {
            CurAC.timelineDirector.stopped += (director) =>
            {
                Is_Action_Playing = false;
                rb.velocity = Vector3.zero;
                rb.isKinematic = false;
            };
        }
        equip = Panel_Mgr.instance.PlayUiPanel.gameObject.GetComponentInChildren<QuickEquip>();

        //SetModelAlpha(1f);
        NavPathMgr.instance.player = this.transform;
        NavPathMgr.instance.CloseNavPath();

        GameObject quick = Panel_Mgr.instance.PlayUiPanel.gameObject.GetComponentInChildren<QuickEquip>().transform.parent.gameObject;
        quick.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = GetActionKey("Drop_Item");
        RefrshArmAttribute();
        Game_Event.instance.BroadcastRefreshAllArmEquip(EquipData, bag.allData_Item);
    }
    public void InitRole()
    {
        allrole.Clear();
        Transform[] childTrans = GetComponentsInChildren<Transform>(false);
        foreach (Transform t in childTrans)
        {
            if (t == this.transform)
            {
                continue;
            }
            ActionControl ac = t.GetComponent<ActionControl>();
            if (ac != null)
            {
                RoleList newRole = new RoleList();
                newRole.RoleID = t.name;
                newRole.RoleObj = t.gameObject;
                newRole.RoleAC = ac;
                allrole.Add(newRole);
                t.gameObject.SetActive(false);
                if (!RoleStatusCache.ContainsKey(newRole.RoleID))
                {
                    RoleStatusCache[newRole.RoleID] = (playerSO.PlayerMaxHP, 0f, 0f);
                }
            }
        }

        if (allrole.Count > 0)
        {
            CurRoleIndex = 0;
            RoleList firstPack = allrole[0];
            CurAC = firstPack.RoleAC;
            firstPack.RoleObj.SetActive(true);
            damageReceiver.currentHp = playerSO.PlayerMaxHP;
            MaxPower = playerSO.MaxPower;
            MaxCharge = playerSO.MaxCharge;
            Speed = playerSO.WalkSpeed;
        }
    }
    public void Update()
    {
        LayerMask avoid = LayerMask.NameToLayer("EquipStage");
        LayerMask normal = LayerMask.NameToLayer("Player");
        bool OnTrade = Panel_Mgr.instance.IsPanelVisible(Panel_Mgr.instance.TraderPanel);
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        foreach (var t in allChildren)
        {
            t.gameObject.layer = OnTrade ? avoid : normal;
        }
        //Charge = MaxCharge;
        //Skill_PowerPool = MaxPower;
        rb.AddForce(Vector3.down * 10f, ForceMode.Force);
        if (IsSwitchingRole)
        {
            return;
        }
        if (CurAC.currentAction != CurAC.Character.EndSkill)
        {
            IsInvincible = false;
        }
        moveDir = moveaction.ReadValue<Vector3>();
        IsHoldingMove = moveDir.sqrMagnitude > 0.0001f;
        IsDodging = CurAC.currentAction == CurAC.Character.Dodge || CurAC.currentAction == CurAC.Character.RunDodge;
        if (CurAC.currentAction == CurAC.Character.Walk)
        {
            Speed = playerSO.WalkSpeed;
        }
        else if (CurAC.currentAction == CurAC.Character.Run)
        {
            Speed = playerSO.RunSpeed;
        }
        Update_Vault();
        if (Panel_Mgr.instance.IsPanelOpen)
        {
            InputMove = Vector3.zero;
        }
        //if (CurAC.canCombo && CurAC.currentAction != CurAC.Character.RushAttack)
        //{
        //    AttackDectetcion();
        //}
        if (!Is_Action_Playing && !IsHoldingMove)
        {
            if (CurAC.currentAction == CurAC.Character.WalkStart || CurAC.currentAction == CurAC.Character.Walk)
            {
                isWalking = false;
                isStopping = true;
                stopMoveLockTime = playerSO.LockDuration;
                StopCurrentAction();
                CurAC.PlayAction(CurAC.Character.WalkEnd);
            }
            else if (CurAC.currentAction == CurAC.Character.Run)
            {
                isWalking = false;
                isStopping = true;
                stopMoveLockTime = playerSO.LockDuration;
                StopCurrentAction();
                CurAC.PlayAction(CurAC.Character.RunEnd);
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
        if (!Panel_Mgr.instance.IsPanelOpen)
        {
            Move_Follow_Camera();
        }
        if (CurAC.currentAction != CurAC.Character.Idle)
        {
            CurAFKCount = 0;
        }
        if(CurAC.currentAction == CurAC.Character.Idle)
        {
            if (CurAFKCount < AFKTime)
            {
                CurAFKCount += Time.fixedDeltaTime;
            }
            else
            {
                Is_Action_Playing = true;
                StopCurrentAction();
                CurAC.PlayAction(CurAC.Character.AfkIdle);
            }
        }
        if (stopMoveLockTime > 0)
        {
            stopMoveLockTime -= Time.fixedDeltaTime;
            if ((CurAC.currentAction == CurAC.Character.WalkEnd || CurAC.currentAction == CurAC.Character.RunEnd) && isWalking)
            {
                CurAC.PlayAction(CurAC.Character.WalkStart);
            }
        }
        else
        {
            isStopping = false;
        }
        Back_To_Move();

        if (!Is_Action_Playing )
        {
            if (Panel_Mgr.instance.IsPanelOpen || IsDead || IsSwitchingRole)
            {
                InputMove = Vector3.zero;
                return;
            }
            float VerticalVelocity = rb.velocity.y;
            Vector3 HorizontalVelocity = moveDir * Speed * buffReceiver.MoveFactor * (1 + 0.01f * SpeedFac);
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
        if (CurAC.timelineDirector != null)
        {
            CurAC.timelineDirector.Stop();
        }
        //CurAC.ActiveSkillIndex = 0;
        //CurAC.ActiveSkillPool = null;
    }
    #region 移动
    public void Back_To_Move()
    {
        if (CurAC.currentAction == CurAC.Character.RunDodge || IsSwitchingRole)
        {
            return;
        }
        if (vaultFinishedFlag)
        {
            vaultFinishedFlag = false;
            if (IsHoldingMove)
            {
                StopCurrentAction();
                isStopping = false;
                isWalking = true;
                CurAC.AttackLevel = 0;
                CurAC.PlayAction(CurAC.Character.WalkStart);
                return;
            }
            else
            {
                StopCurrentAction();
                isWalking = false;
                isStopping = false;
                CurAC.PlayAction(CurAC.Character.Idle);
                return;
            }
        }
        if (isStopping && !IsHoldingMove)
        {
            return;
        }
        if (CurAC.currentAction == CurAC.Character.WalkStart || CurAC.currentAction == CurAC.Character.Walk)
        {
            if (!IsHoldingMove)
            {
                isWalking = false;
                isStopping = true;
                stopMoveLockTime = playerSO.LockDuration;
                CurAC.PlayAction(CurAC.Character.WalkEnd);
                return;
            }
            return;
        }
        if (CurAC.currentAction == CurAC.Character.Run)
        {
            if (!IsHoldingMove)
            {
                isWalking = false;
                isStopping = true;
                stopMoveLockTime = playerSO.LockDuration;
                CurAC.PlayAction(CurAC.Character.RunEnd);
                Debug.Log("2");
                return;
            }
            return;
        }
        if (IsHoldingMove && CurAC.canInterrupt && CurAC.currentAction != CurAC.Character.Run)
        {
            StopCurrentAction();
            isStopping = false;
            isWalking = true;
            CurAC.AttackLevel = 0;
            CurAC.PlayAction(CurAC.Character.WalkStart);
            return;
        }
        if (!Is_Action_Playing && !isStopping)
        {
            if (CurAC.currentAction != CurAC.Character.Idle && CurAC.currentAction != CurAC.Character.AfkIdle && CurAC.currentAction != CurAC.Character.Walk && CurAC.currentAction != CurAC.Character.Run)
            {
                CurAC.PlayAction(CurAC.Character.Idle);
            }
        }
    }
    public void OnMove(InputValue value)
    {
        try
        {
            if (Panel_Mgr.instance.IsPanelOpen || IsDead || IsSwitchingRole)
            {
                return;
            }
            if (IsBlock)
            {
                IsBlock = false;
                return;
            }
            if (!CurAC.canInterrupt)
            {
                return;
            }
            IsInvincible = false;
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
        if (value.isPressed && !IsDead)
        {
            if (InputMove.magnitude > 0.1f)
            {
                StopCurrentAction();
                Is_Action_Playing = true;
                CurAC.PlayAction(CurAC.Character.RunDodge);
            }
            else
            {
                AttackDectetcion();
                StopCurrentAction();
                Is_Action_Playing = true;
                CurAC.PlayAction(CurAC.Character.Dodge);
            }
        }
    }
    public void SetModelAlpha(float alpha)
    {
        //Debug.Log($"变色{alpha}");
        float a = Mathf.Clamp01(alpha);
        foreach (var mat in MatCopies)
        {
            Color c = mat.color;
            c.a = a;
            mat.color = c;
        }
    }
    public void SetDodgeA()
    {
        SetModelAlpha(DodgeAlpha);
    }
    public void ResetA()
    {
        SetModelAlpha(1);
    }
    public void TurnRun()
    {
        if (CurAC.currentAction == CurAC.Character.RunDodge && InputMove.magnitude > 0.1f)
        {
            if (IsHoldingMove)
            {
                StopCurrentAction();
                CurAC.PlayAction(CurAC.Character.Run);
                //Debug.Log("变为疾跑");
            }
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
                Cursor.lockState = CursorLockMode.None;
                Panel_Mgr.instance.OpenPanel(Panel_Mgr.instance.BagPanel);
                Introduction_Mrg.instance.gameObject.SetActive(false);
                bag.Load_Data("Bag_Data");
                bag.ReClean_Bag_Display();
                bag.Refresh_Bag_Display();
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                bag.Save_Bag("Bag_Data");
                //bag.ReClean_Bag_Display();
                Panel_Mgr.instance.HideAllPanel();
            }
        }
    }
    public void Set_StorePanel(bool Is_Ready)
    {
        Panel_Mgr.instance.TraderPanel.gameObject.SetActive(Is_Ready);
    }
    #endregion
    #region 使用快捷道具/丢弃背包物品
    public void OnDrop_Item(InputValue value)
    {
        if (IsDead)
        {
            return;
        }
        if (value.isPressed)
        {
            if (bag.IsDragging && Panel_Mgr.instance.IsPanelOpen)
            {
                bag.currentDraggingItem.Throw_Item();
                bag.ReClean_Bag_Display();
                bag.Refresh_Bag_Display();
            }
            else if (!Panel_Mgr.instance.IsPanelOpen)
            {
                if (!equip.Tool)
                {
                    return;
                }
                Debug.Log("使用道具");
                PickNoticeMgr.instance.ShowFieldTip($"使用{equip.Tool.item_name}");
                buffReceiver.ReceiveBuff(equip.Tool.buff);
                bag.RemoveItemInData(equip.Tool,1);
                bag.RefrshArms();
            }
        }
    }
    #endregion
    #region 合成
    public void OnCraft(InputValue value)
    {
        if (value.isPressed && !Panel_Mgr.instance.IsPanelOpen && !IsDead)
        {
            if (!Panel_Mgr.instance.IsPanelVisible(Panel_Mgr.instance.CraftPanel))
            {
                Cursor.lockState = CursorLockMode.None;
                Panel_Mgr.instance.OpenPanel(Panel_Mgr.instance.CraftPanel);
                Crafting_UI crafting = Panel_Mgr.instance.CraftPanel.GetComponentInChildren<Crafting_UI>();
                crafting.ResetCraftCam();
                TimeMgr.instance.CreateTimer(TimeMgr.TimerMode.RealTimeUnscaled, 0f, 0.4f, null, () =>
                {
                    PickNoticeMgr.instance.ShowDialogueTip(allrole[CurRoleIndex].RoleID, "来做点小手工吧", 2f);
                });
                //Game_Event.instance.Init_Crafting();
            }
            else
            {
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
            CurAC.PlayAction(CurAC.Character.Slide);
        }
    }
    #endregion
    #region 交易
    public void OnTrade(InputValue value)
    {
        if (value.isPressed && !Panel_Mgr.instance.IsPanelOpen && !IsDead)
        {
            if (!Game_Event.instance.Current_Trader && !Can_Trade)
            {
                if (!Panel_Mgr.instance.IsPanelVisible(Panel_Mgr.instance.OnlinePanel))
                {
                    Cursor.lockState = CursorLockMode.None;
                    Panel_Mgr.instance.OpenPanel(Panel_Mgr.instance.OnlinePanel);
                    TimeMgr.instance.CreateTimer(TimeMgr.TimerMode.RealTimeUnscaled, 0f, 0.4f, null, () =>
                    {
                        PickNoticeMgr.instance.ShowDialogueTip(allrole[CurRoleIndex].RoleID, "有谁在线呢", 2f);
                    });
                }
                else
                {
                    Debug.Log("聊天4");
                    Cursor.lockState = CursorLockMode.Locked;
                    Panel_Mgr.instance.HideAllPanel();
                }
                return;
            }
            if (!Panel_Mgr.instance.IsPanelVisible(Panel_Mgr.instance.TraderPanel))
            {
                Cursor.lockState = CursorLockMode.None;
                //Panel_Mgr.instance.OpenPanel(Panel_Mgr.instance.TraderPanel);
                Panel_Mgr.instance.OpenTraderBuyPanel();
                Game_Event.instance.Refresh_Buy_List();
                Game_Event.instance.Refresh_Sell_List();
                Game_Event.instance.Init_Store_Panel(true);
                PickNoticeMgr.instance.ShowDialogueTip(Game_Event.instance.Current_Trader.name, "有什么需要的吗?", 3f);
                //TimeMgr.instance.CreateTimer(TimeMgr.TimerMode.RealTimeUnscaled, 0, 0.5f, null, () =>
                //{
                //    Panel_Mgr.instance.OpenTraderBuyPanel();
                //    Game_Event.instance.Refresh_Buy_List();
                //    Game_Event.instance.Refresh_Sell_List();
                //    Game_Event.instance.Init_Store_Panel(true);
                //});
                Game_Event.instance.Current_Trader.PlayTraderShow(Game_Event.instance.Current_Trader.Idle);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Panel_Mgr.instance.HideAllPanel();
                Game_Event.instance.Current_Trader.PlayTraderShow(Game_Event.instance.Current_Trader.Normal);
                PickNoticeMgr.instance.ShowDialogueTip(Game_Event.instance.Current_Trader.name, "欢迎下次再来噢", 3f);
                OffCameraFrame();
            }
        }
    }
    #endregion
    #region 对话
    public void OnChat(InputValue value)
    {
        Game_Event.instance.PortalAc(this.transform);
        if (value.isPressed && Can_Chat && !Panel_Mgr.instance.IsPanelVisible(Panel_Mgr.instance.BagPanel))
        {
            DialogueWriter dialogueWriter = Panel_Mgr.instance.DialoguePanel.GetComponent<DialogueWriter>();
            if (!Panel_Mgr.instance.IsPanelVisible(Panel_Mgr.instance.DialoguePanel))
            {
                Cursor.lockState = CursorLockMode.None;
                Panel_Mgr.instance.OpenPanel(Panel_Mgr.instance.DialoguePanel);
                CameraPivot.instance.SaveDialogueCameraState();
                dialogueWriter.CurDialogue = Interact_Trigger.interactableChatNPCS[0].Cur_Dialogue;
                dialogueWriter.Actor = Interact_Trigger.interactableChatNPCS[0];
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
                CameraPivot.instance.isPlayingCameraAnim = false;
                if (dialogueWriter.typ != null)
                {
                    StopCoroutine(dialogueWriter.typ);
                    dialogueWriter.typ = null;
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
    #region 任务栏
    public void OnMission(InputValue value)
    {
        if(value.isPressed && !IsDead)
        {
            GameObject mission = Panel_Mgr.instance.MissionPanel.gameObject;
            StoryRelation storyRelation = mission.GetComponentInChildren<StoryRelation>(includeInactive:true);
            if (!Panel_Mgr.instance.IsPanelVisible(Panel_Mgr.instance.MissionPanel))
            {
                Panel_Mgr.instance.OpenPanel(Panel_Mgr.instance.MissionPanel);
                //storyRelation.CreateStoryDropDown();
            }
            else if (Panel_Mgr.instance.IsPanelOpen)
            {
                //storyRelation.DestroyAllDropDown();
                Panel_Mgr.instance.HideAllPanel();
            }
        }
    }
    #endregion
    #region ESC
    public void OnEsc(InputValue value)
    {
        if (value.isPressed && !IsDead)
        {
            if (!Panel_Mgr.instance.IsPanelVisible(Panel_Mgr.instance.EscPanel))
            {
                Panel_Mgr.instance.OpenPanel(Panel_Mgr.instance.EscPanel);
            }
            else if (Panel_Mgr.instance.IsPanelOpen)
            {
                Panel_Mgr.instance.HideAllPanel();
            }
        }
    }
    #endregion
    #region 跳跃与翻越
    public void OnJump(InputValue value)
    {
        //if (CurAC.currentAction == CurAC.Character.Block || CurAC.currentAction == CurAC.Character.Dodge)
        //{
        //    return;
        //}
        if (Panel_Mgr.instance.MissionPanel.gameObject.activeSelf)
        {
            Debug.Log("导航");
            Vector3 questPos = Story_Mgr.instance.CalculateQuestPos();
            MiniMapMgr.instance.trackingTarget = null;
            NavPathMgr.instance.SwitchNavTarget(questPos);
            NavPathMgr.instance.CloseNavPath();
            NavPathMgr.instance.OpenNavPath(questPos);
            return;
        }
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
        GameObject AtkEnemy = BlockScan();
        if (AtkEnemy)
        {
            AttackDectetcion();
            int nextrole = (CurRoleIndex + 1) % allrole.Count;
            SwitchRolePure(nextrole);
            BaseEnemy e = AtkEnemy.GetComponent<BaseEnemy>();
            StopCurrentAction();
            Is_Action_Playing = true;
            bool CanBlock = CurAC.Character.RoleParry == ParryStyle.Block ? true : false;
            if (CanBlock)
            {
                Vector3 pos = AtkEnemy.transform.position + AtkEnemy.transform.forward * 2.5f;
                rb.MovePosition(pos);
                AttackDectetcion();
                CurAC.PlayAction(CurAC.Character.Block);
                TimeMgr.instance.CreateTimer(TimeMgr.TimerMode.RealTimeUnscaled, 0f, 0.3f, () =>
                {
                    e.am.speed = 0;
                    TimeMgr.instance.SuddenStop();
                    CameraPivot.instance.QuickRealTimeShake(2);
                }, () =>
                {
                    e.am.speed = 1;
                    TimeMgr.instance.SuddenResume();
                    e.BeParried();
                    e.NeedBlock = false;
                });
            }
            else
            {
                CurAC.PlayAction(CurAC.Character.Dodge);
            }
        }
        //InputMove = Vector3.zero;
        int jumpResult = JumpScan();
        if (jumpResult > 1)
        {
            return;
        }
        if (value.isPressed && IsHoldingMove)
        {
            if (Is_Action_Playing)
            {
                return;
            }
            isWalking = false;
            StopCurrentAction();
            if (jumpResult == 1)
            {
                CurAC.PlayAction(CurAC.Character.PreVault);
                Is_Action_Playing = true;
                Debug.Log("翻越");
            }
            else
            {
                CurAC.PlayAction(CurAC.Character.Jump);
                Is_Action_Playing = true;
                Debug.Log("跳跃");
            }
        }
    }
    public GameObject BlockScan()
    {
        Collider[] c = Physics.OverlapSphere(transform.position, playerSO.DetectionRadius, EnemyLayer);
        List<GameObject> lst = new List<GameObject>();
        foreach (var i in c)
        {
            if (i.TryGetComponent(out BaseEnemy e) && e.NeedBlock)
            {
                lst.Add(i.gameObject);
            }
        }
        lst.Sort((a, b) =>
        {
            float distA = Vector3.Distance(transform.position, a.transform.position);
            float distB = Vector3.Distance(transform.position, b.transform.position);
            return distA.CompareTo(distB);
        });
        return lst.Count > 0 ? lst[0] : null;
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
            //if (rb.isKinematic)
            //    rb.isKinematic = false;
        }
    }
    public void Vault_Aft()
    {
        StopCurrentAction();
        Is_Action_Playing = true;
        CurAC.PlayAction(CurAC.Character.AftVault);
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
        if (!CurAC.canCombo && !CurAC.canInterrupt)
        {
            return;
        }
        if (!value.isPressed)
        {
            return;
        }
        CurAC.AttackLevel = 0;
        AttackDectetcion();
        bool isAttackComboRelease = CurAC.canCombo && CurAC.currentAction.actionType == ActionType.Attack && CurAC.currentAction.Related;
        bool powerEnough = Skill_PowerPool >= TriggerPower;
        List<Single_SpecialATK> targetSkillPool;
        if (isAttackComboRelease)
        {
            targetSkillPool = powerEnough ? CurAC.Character.RelatedFullE : CurAC.Character.RelatedUnfilledE;
        }
        else
        {
            targetSkillPool = powerEnough ? CurAC.Character.FullE : CurAC.Character.UnfilledE;
        }
        if (targetSkillPool == null || targetSkillPool.Count == 0)
        {
            Debug.Log("当前条件无可用特殊技池，释放失败");
            return;
        }
        StopCurrentAction();
        Is_Action_Playing = true;
        bool canContinueSpecialCombo = CurAC.ActiveSkillPool == targetSkillPool;
        if (canContinueSpecialCombo)
        {
            CurAC.ActiveSkillIndex = (CurAC.ActiveSkillIndex + 1) % targetSkillPool.Count;
        }
        else
        {
            CurAC.ActiveSkillPool = targetSkillPool;
            CurAC.ActiveSkillIndex = 0;
        }

        Single_SpecialATK selectSkill = targetSkillPool[CurAC.ActiveSkillIndex];
        CurAC.PlayAction(selectSkill.Special);
        Debug.Log($"特殊技连招 第{CurAC.ActiveSkillIndex}段 | {selectSkill.Special.actionName}");
        Skill_PowerPool -= selectSkill.Cost;
        Skill_PowerPool = Mathf.Clamp(Skill_PowerPool, 0, playerSO.MaxPower);
        CurAC.canCombo = false;
    }
    #endregion
    #region 终结技
    public void OnEndSkill(InputValue value)
    {
        if (Charge < playerSO.MaxCharge)
        {
            return;
        }
        if (value.isPressed)
        {
            StopCurrentAction();
            Is_Action_Playing = true;
            AttackDectetcion();
            CurAC.PlayAction(CurAC.Character.EndSkill);
            Charge = 0f;
        }
    }
    public void InvincibleOn()
    {
        IsInvincible = true;
        //gameObject.tag = "DeadPlayer";
    }
    public void InvincibleOff()
    {
        IsInvincible = false;
        //gameObject.tag = "Player";
    }
    #endregion
    #region 攻击
    //public void OnAttack(InputValue value)
    //{
    //    if (Panel_Mgr.instance.IsPanelOpen || Panel_Mgr.instance.IsFullMapOpen || IsDead)
    //    {
    //        return;
    //    }
    //    if (actionControl.canCombo)
    //    {
    //        Is_Action_Playing = false;
    //    }
    //    if (value.isPressed)
    //    {
    //        IsAttacking = true;
    //        StopCurrentAction();
    //        Is_Action_Playing = true;
    //        AttackDectetcion();
    //        actionControl.PlayAttackAction();
    //    }
    //}

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
        Collider[] enemies = Physics.OverlapSphere(transform.position, playerSO.DetectionRadius, EnemyLayer);
        if (enemies.Length > 0)
        {
            mindistance = Mathf.Infinity;
            for (int i = 0; i < enemies.Length; i++)
            {
                Collider col = enemies[i];
                GameObject targetObj = col.gameObject;
                if (targetObj.CompareTag("DeadEnemy"))
                {
                    continue;
                }
                int ignoreMask = ~(1 << LayerMask.NameToLayer("Enemy"));
                bool BlockByObstacle = Physics.Linecast(transform.position, col.transform.position, ignoreMask);
                if (BlockByObstacle)
                {
                    continue;
                }
                float distance = Vector3.Distance(col.transform.position, transform.position);
                if (distance < mindistance)
                {
                    mindistance = distance;
                    index = i;
                    AtkTo = enemies[index].gameObject;
                }
            }
            if (index != -1)
            {
                LookDir = enemies[index].transform.position - transform.position;
                LookDir.y = 0;
                LookDir.Normalize();
                transform.rotation = Quaternion.LookRotation(LookDir);
            }
            else
            {
                AtkTo = null;
            }
        }
        else
        {
            AtkTo = null;
        }
    }
    private void AtkDown(InputAction.CallbackContext context)
    {
        if (Panel_Mgr.instance.IsPanelOpen || Panel_Mgr.instance.IsFullMapOpen || IsDead || CurAC.currentAction==CurAC.Character.RushAttack)
        {
            return;
        }
        IsInvincible = false;
        IsHoldAtk = false;
        HoldATK = StartCoroutine(HoldJudge());
    }
    private void AtkUp(InputAction.CallbackContext context)
    {
        if (CurAC.currentAction == CurAC.Character.Run || CurAC.currentAction == CurAC.Character.Dodge)
        {
            Debug.Log("rush");
            if (HoldATK != null)
            {
                StopCoroutine(HoldATK);
                HoldATK = null;
            }
            //InputMove = Vector3.zero;
            //actionControl.canInterrupt = false;
            TapRushAttack();
            return;
        }
        if (HoldATK != null)
        {
            StopCoroutine(HoldATK);
            HoldATK = null;
        }
        if (!IsHoldAtk)
        {
            TapAttack();
        }
    }
    private IEnumerator HoldJudge()
    {
        yield return new WaitForSeconds(HoldJudgeTime);
        IsHoldAtk = true;
        HoldAttack();
    }
    public void TapAttack()
    {
        if (!CurAC.canInterrupt)
        {
            return;
        }
        if (Panel_Mgr.instance.IsPanelOpen || Panel_Mgr.instance.IsFullMapOpen || IsDead || IsInvincible)
        {
            return;
        }
        IsAttacking = true;
        StopCurrentAction();
        Is_Action_Playing = true;
        AttackDectetcion();
        CurAC.PlayAttackAction(false);
    }
    public void HoldAttack()
    {
        if (Panel_Mgr.instance.IsPanelOpen || Panel_Mgr.instance.IsFullMapOpen || IsDead || IsInvincible)
        {
            return;
        }
        IsAttacking = true;
        StopCurrentAction();
        Is_Action_Playing = true;
        //AttackDectetcion();
        CurAC.PlayAttackAction(true);
    }
    public void TapRushAttack()
    {
        if (IsDead || IsInvincible)
        {
            return;
        }
        StopCurrentAction();
        AttackDectetcion();
        Is_Action_Playing = true;
        CurAC.canCombo = false;
        CurAC.PlayAction(CurAC.Character.RushAttack);
    }
    //public void HoldRushAttack()
    //{
    //    if (IsDead || IsInvincible)
    //    {
    //        return;
    //    }
    //    Debug.Log("h");
    //    StopCurrentAction();
    //    AttackDectetcion();
    //    Is_Action_Playing = true;
    //    CurAC.canCombo = false;
    //    CurAC.PlayAction(CurAC.Character.RushHoldAttack);
    //}
    #endregion
    #region 受伤
    public void GetHurt(float damage,Vector3 dir)
    {
        if (IsDead || IsInvincible || CurAC.currentAction==CurAC.Character.Block)
        {
            return;
        }
        if (!IsDodging)
        {
            InputMove = Vector3.zero;
            StopCurrentAction();
            Is_Action_Playing = true;
            CurAC.PlayAction(HitDir(dir));
            StartCoroutine(GetFly(dir));
        }
        float reducerate = DefenseFac / (DefenseFac + 100f);
        damageReceiver.TakeDamage<Player>(damage * (1 - reducerate), dir);
        if (IsDead)
        {
            TurnDeath();
            return;
        }
    }
    public ActionSO HitDir(Vector3 dir)
    {
        Vector3 PlFwd = transform.forward;
        PlFwd.y = 0;
        PlFwd.Normalize();
        dir.y = 0;
        dir.Normalize();
        float angle = Vector3.Angle(PlFwd, dir);
        if (angle < 90f)
        {
            AttackDectetcion();
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y + 180f, 0);
            return CurAC.Character.GetHitF;
        }
        else
        {
            AttackDectetcion();
            return CurAC.Character.GetHitB;
        }
    }
    public IEnumerator GetFly(Vector3 dir)
    {
        Vector3 start = transform.position;
        Vector3 end = start + dir * damageReceiver.knockForce;
        float t = 0;
        bool hitwall = false;
        while (t < 1)
        {
            if (IsDead)
            {
                yield break;
            }
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
        CurAC.PlayAction(CurAC.Character.Death);
    }
    public void Dead()
    {
        RoleList curRole = allrole[CurRoleIndex];
        RoleStatusCache[curRole.RoleID] = (damageReceiver.currentHp, Skill_PowerPool, Charge);

        bool HaveRoleAlive = false;
        int aliveIndex = -1;
        for (int i = 0; i < allrole.Count; i++)
        {
            string id = allrole[i].RoleID;
            float hp;
            if (RoleStatusCache.TryGetValue(id, out var stat))
            {
                hp = stat.hp;
            }
            else
            {
                hp = playerSO.PlayerMaxHP;
            }

            if (hp > 0)
            {
                HaveRoleAlive = true;
                aliveIndex = i;
                break;
            }
        }
        if (HaveRoleAlive && aliveIndex != -1)
        {
            RoleList oldPack = allrole[CurRoleIndex];
            ClearOldActionControl(oldPack.RoleAC);
            oldPack.RoleObj.SetActive(false);
            CurRoleIndex = aliveIndex;
            RoleList newPack = allrole[CurRoleIndex];
            newPack.RoleObj.SetActive(true);
            CurAC = newPack.RoleAC;
            ClearOldActionControl(CurAC);
            if (RoleStatusCache.TryGetValue(newPack.RoleID, out var cacheData))
            {
                damageReceiver.currentHp = cacheData.hp;
                Skill_PowerPool = cacheData.skillpool;
                Charge = cacheData.charge;
            }
            else
            {
                damageReceiver.currentHp = playerSO.PlayerMaxHP;
                Skill_PowerPool = 0;
                Charge = 0;
            }
            IsDead = false;
            gameObject.tag = "Player";
            DeadTime = playerSO.Deadline;
            InputMove = Vector3.zero;
            CurAC.PlayAction(CurAC.Character.Idle);
            return;
        }
        DeathMgr.instance.DearhFade();
    }
    public void BornSet()
    {
        gameObject.tag = "Player";
        rb.position = playerSO.SpawnPoint;
        rb.rotation = playerSO.SpwanRotation;
        foreach (RoleList role in allrole)
        {
            RoleStatusCache[role.RoleID] = (playerSO.PlayerMaxHP, 0f, 0f);
        }
        damageReceiver.currentHp = playerSO.PlayerMaxHP;
        Skill_PowerPool = 0;
        Charge = 0;
    }
    public void TrulyBorn()
    {
        IsDead = false;
    }
    public void BornAction()
    {
        int randomIndex = Random.Range(0, allrole.Count);
        SwitchRolePure(randomIndex);
        if (HaveBornAnim)
        {
            //CameraPivot.instance.PlayRevolveAroundPlayerAnim();
            StopCurrentAction();
            Is_Action_Playing = true;
            CurAC.PlayAction(CurAC.Character.Born);
        }
        else
        {
            StopCurrentAction();
            Is_Action_Playing = true;
            CurAC.PlayAction(CurAC.Character.AfkIdle);
        }
    }
    public void SwitchRolePure(int index)
    {
        if (index != CurRoleIndex)
        {
            RoleList oldPack = allrole[CurRoleIndex];
            RoleStatusCache[oldPack.RoleID] = (damageReceiver.currentHp, Skill_PowerPool, Charge);

            oldPack.RoleObj.SetActive(false);
            CurRoleIndex = index;
            RoleList newPack = allrole[CurRoleIndex];
            newPack.RoleObj.SetActive(true);
            CurAC = newPack.RoleAC;
            ClearOldActionControl(CurAC);
            if (RoleStatusCache.TryGetValue(newPack.RoleID, out var cacheData))
            {
                damageReceiver.currentHp = cacheData.hp;
                Skill_PowerPool = cacheData.skillpool;
                Charge = cacheData.charge;
            }
            else
            {
                damageReceiver.currentHp = playerSO.PlayerMaxHP;
                Skill_PowerPool = 0;
                Charge = 0;
            }
            Speed = playerSO.WalkSpeed;
            MaxPower = playerSO.MaxPower;
            MaxCharge = playerSO.MaxCharge;
        }
    }
    public void OnCameraFrame()
    {
        CameraPivot.instance.isPlayingCameraAnim = true;
        if (CurAC.currentAction == CurAC.Character.EndSkill)
        {
            AttackDectetcion();
        }
    }
    public void OffCameraFrame()
    {
        CameraPivot.instance.isPlayingCameraAnim = false;
        if (CurAC.currentAction == CurAC.Character.EndSkill)
        {
            AttackDectetcion();
        }
    }
    #endregion
    #region 地图
    public void OnMap(InputValue value)
    {
        if(value.isPressed&& !IsDead)
        {
            Panel_Mgr.instance.SwitchMap(Panel_Mgr.instance.CurMapStyle == MapStyle.Min);
        }
    }
    #endregion
    #region 关闭面板
    public void OnClosePanel(InputValue value)
    {
        if (value.isPressed)
        {
            if (Panel_Mgr.instance.IsPanelVisible(Panel_Mgr.instance.ConfirmPanel) || Panel_Mgr.instance.IsPanelVisible(Panel_Mgr.instance.EscPanel))
            {
                return;
            }
            if (Game_Event.instance.Current_Trader)
            {
                Game_Event.instance.Current_Trader.PlayTraderShow(Game_Event.instance.Current_Trader.Normal);
            }
            if (Game_Event.instance.Current_Trader)
            {
                PickNoticeMgr.instance.ShowDialogueTip(Game_Event.instance.Current_Trader.name, "欢迎下次再来噢", 3f);
            }
            Panel_Mgr.instance.HideAllPanel();
            if (Panel_Mgr.instance.CurMapStyle == MapStyle.Max)
            {
                Panel_Mgr.instance.SwitchMap(false);
            }
            OffCameraFrame();
        }
    }
    #endregion
    #region 切换角色
    public void OnSwitchRole(InputValue value)
    {
        if (value.isPressed && !IsDead && !Panel_Mgr.instance.IsPanelOpen && !IsInvincible)
        {
            if (IsSwitchingRole)
            {
                return;
            }
            //切换角色
            IsSwitchingRole = true;
            StopCurrentAction();
            Is_Action_Playing = true;
            CurAC.PlayAction(CurAC.Character.SwitchOut);
            int nextindex = (CurRoleIndex + 1) % allrole.Count;
            StartCoroutine(SwitchRoleCoroutine(nextindex));
        }
    }
    public IEnumerator SwitchRoleCoroutine(int targetindex)
    {
        yield return new WaitForSeconds(0.3f);
        RoleList oldPack = allrole[CurRoleIndex];
        RoleList newPack = allrole[targetindex];
        if (!RoleStatusCache.ContainsKey(oldPack.RoleID))
        {
            RoleStatusCache[oldPack.RoleID] = (damageReceiver.currentHp, Skill_PowerPool, Charge);
        }
        else
        {
            RoleStatusCache[oldPack.RoleID] = (damageReceiver.currentHp, Skill_PowerPool, Charge);
        }

        playerInput.actions.Disable();
        ClearOldActionControl(oldPack.RoleAC);
        damageReceiver.isStiff = false;
        if (HoldATK != null)
        {
            StopCoroutine(HoldATK);
        }
        oldPack.RoleObj.SetActive(false);
        newPack.RoleObj.SetActive(true);
        CurAC = newPack.RoleAC;
        ClearOldActionControl(CurAC);
        StopCurrentAction();
        Is_Action_Playing = true;
        CurAC.PlayAction(CurAC.Character.SwitchIn);

        SetModelAlpha(0);
        float fadeDuration = 0.6f;
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            SetModelAlpha(alpha);
            yield return null;
        }
        SetModelAlpha(1f);

        //yield return WaitPlayableFinish(CurAC.timelineDirector);
        if (RoleStatusCache.TryGetValue(newPack.RoleID, out var cacheData))
        {
            damageReceiver.currentHp = cacheData.hp;
            Skill_PowerPool = cacheData.skillpool;
            Charge = cacheData.charge;
        }
        else
        {
            damageReceiver.currentHp = playerSO.PlayerMaxHP;
            Skill_PowerPool = 0;
            Charge = 0;
        }
        Speed = playerSO.WalkSpeed;
        MaxPower = playerSO.MaxPower;
        MaxCharge = playerSO.MaxCharge;
        playerInput.actions.Enable();
        Debug.Log("切换完成");
        CurRoleIndex = targetindex;
        IsSwitchingRole = false;
    }
    public void ClearOldActionControl(ActionControl oldAC)
    {
        if (oldAC == null)
        {
            return;
        }
        oldAC.timelineDirector.Stop();
        oldAC.timelineDirector.time = 0;
        oldAC.currentAction = null;
        oldAC.AttackLevel = 0;
        oldAC.canCombo = false;
        oldAC.canInterrupt = true;
        oldAC.ClearHitBoxData();
    }
    private IEnumerator WaitPlayableFinish(PlayableDirector dir)
    {
        yield return new WaitForEndOfFrame();
        while (dir.state == PlayState.Playing)
        {
            yield return null;
        }
    }
    #endregion
    #region 辅助
    public string GetActionKey(string actionName)
    {
        InputAction action = playerInput.actions.FindAction(actionName);
        if (action == null)
        {
            return "未绑定";
        }
        foreach(var i in action.bindings)
        {
            if (i.isComposite || i.isPartOfComposite)
            {
                continue;
            }
            return i.ToDisplayString();
        }
        return "未绑定";
    }
    #endregion
    #region 保存和读取装备配备数据
    public void GetWeaponData(BodyArmEquip bodyArm)
    {
        switch (bodyArm.NowKind)
        {
            case WeaponKind.Head:
                EquipData.HeadData = bodyArm.Data ? bodyArm.Data.item_id : -1;
                break;
            case WeaponKind.Chest:
                EquipData.ChestData = bodyArm.Data ? bodyArm.Data.item_id : -1;
                break;
            case WeaponKind.Hand:
                EquipData.HandData = bodyArm.Data ? bodyArm.Data.item_id : -1;
                break;
            case WeaponKind.Foot:
                EquipData.FootData = bodyArm.Data ? bodyArm.Data.item_id : -1;
                break;
            case WeaponKind.Armament:
                EquipData.OnHandData = bodyArm.Data ? bodyArm.Data.item_id : -1;
                break;
            default:
                break;
        }
        SaveWeaponData();
        RefrshArmAttribute();
    }
    public void SaveWeaponData()
    {
        string fullPath = Application.persistentDataPath + "/" + EquipDataPath + ".json";
        string json = JsonUtility.ToJson(EquipData);
        File.WriteAllText(fullPath, json);
        Debug.Log($"装备存档已保存：{fullPath}");
    }
    public EquipWeaponData LoadWeaponData()
    {
        string fullPath = Application.persistentDataPath + "/" + EquipDataPath + ".json";
        if (!File.Exists(fullPath))
        {
            Debug.LogWarning("装备存档不存在，返回空白装备数据");
            return new EquipWeaponData
            {
                HeadData = -1,
                ChestData = -1,
                HandData = -1,
                FootData = -1,
                OnHandData = -1
            };
        }
        string json = File.ReadAllText(fullPath);
        EquipWeaponData data = JsonUtility.FromJson<EquipWeaponData>(json);
        return data;
    }
    public void ImportWeaponData(EquipWeaponData equip)
    {
        EquipData = equip;
        SaveWeaponData();
    }
    public EquipWeaponData ExportWeaponData()
    {
        return new EquipWeaponData
        {
            HeadData = EquipData.HeadData,
            ChestData = EquipData.ChestData,
            HandData = EquipData.HandData,
            FootData = EquipData.FootData,
            OnHandData = EquipData.OnHandData
        };
    }
    public void RefrshArmAttribute()
    {
        SpeedFac = 0;
        DamageFac = 0;
        MaxhpFac = 0;
        DefenseFac = 0;
        SpecialFac = 0;
        EndFac = 0;
        List<int> equipIdList = new List<int>()
        {
            EquipData.HeadData,
            EquipData.ChestData,
            EquipData.HandData,
            EquipData.FootData,
            EquipData.OnHandData,
        };
        foreach (int itemId in equipIdList)
        {
            if (itemId <= 0)
            {
                continue;
            }
            Item_Data equipItem = bag.allData_Item.Data_List.Find(x => x.item_id == itemId);
            if (equipItem == null)
            {
                continue;
            }
            MaxhpFac += equipItem.MaxHP;
            DefenseFac += equipItem.Defense;
            SpeedFac += equipItem.MoveSpeed;
            DamageFac += equipItem.Attack;
            SpecialFac += equipItem.SpecialGain;
            EndFac += equipItem.EndGain;
            Debug.Log($"【装备属性】移速:{equipItem.MoveSpeed} 攻击:{equipItem.Attack} 生命:{equipItem.MaxHP} 防御:{equipItem.Defense}specil:{equipItem.SpecialGain}end:{equipItem.EndGain}");
        }
    }
    #endregion
    #region 好友聊天
    public void OnLineChat(InputValue value)
    {
        if (!Game_Event.instance.Current_Trader)
        {
            Debug.Log("聊天1");
            if (Panel_Mgr.instance.IsPanelOpen || IsDead)
            {
                return;
            }
            Debug.Log("聊天2");
            if (!Panel_Mgr.instance.IsPanelVisible(Panel_Mgr.instance.OnlinePanel))
            {
                Debug.Log("聊天3");
                Cursor.lockState = CursorLockMode.None;
                Panel_Mgr.instance.OpenPanel(Panel_Mgr.instance.OnlinePanel);
                TimeMgr.instance.CreateTimer(TimeMgr.TimerMode.RealTimeUnscaled, 0f, 0.4f, null, () =>
                {
                    PickNoticeMgr.instance.ShowDialogueTip(allrole[CurRoleIndex].RoleID, "有谁在线呢", 2f);
                });
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Panel_Mgr.instance.HideAllPanel();
            }
            return;
        }
    }
    #endregion
    //#region 退出
    //public void OnEscape(InputValue value)
    //{
    //    if(value.isPressed && !IsDead && Panel_Mgr.instance.IsPanelOpen)
    //    {
    //        Panel_Mgr.instance.HideAllPanel();
    //    }
    //}
    //#endregion
    #region 相机
    public void Move_Follow_Camera()
    {
        //Transform cam = Camera.main.transform;
        //Vector3 camForward = cam.forward;
        //Vector3 camRight = cam.right;

        //camForward.y = 0;
        //camRight.y = 0;
        //camForward.Normalize();
        //camRight.Normalize();

        //moveDir = camForward * InputMove.z + camRight * InputMove.x;

        //if (moveDir.magnitude > 0.1f)
        //{
        //    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(moveDir), 15f * Time.fixedDeltaTime);
        //}
        Transform camPivotTrans = null;
        if (CameraPivot.instance != null)
        {
            camPivotTrans = CameraPivot.instance.transform;
        }
        if (camPivotTrans == null)
        {
            camPivotTrans = Camera.main.transform;
        }

        Vector3 camForward = camPivotTrans.forward;
        Vector3 camRight = camPivotTrans.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();
        if (CameraTimelineBehaviour.IsLockMoveToCharForward == true)
        {
            camForward = transform.forward;
            camRight = transform.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();
        }

        moveDir = camForward * InputMove.z + camRight * InputMove.x;

        if (moveDir.magnitude > 0.1f && CurAC.currentAction != CurAC.Character.RushAttack)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(moveDir), 15f * Time.fixedDeltaTime);
        }
    }
    #endregion
    public void RefreshAudioVolume()
    {
        SoundMgr.instance.SyncSingleAudioSource(au);
    }
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