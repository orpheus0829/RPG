using UnityEngine;
public interface IDamageable
{
    void TakeDamage<T>(float damage, Vector3 attackDir) where T : MonoBehaviour;
}