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
}