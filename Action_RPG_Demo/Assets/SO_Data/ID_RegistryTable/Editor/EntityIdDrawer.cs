using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EntityIdAttribute))]
public class EntityIdDrawer : PropertyDrawer
{
    private const string RegistryAssetPath = "Assets/SO_Data/ID_RegistryTable/IDRegistry.asset";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        IdRegistrySO registry = AssetDatabase.LoadAssetAtPath<IdRegistrySO>(RegistryAssetPath);

        if (registry == null || registry.AllID.Count == 0)
        {
            EditorGUI.LabelField(position, label, new GUIContent("无ID注册表或列表为空"));
            return;
        }
        int selectIndex = registry.AllID.IndexOf(property.stringValue);
        if (selectIndex < 0) selectIndex = 0;

        int newIndex = EditorGUI.Popup(position, label.text, selectIndex, registry.AllID.ToArray());
        property.stringValue = registry.AllID[newIndex];
    }
}