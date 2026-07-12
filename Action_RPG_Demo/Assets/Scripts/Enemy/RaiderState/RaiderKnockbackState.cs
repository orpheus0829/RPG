using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaiderKnockbackState : Istate
{
    public Raider raider;
    public EnemyActionCtrl ac;

    public RaiderKnockbackState(Raider raider)
    {
        this.raider = raider;
        ac = raider.eac;
    }

    public void OnEnter()
    {
        ac.PlayAction(ac.Character.KnockBack);
        raider.SetChase(false, raider.agent);
    }

    public void OnExit()
    {

    }

    public void OnFixedUpdate()
    {

    }

    public void OnUpdate()
    {

    }
}
