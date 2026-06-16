using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdleAndPatrolState : Istate
{
    public Enemy enemy;
    public Animator am;
    public EnemyIdleAndPatrolState(Enemy e)
    {
        enemy = e;
        am = e.am;
    }
    public void OnEnter()
    {
        //string cur = enemy.RandomAnim(9, "Idle");
        //Debug.Log($"±¾´Î²¥·Å{cur}");
        //if (enemy.IsSpawnInit)
        //{
        //    am.Play(cur);
        //    enemy.IsSpawnInit = false;
        //}
        //else
        //{
        //    am.CrossFade(cur, 0.3f);
        //}
        enemy.SetChase(false);
        enemy.PlayAnim(9,"Idle", 0.2f);
    }

    public void OnExit()
    {
        
    }

    public void OnFixedUpdate()
    {
        if (enemy.IsDead)
        {
            enemy.TransitionState(EnemyStateType.Dead);
        }
        else if(enemy.AtkCoolDown <= 0 && enemy.PlayerList.Count > 0)
        {
            enemy.TransitionState(enemy.IsChasing ? EnemyStateType.Walk : EnemyStateType.Alert);
        }
    }

    public void OnUpdate()
    {
        
    }
}
