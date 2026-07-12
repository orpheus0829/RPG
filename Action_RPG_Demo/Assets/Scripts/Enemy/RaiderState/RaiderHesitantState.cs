using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaiderHesitantState : Istate
{
    public Raider raider;
    public EnemyActionCtrl ac;
    public RaiderHesitantState(Raider raider)
    {
        this.raider = raider;
        ac = raider.eac;
    }

    public void OnEnter()
    {
        raider.rb.isKinematic = false;
        raider.Count = 0;
        raider.HesWaitTimer = 0;
        raider.SetChase(false, raider.agent);

        raider.GenerateHesPoint();
    }

    public void OnExit()
    {
        raider.rb.velocity = Vector3.zero;
        raider.HesWaitTimer = 0;
    }

    public void OnFixedUpdate()
    {
        raider.rb.isKinematic = false;
        if (raider.NearestPl == null)
        {
            return;
        }
        MoveInHesitation();
        raider.LookAtPlayer();
    }

    public void OnUpdate()
    {
        float dis = Vector3.Distance(raider.NearestPl.transform.position, raider.transform.position);
        if (raider.IsChasing)
        {
            if (dis < raider.enemySO.DangerDistance)
            {
                Debug.Log("危险后退");
                bool bck = Random.Range(0f, 100f) <= raider.enemySO.DangerBackProbablity ? true : false;
                ac.StopCurrentAction();
                raider.TransitionState(bck ? RaiderStateType.Back : RaiderStateType.Atk);
                return;
            }
            else if (dis > raider.enemySO.HesitantDistance + 2f)
            {
                raider.TransitionState(RaiderStateType.Run);
                return;
            }
        }
        else
        {
            raider.TransitionState(RaiderStateType.Idle);
            return;
        }
        if (raider.MaxHesitantCount <= raider.Count && dis <= raider.enemySO.HesitantDistance * 3/2 )
        {
            Debug.Log("想打人了");
            raider.TransitionState(RaiderStateType.Atk);
            return;
        }
    }
    public void MoveInHesitation()
    {
        float distToTarget = Vector3.Distance(raider.transform.position, raider.HesMoveTarget);
        if (distToTarget < 0.01f)
        {
            raider.rb.velocity = Vector3.zero;
            return;
        }
        if (ac.CanHesMove)
        {
            Vector3 moveDir = raider.HesMoveTarget - raider.transform.position;
            moveDir.y = 0;
            moveDir.Normalize();
            raider.rb.velocity = moveDir * raider.enemySO.WalkSpeed;
        }
        else
        {
            raider.rb.velocity = Vector3.zero;
        }
    }
}
