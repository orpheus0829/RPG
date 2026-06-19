using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[InitializeOnLoad]
public static class HitBoxSceneDrawer
{
    private const float LineThickOffset = 0.025f;

    static HitBoxSceneDrawer()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        TimelineClip clip = TimelineEditor.selectedClip;
        if (clip == null)
        {
            return;
        }

        HitBoxClip asset = clip.asset as HitBoxClip;
        if (asset == null)
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

        Vector3 localOffset = asset.boxOffset;
        Vector3 worldCenter = rootTrans.TransformPoint(localOffset);

        Vector3 newWorldCenter = Handles.PositionHandle(worldCenter, rootTrans.rotation);
        Vector3 newLocalOffset = rootTrans.InverseTransformPoint(newWorldCenter);
        if (Vector3.Distance(localOffset, newLocalOffset) > 0.0001f)
        {
            Undo.RecordObject(asset, "ÐÞ¸ÄÅö×²ºÐÆ«ÒÆ");
            asset.boxOffset = newLocalOffset;
            EditorUtility.SetDirty(asset);
        }

        Matrix4x4 localMatrix = Matrix4x4.TRS(rootTrans.position, rootTrans.rotation, rootTrans.lossyScale);
        using (new Handles.DrawingScope(localMatrix))
        {
            Vector3 localCenter = asset.boxOffset;

            if (asset.hitBoxShape == HitBoxShape.Sphere)
            {
                Handles.color = new Color(1f, 0.2f, 0.2f, 0.6f);
                Handles.SphereHandleCap(0, localCenter, Quaternion.identity, asset.boxRadius, EventType.Repaint);
                Handles.SphereHandleCap(0, localCenter, Quaternion.identity, asset.boxRadius + LineThickOffset, EventType.Repaint);

                float oldRadius = asset.boxRadius;
                float newRadius = Handles.RadiusHandle(Quaternion.identity, localCenter, oldRadius);
                if (!Mathf.Approximately(oldRadius, newRadius))
                {
                    Undo.RecordObject(asset, "ÐÞ¸ÄÇòÌå°ë¾¶");
                    asset.boxRadius = Mathf.Max(0.001f, newRadius);
                    EditorUtility.SetDirty(asset);
                }

                Vector3 labelPos = localCenter + Vector3.up * (asset.boxRadius + 0.3f);
                Handles.Label(labelPos, $"¹¥»÷Çò °ë¾¶:{asset.boxRadius:F2}");
            }
            else
            {
                Handles.color = new Color(1f, 1f, 0.2f, 0.6f);
                Vector3 rawSize = asset.hitBoxSize;
                Handles.DrawWireCube(localCenter, rawSize);
                Handles.DrawWireCube(localCenter, rawSize + Vector3.one * LineThickOffset);
                Handles.DrawWireCube(localCenter, rawSize - Vector3.one * LineThickOffset * 0.5f);

                Vector3 oldSize = asset.hitBoxSize;
                Vector3 newSize = Handles.ScaleHandle(oldSize, localCenter, Quaternion.identity, 1f);
                if (Vector3.Distance(oldSize, newSize) > 0.0001f)
                {
                    Undo.RecordObject(asset, "ÐÞ¸ÄºÐ³ß´ç");
                    newSize.x = Mathf.Max(0.001f, newSize.x);
                    newSize.y = Mathf.Max(0.001f, newSize.y);
                    newSize.z = Mathf.Max(0.001f, newSize.z);
                    asset.hitBoxSize = newSize;
                    EditorUtility.SetDirty(asset);
                }

                Vector3 labelPos = localCenter + Vector3.up * (asset.hitBoxSize.y / 2f + 0.3f);
                Handles.Label(labelPos, $"¹¥»÷ºÐ ³ß´ç:{asset.hitBoxSize:F2}");
            }
        }
    }
}