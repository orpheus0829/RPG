using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 受击逻辑组件
/// 处理掉血、僵直、击退效果
/// </summary>
public class DamageReceiver : MonoBehaviour, IDamageable
{
    public EnemySO enemydata;
    public Enemy em;
    public PlayerSO playerdata;
    public Player pl;
    public float currentHp;
    [Header("受击击退")]
    public float knockForce = 5f;
    public float SmoothLerp;
    [Header("受击冷却")]
    public float stiffDuration = 0.3f;
    public float stiffTimer;
    public bool isStiff;
    public Rigidbody Rb;

    private void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        if(this.gameObject.TryGetComponent(out Enemy enemy))
        {
            enemydata = enemy.enemySO;
            em = enemy;
            playerdata = null;
            pl = null;
            Rb = enemy.rb;
        }
        else if(this.gameObject.TryGetComponent(out Player player))
        {
            playerdata = player.playerSO;
            pl = player;
            enemydata = null;
            em = null;
            currentHp = playerdata.PlayerMaxHP;
            Rb = player.rb;
        }
    }

    private void Update()
    {
        if (isStiff)
        {
            stiffTimer -= Time.deltaTime;
            if (stiffTimer <= 0f)
            {
                isStiff = false;
            }
        }
    }

    /// <summary>
    /// 接收伤害处理
    /// </summary>
    public void TakeDamage(float damage, Vector3 attackDir)
    {
        if (damage <= 0)
        {
            return;
        }
        if (isStiff)
        {
            return;
        }
        currentHp -= damage;
        CheckDead();
        if(em && !pl)
        {
            if (em.IsDead)
            {
                return;
            }
            if (damage > 0)
            {
                em.TransitionState(EnemyStateType.Hurt);
            }
        }
        isStiff = true;
        stiffTimer = stiffDuration;

        if (Rb != null)
        {
            Rb.velocity = Vector3.zero;
            Rb.AddForce(attackDir.normalized * knockForce, ForceMode.Impulse);
        }
        Debug.Log($"{gameObject.name} 受到伤害：{damage}");
    }
    public void CheckDead()
    {
        if ((em && em.IsDead) || pl && pl.IsDead)
        {
            return;
        }
        if (currentHp > 0)
        {
            return;
        }
        if (pl && !em)
        {
            Panel_Mgr.instance.HideAllPanel();
            pl.IsDead = true;
            TimeMgr.instance.UnPauseGame();
            pl.StopCurrentAction();
            pl.Is_Action_Playing = true;
            pl.actionControl.PlayAction(pl.actionControl.Character.Death);
            pl.gameObject.tag = "DeadPlayer";
            GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var i in enemys)
            {
                DamageTrigger trigger = i.GetComponent<Enemy>().damageTrigger;
                if (trigger.WaitHurt.Contains(pl))
                {
                    trigger.WaitHurt.Remove(pl);
                }
            }
        }
        else if (em && !pl)
        {
            Debug.Log("怪物死亡");
            em.damageTrigger.WaitHurt.Clear();
            em.IsDead = true;
            em.col.isTrigger = true;
            em.TransitionState(EnemyStateType.Dead);
        }
    }
}