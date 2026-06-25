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
        EditorGUILayout.PropertyField(serializedObject.FindProperty("dialogueMode"));
        EditorGUILayout.Space(10);
        if (so.dialogueMode == DialogueMode.Quest)
        {
            EditorGUILayout.LabelField("任务基础信息", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Quest_Title"), new GUIContent("任务标题"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Quest_Description"), new GUIContent("任务描述"));
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("SpeakerName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("SpeakerId"));
        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Single_Dialogue"));
        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Cut_Show"));
        EditorGUILayout.Space(10);
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
        serializedObject.ApplyModifiedProperties();
    }
}