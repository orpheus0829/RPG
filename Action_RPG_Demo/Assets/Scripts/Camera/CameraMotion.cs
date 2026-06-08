using UnityEngine;

/// <summary>
/// 通用可复用相机移动组件
/// 任意脚本均可调用，脱离Timeline独立使用
/// </summary>
public class CameraMotion : MonoBehaviour
{
    private Vector3 _originLocalPos;

    private void Awake()
    {
        _originLocalPos = transform.localPosition;
    }

    /// <summary>
    /// 平滑插值移动相机
    /// </summary>
    public void LerpMoveCamera(Vector3 targetLocalPos, float progress)
    {
        transform.localPosition = Vector3.Lerp(_originLocalPos, targetLocalPos, progress);
    }

    /// <summary>
    /// 复位相机到初始位置
    /// </summary>
    public void ResetCameraOrigin()
    {
        transform.localPosition = _originLocalPos;
    }

    /// <summary>
    /// 获取相机原始机位
    /// </summary>
    public Vector3 GetOriginPosition()
    {
        return _originLocalPos;
    }
}