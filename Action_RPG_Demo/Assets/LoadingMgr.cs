using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
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
    public void Start()
    {
        CheckAutoAddFriend();
    }
    public void StartTransition(string TargetSceneName, bool IsLoadNewScene, Action OnComplete = null)
    {
        if (_IsTransitionRunning)
        {
            Debug.Log("正在执行过渡动画");
            return;
        }
        StartCoroutine(TransitionProcess(TargetSceneName, IsLoadNewScene, OnComplete));
    }

    public IEnumerator TransitionProcess(string TargetSceneName, bool IsLoadNewScene, Action OnComplete = null)
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
        Portal portal = GameObject.FindObjectOfType<Portal>();
        if (portal)
        {
            Vector3 center = portal.transform.position;
            float angle = UnityEngine.Random.Range(0, Mathf.PI * 2);
            float radius = UnityEngine.Random.Range(1f, 3f);
            Vector3 randomRingPoint = new Vector3(
                center.x + Mathf.Cos(angle) * radius,
                center.y,
                center.z + Mathf.Sin(angle) * radius
            );
            Vector3 finalSpawnPos = randomRingPoint;
            if (NavMesh.SamplePosition(randomRingPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                finalSpawnPos = hit.position;
            }
            else
            {
                bool findSuccess = false;
                for (int i = 0; i < 8; i++)
                {
                    angle = UnityEngine.Random.Range(0, Mathf.PI * 2);
                    radius = UnityEngine.Random.Range(1f, 3f);
                    randomRingPoint = new Vector3(
                        center.x + Mathf.Cos(angle) * radius,
                        center.y,
                        center.z + Mathf.Sin(angle) * radius
                    );
                    if (NavMesh.SamplePosition(randomRingPoint, out hit, 2f, NavMesh.AllAreas))
                    {
                        finalSpawnPos = hit.position;
                        findSuccess = true;
                        break;
                    }
                }
                if (!findSuccess)
                {
                    finalSpawnPos = randomRingPoint;
                }
            }
            p.position = finalSpawnPos;
        }
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
        OnComplete?.Invoke();
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
    #region 刷新好友
    public void CheckAutoAddFriend()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            if (result.Data != null && result.Data.ContainsKey("NeedAddFriendBack"))
            {
                string friendId = result.Data["NeedAddFriendBack"].Value;

                PlayFabClientAPI.AddFriend(new AddFriendRequest { FriendPlayFabId = friendId },
                addSuccess =>
                {
                    Debug.Log("自动完成双向好友：" + friendId);
                    PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
                    {
                        KeysToRemove = new List<string> { "NeedAddFriendBack" }
                    },
                    updateSuccess => { },
                    updateErr => { Debug.LogError(updateErr.ErrorMessage); });
                },
                addErr =>
                {
                    Debug.Log("自动添加好友失败：" + addErr.ErrorMessage);
                });
            }
        },
        err =>
        {
            Debug.LogError("读取用户数据失败：" + err.ErrorMessage);
        });
    }
    #endregion
}