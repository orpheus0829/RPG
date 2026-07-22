using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Item_Data))]
public class ItemDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Display_In_Backpacks"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Drop"));
        SerializedProperty stackProp = serializedObject.FindProperty("Stackable");
        SerializedProperty widthProp = serializedObject.FindProperty("Width");
        SerializedProperty heightProp = serializedObject.FindProperty("Height");
        SerializedProperty kindProp = serializedObject.FindProperty("item_Kind");
        Item_Kind kind = (Item_Kind)kindProp.enumValueIndex;
        if (kind == Item_Kind.Weapon)
        {
            if (stackProp.boolValue)
            {
                stackProp.boolValue = false;
            }
            using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.PropertyField(stackProp);
            EditorGUILayout.PropertyField(widthProp);
            EditorGUILayout.PropertyField(heightProp);
        }
        else
        {
            EditorGUILayout.PropertyField(stackProp);
            bool isStack = stackProp.boolValue;
            if (isStack)
            {
                if (widthProp.intValue != 1) widthProp.intValue = 1;
                if (heightProp.intValue != 1) heightProp.intValue = 1;
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(widthProp);
                    EditorGUILayout.PropertyField(heightProp);
                }
            }
            else
            {
                EditorGUILayout.PropertyField(widthProp);
                EditorGUILayout.PropertyField(heightProp);
            }
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("item_name"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("item_id"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("PriceValue"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Introduction"));
        EditorGUILayout.PropertyField(kindProp);
        if (kind == Item_Kind.Consumable)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("buff"), new GUIContent("Ð¯´øBuff"));
        }
        serializedObject.ApplyModifiedProperties();
    }
}