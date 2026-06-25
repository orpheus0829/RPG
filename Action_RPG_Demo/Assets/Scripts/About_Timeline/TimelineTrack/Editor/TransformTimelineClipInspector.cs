using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TransformTimelineClip))]
public class TransformTimelineClipInspector : Editor
{
    private SerializedProperty moveMode;
    private SerializedProperty direction;
    private SerializedProperty endPos;
    private SerializedProperty moveSpeed;
    private SerializedProperty totalDistance;
    private SerializedProperty startSpeed;
    private SerializedProperty endSpeed;
    private SerializedProperty climbStage;
    private SerializedProperty climbUseVariableSpeed;
    private SerializedProperty climbSpeed;
    private SerializedProperty climbStartSpeed;
    private SerializedProperty climbEndSpeed;
    private SerializedProperty climbAfterExtraDistance;

    private void OnEnable()
    {
        moveMode = serializedObject.FindProperty("moveMode");
        direction = serializedObject.FindProperty("direction");
        endPos = serializedObject.FindProperty("endPos");
        moveSpeed = serializedObject.FindProperty("moveSpeed");
        totalDistance = serializedObject.FindProperty("totalDistance");
        startSpeed = serializedObject.FindProperty("startSpeed");
        endSpeed = serializedObject.FindProperty("endSpeed");
        climbStage = serializedObject.FindProperty("climbStage");
        climbUseVariableSpeed = serializedObject.FindProperty("climbUseVariableSpeed");
        climbSpeed = serializedObject.FindProperty("climbSpeed");
        climbStartSpeed = serializedObject.FindProperty("climbStartSpeed");
        climbEndSpeed = serializedObject.FindProperty("climbEndSpeed");
        climbAfterExtraDistance = serializedObject.FindProperty("climbAfterExtraDistance");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("位移基础设置", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(moveMode, new GUIContent("位移模式"));
        MoveMode mode = (MoveMode)moveMode.enumValueIndex;

        EditorGUILayout.Space();
        switch (mode)
        {
            case MoveMode.FixedEndPos:
                EditorGUILayout.LabelField("固定终点模式.拖拽Scene终点手柄修改坐标", EditorStyles.helpBox);
                EditorGUILayout.PropertyField(endPos, new GUIContent("终点本地坐标"));
                break;

            case MoveMode.SpeedAndDistance:
                EditorGUILayout.LabelField("匀速直线.拖拽终点修改方向与总长度", EditorStyles.helpBox);
                EditorGUILayout.PropertyField(direction, new GUIContent("位移本地方向"));
                EditorGUILayout.PropertyField(totalDistance, new GUIContent("总距离"));
                EditorGUILayout.PropertyField(moveSpeed, new GUIContent("匀速移动速度"));
                break;

            case MoveMode.VariableSpeed:
                EditorGUILayout.LabelField("变速直线.拖拽终点修改方向与总长度", EditorStyles.helpBox);
                EditorGUILayout.PropertyField(direction, new GUIContent("位移本地方向"));
                EditorGUILayout.PropertyField(totalDistance, new GUIContent("总距离"));
                EditorGUILayout.PropertyField(startSpeed, new GUIContent("起始速度"));
                EditorGUILayout.PropertyField(endSpeed, new GUIContent("结束速度"));
                break;

            case MoveMode.ClimbOver:
                EditorGUILayout.LabelField("翻越位移.点位由跳跃扇形检测自动生成，不可拖拽编辑", EditorStyles.helpBox);
                EditorGUILayout.PropertyField(climbStage, new GUIContent("翻越阶段"));
                EditorGUILayout.PropertyField(climbUseVariableSpeed, new GUIContent("变速翻越"));
                if (climbUseVariableSpeed.boolValue)
                {
                    EditorGUILayout.PropertyField(climbStartSpeed, new GUIContent("翻越起始速度"));
                    EditorGUILayout.PropertyField(climbEndSpeed, new GUIContent("翻越结束速度"));
                }
                else
                {
                    EditorGUILayout.PropertyField(climbSpeed, new GUIContent("翻越匀速速度"));
                }
                EditorGUILayout.PropertyField(climbAfterExtraDistance, new GUIContent("翻越后额外前进距离"));
                break;
        }

        serializedObject.ApplyModifiedProperties();
        if (serializedObject.hasModifiedProperties)
        {
            EditorApplication.delayCall += () =>
            {
                SceneView.RepaintAll();
            };
        }
    }
}