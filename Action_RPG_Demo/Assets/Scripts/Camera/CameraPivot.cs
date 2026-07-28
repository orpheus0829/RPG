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
    public float cacheNormalHeight;
    public Coroutine currentCameraAnimCoroutine;

    [Header("镜头震动设置")]
    public Vector3 _originLocalPos;
    public float _shakeTime;
    public float _shakePower;
    public float _shakeDamp;
    public Transform camTrans;

    [Header("对话镜头缓存")]
    public float cacheDiaRotX;
    public float cacheDiaRotY;
    public float cacheDiaDistance;
    public float cacheDiaHeight;

    private Coroutine _quickShakeCor;
    protected override void Awake()
    {
        base.Awake();
        if (instance == this)
        {
            DontDestroyOnLoad(this.gameObject);
        }
        if (camTrans != null)
        {
            _originLocalPos = camTrans.localPosition;
        }
    }

    public void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        rotY = transform.eulerAngles.y;
        TargetDistance = distance;
        SaveNormalCameraState();
    }
    public void OnEnable()
    {
        if (camTrans != null)
        {
            _originLocalPos = camTrans.localPosition;
            _shakeTime = 0;
            camTrans.localPosition = _originLocalPos;
        }
    }
    public void AddZoomDelta(float scrollDelta)
    {
        TargetDistance -= scrollDelta * ZoomSpeed;
    }
    public void Update()
    {
        if (!target)
        {
            GameObject pl = GameObject.FindGameObjectWithTag("Player");
            if (pl)
            {
                target = pl.transform;
            }
        }
        if (!camTrans)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
            camTrans = GameObject.FindGameObjectWithTag("MainCamera").transform;
            camTrans.SetParent(this.transform);
            camTrans.localPosition = Vector3.zero;
            camTrans.localRotation = Quaternion.identity;
            Camera c = camTrans.GetComponent<Camera>();
            c.fieldOfView = 30;
            c.cullingMask &= ~(1 << LayerMask.NameToLayer("EquipStage"));
        }
    }
    public void LateUpdate()
    {
        if (!isPlayingCameraAnim)
        {
            UpdateCameraShake();
        }
        if (isPlayingCameraAnim)
        {
            return;
        }
        if (!target)
        {
            return;
        }
        Quaternion cameraRotation = transform.rotation;
        Vector3 cameraDir = cameraRotation * Vector3.back;
        if (Panel_Mgr.instance && !Panel_Mgr.instance.IsPanelVisible(Panel_Mgr.instance.DialoguePanel) && !Panel_Mgr.instance.IsPanelVisible(Panel_Mgr.instance.ConfirmPanel))
        {
            float mX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
            float mY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
            rotY += mX;
            rotX -= mY;
            rotX = Mathf.Clamp(rotX, minAngle, maxAngle);
            cameraRotation = Quaternion.Euler(rotX, rotY, 0);
            distance = Mathf.Lerp(distance, _targetDistance, ZoomSmooth * Time.deltaTime);
            cameraDir = cameraRotation * Vector3.back;
        }

        Vector3 targetOrigin = target.position + Vector3.up * height;
        float safeCamDist = distance;
        float wallBuffer = 0.1f;
        RaycastHit hit;
        int playerLayer = 1 << LayerMask.NameToLayer("Player");
        int enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
        int ignoreMask = ~(playerLayer | enemyLayer);

        if (Physics.Raycast(targetOrigin, cameraDir, out hit, distance, ignoreMask))
        {
            safeCamDist = hit.distance - wallBuffer;
            safeCamDist = Mathf.Max(safeCamDist, ZoomMin);
        }
        Vector3 safeCamPos = targetOrigin + cameraDir * safeCamDist;
        transform.position = Vector3.Lerp(transform.position, safeCamPos, smooth * Time.deltaTime);
        transform.rotation = cameraRotation;
    }
    #region 镜头动画
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
        if (_quickShakeCor != null)
        {
            StopCoroutine(_quickShakeCor);
            _quickShakeCor = null;
            if (camTrans != null)
            {
                camTrans.localPosition = _originLocalPos;
            }
        }
        isPlayingCameraAnim = false;
        RestoreNormalCameraState();
        _shakeTime = 0;
        if (camTrans != null)
        {
            camTrans.localPosition = _originLocalPos;
        }
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
    #endregion
    private void UpdateCameraShake()
    {
        if (camTrans == null) return;
        if (_shakeTime <= 0)
        {
            camTrans.localPosition = _originLocalPos;
            return;
        }
        _shakeTime -= Time.deltaTime;
        float currentPower = _shakePower * Mathf.Clamp01(_shakeTime / _shakeDamp);
        Vector3 randomOffset = Random.insideUnitSphere * currentPower;
        camTrans.localPosition = _originLocalPos + randomOffset;
    }
    public void StopCameraShake()
    {
        _shakeTime = 0;
        if (camTrans != null)
        {
            camTrans.localPosition = _originLocalPos;
        }
    }
    public void StartCameraShake(float power, float duration, float damp = 2f)
    {
        if (camTrans == null)
        {
            return;
        }
        _shakePower = power;
        _shakeTime = duration;
        _shakeDamp = damp;
    }
    public void QuickRealTimeShake(float shakePower)
    {
        if (camTrans == null)
        {
            return;
        }
        if (_quickShakeCor != null)
        {
            StopCoroutine(_quickShakeCor);
        }
        _quickShakeCor = StartCoroutine(QuickShakeCoroutine(shakePower));
    }
    private IEnumerator QuickShakeCoroutine(float power)
    {
        float totalRealTime = 0.22f;
        float dampFactor = 1.6f;
        float remainTime = totalRealTime;
        while (remainTime > 0f)
        {
            float delta = Time.unscaledDeltaTime;
            remainTime -= delta;
            float fade = Mathf.Clamp01(remainTime / totalRealTime);
            float currentPow = power * fade;
            Vector3 offset = Random.insideUnitSphere * currentPow;
            camTrans.localPosition = _originLocalPos + offset;
            yield return null;
        }
        camTrans.localPosition = _originLocalPos;
        _quickShakeCor = null;
    }
    public void SaveDialogueCameraState()
    {
        cacheDiaRotX = rotX;
        cacheDiaRotY = rotY;
        cacheDiaDistance = distance;
        cacheDiaHeight = height;
    }
    public void RestoreDialogueCameraState()
    {
        rotX = cacheDiaRotX;
        rotY = cacheDiaRotY;
        distance = cacheDiaDistance;
        height = cacheDiaHeight;
        TargetDistance = distance;
    }
}