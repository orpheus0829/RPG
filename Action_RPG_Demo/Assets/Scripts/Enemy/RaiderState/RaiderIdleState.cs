using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaiderIdleState : Istate
{
    public Raider raider;
    public EnemyActionCtrl ac;
    public RaiderIdleState(Raider raider)
    {
        this.raider = raider;
        ac = raider.eac;
    }

    public void OnEnter()
    {
        ac.PlayAction(ac.Character.Idle);
        raider.SetChase(true, raider.agent);
    }

    public void OnExit()
    {
        
    }

    public void OnFixedUpdate()
    {
        
    }

    public void OnUpdate()
    {
        if (raider.IsChasing)
        {
            raider.TransitionState(RaiderStateType.Alert);
        }
    }
}
