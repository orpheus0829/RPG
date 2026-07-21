using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuffReceiver : MonoBehaviour
{
    [System.Serializable]
    public class BuffTask
    {
        public TimeMgr.TimerMode Mode;
        public BuffSO BuffData;
        public GameObject IconPrefab;
        public Image Progress;
        public float CurrentTime;

        public float TotalDuration;
        public float TickInterval;
        public float TickTimer;

        public Action OnBuffStart;
        public Action OnBuffTick;
        public Action OnBuffEnd;

        public bool IsFinished;
        public Coroutine TaskCor;
        public bool IsRunning()
        {
            if (TaskCor == null || IsFinished)
            {
                return false;
            }
            return true;
        }

        public float GetRemainTime()
        {
            float remain = TotalDuration - CurrentTime;
            return Mathf.Max(remain, 0f);
        }

        public float GetProgress()
        {
            if (TotalDuration <= 0)
            {
                return 1f;
            }
            return Mathf.Clamp01(CurrentTime / TotalDuration);
        }
    }
    public List<BuffTask> ActiveBuffTasks = new List<BuffTask>();
    public RectTransform IconParent;
    public GameObject OriginalPrefab;
    public DamageReceiver damageReceiver;
    [Header("倍率")]
    public float DamageFactor = 1;
    public float MoveFactor = 1;
    public void Awake()
    {
        damageReceiver = GetComponent<DamageReceiver>();
        DamageFactor = 1;
        MoveFactor = 1;
    }
    public void Update()
    {
        foreach(var i in ActiveBuffTasks)
        {
            i.Progress.fillAmount = 1 - i.GetProgress();
        }
    }

    #region 接收与移除buff
    public void ReceiveBuff(BuffSO buff)
    {
        Debug.Log(buff.BuffName);
        if (!buff)
        {
            return;
        }
        if (buff.IsInstant)
        {
            OnBuffStartEffect(buff, null);
            return;
        }
        BuffTask existTask = ActiveBuffTasks.Find(t => t.BuffData == buff && !t.IsFinished);
        if (existTask != null)
        {
            StopSingleBuffTask(existTask);
        }

        CreateBuffTask(buff);
    }

    public void RemoveBuff(BuffSO buff)
    {
        for (int i = ActiveBuffTasks.Count - 1; i >= 0; i--)
        {
            var task = ActiveBuffTasks[i];
            if (task.BuffData == buff && !task.IsFinished)
            {
                StopSingleBuffTask(task);
                break;
            }
        }
    }

    public void ClearAllBuff()
    {
        for (int i = ActiveBuffTasks.Count - 1; i >= 0; i--)
        {
            StopSingleBuffTask(ActiveBuffTasks[i]);
        }
        ActiveBuffTasks.Clear();
    }

    public float GetBuffRemainTime(BuffSO buff)
    {
        foreach (var task in ActiveBuffTasks)
        {
            if (task.BuffData == buff && !task.IsFinished)
            {
                return task.GetRemainTime();
            }
        }
        return 0f;
    }
    #endregion

    #region 计时
    private BuffTask CreateBuffTask(BuffSO buff)
    {
        BuffTask newTask = new BuffTask();
        newTask.Mode = TimeMgr.TimerMode.DeltaTime;
        newTask.BuffData = buff;
        newTask.CurrentTime = 0f;
        newTask.TotalDuration = buff.Duration;
        newTask.TickInterval = buff.ActiveInterval;
        newTask.TickTimer = 0f;
        newTask.IsFinished = false;

        newTask.OnBuffStart = () => OnBuffStartEffect(buff, newTask);
        newTask.OnBuffTick = () => OnBuffTickEffect(buff, newTask);
        newTask.OnBuffEnd = () => OnBuffEndEffect(buff, newTask);

        Coroutine cor = StartCoroutine(BuffTaskCoroutine(newTask));
        newTask.TaskCor = cor;

        ActiveBuffTasks.Add(newTask);
        newTask.OnBuffStart?.Invoke();
        return newTask;
    }

    private void StopSingleBuffTask(BuffTask task)
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
        task.OnBuffEnd?.Invoke();
        ActiveBuffTasks.Remove(task);
    }
    private IEnumerator BuffTaskCoroutine(BuffTask task)
    {
        while (!task.IsFinished)
        {
            float delta = 0f;
            switch (task.Mode)
            {
                case TimeMgr.TimerMode.DeltaTime:
                    delta = Time.deltaTime;
                    task.CurrentTime += delta;
                    break;
                case TimeMgr.TimerMode.FixedDeltaTime:
                    yield return new WaitForFixedUpdate();
                    delta = Time.fixedDeltaTime;
                    task.CurrentTime += delta;
                    break;
                case TimeMgr.TimerMode.RealTimeUnscaled:
                    delta = Time.unscaledDeltaTime;
                    task.CurrentTime += delta;
                    break;
            }
            if (task.TickInterval > 0f)
            {
                task.TickTimer += delta;
                while (task.TickTimer >= task.TickInterval)
                {
                    task.OnBuffTick?.Invoke();
                    task.TickTimer -= task.TickInterval;
                }
            }
            if (task.CurrentTime >= task.TotalDuration)
            {
                task.IsFinished = true;
                task.OnBuffEnd?.Invoke();
                yield break;
            }
            yield return null;
        }
    }
    #endregion
    #region 阶段buff逻辑
    private void OnBuffStartEffect(BuffSO buff,BuffTask task)
    {
        if (!buff.IsInstant && task != null)
        {
            if (this.gameObject.CompareTag("Player"))
            {
                GameObject icon = ObjectPoolMgr.instance.GetObj(OriginalPrefab, Panel_Mgr.instance.PlayUiPanel.transform);
                task.IconPrefab = icon;
                RectTransform rect = icon.GetComponent<RectTransform>();
                rect.DOKill();

                Transform allChild = icon.transform;
                foreach (Transform i in allChild)
                {
                    if (!i.GetComponentInChildren<TextMeshProUGUI>(true))
                    {
                        icon.GetComponent<BuffToolTip>().buffData = buff;
                        icon.GetComponent<Image>().sprite = buff.BuffIcon;
                        Image im = i.GetComponent<Image>();
                        im.sprite = buff.BuffIcon;
                        task.Progress = im;
                    }
                }

                rect.localScale = Vector3.one * 2f;
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                Vector2 buffBarWorldCenter = IconParent.TransformPoint(IconParent.rect.center);
                TimeMgr.instance.CreateTimer(TimeMgr.TimerMode.DeltaTime, 0f, 0.5f, () =>
                {
                    CanvasGroup cg = rect.GetComponent<CanvasGroup>();
                    if (cg == null)
                    {
                        cg = icon.AddComponent<CanvasGroup>();
                    }
                    cg.alpha = 0;
                    cg.DOFade(1f, 0.3f);
                }, () =>
                {
                    Sequence seq = DOTween.Sequence();
                    seq.Join(rect.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack));
                    seq.Join(rect.DOMove(buffBarWorldCenter, 0.3f).SetEase(Ease.OutCubic))
                        .OnComplete(() =>
                        {
                            rect.SetParent(IconParent, false);
                            rect.localScale = Vector3.one;
                            LayoutRebuilder.ForceRebuildLayoutImmediate(IconParent);
                            Transform allChild = icon.transform;
                        });
                });
            }
        }
        if (buff.IsInstant)
        {
            switch (buff.TargetValue)
            {
                case TargetStatus.Health:
                    damageReceiver.currentHp += buff.Val;
                    Mathf.Clamp(damageReceiver.currentHp, 0f, damageReceiver.maxHp);
                    break;
                case TargetStatus.Damage:
                    DamageFactor = buff.Val;
                    break;
                case TargetStatus.SpecialPower:
                    if(damageReceiver.gameObject.TryGetComponent(out Player pl1))
                    {
                        pl1.Skill_PowerPool += buff.Val;
                        Mathf.Clamp(pl1.Skill_PowerPool, 0f, pl1.MaxPower);
                    }
                    break;
                case TargetStatus.MoveSpeed:
                    if (damageReceiver.gameObject.TryGetComponent(out Player pl2))
                    {
                        MoveFactor = buff.Val;
                    }
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (buff.TargetValue)
            {
                case TargetStatus.Health:
                    damageReceiver.currentHp += buff.Val;
                    Mathf.Clamp(damageReceiver.currentHp, 0f, damageReceiver.maxHp);
                    break;
                case TargetStatus.Damage:
                    DamageFactor = buff.Val;
                    break;
                case TargetStatus.SpecialPower:
                    if (damageReceiver.gameObject.TryGetComponent(out Player pl1))
                    {
                        pl1.Skill_PowerPool += buff.Val;
                        Mathf.Clamp(pl1.Skill_PowerPool, 0f, pl1.MaxPower);
                    }
                    break;
                case TargetStatus.MoveSpeed:
                    if (damageReceiver.gameObject.TryGetComponent(out Player pl2))
                    {
                        MoveFactor = buff.Val;
                    }
                    break;
                default:
                    break;
            }
        }
    }
    private void OnBuffTickEffect(BuffSO buff,BuffTask task)
    {
        switch (buff.TargetValue)
        {
            case TargetStatus.Health:
                damageReceiver.currentHp += buff.Val;
                Mathf.Clamp(damageReceiver.currentHp, 0f, damageReceiver.maxHp);
                break;
            case TargetStatus.Damage:
                DamageFactor = buff.Val;
                break;
            case TargetStatus.SpecialPower:
                if (damageReceiver.gameObject.TryGetComponent(out Player pl1))
                {
                    pl1.Skill_PowerPool += buff.Val;
                    Mathf.Clamp(pl1.Skill_PowerPool, 0f, pl1.MaxPower);
                }
                break;
            case TargetStatus.MoveSpeed:
                if (damageReceiver.gameObject.TryGetComponent(out Player pl2))
                {
                    MoveFactor = buff.Val;
                }
                break;
            default:
                break;
        }
    }
    private void OnBuffEndEffect(BuffSO buff,BuffTask task)
    {
        if (task != null && task.IconPrefab)
        {
            ObjectPoolMgr.instance.PushObj(task.IconPrefab);
            task.IconPrefab = null;
        }
        switch (buff.TargetValue)
        {
            case TargetStatus.Health:
                break;
            case TargetStatus.Damage:
                DamageFactor = 1;
                break;
            case TargetStatus.SpecialPower:
                break;
            case TargetStatus.MoveSpeed:
                if (damageReceiver.gameObject.TryGetComponent(out Player pl2))
                {
                    MoveFactor = 1;
                }
                break;
            default:
                break;
        }
        StopSingleBuffTask(task);
    }
    #endregion

    private void OnDestroy()
    {
        ClearAllBuff();
    }
}