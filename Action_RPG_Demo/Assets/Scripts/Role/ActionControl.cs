using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class ActionControl : BaseActor, INotificationReceiver
{
    [Header("组件绑定")]
    public Player player;
    public Animator roleAnimator;
    public AudioSource audioSource;
    public Camera mainCamera;
    public PlayableDirector timelineDirector;

    [Header("角色招式配置")]
    public CharacterActionSO Character;
    public ActionSO currentAction;
    //[Header("待机动作")]
    //public ActionSO idleAction;

    //[Header("移动动作")]
    //public ActionSO walkAction;
    //public ActionSO WalkStart;
    //public ActionSO WalkEnd;
    //public ActionSO runAction;

    //[Header("攻击动作")]
    //public ActionSO attack1Action;
    //public ActionSO attack2Action;
    //public ActionSO attack3Action;
    //public ActionSO attack4Action_Nor;
    //public ActionSO attack4Action_Per;
    //[Range(0f,1f)]public float Perfect_Range = 0.33f;

    //[Header("特殊技")]
    //public ActionSO Related_Full_E;
    //public ActionSO Full_E;
    //public ActionSO Related_Unfilled_E;
    //public ActionSO Unfilled_E;

    //[Header("跳跃动作")]
    //public ActionSO JumpAction;
    //[Header("翻越动作")]
    //public ActionSO CrossAction;
    //public ActionSO CrossAtfAction;
    public Vector3 Cross_Location;

    //[Header("滑铲")]
    //public ActionSO SlideAction;

    //[Header("死亡动作")]
    //public ActionSO DeathAction;

    [Header("动作窗口（由 Timeline 信号控制）")]
    public int AttackLevel;
    public bool canCombo;
    public bool canInterrupt;

    [Header("攻击范围盒调试用")]
    public float Hit_Force = 0;
    public Vector3 debugBoxOffset;
    public float debugBoxRadius;
    public bool debugDrawHitBox;

    [HideInInspector] public float CurrentHitDamage;
    [HideInInspector] public HitBoxShape CurrentHitShape;
    [HideInInspector] public Vector3 CurrentHitBoxSize;
    private Vector3 _curBoxOffset;
    private float _curBoxRadius;
    private Vector3 _curBoxSize;
    private HitBoxShape _curShape;
    private void Awake()
    {
        player = GetComponent<Player>();
        roleAnimator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        timelineDirector = GetComponent<PlayableDirector>();
    }
    public void Update()
    {

    }
    public void FixedUpdate()
    {

    }
    public void OnNotify(Playable origin, INotification notification, object context)
    {
        if (notification is SignalEmitter emitter)
        {
            ActionWindowSignal sig = emitter.asset as ActionWindowSignal;

            if (sig != null)
            {
                canCombo = sig.allowCombo;
                canInterrupt = sig.allowInterrupt;
                //Debug.Log($"收到信号: canCombo={canCombo}, canInterrupt={canInterrupt}");
                return;
            }
        }
        if (notification is SignalEmitter emitter_c)
        {
            ComboSignal sig = emitter_c.asset as ComboSignal;

            if (sig != null)
            {
                canCombo = sig.allowCombo;
                //Debug.Log($"收到信号: canCombo={canCombo}");
                return;
            }
        }
        if (notification is SignalEmitter emitter_i)
        {
            InterruptSignal sig = emitter_i.asset as InterruptSignal;

            if (sig != null)
            {
                canInterrupt = sig.allowInterrupt;
                //Debug.Log($"收到信号:  canInterrupt={canInterrupt}");
                return;
            }
        }
    }
    public bool IsInAttackAction(ActionSO action)
    {
        if (Character == null || action == null)
        {
            return false;
        }

        foreach (var item in Character.AtkList)
        {
            if (item.ATK == action || item.PerfectATK == action)
            {
                return true;
            }
        }
        return false;
    }
    public void PlayAction(ActionSO action)
    {
        if (action == null || action.timeline == null)
        {
            return;
        }
        if (IsInAttackAction(action))
        {
            player.InputMove = Vector3.zero;
            player.isWalking = false;
        }
        //Debug.Log("切换为" + action.actionName);
        if (currentAction == Character.Walk)
        {
            currentAction = Character.Walk;
        }
        currentAction = action;

        timelineDirector.Stop();
        timelineDirector.playableAsset = action.timeline;
        timelineDirector.Play();
    }

    public void OnActionEnd()
    {
        player.IsBlock = false;
        if (currentAction == null)
        {
            return;
        }
        if (currentAction.nextAction != null)
        {
            PlayAction(currentAction.nextAction);
            return;
        }
        if (player.IsHoldingMove)
        {
            if (currentAction == Character.RunDodge)
            {
                player.InputMove = player.moveaction.ReadValue<Vector3>();
                player.isStopping = false;
                player.isWalking = true;
                PlayAction(Character.Run);
            }
            else
            {
                player.InputMove = player.moveaction.ReadValue<Vector3>();
                player.isStopping = false;
                player.isWalking = true;
                PlayAction(Character.Walk);
            }
        }
        else
        {
            player.InputMove = Vector3.zero;
            player.rb.velocity = Vector3.zero;
            player.isWalking = false;
            player.isStopping = true;
            PlayAction(Character.Idle);
            Debug.Log("默认转至待机");
        }
    }


    #region 攻击判定
    public void PlayAttackAction()
    {
        PlayAttackAction(false);
    }
    public void PlayAttackAction(bool IsHold)
    {
        AttackLevel = canCombo ? (AttackLevel % Character.AtkList.Count) + 1 : 1;

        if (AttackLevel < 1 || AttackLevel > Character.AtkList.Count)
        {
            AttackLevel = 1;
        }
        Single_ATK atkData = Character.AtkList[AttackLevel - 1];
        ActionSO attackToPlay = atkData.ATK;
        if (atkData.HasVariantATK && IsHold)
        {
            attackToPlay = atkData.PerfectATK;
        }
        PlayAction(attackToPlay);
        player.IsAttacking = false;
    }
    public void SetHitBoxData(Vector3 offset, float radius, Vector3 size, HitBoxShape shape, float damage, float force)
    {
        _curBoxOffset = offset;
        _curBoxRadius = radius;
        _curBoxSize = size;
        _curShape = shape;

        CurrentHitDamage = damage;
        Hit_Force = force;
        CurrentHitShape = shape;
        CurrentHitBoxSize = size;

        debugBoxOffset = offset;
        debugBoxRadius = radius;
        debugDrawHitBox = true;
    }

    public void ClearHitBoxData()
    {
        CurrentHitDamage = 0;
        Hit_Force = 0;
        debugDrawHitBox = false;
    }

    public void DoSingleHitScan()
    {
        Vector3 worldCenter = transform.TransformPoint(_curBoxOffset);
        Collider[] hits;

        if (_curShape == HitBoxShape.Sphere)
        {
            hits = Physics.OverlapSphere(worldCenter, _curBoxRadius);
        }
        else
        {
            hits = Physics.OverlapBox(worldCenter, _curBoxSize * 0.5f, transform.rotation);
        }

        foreach (var col in hits)
        {
            if (!col.CompareTag("Enemy"))
            {
                continue;
            }
            if (!col.TryGetComponent(out IDamageable target))
            {
                continue;
            }
            target.TakeDamage<Enemy>(CurrentHitDamage, transform.forward);
            if (currentAction.actionType == ActionType.Attack)
            {
                player.Skill_PowerPool += CurrentHitDamage * player.PowerFactor;
                player.Skill_PowerPool = Mathf.Clamp(player.Skill_PowerPool, 0, player.MaxPower);
            }
            player.Charge += CurrentHitDamage * player.ChargeFactor;
            player.Charge = Mathf.Clamp(player.Charge, 0, player.MaxCharge);
            //Debug.Log($"造成{CurrentHitDamage}伤害");
            if (Hit_Force != 0 && col.TryGetComponent(out DamageReceiver rec))
            {
                rec.knockForce = Hit_Force;
            }
        }
        Hit_Force = 0;
    }
    public void OnDrawGizmos()
    {
        if (!debugDrawHitBox || currentAction == null)
        {
            return;
        }
        Gizmos.color = Color.red;
        Vector3 worldPos = transform.TransformPoint(debugBoxOffset);
        if (CurrentHitShape == HitBoxShape.Sphere)
        {
            Gizmos.DrawWireSphere(worldPos, debugBoxRadius);
        }
        else
        {
            Gizmos.DrawWireCube(worldPos, CurrentHitBoxSize);
        }
    }
    #endregion

    #region 动画/音效/特效
    public void PlayAnimation(AnimationClip clip)
    {
        if (clip == null) return;
        roleAnimator.Play(clip.name);
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, player.transform.position);
    }

    public GameObject SpawnEffect(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (prefab == null)
        {
            return null;
        }
        return Instantiate(prefab, pos, rot);
    }
    #endregion
    #region 相机
    public CameraMotion GetCameraMotion()
    {
        return mainCamera.GetComponent<CameraMotion>();
    }
    #endregion
}