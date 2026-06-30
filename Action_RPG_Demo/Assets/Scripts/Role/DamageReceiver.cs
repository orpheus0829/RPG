using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReceiver : MonoBehaviour, IDamageable
{
    public EnemySO enemydata;
    public Enemy em;
    public PlayerSO playerdata;
    public Player pl;
    public float currentHp;
    [Header("ÊÜ»÷»÷ÍË")]
    public float knockForce = 5f;
    public float SmoothLerp;
    [Header("ÊÜ»÷ÀäÈ´")]
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
    public void TakeDamage<T>(float damage, Vector3 attackDir) where T : MonoBehaviour
    {
        T target = GetComponent<T>();
        if (target == null)
        {
            return;
        }
        if ((target is Player p && p.IsDead) || (target is Enemy e && e.IsDead))
        {
            return;
        }

        if (damage <= 0 || isStiff)
        {
            return;
        }
        bool killHit = false;
        if (target is Player player)
        {
            pl = player;
            em = null;
            bool realDodge = pl.actionControl.currentAction == pl.actionControl.Character.Dodge || pl.actionControl.currentAction == pl.actionControl.Character.RunDodge;
            if (realDodge)
            {
                TimeMgr.instance.BulletTime(pl.DownSpeed, pl.BulletScale, pl.BulletDuration, pl.UpSpeed);
                isStiff = true;
                stiffTimer = stiffDuration;
                return;
            }
            currentHp -= damage;
            CheckDead<T>(target);
            if (player.IsDead)
            {
                killHit = true;
            }
        }
        else if (target is Enemy enemy)
        {
            em = enemy;
            pl = null;
            currentHp -= damage;
            CheckDead<T>(target);
            if (enemy.IsDead)
            {
                killHit = true;
            }
        }
        if (killHit)
        {
            return;
        }
        if (target is Enemy en)
        {
            en.TransitionState(EnemyStateType.Hurt);
            TimeMgr.instance.HitPause();
        }
        isStiff = true;
        stiffTimer = stiffDuration;
        if (Rb != null)
        {
            Rb.velocity = Vector3.zero;
            Rb.AddForce(attackDir.normalized * knockForce, ForceMode.Impulse);
        }
    }
    private void CheckDead<T>(T target) where T : MonoBehaviour
    {
        if (target is Player p && p.IsDead)
        {
            return;
        }
        if (target is Enemy e && e.IsDead)
        {
            return;
        }
        if (currentHp > 0)
        {
            return;
        }
        if (target is Player player)
        {
            Panel_Mgr.instance.HideAllPanel();
            player.IsDead = true;
            TimeMgr.instance.UnPauseGame();
            player.StopCurrentAction();
            player.Is_Action_Playing = true;
            player.actionControl.PlayAction(player.actionControl.Character.Death);
            player.gameObject.tag = "DeadPlayer";
            GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var obj in enemys)
            {
                Enemy enemy = obj.GetComponent<Enemy>();
                if (enemy == null) continue;
                DamageTrigger trigger = enemy.damageTrigger;
                if (trigger.WaitHurt.Contains(player))
                {
                    trigger.WaitHurt.Remove(player);
                }
            }
        }
        else if (target is Enemy enemy)
        {
            enemy.damageTrigger.WaitHurt.Clear();
            enemy.IsDead = true;
            enemy.col.isTrigger = true;
            enemy.TransitionState(EnemyStateType.Dead);
        }
    }
}