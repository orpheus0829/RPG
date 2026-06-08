using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// 攻击判定帧逻辑执行
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

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {

    }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        ActionControl ctrl = playerData as ActionControl;
        cont = ctrl;
        if (ctrl == null)
        {
            return;
        }

        Vector3 offset = clip.data != null ? clip.data.hitBoxOffset : clip.boxOffset;
        float radius = clip.data != null ? clip.data.hitBoxRadius : clip.boxRadius;
        float start = clip.data != null ? clip.data.hitStartTime : clip.startTime;
        float end = clip.data != null ? clip.data.hitEndTime : clip.endTime;
        float force = clip.data != null ? clip.data.hitForce : clip.HitForce;
        ctrl.Hit_Force = force;

        float t = (float)playable.GetTime() / (float)playable.GetDuration();
        bool inRange = t >= start && t <= end;
        if (inRange != isActive)
        {
            isActive = inRange;
            if (isActive)
            {
                Vector3 boxSize = clip.data != null ? clip.data.hitBoxSize : Vector3.zero;
                ctrl.OpenHitBox(offset, radius, boxSize);
            }
            else
            {
                ctrl.CloseHitBox();
            }
        }
        //if (isActive && clip.data != null)
        //{
        //    Vector3 worldPos = ctrl.transform.TransformPoint(offset);
        //    Gizmos.color = Color.red;
        //    if (clip.data.hitBoxShape == HitBoxShape.Sphere)
        //    {
        //        Gizmos.DrawWireSphere(worldPos, radius);
        //    }
        //    else
        //    {
        //        Gizmos.DrawWireCube(worldPos, clip.data.hitBoxSize);
        //    }
        //}
    }
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (cont != null)
        {
            cont.CloseHitBox();
        }
        isActive = false;
        cont = null;
        // 移除过期的GetBinding取值，依靠运行时传入的playerData即可收尾
    }
}