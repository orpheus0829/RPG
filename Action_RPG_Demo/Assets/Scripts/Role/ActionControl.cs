using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class ActionControl : MonoBehaviour, INotificationReceiver
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
    private SphereCollider _sphereCollider;
    private BoxCollider _boxCollider;
    public float Hit_Force = 0;

    public Vector3 debugBoxOffset;
    public float debugBoxRadius;
    public bool debugDrawHitBox;

    private void Awake()
    {
        player = GetComponent<Player>();
        roleAnimator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        timelineDirector = GetComponent<PlayableDirector>();
    }
    public void Update()
    {
        if (currentAction == Character.Walk && player.InputMove == Vector3.zero)
        {
            player.StopCurrentAction();
            PlayAction(Character.WalkEnd);
        }
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

        //Debug.Log($"收到未知信号: {notification?.GetType().Name}");
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
            Debug.Log("转至" + currentAction.nextAction);
            PlayAction(currentAction.nextAction);
        }
        else
        {
            Debug.Log("默认转至待机");
            PlayAction(Character.Idle);
        }
    }


    #region 攻击判定
    public void PlayAttackAction()
    {
        AttackLevel = canCombo ? (AttackLevel % Character.AtkList.Count) + 1 : 1;

        if (AttackLevel < 1 || AttackLevel > Character.AtkList.Count)
        {
            AttackLevel = 1;
        }
        Single_ATK atkData = Character.AtkList[AttackLevel - 1];
        ActionSO attackToPlay;
        if (atkData.HasVariantATK)
        {
            float random = Random.Range(0f, 100f);
            if (random <= atkData.Percentage)
            {
                attackToPlay = atkData.PerfectATK;
            }
            else
            {
                attackToPlay = atkData.ATK;
            }
        }
        else
        {
            attackToPlay = atkData.ATK;
        }
        PlayAction(attackToPlay);
        player.IsAttacking = false;
    }
    public void OpenHitBox(Vector3 offset, float radius, Vector3 boxSize)
    {
        if (currentAction == null)
        {
            return;
        }
        CloseHitBox();

        if (currentAction.hitBoxShape == HitBoxShape.Sphere)
        {
            if (_sphereCollider == null)
            {
                _sphereCollider = gameObject.AddComponent<SphereCollider>();
                _sphereCollider.isTrigger = true;
            }
            _sphereCollider.center = offset;
            _sphereCollider.radius = radius;
            _sphereCollider.enabled = true;
        }
        else
        {
            if (_boxCollider == null)
            {
                _boxCollider = gameObject.AddComponent<BoxCollider>();
                _boxCollider.isTrigger = true;
            }
            _boxCollider.center = offset;
            _boxCollider.size = boxSize;
            _boxCollider.enabled = true;
        }

        debugBoxOffset = offset;
        debugDrawHitBox = true;
    }
    public void CloseHitBox()
    {
        if (_sphereCollider != null)
        {
            _sphereCollider.enabled = false;
        }
        if (_boxCollider != null)
        {
            _boxCollider.enabled = false;
        }
        debugDrawHitBox = false;
    }
    public void OnDrawGizmos()
    {
        if (!debugDrawHitBox || currentAction == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Vector3 worldPos = transform.TransformPoint(debugBoxOffset);
        if (currentAction.hitBoxShape == HitBoxShape.Sphere)
        {
            Gizmos.DrawWireSphere(worldPos, debugBoxRadius);
        }
        else
        {
            Gizmos.DrawWireCube(worldPos, currentAction.hitBoxSize);
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IDamageable target))
        {
            if(other.TryGetComponent(out DamageReceiver receiver))
            {
                receiver.knockForce = Hit_Force != 0 ? Hit_Force : receiver.knockForce;
            }
            target.TakeDamage(currentAction.damageValue, transform.forward);
            Hit_Force = 0;
        }
    }
}