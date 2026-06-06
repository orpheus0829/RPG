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
    private bool moveFold;

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

        if (data.actionType == ActionType.Attack)
        {
            EditorGUILayout.LabelField("攻击专属参数", EditorStyles.boldLabel);
            data.hitBoxShape = (HitBoxShape)EditorGUILayout.EnumPopup("碰撞形状", data.hitBoxShape);
            data.damageValue = EditorGUILayout.FloatField("基础伤害", data.damageValue);
            data.hitBoxOffset = EditorGUILayout.Vector3Field("判定盒偏移", data.hitBoxOffset);
            if (data.hitBoxShape == HitBoxShape.Sphere)
            {
                data.hitBoxRadius = EditorGUILayout.FloatField("球体半径", data.hitBoxRadius);
            }
            else
            {
                data.hitBoxSize = EditorGUILayout.Vector3Field("方块尺寸(长宽高)", data.hitBoxSize);
            }

            data.hitStartTime = EditorGUILayout.FloatField("判定开始时间", data.hitStartTime);
            data.hitEndTime = EditorGUILayout.FloatField("判定结束时间", data.hitEndTime);
            data.hitForce = EditorGUILayout.FloatField("打击力度", data.hitForce);
            EditorGUILayout.Space(8);
        }

        data.nextAction = EditorGUILayout.ObjectField("无突发时-->自动跳转", data.nextAction, typeof(ActionSO), false) as ActionSO;
        EditorGUILayout.Space(10);

        camFold = EditorGUILayout.Foldout(camFold, "镜头演出参数", true);
        if (camFold)
        {
            EditorGUI.indentLevel++;
            data.cameraMoveMode = (MoveMode)EditorGUILayout.EnumPopup("镜头移动模式", data.cameraMoveMode);
            data.cameraDirection = EditorGUILayout.Vector3Field("镜头移动方向", data.cameraDirection);
            data.cameraTotalDistance = EditorGUILayout.FloatField("镜头总距离", data.cameraTotalDistance);

            switch (data.cameraMoveMode)
            {
                case MoveMode.FixedEndPos:
                    data.cameraTargetLocalPos = EditorGUILayout.Vector3Field("固定目标点", data.cameraTargetLocalPos);
                    break;
                case MoveMode.SpeedAndDistance:
                    data.cameraMoveSpeed = EditorGUILayout.FloatField("移动速度", data.cameraMoveSpeed);
                    break;
                case MoveMode.VariableSpeed:
                    data.cameraStartSpeed = EditorGUILayout.FloatField("起始速度", data.cameraStartSpeed);
                    data.cameraEndSpeed = EditorGUILayout.FloatField("结束速度", data.cameraEndSpeed);
                    break;
            }
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
            data.effectSpawnOffset = EditorGUILayout.Vector3Field("特效生成偏移（局部）", data.effectSpawnOffset);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(8);
        moveFold = EditorGUILayout.Foldout(moveFold, "位移参数", true);
        if (moveFold)
        {
            EditorGUI.indentLevel++;
            data.moveMode = (MoveMode)EditorGUILayout.EnumPopup("位移模式", data.moveMode);

            if (data.moveMode != MoveMode.ClimbOver)
            {
                data.direction = EditorGUILayout.Vector3Field("移动方向", data.direction);
            }

            switch (data.moveMode)
            {
                case MoveMode.FixedEndPos:
                    data.endPos = EditorGUILayout.Vector3Field("目标终点坐标", data.endPos);
                    data.totalDistance = EditorGUILayout.FloatField("总移动距离", data.totalDistance);
                    break;

                case MoveMode.SpeedAndDistance:
                    data.moveSpeed = EditorGUILayout.FloatField("移动速度", data.moveSpeed);
                    data.totalDistance = EditorGUILayout.FloatField("总移动距离", data.totalDistance);
                    break;

                case MoveMode.VariableSpeed:
                    data.startSpeed = EditorGUILayout.FloatField("起始速度", data.startSpeed);
                    data.endSpeed = EditorGUILayout.FloatField("结束速度", data.endSpeed);
                    data.totalDistance = EditorGUILayout.FloatField("总移动距离", data.totalDistance);
                    break;

                case MoveMode.ClimbOver:
                    GUI.enabled = false;
                    EditorGUILayout.FloatField("总移动距离(禁用)", data.totalDistance);
                    GUI.enabled = true;

                    data.climbStage = (ClimbStage)EditorGUILayout.EnumPopup("翻越阶段", data.climbStage);
                    data.climbUseVariableSpeed = EditorGUILayout.Toggle("使用变速", data.climbUseVariableSpeed);

                    if (data.climbStage == ClimbStage.BeforeClimb)
                    {
                        if (data.climbUseVariableSpeed)
                        {
                            data.climbStartSpeed = EditorGUILayout.FloatField("起始速度", data.climbStartSpeed);
                            data.climbEndSpeed = EditorGUILayout.FloatField("结束速度", data.climbEndSpeed);
                        }
                        else
                        {
                            data.climbSpeed = EditorGUILayout.FloatField("匀速", data.climbSpeed);
                        }
                    }
                    else if (data.climbStage == ClimbStage.AfterClimb)
                    {
                        if (data.climbUseVariableSpeed)
                        {
                            data.climbStartSpeed = EditorGUILayout.FloatField("起始速度", data.climbStartSpeed);
                            data.climbEndSpeed = EditorGUILayout.FloatField("结束速度", data.climbEndSpeed);
                        }
                        else
                        {
                            data.climbSpeed = EditorGUILayout.FloatField("匀速", data.climbSpeed);
                        }
                        data.climbAfterExtraDistance = EditorGUILayout.FloatField("翻越后前进距离", data.climbAfterExtraDistance);
                    }
                    break;

            }
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
        if (data == null || data.actionType != ActionType.Attack)
        {
            return;
        }
        Player player = Object.FindFirstObjectByType<Player>();
        if (player == null)
        {
            return;
        }
        Transform root = player.transform;
        Undo.RecordObject(data, "修改攻击碰撞框");
        Vector3 worldCenter = root.TransformPoint(data.hitBoxOffset);

        Event evt = Event.current;

        worldCenter = Handles.PositionHandle(worldCenter, Quaternion.identity);
        data.hitBoxOffset = root.InverseTransformPoint(worldCenter);

        if (data.hitBoxShape == HitBoxShape.Sphere)
        {
            Handles.color = Color.red;
            Handles.SphereHandleCap(0, worldCenter, Quaternion.identity, data.hitBoxRadius, EventType.Repaint);
            data.hitBoxRadius = Handles.RadiusHandle(Quaternion.identity, worldCenter, data.hitBoxRadius);
        }
        else
        {
            Handles.color = Color.yellow;
            Handles.DrawWireCube(worldCenter, data.hitBoxSize);
            data.hitBoxSize = Handles.ScaleHandle(data.hitBoxSize, worldCenter, Quaternion.identity, 1f);
        }
        SceneView.RepaintAll();
    }
}