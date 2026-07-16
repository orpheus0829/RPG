using UnityEngine;
using UnityEngine.Playables;

public class HitBoxBehaviour : PlayableBehaviour
{
    public HitBoxClip clip;

    private bool isActive;
    private BaseActor ctrlCache;
    private float scanTimer;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        isActive = false;
        ctrlCache = null;
        scanTimer = 0f;
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        ctrlCache = playerData as BaseActor;
        if (ctrlCache == null)
        {
            return;
        }

        float curTime = (float)playable.GetTime();
        float deltaTime = (float)info.deltaTime;
        bool inRange = curTime >= clip.startTime && curTime <= clip.endTime;
        if (inRange != isActive)
        {
            isActive = inRange;
            scanTimer = 0f;

            if (isActive)
            {
                SetHitBoxDataToCtrl();
                if (!clip.useRepeatScan)
                {
                    DoHitScan();
                }
            }
            else
            {
                ClearHitBoxDataToCtrl();
            }
        }
        if (isActive && clip.useRepeatScan)
        {
            scanTimer += deltaTime;
            if (scanTimer >= clip.scanInterval)
            {
                DoHitScan();
                scanTimer = 0f;
            }
        }
    }
    private void SetHitBoxDataToCtrl()
    {
        if (ctrlCache is ActionControl playerCtrl)
        {
            playerCtrl.SetHitBoxData(
                clip.boxOffset,
                clip.boxRadius,
                clip.hitBoxSize,
                clip.hitBoxShape,
                clip.damage,
                clip.HitForce
            );
        }
        else if (ctrlCache is EnemyActionCtrl enemyCtrl)
        {
            enemyCtrl.SetHitBoxData(
                clip.boxOffset,
                clip.boxRadius,
                clip.hitBoxSize,
                clip.hitBoxShape,
                clip.damage,
                clip.HitForce
            );
        }
    }
    private void DoHitScan()
    {
        if (ctrlCache is ActionControl playerCtrl)
        {
            playerCtrl.DoSingleHitScan();
        }
        else if (ctrlCache is EnemyActionCtrl enemyCtrl)
        {
            enemyCtrl.DoSingleHitScan();
        }
    }
    private void ClearHitBoxDataToCtrl()
    {
        if (ctrlCache is ActionControl playerCtrl)
        {
            playerCtrl.ClearHitBoxData();
        }
        else if (ctrlCache is EnemyActionCtrl enemyCtrl)
        {
            enemyCtrl.ClearHitBoxData();
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (ctrlCache != null)
        {
            ClearHitBoxDataToCtrl();
        }
        isActive = false;
        scanTimer = 0f;
        ctrlCache = null;
    }
}