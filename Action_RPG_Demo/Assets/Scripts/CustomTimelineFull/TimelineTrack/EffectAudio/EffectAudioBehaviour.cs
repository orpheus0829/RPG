using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// 音效特效触发逻辑
/// </summary>
public class EffectAudioBehaviour : PlayableBehaviour
{
    public EffectAudioClip clip;
    //public AudioClip sound;
    //public GameObject effectPrefab;
    //public Vector3 spawnOffset;
    private bool fired;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        fired = false;
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (fired)
        {
            return;
        }
        ActionControl ctrl = playerData as ActionControl;
        if (ctrl == null)
        {
            return;
        }

        AudioClip snd = clip.data != null ? clip.data.soundClip : clip.sound;
        GameObject fx = clip.data != null ? clip.data.effectPrefab : clip.effectPrefab;
        Vector3 offset = clip.data != null ? clip.data.hitBoxOffset : clip.spawnOffset;

        ctrl.PlaySound(snd);
        Vector3 pos = ctrl.transform.TransformPoint(offset);
        ctrl.SpawnEffect(fx, pos, ctrl.transform.rotation);
        fired = true;
    }
}