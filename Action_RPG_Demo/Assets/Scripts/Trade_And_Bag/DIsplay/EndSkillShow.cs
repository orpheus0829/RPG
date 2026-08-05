using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndSkillShow : MonoBehaviour
{
    public Player player;
    public List<Image> All;
    public float CurCharge;
    public float MaxCharge;

    public float LerpFactor;
    public void Awake()
    {
        player = FindObjectOfType<Player>();
        Image[] images = GetComponentsInChildren<UnityEngine.UI.Image>();
        CurCharge = player.Charge;
        MaxCharge = player.MaxCharge;
        foreach(var i in images)
        {
            All.Add(i);
        }
        
    }
    public void Update()
    {
        MaxCharge = player.MaxCharge;
        CurCharge = player.Charge <= player.MaxCharge ? player.Charge : player.MaxCharge;
        float percharge = CurCharge / MaxCharge;
        foreach (var i in All)
        {
            i.fillAmount = Mathf.Lerp(i.fillAmount, percharge, LerpFactor * Time.deltaTime);
        }
        //i.f = Mathf.Lerp(SpSlider.value, PerSp, LerpFactor * Time.deltaTime);
    }
}
