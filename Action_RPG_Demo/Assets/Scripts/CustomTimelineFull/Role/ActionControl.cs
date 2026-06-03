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

    [Header("待机动作")]
    public ActionSO idleAction;

    [Header("移动动作")]
    public ActionSO walkAction;
    public ActionSO WalkStart;
    public ActionSO WalkEnd;
    public ActionSO runAction;

    [Header("攻击动作")]
    public ActionSO attack1Action;
    public ActionSO attack2Action;
    public ActionSO attack3Action;
    public ActionSO attack4Action_Nor;
    public ActionSO attack4Action_Per;

    public ActionSO currentAction;

    [Header("跳跃动作")]
    public ActionSO JumpAction;
    [Header("翻越动作")]
    public ActionSO CrossAction;
    [Header("爬墙")]
    public ActionSO WallUp_Start;
    public ActionSO WallUp_Run;
    public ActionSO WallUp_Catch;
    public ActionSO Hang;
    public ActionSO WallUp_Success;
    public ActionSO WallClimb_Spring;

    [Header("滑铲")]
    public ActionSO SlideAction;

    [Header("落地")]
    public ActionSO Fall_InAir;
    public ActionSO Fall_Roll;
    public ActionSO Fall_Normal;

    [Header("动作窗口（由 Timeline 信号控制）")]
    public int AttackLevel;
    public bool canCombo;
    public bool canInterrupt;
    public bool isClimbing;

    [Header("攻击范围盒调试用")]
    private SphereCollider hitCollider;
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

        hitCollider = gameObject.AddComponent<SphereCollider>();
        hitCollider.isTrigger = true;
        hitCollider.enabled = false;
    }
    public void Update()
    {
        if (currentAction == walkAction && player.InputMove == Vector3.zero)
        {
            player.StopCurrentAction();
            PlayAction(WalkEnd);
        }
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
                isClimbing = sig.IsClimbing;
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
        if (notification is SignalEmitter emitter_climb)
        {
            ClimbingSignal sig = emitter_climb.asset as ClimbingSignal;

            if (sig != null)
            {
                isClimbing = sig.IsClimbing;
                //Debug.Log($"收到信号:  isClimbing={isClimbing}");
                return;
            }
        }

        //Debug.Log($"收到未知信号: {notification?.GetType().Name}");
    }

    public void PlayAction(ActionSO action)
    {
        if (action == null || action.timeline == null)
        {
            return;
        }
        if (currentAction == attack1Action || currentAction == attack2Action || currentAction == attack3Action || currentAction == attack4Action_Nor || currentAction==attack4Action_Per)
        {
            player.isWalking = false;
        }
        //Debug.Log("切换为" + action.actionName);
        if (currentAction == walkAction)
        {
            currentAction = walkAction;
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
            PlayAction(idleAction);
        }
    }


    #region 攻击判定
    public void PlayAttackAction()
    {
        AttackLevel = canCombo ? (AttackLevel % 4) + 1 : 1;
        ActionSO attack_so;
        switch (AttackLevel)
        {
            case 1:
                attack_so = attack1Action;
                break;
            case 2:
                attack_so = attack2Action;
                break;
            case 3:
                attack_so = attack3Action;
                break;
            case 4:
                int choice = Random.Range(0, 100);
                attack_so = choice <= 33 ? attack4Action_Nor : attack4Action_Per;
                break;
            default:
                attack_so = attack1Action;
                break;
        }
        PlayAction(attack_so);
        player.IsAttacking = false;
    }
    public void OpenHitBox(Vector3 offset, float radius)
    {
        if (hitCollider)
        {
            hitCollider.isTrigger = true;
        }
        hitCollider.center = offset;
        hitCollider.radius = radius;
        hitCollider.enabled = true;

        debugDrawHitBox = true;
        debugBoxOffset = offset;
        debugBoxRadius = radius;
    }
    public void CloseHitBox()
    {
        hitCollider.enabled = false;

        debugDrawHitBox = false;
    }
    public void OnDrawGizmos()
    {
        if (!debugDrawHitBox)
        {
            return;
        }

        Gizmos.color = Color.red;
        Vector3 worldPos = transform.TransformPoint(debugBoxOffset);
        Gizmos.DrawWireSphere(worldPos, debugBoxRadius);
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