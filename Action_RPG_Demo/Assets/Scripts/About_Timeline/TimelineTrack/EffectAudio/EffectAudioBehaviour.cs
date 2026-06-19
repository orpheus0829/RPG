using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// “Ù–ßÃÿ–ß¥•∑¢¬ﬂº≠£®–ﬁ∏¥≤–¡Ù—≠ª∑≤•∑≈£©
/// </summary>
public class EffectAudioBehaviour : PlayableBehaviour
{
    public EffectAudioClip clip;
    private bool fired;
    public List<GameObject> VWait_for_Des = new List<GameObject>();
    private AudioSource _cacheAudioSource;

    public override void OnGraphStart(Playable playable)
    {
        fired = false;
        VWait_for_Des.Clear();
    }

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
        _cacheAudioSource = ctrl.audioSource;

        AudioClip snd = clip.sound;
        GameObject fx = clip.effectPrefab;
        Vector3 offset = clip.spawnOffset;
        Quaternion localRot = Quaternion.Euler(clip.spawnEuler);
        Vector3 localScl = clip.spawnScale;

        Vector3 worldPos = ctrl.transform.TransformPoint(offset);
        Quaternion worldRot = ctrl.transform.rotation * localRot;
        if (snd != null && _cacheAudioSource != null)
        {
            _cacheAudioSource.clip = snd;
            _cacheAudioSource.Play();
        }

        if (fx != null)
        {
            GameObject vfx = ctrl.SpawnEffect(fx, worldPos, worldRot);
            vfx.transform.localScale = localScl;
            VWait_for_Des.Add(vfx);
        }
        fired = true;
    }
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (_cacheAudioSource != null)
        {
            _cacheAudioSource.Stop();
        }
        foreach (var obj in VWait_for_Des)
        {
            if (obj != null)
                GameObject.Destroy(obj);
        }
        VWait_for_Des.Clear();
        fired = false;
    }
    public override void OnGraphStop(Playable playable)
    {
        if (_cacheAudioSource != null)
        {
            _cacheAudioSource.Stop();
        }

        foreach (var obj in VWait_for_Des)
        {
            if (obj != null)
                GameObject.Destroy(obj);
        }
        VWait_for_Des.Clear();
    }
}