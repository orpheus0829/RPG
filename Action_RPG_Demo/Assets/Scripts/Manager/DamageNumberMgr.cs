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
    public void ShowDamageNumber(GameObject targetObj, float damageValue)
    {
        if (!targetObj)
        {
            return;
        }
        Vector3 spawnBasePos;
        if (targetObj.CompareTag("Enemy"))
        {
            HealthSlider slider = FindHealthSliderDFS(targetObj.transform);
            if (slider)
            {
                spawnBasePos = slider.transform.parent.position;
            }
            else
            {
                spawnBasePos = targetObj.transform.position;
            }
        }
        else
        {
            spawnBasePos = targetObj.transform.position;
        }

        GameObject numObj = ObjectPoolMgr.instance.GetObj(DamageNumPrefab, spawnBasePos);
        DamageNumItem damageNumItem = numObj.GetComponent<DamageNumItem>();

        if (!damageNumItem)
        {
            return;
        }
        damageNumItem.Initialize(damageValue, spawnBasePos);
    }
    private HealthSlider FindHealthSliderDFS(Transform root)
    {
        HealthSlider slider = root.GetComponent<HealthSlider>();
        if (slider)
        {
            return slider;
        }

        foreach (Transform child in root)
        {
            HealthSlider result = FindHealthSliderDFS(child);
            if (result)
            {
                return result;
            }
        }
        return null;
    }
}