using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// 攻击判定轨道载体
/// 绑定角色控制器，限定片段类型
/// </summary>
[TrackBindingType(typeof(ActionControl))]
[TrackClipType(typeof(HitBoxClip))]
public class HitBoxTrack : TrackAsset
{

}