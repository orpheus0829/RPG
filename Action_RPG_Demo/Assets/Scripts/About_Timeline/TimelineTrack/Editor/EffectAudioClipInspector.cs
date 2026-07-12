using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

[CustomEditor(typeof(EffectAudioClip))]
public class EffectAudioClipInspector : Editor
{
    SerializedProperty useRepeatSpawn;
    SerializedProperty spawnInterval;
    SerializedProperty sound;
    SerializedProperty effectPrefab;
    SerializedProperty spawnOffset;
    SerializedProperty spawnEuler;
    SerializedProperty spawnScale;

    private void OnEnable()
    {
        useRepeatSpawn = serializedObject.FindProperty("useRepeatSpawn");
        spawnInterval = serializedObject.FindProperty("spawnInterval");
        sound = serializedObject.FindProperty("sound");
        effectPrefab = serializedObject.FindProperty("effectPrefab");
        spawnOffset = serializedObject.FindProperty("spawnOffset");
        spawnEuler = serializedObject.FindProperty("spawnEuler");
        spawnScale = serializedObject.FindProperty("spawnScale");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("重复生成设置", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(useRepeatSpawn, new GUIContent("间隔重复生成"));
        if (useRepeatSpawn.boolValue)
        {
            EditorGUILayout.PropertyField(spawnInterval, new GUIContent("生成间隔(秒)"));
            EditorGUILayout.HelpBox("开启后片段全程每间隔生成一次特效+播放音效；关闭仅进入片段瞬间触发一次", MessageType.Info);
        }
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("特效音效参数", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(sound);
        EditorGUILayout.PropertyField(effectPrefab);
        EditorGUILayout.PropertyField(spawnOffset, new GUIContent("局部偏移"));
        EditorGUILayout.PropertyField(spawnEuler, new GUIContent("局部欧拉旋转"));
        EditorGUILayout.PropertyField(spawnScale, new GUIContent("局部缩放"));

        serializedObject.ApplyModifiedProperties();
    }
}