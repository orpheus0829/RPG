using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGetUpState : Istate
{
    public Enemy enemy;
    public Animator am;
    public EnemyGetUpState(Enemy e)
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
        //string cur = enemy.RandomAnim(1, "GetUp");
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
        enemy.PlayAnim(1, "GetUp", 0.4f);
        enemy.RotateForward();
    }

    public void OnExit()
    {

    }

    public void OnFixedUpdate()
    {
        if (enemy.IsDead)
        {
            enemy.PlayAnim(1, "Dead", 0.6f);
        }
        if (enemy.IsAnimFinished() && enemy.PlayerList.Count > 0)
        {
            enemy.TransitionState(Random.Range(1, 101) <= enemy.enemySO.WalkPer ? EnemyStateType.Walk : EnemyStateType.Run);
        }
        else if (enemy.IsAnimFinished() && enemy.PlayerList.Count <= 0)
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
