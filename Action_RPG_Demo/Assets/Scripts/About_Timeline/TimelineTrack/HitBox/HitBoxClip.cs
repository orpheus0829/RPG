using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// ¹¥»÷ÅÐ¶¨²ÎÊý´æ´¢Æ¬¶Î
/// </summary>
public class HitBoxClip : PlayableAsset
{
    [Header("¹¥»÷ºÐ²ÎÊý")]
    public Vector3 boxOffset;
    public float boxRadius;
    public float damage;
    public float startTime;
    public float endTime;
    public float HitForce;
    public HitBoxShape hitBoxShape;
    public Vector3 hitBoxSize;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var b = new HitBoxBehaviour();
        b.clip = this;
        return ScriptPlayable<HitBoxBehaviour>.Create(graph, b);
    }
}
