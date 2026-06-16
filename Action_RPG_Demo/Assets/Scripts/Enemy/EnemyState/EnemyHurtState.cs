using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHurtState : Istate
{
    public Enemy enemy;
    public Animator am;
    public EnemyHurtState(Enemy e)
    {
        enemy = e;
        am = e.am;
    }

    public void OnEnter()
    {
        enemy.PlayAnim(4, "Hit", 0.1f);
        enemy.IsChasing = false;
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
            enemy.TransitionState(EnemyStateType.Walk);
        }
    }

    public void OnUpdate()
    {
        enemy.SetChase(false);
    }
}
