using UnityEngine;
using System.Collections;

public class DamageNumberMgr : Base_mgr<DamageNumberMgr>
{
    [Header("ÉËº¦×ÖÌå")]
    public GameObject DamageNumPrefab;

    [Header("¶¯»­")]
    public float RiseSpeed = 2.2f;
    public float FadeTotalTime = 1f;
    public float HighDamageThreshold = 30f;

    [Header("×ÖºÅ")]
    public float NormalFontSize = 0.12f;
    public float CritFontSize = 0.18f;

    [Header("É¢²¼·¶Î§")]
    public float RandomXRange = 0.4f;
    public float RandomZRange = 0.4f;
    public float BaseSpawnYOffset = 1.3f;

    protected override void Awake()
    {
        base.Awake();
        if (instance == this)
        {
            DontDestroyOnLoad(gameObject);
        }
    }
    public void ShowDamageNumber(Vector3 monsterWorldPos, float damageValue)
    {
        GameObject numObj = ObjectPoolMgr.instance.GetObj(DamageNumPrefab, monsterWorldPos);
        DamageNumItem damageNumItem = numObj.GetComponent<DamageNumItem>();

        if (damageNumItem == null)
        {
            return;
        }

        damageNumItem.Initialize(damageValue, monsterWorldPos);
    }
}