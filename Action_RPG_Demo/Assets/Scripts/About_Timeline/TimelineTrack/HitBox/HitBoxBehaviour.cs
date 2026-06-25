using UnityEngine;
using UnityEngine.Playables;

public class HitBoxBehaviour : PlayableBehaviour
{
    public HitBoxClip clip;

    private bool isActive;
    private ActionControl ctrlCache;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        isActive = false;
        ctrlCache = null;
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        ActionControl ctrl = playerData as ActionControl;
        ctrlCache = ctrl;
        if (ctrl == null)
            return;

        float curTime = (float)playable.GetTime();
        float duration = (float)playable.GetDuration();
        bool inRange = /*duration > 0.0001f && */curTime >= clip.startTime && curTime <= clip.endTime;

        if (inRange != isActive)
        {
            isActive = inRange;
            if (isActive)
            {
                ctrl.SetHitBoxData(
                    clip.boxOffset,
                    clip.boxRadius,
                    clip.hitBoxSize,
                    clip.hitBoxShape,
                    clip.damage,
                    clip.HitForce
                );
                ctrl.DoSingleHitScan();
            }
            else
            {
                ctrl.ClearHitBoxData();
            }
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (ctrlCache != null)
        {
            ctrlCache.ClearHitBoxData();
        }
        isActive = false;
        ctrlCache = null;
    }
}