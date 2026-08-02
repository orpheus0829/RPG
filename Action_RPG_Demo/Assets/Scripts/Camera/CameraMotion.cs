using UnityEngine;
public class CameraMotion : MonoBehaviour
{
    private Vector3 _originLocalPos;

    private void Awake()
    {
        _originLocalPos = transform.localPosition;
    }
    public void LerpMoveCamera(Vector3 targetLocalPos, float progress)
    {
        transform.localPosition = Vector3.Lerp(_originLocalPos, targetLocalPos, progress);
    }
    public void ResetCameraOrigin()
    {
        transform.localPosition = _originLocalPos;
    }
    public Vector3 GetOriginPosition()
    {
        return _originLocalPos;
    }
}