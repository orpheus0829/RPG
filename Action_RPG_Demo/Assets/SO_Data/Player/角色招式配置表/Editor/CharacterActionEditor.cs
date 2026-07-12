using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterActionSO))]
public class CharacterActionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Roledata"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("RoleParry"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Idle"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("AfkIdle"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("BornSet"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Born"));
        EditorGUILayout.Space(8);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("WalkStart"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Walk"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("WalkEnd"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Dodge"));
        EditorGUILayout.Space(8);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("RunDodge"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Run"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("RunEnd"));
        EditorGUILayout.Space(8);

        SerializedProperty atkList = serializedObject.FindProperty("AtkList");
        EditorGUILayout.LabelField("攻击列表", EditorStyles.boldLabel);

        atkList.isExpanded = EditorGUILayout.Foldout(atkList.isExpanded, $"数量: {atkList.arraySize}", true);
        if (atkList.isExpanded)
        {
            EditorGUI.indentLevel++;

            for (int i = 0; i < atkList.arraySize; i++)
            {
                SerializedProperty item = atkList.GetArrayElementAtIndex(i);
                item.isExpanded = EditorGUILayout.Foldout(item.isExpanded, $"攻击段 {i + 1}", true);

                if (item.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(item.FindPropertyRelative("ATK"));

                    SerializedProperty hasVariant = item.FindPropertyRelative("HasVariantATK");
                    EditorGUILayout.PropertyField(hasVariant);
                    if (hasVariant.boolValue)
                    {
                        EditorGUILayout.LabelField("重击", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(item.FindPropertyRelative("PerfectATK"));
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.Space(4);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("添加", GUILayout.Width(60)))
            {
                atkList.arraySize++;
            }
            if (GUILayout.Button("删除最后一项", GUILayout.Width(100)) && atkList.arraySize > 0)
            {
                atkList.arraySize--;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("RushAttack"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Block"));
        EditorGUILayout.Space(8);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("RelatedFullE"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("FullE"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("RelatedUnfilledE"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("UnfilledE"));
        EditorGUILayout.Space(8);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("EndSkill"));

        EditorGUILayout.PropertyField(serializedObject.FindProperty("Jump"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("PreVault"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("AftVault"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Slide"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("GetHit"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Death"));

        EditorGUILayout.PropertyField(serializedObject.FindProperty("SwitchIn"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("SwitchOut"));

        serializedObject.ApplyModifiedProperties();
    }
}