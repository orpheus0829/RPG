using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// 相机移动轨道容器
/// </summary>
[TrackBindingType(typeof(ActionControl))]
[TrackClipType(typeof(CameraTimelineClip))]
public class CameraTimelineTrack : TrackAsset
{

}