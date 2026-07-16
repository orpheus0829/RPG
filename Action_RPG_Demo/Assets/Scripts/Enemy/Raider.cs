using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum RaiderStateType
{
    Idle,
    Alert,
    Hesitant,
    Run,
    Atk,
    HeavyAtk,
    Knockback,
    Back,
    Hurt,
    Dead,
}
public class Raider : BaseEnemy
{
    public GameObject NearestPl;
    public float RotateSmooth;
    [Header("FSM")]
    public Istate CurrentState;
    public AnimatorStateInfo animstate;
    public Dictionary<RaiderStateType, Istate> RaiderStates = new Dictionary<RaiderStateType, Istate>();
    public RaiderStateType CurType;
    [Header("踱步")]
    public bool IsIdlePausing;
    public int Count;
    public Vector3 HesMoveTarget;
    public float HesWaitTimer;
    public int MaxHesitantCount;
    public bool MoveToLeft;
    public TimeMgr.TimerTask HesitantIdleCorountine;
    [Header("引用")]
    public EnemyActionCtrl eac;
    //public CapsuleCollider col;
    //public DamageReceiver damageReceiver;
    //public Animator am;
    //public Rigidbody rb;
    //public NavMeshAgent agent;
    public override void Awake()
    {
        eac = GetComponent<EnemyActionCtrl>();
        damageReceiver = GetComponent<DamageReceiver>();
        am = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        col = GetComponent<CapsuleCollider>();
        renderer = GetComponentInChildren<SkinnedMeshRenderer>();
        flashRenderer = GetComponent<HurtFlashRenderer>();
        flashRenderer.TargetSkinRender = renderer;
        flashRenderer.InitMaterial();

        RaiderStates.Add(RaiderStateType.Idle, new RaiderIdleState(this));
        RaiderStates.Add(RaiderStateType.Alert, new RaiderAlertState(this));
        RaiderStates.Add(RaiderStateType.Hesitant, new RaiderHesitantState(this));
        RaiderStates.Add(RaiderStateType.Run, new RaiderRunState(this));
        RaiderStates.Add(RaiderStateType.Atk, new RaiderAtkState(this));
        RaiderStates.Add(RaiderStateType.HeavyAtk, new RaiderHeavyAtkState(this));
        RaiderStates.Add(RaiderStateType.Knockback, new RaiderKnockbackState(this));
        RaiderStates.Add(RaiderStateType.Back, new RaiderBackState(this));
        RaiderStates.Add(RaiderStateType.Hurt, new RaiderHurtState(this));
        RaiderStates.Add(RaiderStateType.Dead, new RaiderDeadState(this));
    }
    public void Start()
    {

    }
    public override void OnEnable()
    {
        base.OnEnable();
        DeadTime = enemySO.DisappearTime;
        col.isTrigger = false;
        if (!IsQuest)
        {
            damageReceiver.currentHp = enemySO.MaxHP;
        }
        rb.isKinematic = false;
        TransitionState(RaiderStateType.Idle);
    }
    public override void OnDisable()
    {
        base.OnDisable();
        if (flashRenderer != null)
        {
            flashRenderer.ClearTimer();
        }
    }
    public void TransitionState(RaiderStateType type)
    {
        if (CurrentState != null)
        {
            CurrentState.OnExit();
        }
        if (HesitantIdleCorountine != null)
        {
            if (TimeMgr.instance != null)
            {
                TimeMgr.instance.StopTimer(HesitantIdleCorountine);
            }
            HesitantIdleCorountine = null;
        }
        IsIdlePausing = false;
        CurrentState = RaiderStates[type];
        CurType = type;
        Debug.Log($"进入{type}");
        CurrentState.OnEnter();
    }
    public void Update()
    {
        CurrentState.OnUpdate();
        SearchPlayer(enemySO.ChaseRadius);
        NearestPl = PlayerList.Count > 0 ? PlayerList[0] : null;
        IsChasing = PlayerList.Count > 0 ? true : false;
    }
    public void FixedUpdate()
    {
        CurrentState.OnFixedUpdate();
    }
    public override void SearchPlayer(float chaseraidus)
    {
        base.SearchPlayer(chaseraidus);
    }
    public override void ResortPlayerList()
    {
        base.ResortPlayerList();
    }
    public override void TurnToPlayer(float time, float rotatesmmoth)
    {
        base.TurnToPlayer(time, rotatesmmoth);
    }
    public override void LookAtPlayer()
    {
        base.LookAtPlayer();
    }
    public void AlertTurn()
    {
        TurnToPlayer((float)eac.Character.Alert.timeline.duration, RotateSmooth);
    }
    public override void SetChase(bool chase, NavMeshAgent agent)
    {
        base.SetChase(chase, agent);
    }
    public void EndStateDetect()
    {
        if (!IsChasing || NearestPl == null)
        {
            TransitionState(RaiderStateType.Idle);
            return;
        }
        LookAtPlayer();
        float dis = Vector3.Distance(NearestPl.transform.position, transform.position);
        float hesDist = enemySO.HesitantDistance;
        float rate;
        float realThreshold;
        switch (CurType)
        {
            case RaiderStateType.Idle:
                TransitionState(RaiderStateType.Idle);
                break;
            case RaiderStateType.Alert:
                if (dis > hesDist)
                {
                    TransitionState(RaiderStateType.Run);
                }
                else
                {
                    rate = Mathf.Clamp01(1 - dis / hesDist);
                    realThreshold = enemySO.AtkProbablity * rate;
                    bool canAtk = Random.Range(0f, 100f) < realThreshold;
                    TransitionState(canAtk ? RaiderStateType.Atk : RaiderStateType.Hesitant);
                }
                break;
            case RaiderStateType.Hesitant:
                rate = Mathf.Clamp01(1 - dis / hesDist);
                realThreshold = enemySO.AtkProbablity * rate;
                bool rollAtk = Random.Range(0f, 100f) < realThreshold;
                if (rollAtk)
                {
                    TransitionState(RaiderStateType.Atk);
                }
                break;
            case RaiderStateType.Run:
                if (dis <= hesDist)
                {
                    TransitionState(RaiderStateType.Atk);
                }
                else
                {
                    TransitionState(RaiderStateType.Run);
                }
                break;
            case RaiderStateType.Atk:
            case RaiderStateType.HeavyAtk:
                bool wantBack = Random.Range(0f, 100f) < enemySO.DodgeProbablity;
                TransitionState(wantBack ? RaiderStateType.Back : RaiderStateType.Hesitant);
                break;
            case RaiderStateType.Knockback:
                TransitionState(RaiderStateType.Hesitant);
                break;
            case RaiderStateType.Back:
                TransitionState(RaiderStateType.Hesitant);
                break;
            case RaiderStateType.Hurt:
                if (dis <= hesDist)
                {
                    TransitionState(RaiderStateType.Atk);
                }
                else
                {
                    TransitionState(RaiderStateType.Run);
                }
                break;
            case RaiderStateType.Dead:
                break;
            default:
                break;
        }
    }
    public void GenerateHesPoint()
    {
        if (IsIdlePausing)
        {
            return;
        }
        Transform playerTf = NearestPl.transform;
        float circleRadius = enemySO.HesitantDistance * 0.75f;
        Vector3 toEnemy = transform.position - playerTf.position;
        toEnemy.y = 0;
        float currentAngle = Mathf.Atan2(toEnemy.z, toEnemy.x) * Mathf.Rad2Deg;
        float angleOffset = Random.Range(-70f, 70f);
        float targetAngle = currentAngle + angleOffset;
        float rad = targetAngle * Mathf.Deg2Rad;
        float targetX = playerTf.position.x + Mathf.Cos(rad) * circleRadius;
        float targetZ = playerTf.position.z + Mathf.Sin(rad) * circleRadius;
        Vector3 targetPoint = new Vector3(targetX, transform.position.y, targetZ);
        HesMoveTarget = targetPoint;
        Vector3 selfForward = transform.forward;
        selfForward.y = 0;
        Vector3 selfRight = Vector3.Cross(Vector3.up, selfForward);
        Vector3 dirToTarget = targetPoint - transform.position;
        dirToTarget.y = 0;
        float cross = Vector3.Cross(selfForward, dirToTarget).y;
        MoveToLeft = cross > 0;
        ActionSO targetWalkAnim = MoveToLeft ? eac.Character.HesitantR : eac.Character.HesitantL;
        eac.StopCurrentAction();
        eac.PlayAction(targetWalkAnim);
    }
    public void StartHesitantIdleTimer()
    {
        if (HesitantIdleCorountine != null)
        {
            TimeMgr.instance.StopTimer(HesitantIdleCorountine);
        }
        IsIdlePausing = true;
        HesitantIdleCorountine = TimeMgr.instance.CreateTimer(
            TimeMgr.TimerMode.DeltaTime,
            0,
            enemySO.HesitantInterval,
            HesIdleStart,
            HesIdleEnd
        );
    }
    public void HesIdleStart()
    {
        if (eac.CurAction == eac.Character.HesitantIdle)
        {
            return;
        }
        eac.StopCurrentAction();
        eac.PlayAction(eac.Character.HesitantIdle);
    }
    public void HesIdleEnd()
    {
        Count++;
        IsIdlePausing = false;
        GenerateHesPoint();
        HesitantIdleCorountine = null;
    }
    public void HesStop()
    {
        rb.velocity = Vector3.zero;
        if ((HesitantIdleCorountine == null || !HesitantIdleCorountine.IsRunning()) && !IsIdlePausing)
        {
            StartHesitantIdleTimer();
        }
    }
    public override void SwitchHurtState()
    {
        if (CurType == RaiderStateType.Atk)
        {
            return;
        }
        TransitionState(RaiderStateType.Hurt);
    }
    public override void SwitchDeadState()
    {
        TransitionState(RaiderStateType.Dead);
        TimeMgr.instance.CreateTimer(TimeMgr.TimerMode.DeltaTime, 0, DeadTime, null, () =>
        {
            ObjectPoolMgr.instance.PushObj(gameObject);
            Story_Mgr.instance.CheckAllEnemyDead();
        });
    }
    public override void BeParried()
    {
        base.BeParried();
        TransitionState(RaiderStateType.Hurt);
    }
}
