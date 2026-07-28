using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingMgr : Base_mgr<LoadingMgr>
{
    public RectTransform ToLoad;
    public RectTransform AllBlack;

    public float MaskEnterOffsetY = 960f;
    public float MaskExitOffsetY = -960f;

    [Header("动画")]
    public float MaskDropDuration = 1.2f;
    public float WaitAfterDrop = 1f;
    public float MaskLeaveDuration = 1.2f;
    public float MinimumTotalTransitionTime = 4f;

    public float QuitGameHoldTime = 1.5f;

    private AsyncOperation _AsyncLoadOperation;
    private bool _LoadFinished;
    private bool _MinTimeReached;
    private bool _IsTransitionRunning;

    protected override void Awake()
    {
        base.Awake();
        if (instance == this)
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void StartTransition(string TargetSceneName, bool IsLoadNewScene)
    {
        if (_IsTransitionRunning)
        {
            Debug.Log("正在执行过渡动画");
            return;
        }
        StartCoroutine(TransitionProcess(TargetSceneName, IsLoadNewScene));
    }

    public IEnumerator TransitionProcess(string TargetSceneName, bool IsLoadNewScene)
    {
        _IsTransitionRunning = true;
        _LoadFinished = false;
        _MinTimeReached = false;

        ToLoad.gameObject.SetActive(true);
        ToLoad.localScale = Vector3.one;
        ToLoad.anchoredPosition = new Vector2(0, MaskEnterOffsetY);

        float Timer = 0f;
        while (Timer < MaskDropDuration)
        {
            Timer += Time.unscaledDeltaTime;
            float T = Timer / MaskDropDuration;
            ToLoad.anchoredPosition = Vector2.Lerp(new Vector2(0, MaskEnterOffsetY), Vector2.zero, T);
            yield return null;
        }
        //if (CameraPivot.instance)
        //{
        //    for(int i = CameraPivot.instance.transform.childCount - 1; i >= 0; i--)
        //    {
        //        Destroy(CameraPivot.instance.transform.GetChild(i).gameObject);
        //    }
        //    CameraPivot.instance.transform.GetChild(0).SetParent(null);
        //}
        ToLoad.anchoredPosition = Vector2.zero;

        yield return new WaitForSecondsRealtime(WaitAfterDrop);

        AllBlack.gameObject.SetActive(true);
        AllBlack.anchoredPosition = Vector2.zero;

        StartCoroutine(WaitMinimumTime());

        if (IsLoadNewScene)
        {
            _AsyncLoadOperation = SceneManager.LoadSceneAsync(TargetSceneName);
            _AsyncLoadOperation.allowSceneActivation = false;
            StartCoroutine(CheckSceneLoadProgress());

            yield return new WaitUntil(() => _LoadFinished && _MinTimeReached);
            _AsyncLoadOperation.allowSceneActivation = true;
            yield return null;
            yield return new WaitUntil(() => _AsyncLoadOperation.isDone);
        }
        else
        {
            yield return new WaitUntil(() => _MinTimeReached);
        }
        AllBlack.gameObject.SetActive(false);

        ToLoad.localScale = new Vector3(1, -1, 1);

        Timer = 0f;
        Vector2 StartPos = ToLoad.anchoredPosition;
        Vector2 EndPos = new Vector2(0, MaskExitOffsetY);
        while (Timer < MaskLeaveDuration)
        {
            Timer += Time.unscaledDeltaTime;
            float T = Timer / MaskLeaveDuration;
            ToLoad.anchoredPosition = Vector2.Lerp(StartPos, EndPos, T);
            yield return null;
        }
        Transform p = GameObject.FindObjectOfType<Player>().transform;
        p.position = GameObject.FindObjectOfType<Portal>().gameObject.transform.position;
        Story_Mgr.instance.Refresh_StoryProgress();
        Story_Mgr.instance.Refresh_StoryUI(Story_Mgr.instance.CurQuest);
        ToLoad.localScale = Vector3.one;
        ToLoad.gameObject.SetActive(false);
        AllBlack.gameObject.SetActive(false);
        _IsTransitionRunning = false;
        if (CameraPivot.instance)
        {
            CameraPivot.instance.camTrans = null;
            CameraPivot.instance.distance = 3;
        }
    }

    public IEnumerator CheckSceneLoadProgress()
    {
        while (_AsyncLoadOperation.progress < 0.9f)
        {
            yield return null;
        }
        _LoadFinished = true;
    }

    public IEnumerator WaitMinimumTime()
    {
        yield return new WaitForSecondsRealtime(MinimumTotalTransitionTime);
        _MinTimeReached = true;
    }
    #region 退出
    public void StartQuitTransition()
    {
        if (_IsTransitionRunning)
        {
            return;
        }
        StartCoroutine(QuitProcess());
    }
    public IEnumerator QuitProcess()
    {
        _IsTransitionRunning = true;

        ToLoad.gameObject.SetActive(true);
        ToLoad.localScale = Vector3.one;
        ToLoad.anchoredPosition = new Vector2(0, MaskEnterOffsetY);

        float Timer = 0f;
        while (Timer < MaskDropDuration)
        {
            Timer += Time.unscaledDeltaTime;
            float T = Timer / MaskDropDuration;
            ToLoad.anchoredPosition = Vector2.Lerp(new Vector2(0, MaskEnterOffsetY), Vector2.zero, T);
            yield return null;
        }
        ToLoad.anchoredPosition = Vector2.zero;

        yield return new WaitForSecondsRealtime(QuitGameHoldTime);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
        yield break;
    }
    #endregion
    # region 开游戏
    public void StartOpeningTransition()
    {
        StartCoroutine(OpeningProcess());
    }
    public IEnumerator OpeningProcess()
    {
        _IsTransitionRunning = true;

        ToLoad.gameObject.SetActive(true);
        ToLoad.localScale = Vector3.one;
        ToLoad.anchoredPosition = Vector2.zero;

        yield return new WaitForSecondsRealtime(0.3f);

        ToLoad.localScale = new Vector3(1, -1, 1);

        float Timer = 0f;
        Vector2 StartPos = ToLoad.anchoredPosition;
        Vector2 EndPos = new Vector2(0, MaskExitOffsetY);
        while (Timer < MaskLeaveDuration)
        {
            Timer += Time.unscaledDeltaTime;
            float T = Timer / MaskLeaveDuration;
            ToLoad.anchoredPosition = Vector2.Lerp(StartPos, EndPos, T);
            yield return null;
        }
        ToLoad.localScale = Vector3.one;
        ToLoad.gameObject.SetActive(false);
        _IsTransitionRunning = false;
    }
    #endregion
}