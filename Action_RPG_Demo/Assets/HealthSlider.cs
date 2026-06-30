using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthSlider : MonoBehaviour
{
    public Slider HpSlider;
    public Player pl;
    public float LerpFactor;
    public float PerHp;
    public void Awake()
    {
        HpSlider = GetComponent<Slider>();
        pl = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }
    public void Start()
    {
        HpSlider.maxValue = 1;
        //HpSlider.value = pl.damageReceiver.currentHp;
    }
    public void Update()
    {
        PerHp = pl.damageReceiver.currentHp / pl.playerSO.PlayerMaxHP;
        if (Mathf.Abs(HpSlider.value - PerHp) > 0.001f)
        {
            HpSlider.value = Mathf.Lerp(HpSlider.value, PerHp, LerpFactor * Time.deltaTime);
        }
    }
}
