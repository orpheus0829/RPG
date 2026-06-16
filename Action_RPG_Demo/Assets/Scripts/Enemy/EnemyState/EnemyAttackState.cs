using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackState : Istate
{
    public Enemy enemy;
    public Animator am;
    public EnemyAttackState(Enemy e)
    {
        enemy = e;
        am = e.am;
    }

    public void OnEnter()
    {
        enemy.PlayAnim(3, "RunAttack", 0);
        enemy.agent.speed = enemy.enemySO.RunSpeed;
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
        if (enemy.IsAnimFinished())
        {
            //enemy.agent.isStopped = false;
            enemy.AtkCoolDown = enemy.enemySO.HitCool;
            enemy.TransitionState(EnemyStateType.IdleAndPatrol);
        }
    }

    public void OnUpdate()
    {
        
    }
}
