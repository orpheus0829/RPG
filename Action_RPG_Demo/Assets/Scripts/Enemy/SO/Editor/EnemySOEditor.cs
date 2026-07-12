using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemySO))]
public class EnemySOEditor : Editor
{
    SerializedProperty pEnemyType;

    SerializedProperty pDamage;
    SerializedProperty pMaxHP;
    SerializedProperty pWalkSpeed;
    SerializedProperty pRunSpeed;
    SerializedProperty pChaseRadius;
    SerializedProperty pDisappearTime;

    SerializedProperty pHitDetectLengeh;
    SerializedProperty pHitLength;
    SerializedProperty pHitRadius;
    SerializedProperty pHitHigh;
    SerializedProperty pHitCool;
    SerializedProperty pIdlePer;
    SerializedProperty pWalkPer;
    SerializedProperty pRunFromWalk;
    SerializedProperty pChangeInterval;

    SerializedProperty pMinHesitantWalk;
    SerializedProperty pHesitantDistance;
    SerializedProperty pHesitantInterval;
    SerializedProperty pAtkProbablity;
    SerializedProperty pDodgeProbablity;
    SerializedProperty pDangerDistance;
    SerializedProperty pDangerBackProbablity;

    private void OnEnable()
    {
        serializedObject.Update();
        pEnemyType = serializedObject.FindProperty("enemyType");
        pDamage = serializedObject.FindProperty("Damage");
        pMaxHP = serializedObject.FindProperty("MaxHP");
        pWalkSpeed = serializedObject.FindProperty("WalkSpeed");
        pRunSpeed = serializedObject.FindProperty("RunSpeed");
        pChaseRadius = serializedObject.FindProperty("ChaseRadius");
        pDisappearTime = serializedObject.FindProperty("DisappearTime");

        pHitDetectLengeh = serializedObject.FindProperty("HitDetectLengeh");
        pHitLength = serializedObject.FindProperty("HitLength");
        pHitRadius = serializedObject.FindProperty("HitRadius");
        pHitHigh = serializedObject.FindProperty("HitHigh");
        pHitCool = serializedObject.FindProperty("HitCool");
        pIdlePer = serializedObject.FindProperty("IdlePer");
        pWalkPer = serializedObject.FindProperty("WalkPer");
        pRunFromWalk = serializedObject.FindProperty("RunFromWalk");
        pChangeInterval = serializedObject.FindProperty("ChangeInterval");

        pMinHesitantWalk = serializedObject.FindProperty("MinHesitantWalk");
        pHesitantDistance = serializedObject.FindProperty("HesitantDistance");
        pHesitantInterval = serializedObject.FindProperty("HesitantInterval");
        pAtkProbablity = serializedObject.FindProperty("AtkProbablity");
        pDodgeProbablity = serializedObject.FindProperty("DodgeProbablity");
        pDangerDistance = serializedObject.FindProperty("DangerDistance");
        pDangerBackProbablity = serializedObject.FindProperty("DangerBackProbablity");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("怪物类型选择", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(pEnemyType, new GUIContent("目标对象"));
        EnemySO.EnemyType selectType = (EnemySO.EnemyType)pEnemyType.enumValueIndex;

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("公有基础属性", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(pDamage, new GUIContent("伤害"));
        EditorGUILayout.PropertyField(pMaxHP, new GUIContent("最大血量"));
        EditorGUILayout.PropertyField(pWalkSpeed, new GUIContent("走路速度"));
        EditorGUILayout.PropertyField(pRunSpeed, new GUIContent("奔跑速度"));
        EditorGUILayout.PropertyField(pChaseRadius, new GUIContent("玩家追逐半径"));
        EditorGUILayout.PropertyField(pDisappearTime, new GUIContent("死亡消失时间"));
        EditorGUILayout.Space();

        if (selectType == EnemySO.EnemyType.Zombie)
        {
            EditorGUILayout.LabelField("僵尸配置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(pHitDetectLengeh, new GUIContent("前方检测长度"));
            EditorGUILayout.PropertyField(pHitLength, new GUIContent("攻击盒长度"));
            EditorGUILayout.PropertyField(pHitRadius, new GUIContent("攻击盒半径"));
            EditorGUILayout.PropertyField(pHitHigh, new GUIContent("攻击盒高度偏移"));
            EditorGUILayout.PropertyField(pHitCool, new GUIContent("攻击冷却"));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("出生行为概率");
            EditorGUILayout.PropertyField(pIdlePer, new GUIContent("待机概率"));
            EditorGUILayout.PropertyField(pWalkPer, new GUIContent("巡逻走路概率"));
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(pRunFromWalk, new GUIContent("追逐后走路转奔跑概率"));
            EditorGUILayout.PropertyField(pChangeInterval, new GUIContent("行走/状态切换间隔"));
        }
        else if (selectType == EnemySO.EnemyType.Raider)
        {
            EditorGUILayout.LabelField("劫匪配置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(pMinHesitantWalk, new GUIContent("僵持徘徊最小位移"));
            EditorGUILayout.PropertyField(pHesitantDistance, new GUIContent("僵持触发距离"));
            EditorGUILayout.PropertyField(pHesitantInterval, new GUIContent("僵持切换间隔"));
            EditorGUILayout.PropertyField(pAtkProbablity, new GUIContent("僵持后发起攻击概率"));
            EditorGUILayout.PropertyField(pDodgeProbablity, new GUIContent("僵持后闪避概率"));
            EditorGUILayout.PropertyField(pDangerDistance, new GUIContent("危险距离"));
            EditorGUILayout.PropertyField(pDangerBackProbablity, new GUIContent("危险距离后退概率"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}