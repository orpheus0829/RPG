using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 受击逻辑组件
/// 处理掉血、僵直、击退效果
/// </summary>
public class DamageReceiver : MonoBehaviour, IDamageable
{
    [Header("受击参数")]
    public float stiffDuration = 0.3f;
    public float knockForce = 5f;
    public float maxHp = 100f;

    private float currentHp;
    private float stiffTimer;
    private bool isStiff;
    private Rigidbody enemyRb;

    private void Awake()
    {
        enemyRb = GetComponent<Rigidbody>();
        currentHp = maxHp;
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
        if (isStiff) return;

        currentHp -= damage;
        isStiff = true;
        stiffTimer = stiffDuration;

        if (enemyRb != null)
        {
            enemyRb.velocity = Vector3.zero;
            enemyRb.AddForce(attackDir.normalized * knockForce, ForceMode.Impulse);
        }
        Debug.Log($"{gameObject.name} 受到伤害：{damage}");
    }
}