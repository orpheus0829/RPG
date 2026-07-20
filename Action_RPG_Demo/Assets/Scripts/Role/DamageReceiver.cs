using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReceiver : MonoBehaviour, IDamageable
{
    public EnemySO enemydata;
    public BaseEnemy em;
    public PlayerSO playerdata;
    public Player pl;
    public float currentHp;
    public float maxHp;
    [Header("受击击退")]
    public float knockForce = 5f;
    public float SmoothLerp;
    [Header("受击冷却")]
    public float stiffDuration = 0.3f;
    public float stiffTimer;
    public bool isStiff;
    public Rigidbody Rb;
    public BuffReceiver buffReceiver;

    public void Awake()
    {
        if(!gameObject.TryGetComponent(out BuffReceiver b))
        {
            buffReceiver = gameObject.AddComponent<BuffReceiver>();
        }
        else
        {
            buffReceiver = GetComponent<BuffReceiver>();
        }
        Rb = GetComponent<Rigidbody>();
        if (!Rb)
        {
            Rb = gameObject.AddComponent<Rigidbody>();
        }
        if (TryGetEnemyComponent(out BaseEnemy enemyBase))
        {
            InitEnemyData(enemyBase);
        }
        else if (TryGetComponent(out Player player))
        {
            InitPlayerData(player);
        }
    }
    #region 初始化对象
    private bool TryGetEnemyComponent(out BaseEnemy enemyBase)
    {
        enemyBase = null;
        if (TryGetComponent(out Enemy enemy))
        {
            enemyBase = enemy;
            return true;
        }
        if (TryGetComponent(out Raider raider))
        {
            enemyBase = raider;
            return true;
        }
        return false;
    }

    private void InitEnemyData<T>(T enemy) where T : BaseEnemy
    {
        enemydata = enemy.enemySO;
        em = enemy;
        if (!em.IsQuest)
        {
            currentHp = enemydata.MaxHP;
            maxHp = currentHp;
        }
        playerdata = null;
        pl = null;
    }

    private void InitPlayerData(Player player)
    {
        playerdata = player.playerSO;
        pl = player;
        enemydata = null;
        em = null;
        currentHp = playerdata.PlayerMaxHP;
        maxHp = currentHp;
    }
    #endregion
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
        if (IsTargetDead(target) || damage <= 0 || isStiff)
        {
            return;
        }
        bool killHit = false;
        if (target is Player player)
        {
            HandlePlayerDamage(player, damage, out killHit);
        }
        else if (target is BaseEnemy enemy)
        {
            HandleEnemyDamage(enemy, damage, out killHit);
        }
        if (killHit)
        {
            return;
        }
        ApplyHurtEffect(target, attackDir);
    }
    private bool IsTargetDead(MonoBehaviour target)
    {
        if (target is Player p)
        {
            return p.IsDead;
        }
        if (target is BaseEnemy e)
        {
            return e.IsDead;
        }
        return false;
    }
    private void HandlePlayerDamage(Player player, float damage, out bool killHit)
    {
        killHit = false;
        pl = player;
        em = null;
        bool realDodge = pl.CurAC.currentAction == pl.CurAC.Character.Dodge || pl.CurAC.currentAction == pl.CurAC.Character.RunDodge;
        if (realDodge)
        {
            TimeMgr.instance.BulletTime(pl.DownSpeed, pl.BulletScale, pl.BulletDuration, pl.UpSpeed);
            isStiff = true;
            stiffTimer = stiffDuration;
            return;
        }
        currentHp -= damage;
        CheckDead(player);
        killHit = player.IsDead;
    }
    private void HandleEnemyDamage(BaseEnemy enemy, float damage, out bool killHit)
    {
        killHit = false;
        em = enemy;
        pl = null;
        currentHp -= damage;
        CheckDead(enemy);
        killHit = enemy.IsDead;
    }
    private void ApplyHurtEffect(MonoBehaviour target, Vector3 attackDir)
    {
        isStiff = true;
        stiffTimer = stiffDuration;
        if(target.TryGetComponent(out HurtFlashRenderer hurtflash))
        {
            hurtflash.PlayFlashRed();
        }
        if (target is BaseEnemy enemy)
        {
            TimeMgr.instance.HitPause();
            enemy.LookAtPlayer();
            enemy.SwitchHurtState();
        }
        if (Rb != null)
        {
            Rb.velocity = Vector3.zero;
            Rb.AddForce(attackDir.normalized * knockForce, ForceMode.Impulse);
        }
    }
    private void CheckDead<T>(T target) where T : MonoBehaviour
    {
        if (IsTargetDead(target) || currentHp > 0)
        {
            return;
        }
        if (target.TryGetComponent(out HurtFlashRenderer hurtflash))
        {
            hurtflash.PlayFlashRed();
        }
        if (target is Player player)
        {
            HandlePlayerDeath(player);
        }
        else if (target is BaseEnemy enemy)
        {
            HandleEnemyDeath(enemy);
        }
    }
    private void HandlePlayerDeath(Player player)
    {
        Panel_Mgr.instance.HideAllPanel();
        player.IsDead = true;
        TimeMgr.instance.UnPauseGame();
        player.StopCurrentAction();
        player.Is_Action_Playing = true;
        player.CurAC.PlayAction(player.CurAC.Character.Death);
        player.gameObject.tag = "DeadPlayer";
        GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var obj in enemys)
        {
            BaseEnemy enemy = obj.GetComponent<BaseEnemy>();
            if (enemy == null)
            {
                continue;
            }
            DamageTrigger trigger = enemy.damageTrigger;
            if (trigger.WaitHurt.Contains(player))
            {
                trigger.WaitHurt.Remove(player);
            }
        }
    }
    private void HandleEnemyDeath(BaseEnemy enemy)
    {
        if (enemy.damageTrigger)
        {
            enemy.damageTrigger.WaitHurt.Clear();
        }
        enemy.IsDead = true;
        enemy.col.isTrigger = true;
        enemy.SwitchDeadState();
    }
}