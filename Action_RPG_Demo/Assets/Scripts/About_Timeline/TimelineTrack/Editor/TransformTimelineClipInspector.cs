using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TransformTimelineClip))]
public class TransformTimelineClipInspector : Editor
{
    private SerializedProperty moveMode;
    private SerializedProperty direction;
    private SerializedProperty endPos;
    private SerializedProperty endEuler;
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

    private SerializedProperty circleCenterLocal;
    private SerializedProperty circleRadius;
    private SerializedProperty circleTotalAngle;
    private SerializedProperty circleVariableSpeed;
    private SerializedProperty circleStartAngSpeed;
    private SerializedProperty circleEndAngSpeed;
    private SerializedProperty circleConstantSpeed;

    private void OnEnable()
    {
        moveMode = serializedObject.FindProperty("moveMode");
        direction = serializedObject.FindProperty("direction");
        endPos = serializedObject.FindProperty("endPos");
        endEuler = serializedObject.FindProperty("endEuler");
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

        circleCenterLocal = serializedObject.FindProperty("circleCenterLocal");
        circleRadius = serializedObject.FindProperty("circleRadius");
        circleTotalAngle = serializedObject.FindProperty("circleTotalAngle");
        circleVariableSpeed = serializedObject.FindProperty("circleVariableSpeed");
        circleStartAngSpeed = serializedObject.FindProperty("circleStartAngSpeed");
        circleEndAngSpeed = serializedObject.FindProperty("circleEndAngSpeed");
        circleConstantSpeed = serializedObject.FindProperty("circleConstantSpeed");
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
                EditorGUILayout.PropertyField(endEuler, new GUIContent("终点本地欧拉旋转"));
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
            case MoveMode.CircleRotate:
                EditorGUILayout.LabelField("绕圈模式，蓝色球体为旋转中心，支持顺/逆时针切换", EditorStyles.helpBox);
                EditorGUILayout.PropertyField(circleCenterLocal, new GUIContent("旋转中心点(本地偏移)"));
                EditorGUILayout.PropertyField(circleRadius, new GUIContent("转圈半径"));
                EditorGUILayout.PropertyField(circleTotalAngle, new GUIContent("总旋转角度(360=完整一圈)"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("circleClockwise"), new GUIContent("顺时针旋转"));
                EditorGUILayout.PropertyField(circleVariableSpeed, new GUIContent("变速旋转"));
                if (circleVariableSpeed.boolValue)
                {
                    EditorGUILayout.PropertyField(circleStartAngSpeed, new GUIContent("起始角速度"));
                    EditorGUILayout.PropertyField(circleEndAngSpeed, new GUIContent("结束角速度"));
                }
                else
                {
                    EditorGUILayout.PropertyField(circleConstantSpeed, new GUIContent("匀速角速度"));
                }
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