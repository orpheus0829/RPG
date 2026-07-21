using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BuffSO))]
public class BuffSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("SuitableTags"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("BuffName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("BuffIntro"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("BuffIcon"));
        SerializedProperty isInstantProp = serializedObject.FindProperty("IsInstant");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("TargetValue"));
        EditorGUILayout.PropertyField(isInstantProp, new GUIContent("IsInstant 瞬发"));
        bool isInstant = isInstantProp.boolValue;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Val"), new GUIContent("数值 Val"));
        if (isInstant)
        {
            EditorGUILayout.HelpBox("瞬发Buff", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("持续性Buff", MessageType.Info);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Duration"), new GUIContent("持续时长 Duration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ActiveInterval"), new GUIContent("生效间隔 ActiveInterval"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}