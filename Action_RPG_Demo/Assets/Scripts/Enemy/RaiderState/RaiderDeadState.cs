using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaiderDeadState : Istate
{
    public Raider raider;
    public EnemyActionCtrl ac;

    public RaiderDeadState(Raider raider)
    {
        this.raider = raider;
        ac = raider.eac;
    }

    public void OnEnter()
    {
        if (raider.tag == "DeadEnemy")
        {
            return;
        }
        raider.gameObject.tag = "DeadEnemy";
        ac.PlayAction(ac.Character.Die);
        raider.SetChase(false, raider.agent);
        raider.rb.isKinematic = true;
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
