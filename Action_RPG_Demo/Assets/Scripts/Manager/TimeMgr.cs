using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeMgr : Base_mgr<TimeMgr>
{
    [Header("时间配置")]
    public float NormalTimeScale = 1f;
    public float SlowTimeScale = 0.3f;
    public float TimeLerpSpeed = 4f;
    [Header("原始物理固定步长")]
    public float OriginFixedDeltaTime;
    [Header("目标时间倍率")]
    public float TargetTimeScale;

    private Coroutine ActiveBulletTimeCor;
    private float DefaultLerpSpeedBackup;

    protected override void Awake()
    {
        base.Awake();
        if (instance == this)
        {
            DontDestroyOnLoad(this.gameObject);
            OriginFixedDeltaTime = Time.fixedDeltaTime;
            TargetTimeScale = NormalTimeScale;
            Time.timeScale = NormalTimeScale;
        }
    }

    private void Update()
    {
        if (!Mathf.Approximately(Time.timeScale, TargetTimeScale))
        {
            float curScale = Mathf.Lerp(Time.timeScale, TargetTimeScale, TimeLerpSpeed * Time.unscaledDeltaTime);
            SetTimeScaleDirect(curScale);
        }
    }

    public void OpenSlowMotion()
    {
        TargetTimeScale = SlowTimeScale;
    }

    public void RestoreNormalTime()
    {
        TargetTimeScale = NormalTimeScale;
    }

    public void SetCustomTimeScale(float scale)
    {
        TargetTimeScale = Mathf.Clamp(scale, 0f, 2f);
    }

    public void PauseGame()
    {
        TargetTimeScale = 0f;
    }

    public void UnPauseGame()
    {
        RestoreNormalTime();
    }

    private void SetTimeScaleDirect(float scale)
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = OriginFixedDeltaTime * scale;
    }
    public void BulletTime(float downSpeed, float targetScale, float bulletDuration, float upSpeed)
    {
        if (ActiveBulletTimeCor != null)
        {
            StopCoroutine(ActiveBulletTimeCor);
            TimeLerpSpeed = DefaultLerpSpeedBackup;
        }
        ActiveBulletTimeCor = StartCoroutine(BulletCoroutine(downSpeed, targetScale, bulletDuration, upSpeed));
    }

    private IEnumerator BulletCoroutine(float downSpeed, float targetScale, float realDuration, float upSpeed)
    {
        Game_Event.instance.SetAlpha();
        TimeLerpSpeed = downSpeed;
        TargetTimeScale = Mathf.Clamp(targetScale, 0.01f, 2f);
        yield return WaitUntilTimeScaleReach(targetScale);
        yield return new WaitForSecondsRealtime(realDuration);
        Game_Event.instance.ReSetAlpha();
        TimeLerpSpeed = upSpeed;
        TargetTimeScale = NormalTimeScale;
        yield return WaitUntilTimeScaleReach(NormalTimeScale);
        TimeLerpSpeed = DefaultLerpSpeedBackup;
        ActiveBulletTimeCor = null;
    }
    private IEnumerator WaitUntilTimeScaleReach(float target)
    {
        while (!Mathf.Approximately(Time.timeScale, target))
        {
            yield return null;
        }
    }
}