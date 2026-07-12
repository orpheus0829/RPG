using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaiderAtkState : Istate
{
    public Raider raider;
    public EnemyActionCtrl ac;

    public RaiderAtkState(Raider raider)
    {
        this.raider = raider;
        ac = raider.eac;
    }

    public void OnEnter()
    {
        raider.rb.isKinematic = true;
        ac.PlayAction(ac.Character.AtkList[Random.Range(0, ac.Character.AtkList.Count - 1)]);
        raider.SetChase(false, raider.agent);
        raider.LookAtPlayer();
    }

    public void OnExit()
    {
        
    }

    public void OnFixedUpdate()
    {
        raider.rb.isKinematic = true;
    }

    public void OnUpdate()
    {

    }
}
