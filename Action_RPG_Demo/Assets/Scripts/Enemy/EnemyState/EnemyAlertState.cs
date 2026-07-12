using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAlertState : Istate
{
    public Enemy enemy;
    public Animator am;
    public EnemyAlertState(Enemy e)
    {
        enemy = e;
        am = e.am;
    }
    public void OnEnter()
    {
        if (enemy.tag == "DeadEnemy")
        {
            return;
        }
        enemy.RotateForward();
        enemy.SetChase(false, enemy.agent);
        //string cur= enemy.RandomAnim(2, "Detection");
        //Debug.Log($"±¾´Î²¥·Å{cur}");
        //if (enemy.IsSpawnInit)
        //{
        //    am.Play(cur);
        //    enemy.IsSpawnInit = false;
        //}
        //else
        //{
        //    am.CrossFade(cur, 0.4f);
        //}
        enemy.PlayAnim(3, "Detection", 0.4f);
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
        if (enemy.AtkCoolDown > 0)
        {
            enemy.TransitionState(EnemyStateType.IdleAndPatrol);
        }
        if (enemy.damageTrigger.WaitHurt.Count > 0)
        {
            enemy.TransitionState(EnemyStateType.Run);
        }
        if (enemy.IsAnimFinished() && enemy.PlayerList.Count > 0)
        {
            enemy.TransitionState(Random.Range(1, 101) <= enemy.enemySO.WalkPer ? EnemyStateType.Walk : EnemyStateType.Run);
        }
        else if(enemy.IsAnimFinished() && enemy.PlayerList.Count <= 0)
        {
            enemy.TransitionState(EnemyStateType.IdleAndPatrol);
            enemy.SetChase(false, enemy.agent);
        }
    }

    public void OnUpdate()
    {
        enemy.SetChase(false, enemy.agent);
    }
}
