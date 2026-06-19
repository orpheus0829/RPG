using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// ¹¥»÷ÅÐ¶¨Ö¡Âß¼­Ö´ÐÐ
/// </summary>
public class HitBoxBehaviour : PlayableBehaviour
{
    public HitBoxClip clip;

    //public Vector3 boxOffset;
    //public float boxRadius;
    //public float startTime;
    //public float endTime;

    private bool isActive;
    private ActionControl cont;
    private float _currentDamage;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        _currentDamage = clip.damage;
    }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        ActionControl ctrl = playerData as ActionControl;
        cont = ctrl;
        if (ctrl == null)
        {
            return;
        }

        Vector3 offset = clip.boxOffset;
        float radius = clip.boxRadius;
        float start = clip.startTime;
        float end = clip.endTime;
        float force = clip.HitForce;
        ctrl.Hit_Force = force;

        ctrl.CurrentHitDamage = _currentDamage;
        ctrl.CurrentHitShape = clip.hitBoxShape;
        ctrl.CurrentHitBoxSize = clip.hitBoxSize;

        float t = (float)playable.GetTime() / (float)playable.GetDuration();
        bool inRange = t >= start && t <= end;
        if (inRange != isActive)
        {
            isActive = inRange;
            if (isActive)
            {
                Vector3 boxSize = clip.hitBoxSize;
                ctrl.OpenHitBox(offset, radius, boxSize);
            }
            else
            {
                ctrl.CloseHitBox();
            }
        }
    }
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (cont != null)
        {
            cont.CloseHitBox();
            cont.CurrentHitDamage = 0;
            cont.CurrentHitShape = HitBoxShape.Sphere;
            cont.CurrentHitBoxSize = Vector3.zero;
        }
        isActive = false;
        cont = null;
    }
}