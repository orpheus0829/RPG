using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
public class HitBoxClip : PlayableAsset
{
    [Header("攻击盒参数")]
    public Vector3 boxOffset;
    public float boxRadius;
    public float damage;
    public float startTime;
    public float endTime;
    public float HitForce;
    public HitBoxShape hitBoxShape;
    public Vector3 hitBoxSize;

    [Header("重复判定设置")]
    public bool useRepeatScan;
    public float scanInterval = 0.1f;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var b = new HitBoxBehaviour();
        b.clip = this;
        return ScriptPlayable<HitBoxBehaviour>.Create(graph, b);
    }
}
