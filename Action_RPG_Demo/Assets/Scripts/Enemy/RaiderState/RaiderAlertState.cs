using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaiderAlertState : Istate
{
    public Raider raider;
    public EnemyActionCtrl ac;
    public RaiderAlertState(Raider raider)
    {
        this.raider = raider;
        ac = raider.eac;
    }

    public void OnEnter()
    {
        ac.PlayAction(ac.Character.Alert);
        raider.SetChase(false, raider.agent);
        raider.AlertTurn();
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
