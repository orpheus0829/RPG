using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[InitializeOnLoad]
public static class EffectAudioSceneDrawer
{
    private static GameObject _tempPreviewVfx;
    private static TimelineClip _lastSelectClip;

    static EffectAudioSceneDrawer()
    {
        SceneView.duringSceneGui += DrawEffectPreview;
        Selection.selectionChanged += ClearPreview;
        EditorApplication.playModeStateChanged += OnPlayStateChange;
    }

    private static void OnPlayStateChange(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            ClearPreview();
        }
    }

    private static void ClearPreview()
    {
        if (_tempPreviewVfx != null)
        {
            GameObject.DestroyImmediate(_tempPreviewVfx);
            _tempPreviewVfx = null;
        }
        _lastSelectClip = null;
    }

    private static void DrawEffectPreview(SceneView sceneView)
    {
        TimelineClip clip = TimelineEditor.selectedClip;
        if (clip != _lastSelectClip)
        {
            ClearPreview();
            _lastSelectClip = clip;
        }

        if (clip == null)
        {
            ClearPreview();
            return;
        }

        EffectAudioClip asset = clip.asset as EffectAudioClip;
        if (asset == null || asset.effectPrefab == null)
        {
            ClearPreview();
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
        }
        if (binding == null)
        {
            return;
        }

        Transform root = binding.transform;
        Vector3 localOff = asset.spawnOffset;
        Quaternion localRot = Quaternion.Euler(asset.spawnEuler);
        Vector3 localScl = asset.spawnScale;

        Vector3 worldPos = root.TransformPoint(localOff);
        Quaternion worldRot = root.rotation * localRot;
        if (_tempPreviewVfx == null)
        {
            _tempPreviewVfx = Object.Instantiate(asset.effectPrefab, worldPos, worldRot);
            _tempPreviewVfx.name = asset.effectPrefab.name + "_VfxPreview";
            _tempPreviewVfx.hideFlags = HideFlags.DontSaveInEditor | HideFlags.NotEditable | HideFlags.HideInHierarchy;

            ParticleSystem[] allParticles = _tempPreviewVfx.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in allParticles)
            {
                ps.Stop();
                ps.Clear();
            }
        }
        _tempPreviewVfx.transform.position = worldPos;
        _tempPreviewVfx.transform.rotation = worldRot;
        _tempPreviewVfx.transform.localScale = localScl;

        Vector3 newWorldPos = Handles.PositionHandle(worldPos, root.rotation);
        Vector3 newLocalOff = root.InverseTransformPoint(newWorldPos);
        if (Vector3.Distance(localOff, newLocalOff) > 0.0001f)
        {
            Undo.RecordObject(asset, "修改特效偏移");
            asset.spawnOffset = newLocalOff;
            EditorUtility.SetDirty(asset);
        }

        Quaternion newWorldRot = Handles.RotationHandle(worldRot, worldPos);
        Quaternion newLocalRot = Quaternion.Inverse(root.rotation) * newWorldRot;
        Vector3 newEuler = newLocalRot.eulerAngles;
        if (Quaternion.Angle(localRot, newLocalRot) > 0.01f)
        {
            Undo.RecordObject(asset, "修改特效旋转");
            asset.spawnEuler = newEuler;
            EditorUtility.SetDirty(asset);
        }

        Vector3 newLocalScale = Handles.ScaleHandle(localScl, worldPos, worldRot, 1f);
        newLocalScale = Vector3.Max(newLocalScale, Vector3.one * 0.001f);
        if (Vector3.Distance(localScl, newLocalScale) > 0.0001f)
        {
            Undo.RecordObject(asset, "修改特效缩放");
            asset.spawnScale = newLocalScale;
            EditorUtility.SetDirty(asset);
        }

        Handles.Label(worldPos + Vector3.up * 0.3f, $"特效预览:{asset.effectPrefab.name}");
    }
}