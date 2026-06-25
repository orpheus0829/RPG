using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDeadState : Istate
{
    public Enemy enemy;
    public Animator am;
    public EnemyDeadState(Enemy e)
    {
        enemy = e;
        am = e.am;
    }

    public void OnEnter()
    {
        //Debug.Log("À¿Õˆenter");
        enemy.gameObject.tag = "DeadEnemy";
        enemy.PlayAnim(2, "DeadDown", 0.1f);
        enemy.SetChase(false);
    }

    public void OnExit()
    {
        
    }

    public void OnFixedUpdate()
    {

    }
    public void OnUpdate()
    {
        enemy.SetChase(false);
    }
}
