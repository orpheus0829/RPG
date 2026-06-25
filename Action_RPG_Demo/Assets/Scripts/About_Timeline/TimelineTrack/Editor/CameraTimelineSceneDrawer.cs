using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[InitializeOnLoad]
public static class CameraTimelineSceneDrawer
{
    static CameraTimelineSceneDrawer()
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

        CameraTimelineClip camClip = clip.asset as CameraTimelineClip;
        if (camClip == null)
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

        ActionControl binding = director.GetGenericBinding(track) as ActionControl;
        if (binding == null)
        {
            binding = Object.FindFirstObjectByType<ActionControl>();
            if (binding == null)
            {
                return;
            }
        }
        Transform rootTrans = binding.transform;
        CameraPivot camPivot = CameraPivot.instance;
        if (camPivot == null)
        {
            return;
        }
        Quaternion originRot = Quaternion.Euler(camPivot.cacheNormalRotX, camPivot.cacheNormalRotY, 0);
        Vector3 dir = originRot * Vector3.back;
        Vector3 originWorldPos = rootTrans.position + dir * camPivot.cacheNormalDistance;
        originWorldPos.y += camPivot.cacheNormalHeight;
        Vector3 targetWorldPos = rootTrans.TransformPoint(camClip.cameraTargetLocalPos);
        Quaternion targetWorldRot = rootTrans.rotation * Quaternion.Euler(camClip.cameraTargetEuler);

        Handles.color = Color.blue;
        Handles.DrawLine(originWorldPos, targetWorldPos, 15f);

        float coneLength = 3f;
        float coneHalfAngle = 30f;
        Handles.color = new Color(1f, 1f, 1f, 0.4f);
        using (new Handles.DrawingScope(Matrix4x4.TRS(targetWorldPos, targetWorldRot, Vector3.one)))
        {
            Vector3 forwardTip = Vector3.forward * coneLength;
            int segmentCount = 16;
            float radStep = Mathf.PI * 2f / segmentCount;
            float radius = Mathf.Tan(Mathf.Deg2Rad * coneHalfAngle) * coneLength;
            Vector3 lastPoint = new Vector3(Mathf.Cos(0) * radius, Mathf.Sin(0) * radius, coneLength);
            for (int i = 1; i <= segmentCount; i++)
            {
                float rad = radStep * i;
                Vector3 curPoint = new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, coneLength);
                Handles.DrawLine(lastPoint, curPoint, 2f);
                lastPoint = curPoint;
            }
            Handles.DrawLine(Vector3.zero, forwardTip, 3f);
            int boneCount = 8;
            float boneStep = Mathf.PI * 2f / boneCount;
            for (int i = 0; i < boneCount; i++)
            {
                float rad = boneStep * i;
                Vector3 ringPoint = new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, coneLength);
                Handles.DrawLine(Vector3.zero, ringPoint, 1.5f);
            }
        }

        Vector3 newTargetWorld = Handles.PositionHandle(targetWorldPos, rootTrans.rotation);
        Vector3 newLocalOffset = rootTrans.InverseTransformPoint(newTargetWorld);
        if (Vector3.Distance(camClip.cameraTargetLocalPos, newLocalOffset) > 0.0001f)
        {
            Undo.RecordObject(camClip, "修改相机目标机位坐标");
            camClip.cameraTargetLocalPos = newLocalOffset;
            EditorUtility.SetDirty(camClip);
        }

        Quaternion newTargetRot = Handles.RotationHandle(targetWorldRot, targetWorldPos);
        Quaternion localRot = Quaternion.Inverse(rootTrans.rotation) * newTargetRot;
        Vector3 newEuler = localRot.eulerAngles;
        if (Quaternion.Angle(targetWorldRot, newTargetRot) > 0.01f)
        {
            Undo.RecordObject(camClip, "修改相机目标机位旋转");
            camClip.cameraTargetEuler = newEuler;
            EditorUtility.SetDirty(camClip);
        }

        string tip = camClip.useSurroundMode ? "圆弧终点预览（拖拽不改变圆弧半径）" : "直线终点机位（可拖位置+旋转）";
        Handles.Label(targetWorldPos + Vector3.up * 0.3f, tip);
        Handles.Label(originWorldPos + Vector3.up * 0.3f, "相机基准机位（圆弧起点方位）");

        sceneView.Repaint();
    }
}