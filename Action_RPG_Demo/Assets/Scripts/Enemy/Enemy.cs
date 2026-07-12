using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStateType
{
    IdleAndPatrol,
    lie,
    GetUp,
    Alert,
    Walk,
    Run,
    Attack,
    Hurt,
    Dead,
}
public class Enemy : BaseEnemy
{
    public bool IsSpawnInit = true;
    public float RotateSmooth;
    [Header("FSM")]
    public Istate CurrentState;
    public AnimatorStateInfo animstate;
    public Dictionary<EnemyStateType, Istate> EnemyStates = new Dictionary<EnemyStateType, Istate>();
    public EnemyStateType CurType;
    [Header("攻击")]
    public float AtkCoolDown;
    public float damage;
    public bool IsAttacking;
    //public DamageTrigger damageTrigger;
    //[Header("死亡")]
    //public float DeadTime;
    //public bool IsDead;
    //[Header("引用")]
    //public CapsuleCollider col;
    //public DamageReceiver damageReceiver;
    //public Animator am;
    //public Rigidbody rb;
    //public NavMeshAgent agent;
    public override void Awake()
    {
        base.Awake();
        am = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        col = GetComponent<CapsuleCollider>();
        damageReceiver = GetComponent<DamageReceiver>();
        damageTrigger = GetComponentInChildren<DamageTrigger>();
        renderer = GetComponentInChildren<SkinnedMeshRenderer>();
        flashRenderer = GetComponent<HurtFlashRenderer>();
        flashRenderer.TargetSkinRender = renderer;
        flashRenderer.InitMaterial();
        damage = enemySO.Damage;
        damageReceiver.currentHp = enemySO.MaxHP;
        DeadTime = enemySO.DisappearTime;

        EnemyStates.Add(EnemyStateType.IdleAndPatrol, new EnemyIdleAndPatrolState(this));
        EnemyStates.Add(EnemyStateType.lie, new EnemyLieState(this));
        EnemyStates.Add(EnemyStateType.GetUp, new EnemyGetUpState(this));
        EnemyStates.Add(EnemyStateType.Alert, new EnemyAlertState(this));
        EnemyStates.Add(EnemyStateType.Walk, new EnemyWalkState(this));
        EnemyStates.Add(EnemyStateType.Run, new EnemyRunState(this));
        EnemyStates.Add(EnemyStateType.Attack, new EnemyAttackState(this));
        EnemyStates.Add(EnemyStateType.Hurt, new EnemyHurtState(this));
        EnemyStates.Add(EnemyStateType.Dead, new EnemyDeadState(this));
        TransitionState(Random.Range(1, 101) <= enemySO.IdlePer ? EnemyStateType.IdleAndPatrol : EnemyStateType.lie);
    }
    public void Start()
    {

    }
    public override void OnEnable()
    {
        base.OnEnable();
        IsSpawnInit = true;
        TransitionState(Random.Range(1, 101) <= enemySO.IdlePer ? EnemyStateType.IdleAndPatrol : EnemyStateType.lie);
        DeadTime = enemySO.DisappearTime;
        col.isTrigger = false;
        damageReceiver.currentHp = enemySO.MaxHP;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        if (flashRenderer != null)
        {
            flashRenderer.ClearTimer();
        }
    }
    public void TransitionState(EnemyStateType type)
    {
        if (CurrentState != null)
        {
            CurrentState.OnExit();
        }
        CurrentState = EnemyStates[type];
        CurType = type;
        CurrentState.OnEnter();
    }
    public void PlayAnim(int anim_num,string anim_name,float crossfade_time, int layer = 0)
    {
        string cur = RandomAnim(anim_num, anim_name);
        int useLayer = layer < 0 ? 0 : layer;
        if (IsSpawnInit)
        {
            am.Play(cur);
            IsSpawnInit = false;
        }
        else
        {
            am.CrossFade(cur, crossfade_time, useLayer);
        }
    }
    public bool IsAnimFinished()
    {
        animstate = am.GetCurrentAnimatorStateInfo(0);
        return animstate.normalizedTime >= 1.05f;
    }
    public void Update()
    {
        CurrentState.OnUpdate();
        SearchPlayer(enemySO.ChaseRadius);
        IsChasing = PlayerList.Count > 0 ? true : false;
    }
    public void FixedUpdate()
    {
        CurrentState.OnFixedUpdate();
        if (AtkCoolDown > 0)
        {
            AtkCoolDown -= Time.fixedDeltaTime;
        }
        //if (IsDead)
        //{
        //    DeadTime -= Time.fixedDeltaTime;
        //    if (DeadTime <= 0)
        //    {
        //        ObjectPoolMgr.instance.PushObj(gameObject);
        //        Story_Mgr.instance.CheckAllEnemyDead();
        //    }
        //}
    }
    public string RandomAnim(int max,string anim_name)
    {
        int num = Random.Range(1, max+1);
        string anim = $"{anim_name}{num}";
        return anim;
    }
    public override void SearchPlayer(float chaseradius)
    {
        base.SearchPlayer(chaseradius);
    }
    public override void ResortPlayerList()
    {
        base.ResortPlayerList();
    }
    public void RotateForward()
    {
        AnimatorClipInfo[] clipinfo = am.GetCurrentAnimatorClipInfo(0);
        float animTime = 1f;
        if (clipinfo.Length > 0)
        {
            animTime = clipinfo[0].clip.length;
        }
        if (PlayerList.Count > 0)
        {
            TurnToPlayer(animTime,RotateSmooth);
        }
    }
    public override void TurnToPlayer(float time, float rotatesmmoth)
    {
        base.TurnToPlayer(time, rotatesmmoth);
    }
    public override void SetChase(bool chase, NavMeshAgent agent)
    {
        base.SetChase(chase, agent);
    }
    public void EnemyAttack()
    {
        foreach(var i in damageTrigger.WaitHurt)
        {
            if (i.tag == "Player")
            {
                i.GetHurt(damage, transform.forward);
                i.InputMove = Vector3.zero;
            }
            //Debug.Log($"打到了玩家{i.name}");
        }
    }
    public void FrezzeMove()
    {
        agent.isStopped = true;
    }
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 hitcenter = transform.position + new Vector3(0, enemySO.HitHigh, 0) + transform.forward * enemySO.HitLength;
        Gizmos.DrawWireSphere(hitcenter, enemySO.HitRadius);
        Gizmos.DrawLine(transform.position + transform.up * enemySO.HitHigh, transform.position + new Vector3(0, 0, enemySO.HitDetectLengeh));
    }
    public override void SwitchHurtState()
    {
        TransitionState(EnemyStateType.Hurt);
    }
    public override void SwitchDeadState()
    {
        TransitionState(EnemyStateType.Dead);
        TimeMgr.instance.CreateTimer(TimeMgr.TimerMode.DeltaTime, 0, DeadTime, null, () =>
        {
            ObjectPoolMgr.instance.PushObj(gameObject);
            Story_Mgr.instance.CheckAllEnemyDead();
        });
    }
}
