using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class EnemyActionCtrl : BaseActor, INotificationReceiver
{
    public BaseEnemy enemy;
    [Header("敌人招式配置")]
    public EnemyRoleSO Character;
    public ActionSO CurAction;
    [Header("动作窗口（由 Timeline 信号控制）")]
    public bool canInterrupt;
    public bool CanHesMove;

    [HideInInspector] public float CurrentHitDamage;
    [HideInInspector] public HitBoxShape CurrentHitShape;
    [HideInInspector] public Vector3 CurrentHitBoxSize;
    private Vector3 _curBoxOffset;
    private float _curBoxRadius;
    private Vector3 _curBoxSize;
    private HitBoxShape _curShape;

    [Header("引用")]
    public Animator roleAnimator;
    public PlayableDirector timelineDirector;
    public AudioSource au;

    private void Awake()
    {
        roleAnimator = GetComponent<Animator>();
        timelineDirector = GetComponent<PlayableDirector>();
        enemy = GetComponent<BaseEnemy>();
        au = GetComponent<AudioSource>();
    }

    public void Update()
    {

    }
    public void FixedUpdate()
    {

    }

    public void OnNotify(Playable origin, INotification notification, object context)
    {
        if (notification is SignalEmitter emitter_i)
        {
            InterruptSignal sig = emitter_i.asset as InterruptSignal;
            if (sig != null)
            {
                canInterrupt = sig.allowInterrupt;
                return;
            }
        }
        if (notification is SignalEmitter emitter_h)
        {
            HesitantSignal sig = emitter_h.asset as HesitantSignal;
            if (sig != null)
            {
                CanHesMove = sig.allowHesMove;
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
            if (item == action)
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
        Debug.Log("切换为" + action.actionName);
        CurAction = action;

        timelineDirector.Stop();
        timelineDirector.playableAsset = action.timeline;
        timelineDirector.Play();
    }
    public void StopCurrentAction()
    {
        if (timelineDirector != null)
        {
            timelineDirector.Stop();
        }
    }
    public void OnActionEnd()
    {
        if (CurAction == null)
        {
            return;
        }
        if(enemy is Raider raider)
        {
            if (CurAction.nextAction != null)
            {
                PlayAction(CurAction.nextAction);
                return;
            }
            else
            {
                raider.EndStateDetect();
                return;
            }
        }
        else
        {
            if (CurAction.nextAction != null)
            {
                PlayAction(CurAction.nextAction);
                return;
            }
            else
            {
                PlayAction(Character.Idle);
                return;
            }
        }
    }

    #region 攻击判定
    public void SetHitBoxData(Vector3 offset, float radius, Vector3 size, HitBoxShape shape, float damage, float force)
    {
        _curBoxOffset = offset;
        _curBoxRadius = radius;
        _curBoxSize = size;
        _curShape = shape;

        CurrentHitDamage = damage;
        CurrentHitShape = shape;
        CurrentHitBoxSize = size;
    }

    public void ClearHitBoxData()
    {
        CurrentHitDamage = 0;
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
            if (!col.CompareTag("Player"))
            {
                continue;
            }
            if (!col.TryGetComponent(out IDamageable target))
            {
                continue;
            }
            Debug.Log("hit玩家");
            if (col.tag == "Player")
            {
                Player p = col.GetComponent<Player>();
                p.GetHurt(CurrentHitDamage, transform.forward);
                p.InputMove = Vector3.zero;
            }
            else
            {
                target.TakeDamage<Player>(CurrentHitDamage, transform.forward);
            }
            if (col.TryGetComponent(out DamageReceiver rec))
            {
                rec.knockForce = 0;
            }
        }
    }
    #endregion

    #region 动画/音效/特效
    public void PlayAnimation(AnimationClip clip)
    {
        if (clip == null)
        {
            return;
        }
        roleAnimator.Play(clip.name);
    }

    public override void PlaySound(AudioClip clip)
    {
        base.PlaySound(clip);
    }

    public override GameObject SpawnEffect(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        return base.SpawnEffect(prefab, pos, rot);
    }
    #endregion
}