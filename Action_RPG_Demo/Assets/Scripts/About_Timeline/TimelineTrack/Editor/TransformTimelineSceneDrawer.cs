using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[InitializeOnLoad]
public static class TransformTimelineSceneDrawer
{
    static TransformTimelineSceneDrawer()
    {
        SceneView.duringSceneGui += OnSceneDraw;
    }

    private static void OnSceneDraw(SceneView sceneView)
    {
        TimelineClip clip = TimelineEditor.selectedClip;
        if (clip == null)
        {
            return;
        }

        TransformTimelineClip transClip = clip.asset as TransformTimelineClip;
        if (transClip == null)
        {
            return;
        }

        PlayableDirector director = TimelineEditor.inspectedDirector;
        if (director == null)
        {
            return;
        }

        TrackAsset track = clip.parentTrack;
        if (track == null)
        {
            return;
        }

        Transform bindTrans = director.GetGenericBinding(track) as Transform;
        if (bindTrans == null)
        {
            return;
        }

        MoveMode curMode = transClip.moveMode;
        float clipDuration = (float)clip.duration;
        Vector3 worldStart = bindTrans.position;
        Handles.color = Color.white;
        bool speedZeroWarn = false;
        if (curMode == MoveMode.SpeedAndDistance && Mathf.Approximately(transClip.moveSpeed, 0f))
        {
            speedZeroWarn = true;
        }
        if (curMode == MoveMode.VariableSpeed)
        {
            if (Mathf.Approximately(transClip.startSpeed, 0f) && Mathf.Approximately(transClip.endSpeed, 0f))
            {
                speedZeroWarn = true;
            }
        }
        if (speedZeroWarn)
        {
            Handles.color = Color.red;
            Handles.Label(worldStart + Vector3.up * 1.2f, "速度全为0，位移不生效");
            Handles.color = Color.white;
        }

        if (curMode == MoveMode.FixedEndPos)
        {
            Vector3 worldEnd = bindTrans.TransformPoint(transClip.endPos);
            Handles.DrawLine(worldStart, worldEnd, 4f);

            Handles.SphereHandleCap(0, worldStart, Quaternion.identity, 0.15f, EventType.Repaint);
            Handles.Label(worldStart + Vector3.up * 0.3f, "位移起点");

            Vector3 newWorldEnd = Handles.PositionHandle(worldEnd, bindTrans.rotation);
            Vector3 newLocalEnd = bindTrans.InverseTransformPoint(newWorldEnd);
            if (Vector3.Distance(transClip.endPos, newLocalEnd) > 0.0001f)
            {
                Undo.RecordObject(transClip, "修改瞬移终点");
                transClip.endPos = newLocalEnd;
                EditorUtility.SetDirty(transClip);
            }

            Handles.SphereHandleCap(0, worldEnd, Quaternion.identity, 0.15f, EventType.Repaint);
            Handles.Label(worldEnd + Vector3.up * 0.3f, "瞬移终点（不影响速度）");
        }
        else if (curMode == MoveMode.SpeedAndDistance || curMode == MoveMode.VariableSpeed)
        {
            Vector3 rawLocalDir = transClip.direction;
            rawLocalDir.Normalize();
            Vector3 worldDir = bindTrans.TransformDirection(rawLocalDir);
            Vector3 worldEnd = worldStart + worldDir * transClip.totalDistance;

            Handles.DrawLine(worldStart, worldEnd, 4f);
            Handles.SphereHandleCap(0, worldStart, Quaternion.identity, 0.15f, EventType.Repaint);
            Handles.Label(worldStart + Vector3.up * 0.3f, "位移起点");

            Vector3 newWorldEnd = Handles.PositionHandle(worldEnd, bindTrans.rotation);
            Vector3 deltaWorld = newWorldEnd - worldStart;
            float dragDist = Mathf.Max(0.01f, deltaWorld.magnitude);
            Vector3 newLocalDir = bindTrans.InverseTransformDirection(deltaWorld);
            newLocalDir.Normalize();

            if (Vector3.Distance(worldEnd, newWorldEnd) > 0.0001f && clipDuration > 0.0001f)
            {
                Undo.RecordObject(transClip, "拖拽调整路线，计算速度");
                transClip.direction = newLocalDir;
                transClip.totalDistance = dragDist;

                if (curMode == MoveMode.SpeedAndDistance)
                {
                    transClip.moveSpeed = dragDist / clipDuration;
                }
                else
                {
                    float v0 = transClip.startSpeed;
                    float v1 = (2f * dragDist / clipDuration) - v0;
                    transClip.endSpeed = v1;
                }
                EditorUtility.SetDirty(transClip);
            }

            Handles.SphereHandleCap(0, worldEnd, Quaternion.identity, 0.15f, EventType.Repaint);
            string tip = curMode == MoveMode.SpeedAndDistance ? "匀速终点｜拖拽自动反推moveSpeed" : "变速终点｜锁定初速，自动反推末速度";
            Handles.Label(worldEnd + Vector3.up * 0.3f, tip);
        }
        else if (curMode == MoveMode.ClimbOver)
        {
            Handles.color = Color.yellow;
            Handles.Label(worldStart + Vector3.up * 0.6f, "翻越模式：由跳跃检测生成点位，不可拖拽");
        }

        sceneView.Repaint();
    }
}