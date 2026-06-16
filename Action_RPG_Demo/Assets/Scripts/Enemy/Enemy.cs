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
public class Enemy : MonoBehaviour
{
    public EnemySO enemySO;
    public bool IsSpawnInit = true;
    public bool IsQuest;
    public float RotateSmooth;
    [Header("FSM")]
    public Istate CurrentState;
    public AnimatorStateInfo animstate;
    public Dictionary<EnemyStateType, Istate> EnemyStates = new Dictionary<EnemyStateType, Istate>();
    public EnemyStateType CurType;
    [Header("检测玩家")]
    public List<GameObject> PlayerList = new List<GameObject>();
    [Header("攻击")]
    public float AtkCoolDown;
    public bool IsChasing;
    public float damage;
    public DamageTrigger damageTrigger;
    [Header("死亡")]
    public bool IsDead;
    public float DeadTime;
    [Header("引用")]
    public CapsuleCollider col;
    public Animator am;
    public DamageReceiver damageReceiver;
    public Rigidbody rb;
    public NavMeshAgent agent;
    public void Awake()
    {
        col = GetComponent<CapsuleCollider>();
        am = GetComponent<Animator>();
        damageReceiver = GetComponent<DamageReceiver>();
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        damageTrigger = GetComponentInChildren<DamageTrigger>();
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
    public void OnEnable()
    {
        IsSpawnInit = true;
        IsDead = false;
        DeadTime = enemySO.DisappearTime;
        TransitionState(Random.Range(1, 101) <= enemySO.IdlePer ? EnemyStateType.IdleAndPatrol : EnemyStateType.lie);
        gameObject.tag = "Enemy";
        col.isTrigger = false;
        damageReceiver.currentHp = enemySO.MaxHP;
    }
    public void OnDisable()
    {
        QuestBase_SO questBase = Story_Mgr.instance.GetCurrentQuest(); 
        if (IsQuest && questBase is FightQuest_SO fight && Story_Mgr.instance.CurEnemys.Contains(this.gameObject))
        {
            Story_Mgr.instance.CurEnemys.Remove(this.gameObject);
            if (Story_Mgr.instance.CurEnemys.Count <= 0)
            {
                Story_Mgr.instance.CurEnemys.Clear();
                //Story_Mgr.instance.QuestAdvance();
            }
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
        //Debug.Log($"进入{type}");
        CurrentState.OnEnter();
    }
    public void PlayAnim(int anim_num,string anim_name,float crossfade_time, int layer = 0)
    {
        string cur = RandomAnim(anim_num, anim_name);
        int useLayer = layer < 0 ? 0 : layer;
        //Debug.Log($"本次播放{cur}");
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
        SearchPlayer();
        IsChasing = PlayerList.Count > 0 ? true : false;
    }
    public void FixedUpdate()
    {
        CurrentState.OnFixedUpdate();
        if (AtkCoolDown > 0)
        {
            AtkCoolDown -= Time.fixedDeltaTime;
        }
        if (IsDead)
        {
            DeadTime -= Time.fixedDeltaTime;
            if (DeadTime <= 0)
            {
                ObjectPoolMgr.instance.PushObj(gameObject);
                Story_Mgr.instance.CheckAllEnemyDead();
            }
        }
    }
    public string RandomAnim(int max,string anim_name)
    {
        int num = Random.Range(1, max+1);
        string anim = $"{anim_name}{num}";
        return anim;
    }
    public void SearchPlayer()
    {
        PlayerList.Clear();
        Collider[] colliders = Physics.OverlapSphere(this.gameObject.transform.position, enemySO.ChaseRadius);
        foreach(var i in colliders)
        {
            if (i.CompareTag("Player"))
            {
                PlayerList.Add(i.gameObject);
            }
        }
        ResortPlayerList();
    }
    public void ResortPlayerList()
    {
        PlayerList.Sort((a, b) =>
        {
            float disa = (a.transform.position - transform.position).sqrMagnitude;
            float disb = (b.transform.position - transform.position).sqrMagnitude;
            return disa.CompareTo(disb);
        });
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
            TurnToPlayer(animTime);
        }
    }
    public void TurnToPlayer(float time)
    {
        //if (PlayerList.Count <= 0)
        //{
        //    return;
        //}
        Vector3 dir = PlayerList[0].transform.position - transform.position;
        Quaternion d = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Lerp(transform.rotation, d, Time.deltaTime * RotateSmooth);
    }
    public void SetChase(bool chase)
    {
        if (chase && PlayerList.Count > 0)
        {
            agent.SetDestination(PlayerList[0].transform.position);
        }
        else
        {
            agent.ResetPath();
        }
    }
    public void EnemyAttack()
    {
        foreach(var i in damageTrigger.WaitHurt)
        {
            i.GetHurt(damage, transform.forward);
            i.InputMove = Vector3.zero;
            Debug.Log($"打到了玩家{i.name}");
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
}
