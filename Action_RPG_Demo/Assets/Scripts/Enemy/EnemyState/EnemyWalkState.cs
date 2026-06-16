using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWalkState : Istate
{
    public Enemy enemy;
    public Animator am;
    public Coroutine ChangeCorountine;
    public EnemyWalkState(Enemy e)
    {
        enemy = e;
        am = e.am;
    }

    public void OnEnter()
    {
        //string cur = enemy.RandomAnim(1, "WalkFront");
        //Debug.Log($"本次播放{cur}");
        //if (enemy.IsSpawnInit)
        //{
        //    am.Play(cur);
        //    enemy.IsSpawnInit = false;
        //}
        //else
        //{
        //    am.CrossFade(cur, 0.3f);
        //}
        enemy.PlayAnim(1, "WalkFront", 0.3f);
        enemy.agent.speed = enemy.enemySO.WalkSpeed;
        ChangeCorountine = enemy.StartCoroutine(TimeToChange());
    }
    public void ChangeSpeed()
    {
        //Debug.Log("改变速度");
        int num = Random.Range(0, 101);
        if (num <= enemy.enemySO.RunFromWalk && enemy.AtkCoolDown <= 0)
        {
            enemy.TransitionState(EnemyStateType.Run);
        }
    }
    public void OnExit()
    {

    }
    public IEnumerator TimeToChange()
    {
        while (enemy.gameObject)
        {
            yield return new WaitForSeconds(enemy.enemySO.ChangeInterval);
            ChangeSpeed();
        }
    }
    public void ShutDownCorountine()
    {
        enemy.StartCoroutine(TimeToChange());
        ChangeCorountine = null;
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
            ShutDownCorountine();
            enemy.TransitionState(EnemyStateType.IdleAndPatrol);
            enemy.SetChase(false);
        }
        //RaycastHit hit;
        //if (Physics.Raycast(enemy.transform.position, enemy.transform.forward, out hit, enemy.enemySO.HitLength))
        //{
        //    if (hit.collider.tag == "Player")
        //    {
        //        enemy.TransitionState(EnemyStateType.Attack);
        //    }
        //}
        //RaycastHit hitinfo;
        //if (Physics.Raycast(enemy.transform.position, enemy.transform.forward, out hitinfo, enemy.enemySO.HitDetectLengeh))
        //{
        //    if (hitinfo.collider.tag == "Player")
        //    {
        //        ShutDownCorountine();
        //        enemy.TransitionState(EnemyStateType.Run);
        //    }
        //}
        if (enemy.damageTrigger.WaitHurt.Count > 0)
        {
            enemy.TransitionState(EnemyStateType.Run);
        }
    }

    public void OnUpdate()
    {
        enemy.SetChase(true);
    }
}
