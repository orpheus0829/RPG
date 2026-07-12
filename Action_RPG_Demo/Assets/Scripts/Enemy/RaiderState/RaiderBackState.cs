using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaiderBackState : Istate
{
    public Raider raider;
    public EnemyActionCtrl ac;


    public RaiderBackState(Raider raider)
    {
        this.raider = raider;
        ac = raider.eac;
    }

    public void OnEnter()
    {
        raider.rb.isKinematic = true;
        ac.PlayAction(ac.Character.BackDodge);
        raider.SetChase(false, raider.agent);

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
