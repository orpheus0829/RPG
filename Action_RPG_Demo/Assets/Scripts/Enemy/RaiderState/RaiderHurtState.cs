using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaiderHurtState : Istate
{
    public Raider raider;
    public EnemyActionCtrl ac;

    public RaiderHurtState(Raider raider)
    {
        this.raider = raider;
        ac = raider.eac;
    }

    public void OnEnter()
    {
        raider.rb.isKinematic = false;
        ac.PlayAction(ac.Character.Hurt);
        raider.SetChase(false, raider.agent);
    }

    public void OnExit()
    {

    }

    public void OnFixedUpdate()
    {
        raider.rb.isKinematic = false;
    }

    public void OnUpdate()
    {

    }
}
