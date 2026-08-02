using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
public class EffectAudioBehaviour : PlayableBehaviour
{
    public EffectAudioClip clip;
    private bool fired;
    private float spawnTimer;
    public List<GameObject> VWait_for_Des = new List<GameObject>();
    private BaseActor _cacheCtrl;

    public override void OnGraphStart(Playable playable)
    {
        fired = false;
        spawnTimer = 0f;
        VWait_for_Des.Clear();
        _cacheCtrl = null;
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        fired = false;
        spawnTimer = 0f;
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        _cacheCtrl = playerData as BaseActor;
        if (_cacheCtrl == null)
        {
            return;
        }

        float curTime = (float)playable.GetTime();
        float duration = (float)playable.GetDuration();
        float deltaTime = (float)info.deltaTime;
        bool inTimeRange = curTime >= 0 && curTime <= duration;
        if (inTimeRange != fired)
        {
            fired = inTimeRange;
            spawnTimer = 0f;

            if (inTimeRange)
            {
                if (!clip.useRepeatSpawn)
                {
                    SpawnAudioAndVfx();
                }
            }
        }
        if (inTimeRange && clip.useRepeatSpawn)
        {
            spawnTimer += deltaTime;
            if (spawnTimer >= clip.spawnInterval)
            {
                SpawnAudioAndVfx();
                spawnTimer = 0f;
            }
        }
    }
    private void SpawnAudioAndVfx()
    {
        AudioClip snd = clip.sound;
        GameObject fx = clip.effectPrefab;
        Vector3 offset = clip.spawnOffset;
        Quaternion localRot = Quaternion.Euler(clip.spawnEuler);
        Vector3 localScl = clip.spawnScale;

        Vector3 worldPos = _cacheCtrl.transform.TransformPoint(offset);
        Quaternion worldRot = _cacheCtrl.transform.rotation * localRot;
        if (snd != null)
        {
            if (_cacheCtrl is ActionControl playerCtrl)
            {
                playerCtrl.PlaySound(snd);
            }
            else if (_cacheCtrl is EnemyActionCtrl enemyCtrl)
            {
                enemyCtrl.PlaySound(snd);
            }
        }
        if (fx != null)
        {
            GameObject vfx = null;
            if (_cacheCtrl is ActionControl playerCtrl)
            {
                vfx = playerCtrl.SpawnEffect(fx, worldPos, worldRot);
            }
            else if (_cacheCtrl is EnemyActionCtrl enemyCtrl)
            {
                vfx = enemyCtrl.SpawnEffect(fx, worldPos, worldRot);
            }

            if (vfx != null)
            {
                vfx.transform.localScale = localScl;
                VWait_for_Des.Add(vfx);
            }
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        RecycleAllVfx();
        fired = false;
        spawnTimer = 0f;
        _cacheCtrl = null;
    }

    public override void OnGraphStop(Playable playable)
    {
        RecycleAllVfx();
        _cacheCtrl = null;
    }

    private void RecycleAllVfx()
    {
        foreach (var obj in VWait_for_Des)
        {
            if (obj != null){
                ObjectPoolMgr.instance.PushObj(obj);
            }
        }
        VWait_for_Des.Clear();
    }
}