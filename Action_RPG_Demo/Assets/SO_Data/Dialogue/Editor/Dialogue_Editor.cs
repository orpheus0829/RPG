using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Dialogue_SO))]
public class Dialogue_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Dialogue_SO so = target as Dialogue_SO;

        // 基础信息
        EditorGUILayout.PropertyField(serializedObject.FindProperty("dialogueMode"));
        EditorGUILayout.Space(10);

        // 发言人
        EditorGUILayout.PropertyField(serializedObject.FindProperty("SpeakerName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("SpeakerId"));
        EditorGUILayout.Space(10);

        // 台词
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Single_Dialogue"));
        EditorGUILayout.Space(10);

        // 演出
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Cut_Show"));
        EditorGUILayout.Space(10);

        // 跳转方式
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ContinueWay"));
        EditorGUILayout.Space(10);

        switch (so.ContinueWay)
        {
            case WayToNextDialogue.Next:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("nextDialogue"));
                break;

            case WayToNextDialogue.Choice:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("choiceDialogues"));
                break;

            case WayToNextDialogue.NoNext:
                EditorGUILayout.HelpBox("对话结束，无后续内容", MessageType.Info);
                break;
        }

        // 应用修改
        serializedObject.ApplyModifiedProperties();
    }
}