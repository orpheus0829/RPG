using UnityEngine;

/// <summary>
/// 可受击接口
/// 统一所有可被击打对象受伤规范
/// </summary>
public interface IDamageable
{
    void TakeDamage<T>(float damage, Vector3 attackDir) where T : MonoBehaviour;
}