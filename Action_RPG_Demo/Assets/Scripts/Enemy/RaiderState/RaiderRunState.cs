using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaiderRunState : Istate
{
    public Raider raider;
    public EnemyActionCtrl ac;

    public RaiderRunState(Raider raider)
    {
        this.raider = raider;
        ac = raider.eac;
    }

    public void OnEnter()
    {
        ac.PlayAction(ac.Character.Run);
        raider.SetChase(true, raider.agent);
    }

    public void OnExit()
    {
        
    }

    public void OnFixedUpdate()
    {
        raider.TurnToPlayer(0, 0);
    }

    public void OnUpdate()
    {
        if (raider.IsChasing)
        {
            float dis = Vector3.Distance(raider.NearestPl.transform.position, raider.transform.position);
            if (dis <= raider.enemySO.HesitantDistance)
            {
                bool atk = Random.Range(0f, 100f) < raider.enemySO.AtkProbablity ? true : false;
                raider.TransitionState(atk ? RaiderStateType.Atk : RaiderStateType.Hesitant); ;
            }
        }
        else
        {
            raider.TransitionState(RaiderStateType.Idle);
        }
    }
}
