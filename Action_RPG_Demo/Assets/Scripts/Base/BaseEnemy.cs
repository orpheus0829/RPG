using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BaseEnemy : MonoBehaviour
{
    public EnemySO enemySO;
    public bool IsQuest;
    [Header("死亡")]
    public bool IsDead;
    public float DeadTime;
    [Header("玩家追逐")]
    public List<GameObject> PlayerList = new List<GameObject>();
    public bool IsChasing;
    [Header("敌人共有引用")]
    public CapsuleCollider col;
    public DamageTrigger damageTrigger;
    public DamageReceiver damageReceiver;
    public Animator am;
    public Rigidbody rb;
    public NavMeshAgent agent;
    public SkinnedMeshRenderer renderer;
    public HurtFlashRenderer flashRenderer;
    public virtual void Awake()
    {

    }
    public virtual void OnEnable()
    {
        IsDead = false;
        gameObject.tag = "Enemy";
    }
    public virtual void OnDisable()
    {
        QuestBase_SO questBase = Story_Mgr.instance.GetCurrentQuest();
        if (IsQuest && questBase is FightQuest_SO fight && Story_Mgr.instance.CurEnemys.Contains(this.gameObject))
        {
            Story_Mgr.instance.CurEnemys.Remove(this.gameObject);
            if (Story_Mgr.instance.CurEnemys.Count <= 0)
            {
                Story_Mgr.instance.CurEnemys.Clear();
            }
        }
        if (MiniMapMgr.instance.trackingTarget == this.gameObject)
        {
            NavPathMgr.instance.CloseNavPath();
            MiniMapMgr.instance.trackingTarget = null;
        }
    }
    public virtual void SearchPlayer(float chaseraidus)
    {
        PlayerList.Clear();
        Collider[] colliders = Physics.OverlapSphere(this.gameObject.transform.position, chaseraidus);
        foreach (var i in colliders)
        {
            if (i.CompareTag("Player"))
            {
                PlayerList.Add(i.gameObject);
            }
        }
        ResortPlayerList();
    }
    public virtual void ResortPlayerList()
    {
        PlayerList.Sort((a, b) =>
        {
            float disa = (a.transform.position - transform.position).sqrMagnitude;
            float disb = (b.transform.position - transform.position).sqrMagnitude;
            return disa.CompareTo(disb);
        });
    }
    public virtual void TurnToPlayer(float time , float rotatesmmoth)
    {
        Vector3 dir = PlayerList[0].transform.position - transform.position;
        Quaternion d = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Lerp(transform.rotation, d, Time.deltaTime * rotatesmmoth);
    }
    public virtual void LookAtPlayer()
    {
        Vector3 dir = PlayerList[0].transform.position - transform.position;
        transform.LookAt(transform.position + dir);
    }
    public virtual void SetChase(bool chase,NavMeshAgent agent)
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
    public virtual void SwitchHurtState()
    {

    }
    public virtual void SwitchDeadState()
    {

    }
}
