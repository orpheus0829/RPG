using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Item_Data))]
public class ItemDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Item_Data targetItem = (Item_Data)target;
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("Display_In_Backpacks"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Drop"));
        SerializedProperty stackProp = serializedObject.FindProperty("Stackable");
        EditorGUILayout.PropertyField(stackProp);
        bool isStack = stackProp.boolValue;
        if (isStack)
        {
            targetItem.Width = 1;
            targetItem.Height = 1;
        }
        else
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Width"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Height"));
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("item_name"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("item_id"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("PriceValue"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Introduction"));
        EditorGUILayout.Space();
        SerializedProperty kindProp = serializedObject.FindProperty("item_Kind");
        EditorGUILayout.PropertyField(kindProp);
        Item_Kind kind = (Item_Kind)kindProp.enumValueIndex;
        if (kind == Item_Kind.Consumable)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("buff"), new GUIContent("Ð¯´øBuff"));
        }
        serializedObject.ApplyModifiedProperties();
    }
}