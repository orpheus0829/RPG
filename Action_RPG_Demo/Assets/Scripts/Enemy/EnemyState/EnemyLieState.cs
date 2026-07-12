using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLieState : Istate
{
    public Enemy enemy;
    public Animator am;
    public EnemyLieState(Enemy e)
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
        //string cur = enemy.RandomAnim(3, "LieGround");
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
        enemy.PlayAnim(3, "LieGround", 0.3f);
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
        if (enemy.PlayerList.Count > 0)
        {
            enemy.TransitionState(EnemyStateType.GetUp);
        }
    }

    public void OnUpdate()
    {
        enemy.SetChase(false, enemy.agent);
    }
}
