using System;
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

    [Header("卡肉")]
    public float HitPauseTime;
    public bool IsHitPausing;

    public Coroutine Hit;
    private Coroutine ActiveBulletTimeCor;
    private float DefaultLerpSpeedBackup;

    public List<TimerTask> AllTimerTasks = new List<TimerTask>();

    public enum TimerMode
    {
        DeltaTime,
        FixedDeltaTime,
        RealTimeUnscaled
    }
    [System.Serializable]
    public class TimerTask
    {
        public TimerMode Mode;
        public float CurrentTime;
        public float TargetTime;
        public Action OnStart;
        public Action OnComplete;

        public Action OnTick;
        public float TickInterval;
        public float _tickTimer;

        public bool IsFinished;
        public Coroutine TaskCor;

        public bool IsRunning()
        {
            if (TaskCor == null)
            {
                return false;
            }
            if (IsFinished)
            {
                return false;
            }
            return true;
        }

        public float GetRemainTime()
        {
            float remain = TargetTime - CurrentTime;
            if (remain < 0f)
            {
                return 0f;
            }
            return remain;
        }

        public float GetProgress()
        {
            if (TargetTime <= 0f)
            {
                return 1f;
            }
            float progress = CurrentTime / TargetTime;
            return Mathf.Clamp01(progress);
        }
    }
    protected override void Awake()
    {
        base.Awake();
        if (instance == this)
        {
            DontDestroyOnLoad(this.gameObject);
            OriginFixedDeltaTime = Time.fixedDeltaTime;
            TargetTimeScale = NormalTimeScale;
            Time.timeScale = NormalTimeScale;
            DefaultLerpSpeedBackup = TimeLerpSpeed;
        }
        IsHitPausing = false;
    }

    private void Update()
    {
        if (IsHitPausing)
        {
            return;
        }
        if (!Mathf.Approximately(Time.timeScale, TargetTimeScale))
        {
            float curScale = Mathf.Lerp(Time.timeScale, TargetTimeScale, TimeLerpSpeed * Time.unscaledDeltaTime);
            SetTimeScaleDirect(curScale);
        }

        for (int i = AllTimerTasks.Count - 1; i >= 0; i--)
        {
            TimerTask task = AllTimerTasks[i];
            if (task == null || task.IsFinished)
            {
                AllTimerTasks.RemoveAt(i);
            }
        }
    }
    public void HitPause()
    {
        if (Hit != null)
        {
            StopCoroutine(Hit);
        }
        Hit = StartCoroutine(HitStop());
    }
    public IEnumerator HitStop()
    {
        Debug.Log("卡肉");
        IsHitPausing = true;
        SetTimeScaleDirect(0f);
        yield return new WaitForSecondsRealtime(HitPauseTime);
        SetTimeScaleDirect(NormalTimeScale);
        IsHitPausing = false;
        Hit = null;
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
    public void SuddenStop()
    {
        SetCustomTimeScale(0f);
    }
    public void SuddenResume()
    {
        SetCustomTimeScale(NormalTimeScale);
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
    public TimerTask CreateTimer(TimerMode mode, float initialTime, float targetTime, Action onStart, Action onComplete)
    {
        return CreateTimer(mode, initialTime, targetTime, onStart, onComplete, null, 0);
    }
    public TimerTask CreateTimer(TimerMode mode, float initialTime, float targetTime, Action onStart, Action onComplete, Action onTick, float tickInterval)
    {
        TimerTask newTask = new TimerTask();
        newTask.Mode = mode;
        newTask.CurrentTime = initialTime;
        newTask.TargetTime = targetTime;
        newTask.OnStart = onStart;
        newTask.OnComplete = onComplete;
        newTask.OnTick = onTick;
        newTask.TickInterval = Mathf.Max(tickInterval, 0f);
        newTask._tickTimer = newTask.TickInterval;
        newTask.IsFinished = false;
        Coroutine cor = StartCoroutine(TimerCoroutine(newTask));
        newTask.TaskCor = cor;
        AllTimerTasks.Add(newTask);
        newTask.OnStart?.Invoke();
        return newTask;
    }
    public void StopTimer(TimerTask task)
    {
        if (task == null || task.IsFinished)
        {
            return;
        }
        if (task.TaskCor != null)
        {
            StopCoroutine(task.TaskCor);
        }
        task.IsFinished = true;
        AllTimerTasks.Remove(task);
    }

    public void ClearAllTimer()
    {
        foreach (TimerTask t in AllTimerTasks)
        {
            if (t.TaskCor != null)
            {
                StopCoroutine(t.TaskCor);
            }
            t.IsFinished = true;
        }
        AllTimerTasks.Clear();
    }

    private IEnumerator TimerCoroutine(TimerTask task)
    {
        while (!task.IsFinished)
        {
            float delta = 0f;
            switch (task.Mode)
            {
                case TimerMode.DeltaTime:
                    delta = Time.deltaTime;
                    task.CurrentTime += delta;
                    break;
                case TimerMode.FixedDeltaTime:
                    yield return new WaitForFixedUpdate();
                    delta = Time.fixedDeltaTime;
                    task.CurrentTime += delta;
                    break;
                case TimerMode.RealTimeUnscaled:
                    delta = Time.unscaledDeltaTime;
                    task.CurrentTime += delta;
                    break;
            }

            if (task.OnTick != null && task.TickInterval > 0f)
            {
                task._tickTimer += delta;
                if (task._tickTimer >= task.TickInterval)
                {
                    task.OnTick.Invoke();
                    task._tickTimer -= task.TickInterval;
                }
            }
            if (task.CurrentTime >= task.TargetTime)
            {
                task.IsFinished = true;
                task.OnComplete?.Invoke();
                yield break;
            }
            yield return null;
        }
    }
    private void OnDestroy()
    {
        ClearAllTimer();
    }
}