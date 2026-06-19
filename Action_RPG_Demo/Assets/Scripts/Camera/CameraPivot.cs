using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPivot : Base_mgr<CameraPivot>
{
    [Header("跟随目标")]
    public Transform target;
    [Header("环绕距离")]
    public float distance = 4f;
    [Header("缩放")]
    public float ZoomMax = 8f;
    public float ZoomMin = 2f;
    public float ZoomSmooth = 12f;
    public float ZoomSpeed = 1.2f;
    private float _targetDistance;
    public float TargetDistance
    {
        get
        {
            return _targetDistance;
        }
        set
        {
            _targetDistance = Mathf.Clamp(value, ZoomMin, ZoomMax);
        }
    }
    [Header("高度偏移")]
    public float height = 1.5f;
    [Header("灵敏度")]
    public float sensitivity = 150f;
    [Header("上下限制角度")]
    public float minAngle = -30f;
    public float maxAngle = 85f;

    [Header("平滑")]
    public float smooth = 8f;

    [Header("环绕镜头动画参数")]
    public float loopTime = 2f;
    public float circleRadius = 5f;
    public float circleHeightOffset = 1.8f;
    public float animLerpSpeed = 12f;
    public float pauseDuration = 0.4f;
    public float pullOutAccelSpeed = 22f;
    public float endNormalDist = 4f;
    [Header("角色位移跟随参数(环绕动画同步用)")]
    public float targetMoveTotalDist = 2.8f;
    public float targetMoveSpeed = 1.1f;

    public float rotX;
    public float rotY;

    public bool isPlayingCameraAnim;
    public float cacheNormalRotX;
    public float cacheNormalRotY;
    public float cacheNormalDistance;
    // 缓存原始常态高度
    public float cacheNormalHeight;
    public Coroutine currentCameraAnimCoroutine;
    protected override void Awake()
    {
        base.Awake();
        if (instance == this)
        {
            DontDestroyOnLoad(this.gameObject);
        }
        GameObject pl = GameObject.FindGameObjectWithTag("Player");
        target = pl.GetComponent<Transform>();
    }

    public void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        rotY = transform.eulerAngles.y;
        TargetDistance = distance;
        SaveNormalCameraState();
    }

    public void AddZoomDelta(float scrollDelta)
    {
        TargetDistance -= scrollDelta * ZoomSpeed;
    }

    public void LateUpdate()
    {
        if (isPlayingCameraAnim)
        {
            return;
        }
        if (!target)
        {
            return;
        }
        float mX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
        rotY += mX;
        rotX -= mY;
        rotX = Mathf.Clamp(rotX, minAngle, maxAngle);
        Quaternion cameraRotation = Quaternion.Euler(rotX, rotY, 0);
        distance = Mathf.Lerp(distance, _targetDistance, ZoomSmooth * Time.deltaTime);
        Vector3 cameraDir = cameraRotation * Vector3.back;
        Vector3 cameraPos = target.position + cameraDir * distance;
        cameraPos.y += height;
        transform.position = Vector3.Lerp(transform.position, cameraPos, smooth * Time.deltaTime);
        transform.rotation = cameraRotation;
    }

    public void SaveNormalCameraState()
    {
        cacheNormalRotX = rotX;
        cacheNormalRotY = rotY;
        cacheNormalDistance = distance;
        cacheNormalHeight = height;
    }

    public void RestoreNormalCameraState()
    {
        rotX = cacheNormalRotX;
        rotY = cacheNormalRotY;
        distance = cacheNormalDistance;
        height = cacheNormalHeight;
        TargetDistance = distance;
    }

    public void StopAllCameraAnimation()
    {
        if (currentCameraAnimCoroutine != null)
        {
            StopCoroutine(currentCameraAnimCoroutine);
            currentCameraAnimCoroutine = null;
        }
        isPlayingCameraAnim = false;
        RestoreNormalCameraState();
    }

    public void PlayRevolveAroundPlayerAnim()
    {
        if (target == null)
        {
            return;
        }
        StopAllCameraAnimation();
        SaveNormalCameraState();
        currentCameraAnimCoroutine = StartCoroutine(DoRevolveAnim());
    }

    public IEnumerator DoRevolveAnim()
    {
        isPlayingCameraAnim = true;
        float timer = 0f;
        Vector3 targetStartPos = target.position;
        Vector3 targetForwardDir = target.forward;
        Vector3 currentCamPos = transform.position;

        while (timer < loopTime)
        {
            timer += Time.deltaTime;
            float moveOffset = Mathf.Min(targetMoveSpeed * timer, targetMoveTotalDist);
            Vector3 realTargetPos = targetStartPos + targetForwardDir * moveOffset;
            float progress = timer / loopTime;
            float angle = progress * 360f;
            float rad = Mathf.Deg2Rad * angle;
            Quaternion targetRot = Quaternion.LookRotation(targetForwardDir);
            Vector3 localCircleOffset = new Vector3(Mathf.Sin(rad) * circleRadius, circleHeightOffset, -Mathf.Cos(rad) * circleRadius);
            Vector3 circleWorldPos = realTargetPos + targetRot * localCircleOffset;

            currentCamPos = Vector3.Lerp(currentCamPos, circleWorldPos, animLerpSpeed * Time.deltaTime);
            transform.position = currentCamPos;
            transform.LookAt(realTargetPos + Vector3.up * 1.2f);
            yield return null;
        }
        float pauseTimer = 0f;
        while (pauseTimer < pauseDuration)
        {
            pauseTimer += Time.deltaTime;
            transform.position = currentCamPos;
            transform.LookAt(target.position + Vector3.up * 1.2f);
            yield return null;
        }
        float pullTimer = 0f;
        Vector3 targetBackDir = target.forward * -1f;
        Vector3 finalNormalPos = target.position + targetBackDir * endNormalDist;
        finalNormalPos.y += cacheNormalHeight;
        while (pullTimer < 1f)
        {
            pullTimer += Time.deltaTime * pullOutAccelSpeed;
            float t = Mathf.Clamp01(pullTimer);
            currentCamPos = Vector3.Lerp(currentCamPos, finalNormalPos, t);
            transform.position = currentCamPos;
            transform.LookAt(target.position + Vector3.up * 1.2f);
            yield return null;
        }
        distance = endNormalDist;
        TargetDistance = endNormalDist;
        height = cacheNormalHeight;

        Vector3 finalEuler = transform.eulerAngles;
        rotX = finalEuler.x;
        rotY = finalEuler.y;

        isPlayingCameraAnim = false;
        currentCameraAnimCoroutine = null;
    }
}