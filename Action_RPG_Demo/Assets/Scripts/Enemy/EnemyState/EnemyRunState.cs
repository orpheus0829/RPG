using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRunState : Istate
{
    public Enemy enemy;
    public Animator am;
    public EnemyRunState(Enemy e)
    {
        enemy = e;
        am = e.am;
    }

    public void OnEnter()
    {
        enemy.PlayAnim(1, "RunFront", 0.3f);
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
        enemy.SetChase(true);
        if (enemy.AtkCoolDown > 0)
        {
            enemy.TransitionState(EnemyStateType.IdleAndPatrol);
        }
        if (enemy.PlayerList.Count <= 0)
        {
            enemy.TransitionState(EnemyStateType.IdleAndPatrol);
        }
        else
        {
            //RaycastHit hitinfo;
            //if(Physics.Raycast(enemy.transform.position + enemy.transform.up * enemy.enemySO.HitHigh,enemy.transform.forward,out hitinfo, enemy.enemySO.HitLength))
            //{
            //    if (hitinfo.collider.tag == "Player")
            //    {
            //        enemy.TransitionState(EnemyStateType.Attack);
            //    }
            //}
            if (enemy.damageTrigger.WaitHurt.Count > 0 && enemy.AtkCoolDown <= 0)
            {
                Debug.Log("ÊÇÊ±ºò¹¥»÷ÁË");
                enemy.TransitionState(EnemyStateType.Attack);
            }
            //else
            //{
            //    enemy.TransitionState(EnemyStateType.Walk);
            //}
        }
    }

    public void OnUpdate()
    {
        enemy.SetChase(true);
    }
}
