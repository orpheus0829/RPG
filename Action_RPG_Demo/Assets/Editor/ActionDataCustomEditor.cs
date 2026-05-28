using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

/// <summary>
/// 自定义动作配置面板
/// 切换枚举自动显隐对应参数，场景攻击范围预览，自动保存配置
/// </summary>
[CustomEditor(typeof(ActionSO))]
public class ActionDataCustomEditor : Editor
{
    private bool camFold;
    private bool effectFold;

    public override void OnInspectorGUI()
    {
        ActionSO data = target as ActionSO;
        if (data == null) return;

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("通用基础信息", EditorStyles.boldLabel);
        data.actionID = EditorGUILayout.IntField("动作编号", data.actionID);
        data.actionName = EditorGUILayout.TextField("动作名称", data.actionName);
        data.timeline = EditorGUILayout.ObjectField("动作时间线", data.timeline, typeof(TimelineAsset), false) as TimelineAsset;
        data.actionType = (ActionType)EditorGUILayout.EnumPopup("动作类型", data.actionType);

        EditorGUILayout.Space(10);

        if (data.actionType == ActionType.Attack)
        {
            EditorGUILayout.LabelField("攻击专属参数", EditorStyles.boldLabel);
            data.damageValue = EditorGUILayout.FloatField("基础伤害", data.damageValue);
            data.hitBoxOffset = EditorGUILayout.Vector3Field("判定盒偏移", data.hitBoxOffset);
            data.hitBoxRadius = EditorGUILayout.FloatField("判定盒半径", data.hitBoxRadius);
            data.hitStartTime = EditorGUILayout.FloatField("判定开始时间", data.hitStartTime);
            data.hitEndTime = EditorGUILayout.FloatField("判定结束时间", data.hitEndTime);
            EditorGUILayout.Space(8);
        }

        data.nextAction = EditorGUILayout.ObjectField("无突发时-->自动跳转", data.nextAction, typeof(ActionSO), false) as ActionSO;
        EditorGUILayout.Space(10);

        camFold = EditorGUILayout.Foldout(camFold, "镜头演出参数", true);
        if (camFold)
        {
            EditorGUI.indentLevel++;
            data.cameraTargetLocalPos = EditorGUILayout.Vector3Field("目标机位坐标", data.cameraTargetLocalPos);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(8);

        effectFold = EditorGUILayout.Foldout(effectFold, "特效音效参数", true);
        if (effectFold)
        {
            EditorGUI.indentLevel++;
            data.effectPrefab = EditorGUILayout.ObjectField("动作特效", data.effectPrefab, typeof(GameObject), false) as GameObject;
            data.soundClip = EditorGUILayout.ObjectField("动作音效", data.soundClip, typeof(AudioClip), false) as AudioClip;
            data.effectTriggerTime = EditorGUILayout.FloatField("触发时间点", data.effectTriggerTime);
            EditorGUI.indentLevel--;
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
        }
    }

    private void OnSceneGUI()
    {
        ActionSO data = target as ActionSO;
        if (data.actionType == ActionType.Attack)
        {
            Handles.color = Color.red;
            Handles.SphereHandleCap(0, data.hitBoxOffset, Quaternion.identity, data.hitBoxRadius, EventType.Repaint);
        }
    }
}