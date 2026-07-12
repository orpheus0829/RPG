using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

[CustomEditor(typeof(HitBoxClip))]
public class HitBoxClipInspector : Editor
{
    private SerializedProperty hitBoxShape;
    private SerializedProperty boxOffset;
    private SerializedProperty boxRadius;
    private SerializedProperty hitBoxSize;
    private SerializedProperty damage;
    private SerializedProperty HitForce;
    private SerializedProperty startTime;
    private SerializedProperty endTime;
    private SerializedProperty useRepeatScan;
    private SerializedProperty scanInterval;

    private void OnEnable()
    {
        hitBoxShape = serializedObject.FindProperty("hitBoxShape");
        boxOffset = serializedObject.FindProperty("boxOffset");
        boxRadius = serializedObject.FindProperty("boxRadius");
        hitBoxSize = serializedObject.FindProperty("hitBoxSize");
        damage = serializedObject.FindProperty("damage");
        HitForce = serializedObject.FindProperty("HitForce");
        startTime = serializedObject.FindProperty("startTime");
        endTime = serializedObject.FindProperty("endTime");
        useRepeatScan = serializedObject.FindProperty("useRepeatScan");
        scanInterval = serializedObject.FindProperty("scanInterval");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("碰撞盒设置", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(useRepeatScan, new GUIContent("间隔重复判定"));
        if (useRepeatScan.boolValue)
        {
            EditorGUILayout.PropertyField(scanInterval, new GUIContent("扫描间隔(秒)"));
        }
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(hitBoxShape);
        EditorGUILayout.PropertyField(boxOffset, new GUIContent("局部偏移"));

        HitBoxShape shape = (HitBoxShape)hitBoxShape.enumValueIndex;
        if (shape == HitBoxShape.Sphere)
        {
            EditorGUILayout.PropertyField(boxRadius, new GUIContent("球体半径"));
        }
        else
        {
            EditorGUILayout.PropertyField(hitBoxSize, new GUIContent("盒子尺寸"));
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("伤害参数", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(damage, new GUIContent("伤害值"));
        EditorGUILayout.PropertyField(HitForce, new GUIContent("击退力度"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("判定时间", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(startTime, new GUIContent("判定开始时间"));
        EditorGUILayout.PropertyField(endTime, new GUIContent("判定结束时间"));

        serializedObject.ApplyModifiedProperties();
    }
}