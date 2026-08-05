using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpecialSlider : MonoBehaviour
{
    public Slider SpSlider;
    public Player pl;
    public float LerpFactor;
    public float PerSp;
    public void Awake()
    {
        SpSlider = GetComponent<Slider>();
        pl = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }
    public void Start()
    {
        SpSlider.maxValue = 1;
    }
    public void Update()
    {
        PerSp = pl.Skill_PowerPool / pl.MaxPower;
        if (Mathf.Abs(SpSlider.value - PerSp) > 0.001f)
        {
            SpSlider.value = Mathf.Lerp(SpSlider.value, PerSp, LerpFactor * Time.deltaTime);
        }
    }
}
