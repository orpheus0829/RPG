using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthSlider : MonoBehaviour
{
    public Image HpFillImage;
    public Player Pl;
    public BaseEnemy En;
    public float LerpFactor;
    public float PerHp;
    public float MaxHp;
    public DamageReceiver DamageReceiver;
    public Transform t;

    public void Start()
    {
        Transform RootTrans = transform;
        while (RootTrans.parent != null)
        {
            RootTrans = RootTrans.parent;
        }

        if (RootTrans.gameObject.CompareTag("Enemy"))
        {
            En = RootTrans.GetComponent<BaseEnemy>();
            DamageReceiver = En.damageReceiver;
        }
        else
        {
            Pl = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            DamageReceiver = Pl.damageReceiver;
        }

        MaxHp = DamageReceiver.maxHp;
        HpFillImage = GetDeepestImage(transform);
        if (HpFillImage != null)
        {
            HpFillImage.type = Image.Type.Filled;
            HpFillImage.fillMethod = Image.FillMethod.Horizontal;
        }

        t = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }
    public void OnEnable()
    {
        
    }

    public void Update()
    {
        PerHp = DamageReceiver.currentHp / MaxHp;
        if (Mathf.Abs(HpFillImage.fillAmount - PerHp) > 0.001f)
        {
            HpFillImage.fillAmount = Mathf.Lerp(HpFillImage.fillAmount, PerHp, LerpFactor * Time.deltaTime);
        }
        if (En != null)
        {
            transform.LookAt(t);
            transform.Rotate(0f, 180f, 0f);
        }
    }
    public Image GetDeepestImage(Transform StartTrans)
    {
        Image DeepestImage = null;
        int MaxDepth = -1;
        DfsLoop(StartTrans, 0);
        return DeepestImage;

        void DfsLoop(Transform ChildTrans, int CurrentDepth)
        {
            Image TempImg = ChildTrans.GetComponent<Image>();
            if (TempImg != null && CurrentDepth > MaxDepth)
            {
                MaxDepth = CurrentDepth;
                DeepestImage = TempImg;
            }
            for (int i = 0; i < ChildTrans.childCount; i++)
            {
                DfsLoop(ChildTrans.GetChild(i), CurrentDepth + 1);
            }
        }
    }
}