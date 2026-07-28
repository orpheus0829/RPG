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
        SerializedProperty stackMaxProp = serializedObject.FindProperty("StackMax");
        SerializedProperty widthProp = serializedObject.FindProperty("Width");
        SerializedProperty heightProp = serializedObject.FindProperty("Height");
        SerializedProperty kindProp = serializedObject.FindProperty("item_Kind");

        Item_Kind kind = (Item_Kind)kindProp.enumValueIndex;
        if (kind == Item_Kind.Weapon)
        {
            stackProp.boolValue = false;
            stackMaxProp.intValue = 1;
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(stackProp);
                EditorGUILayout.PropertyField(stackMaxProp);
            }

            EditorGUILayout.PropertyField(widthProp);
            EditorGUILayout.PropertyField(heightProp);
        }
        else
        {
            EditorGUILayout.PropertyField(stackProp);
            bool isStack = stackProp.boolValue;

            if (isStack)
            {
                EditorGUILayout.PropertyField(stackMaxProp);
                widthProp.intValue = 1;
                heightProp.intValue = 1;
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(widthProp);
                    EditorGUILayout.PropertyField(heightProp);
                }
            }
            else
            {
                stackMaxProp.intValue = 1;
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(stackMaxProp);
                }

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
            EditorGUILayout.PropertyField(serializedObject.FindProperty("buff"), new GUIContent("携带Buff"));
        }
        if (kind == Item_Kind.Weapon)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("装备属性(百分比)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("EquipmentSlot"), new GUIContent("装备部位"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxHP"), new GUIContent("血量上限加成"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Defense"), new GUIContent("防御力加成"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MoveSpeed"), new GUIContent("移动速度加成"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Attack"), new GUIContent("攻击力加成"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("SpecialGain"), new GUIContent("特殊技能量获取加成"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("EndGain"), new GUIContent("终结技能量获取加成"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}