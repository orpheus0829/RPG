using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// 相机机位参数片段
/// </summary>
public class CameraTimelineClip : PlayableAsset
{
    [Header("镜头数据盒")]
    public ActionSO data;

    [Header("镜头盒参数")]
    public Vector3 targetLocalPos;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var b = new CameraTimelineBehaviour();
        //b.targetLocalPos = targetLocalPos;
        b.clip = this;
        return ScriptPlayable<CameraTimelineBehaviour>.Create(graph, b);
    }
}