using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraTimelineClip))]
public class CameraTimelineClipInspector : Editor
{
    private SerializedProperty cameraMoveMode;

    // 归位
    private SerializedProperty resetSubMode;
    private SerializedProperty resetLerpFactor;
    private SerializedProperty resetUseVariableSpeed;
    private SerializedProperty resetStartSpeed;
    private SerializedProperty resetEndSpeed;

    // 环绕
    private SerializedProperty useSurroundMode;
    private SerializedProperty surroundRadius;
    private SerializedProperty surroundTotalAngle;
    private SerializedProperty surroundFixedHeight;

    // 直线通用
    private SerializedProperty cameraTargetLocalPos;
    private SerializedProperty cameraTargetEuler;
    private SerializedProperty lockLookAtPlayer;
    private SerializedProperty lockMoveToCharacterForward;
    private SerializedProperty smoothLerpFactor;
    private SerializedProperty useVariableSpeed;
    private SerializedProperty startSpeed;
    private SerializedProperty endSpeed;
    private SerializedProperty useLastFrameAsOrigin;

    private void OnEnable()
    {
        cameraMoveMode = serializedObject.FindProperty("cameraMoveMode");

        resetSubMode = serializedObject.FindProperty("resetSubMode");
        resetLerpFactor = serializedObject.FindProperty("resetLerpFactor");
        resetUseVariableSpeed = serializedObject.FindProperty("resetUseVariableSpeed");
        resetStartSpeed = serializedObject.FindProperty("resetStartSpeed");
        resetEndSpeed = serializedObject.FindProperty("resetEndSpeed");

        useSurroundMode = serializedObject.FindProperty("useSurroundMode");
        surroundRadius = serializedObject.FindProperty("surroundRadius");
        surroundTotalAngle = serializedObject.FindProperty("surroundTotalAngle");
        surroundFixedHeight = serializedObject.FindProperty("surroundFixedHeight");

        cameraTargetLocalPos = serializedObject.FindProperty("cameraTargetLocalPos");
        cameraTargetEuler = serializedObject.FindProperty("cameraTargetEuler");
        lockLookAtPlayer = serializedObject.FindProperty("lockLookAtPlayer");
        lockMoveToCharacterForward = serializedObject.FindProperty("lockMoveToCharacterForward");
        smoothLerpFactor = serializedObject.FindProperty("smoothLerpFactor");
        useVariableSpeed = serializedObject.FindProperty("useVariableSpeed");
        startSpeed = serializedObject.FindProperty("startSpeed");
        endSpeed = serializedObject.FindProperty("endSpeed");
        useLastFrameAsOrigin = serializedObject.FindProperty("useLastFrameAsOrigin");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        CamMoveMode mode = (CamMoveMode)cameraMoveMode.enumValueIndex;

        EditorGUILayout.LabelField("镜头基础设置", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(cameraMoveMode, new GUIContent("镜头总移动模式"));
        if (mode == CamMoveMode.ResetOrigin)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("以片段开播相机位置为目标", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(resetSubMode, new GUIContent("归位运动方式"));
            ResetCamSubMode sub = (ResetCamSubMode)resetSubMode.enumValueIndex;

            if (sub == ResetCamSubMode.SmoothLerp)
            {
                EditorGUILayout.PropertyField(resetLerpFactor, new GUIContent("归位平滑系数"));
                EditorGUILayout.PropertyField(resetUseVariableSpeed, new GUIContent("归位启用变速"));
                if (resetUseVariableSpeed.boolValue)
                {
                    EditorGUILayout.PropertyField(resetStartSpeed, new GUIContent("归位起点速度"));
                    EditorGUILayout.PropertyField(resetEndSpeed, new GUIContent("归位终点速度"));
                }
            }

            EditorGUILayout.PropertyField(lockLookAtPlayer, new GUIContent("片段内持续看向角色"));
            serializedObject.ApplyModifiedProperties();
            return;
        }

        // ========== 直线/环绕共用面板 ==========
        EditorGUILayout.PropertyField(useLastFrameAsOrigin, new GUIContent("继承上一片段终点为本段起点"));
        EditorGUILayout.PropertyField(useSurroundMode, new GUIContent("开启固定半径环绕"));

        if (useSurroundMode.boolValue)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("环绕参数", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(surroundRadius, new GUIContent("固定环绕半径（仅面板修改）"));
            EditorGUILayout.PropertyField(surroundTotalAngle, new GUIContent("总环绕角度(180=右半圈绕脑后)"));
            EditorGUILayout.PropertyField(surroundFixedHeight, new GUIContent("环绕固定高度"));
            EditorGUILayout.HelpBox("环绕规则：\n1.圆弧半径固定为此处数值，拖拽目标坐标不改变圆弧大小\n2.全程高度不变，无升降\n3.圆弧起点方位由片段初始相机位置决定", MessageType.Info);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("目标机位（直线/环绕生效）", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(cameraTargetLocalPos, new GUIContent("目标坐标"));
        EditorGUILayout.PropertyField(cameraTargetEuler, new GUIContent("目标欧拉旋转"));
        EditorGUILayout.PropertyField(lockLookAtPlayer, new GUIContent("片段内持续看向角色"));
        EditorGUILayout.PropertyField(lockMoveToCharacterForward, new GUIContent("锁定移动基准为角色正面(滑铲开启)"));

        if (mode == CamMoveMode.SmoothLerp)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("平滑/变速设置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(smoothLerpFactor, new GUIContent("平滑插值系数"));
            EditorGUILayout.PropertyField(useVariableSpeed, new GUIContent("启用变速插值"));
            if (useVariableSpeed.boolValue == true)
            {
                EditorGUILayout.PropertyField(startSpeed, new GUIContent("起点速度(0=静止起步)"));
                EditorGUILayout.PropertyField(endSpeed, new GUIContent("终点速度"));
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}