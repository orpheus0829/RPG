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

    public List<GameObject> VWait_for_Des = new List<GameObject>();

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

        AudioClip snd = null;
        GameObject fx = null;
        Vector3 offset = Vector3.zero;

        if (clip.data != null)
        {
            snd = clip.data.soundClip;
            fx = clip.data.effectPrefab;
            offset = clip.data.effectSpawnOffset;
        }
        else
        {
            snd = clip.sound;
            fx = clip.effectPrefab;
            offset = clip.spawnOffset;
        }
        Vector3 worldPos = ctrl.transform.TransformPoint(offset);
        if (snd != null)
        {
            ctrl.PlaySound(snd);
        }
        if (fx != null)
        {
            GameObject vfx = ctrl.SpawnEffect(fx, worldPos, ctrl.transform.rotation);
            VWait_for_Des.Add(vfx);
        }
        fired = true;
    }
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        foreach (var i in VWait_for_Des)
        {
            if (i != null)
            {
                GameObject.Destroy(i);
            }
        }
        VWait_for_Des.Clear();
    }
}