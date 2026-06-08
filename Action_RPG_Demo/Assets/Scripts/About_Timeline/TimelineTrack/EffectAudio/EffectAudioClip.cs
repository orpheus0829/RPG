using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// 音效特效参数片段
/// </summary>
public class EffectAudioClip : PlayableAsset
{
    [Header("特效音效数据盒")]
    public ActionSO data;

    [Header("特效音效盒参数")]
    public AudioClip sound;
    public GameObject effectPrefab;
    public Vector3 spawnOffset;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var b = new EffectAudioBehaviour();
        b.clip = this;
        return ScriptPlayable<EffectAudioBehaviour>.Create(graph, b);
    }
}