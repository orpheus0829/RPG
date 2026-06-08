using UnityEngine;
using UnityEngine.Playables;

public class CameraTimelineClip : PlayableAsset
{
    [Header("镜头数据盒")]
    public ActionSO data;

    [Header("镜头单独参数")]
    public MoveMode cameraMoveMode;
    public Vector3 cameraDirection;
    public float cameraTotalDistance;
    public Vector3 cameraTargetLocalPos;
    public float cameraMoveSpeed;
    public float cameraStartSpeed;
    public float cameraEndSpeed;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var b = new CameraTimelineBehaviour();
        b.clip = this;
        return ScriptPlayable<CameraTimelineBehaviour>.Create(graph, b);
    }
}