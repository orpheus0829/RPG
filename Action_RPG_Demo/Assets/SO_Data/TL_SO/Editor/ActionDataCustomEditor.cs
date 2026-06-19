using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

[CustomEditor(typeof(ActionSO))]
public class ActionDataCustomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ActionSO data = target as ActionSO;
        if (data == null)
        {
            return;
        }

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("通用基础信息", EditorStyles.boldLabel);
        data.actionID = EditorGUILayout.IntField("动作编号", data.actionID);
        data.actionName = EditorGUILayout.TextField("动作名称", data.actionName);
        data.timeline = EditorGUILayout.ObjectField("动作时间线", data.timeline, typeof(TimelineAsset), false) as TimelineAsset;
        data.actionType = (ActionType)EditorGUILayout.EnumPopup("动作类型", data.actionType);

        EditorGUILayout.Space(10);

        data.nextAction = EditorGUILayout.ObjectField("无突发时-->自动跳转", data.nextAction, typeof(ActionSO), false) as ActionSO;
        EditorGUILayout.Space(10);

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
        }
    }
}