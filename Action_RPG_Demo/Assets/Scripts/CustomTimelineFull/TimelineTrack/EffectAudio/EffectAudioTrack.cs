using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// 特效音效轨道容器
/// </summary>
[TrackBindingType(typeof(ActionControl))]
[TrackClipType(typeof(EffectAudioClip))]
public class EffectAudioTrack : TrackAsset
{

}