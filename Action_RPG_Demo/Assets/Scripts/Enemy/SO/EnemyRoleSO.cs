using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyRole_Data", menuName = "Enemy/EnemyRole_Data")]
public class EnemyRoleSO : ScriptableObject
{
    public ActionSO Idle;
    public ActionSO Alert;
    public ActionSO HesitantIdle;
    public ActionSO HesitantL;
    public ActionSO HesitantR;
    public ActionSO BackDodge;
    public ActionSO Run;
    public List<ActionSO> AtkList = new List<ActionSO>();
    public ActionSO HeavyAtk;
    public ActionSO KnockBack;
    public ActionSO Hurt;
    public ActionSO Die;
}
