using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// 攻击判定参数存储片段
/// </summary>
public class HitBoxClip : PlayableAsset
{
    [Header("动作数据盒")]
    public ActionSO data;

    [Header("攻击盒参数")]
    public Vector3 boxOffset;
    public float boxRadius;
    public float startTime;
    public float endTime;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var b = new HitBoxBehaviour();
        //b.boxOffset = boxOffset;
        //b.boxRadius = boxRadius;
        //b.startTime = startTime;
        //b.endTime = endTime;
        b.clip = this;
        return ScriptPlayable<HitBoxBehaviour>.Create(graph, b);
    }
}
