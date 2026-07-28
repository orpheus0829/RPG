using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class HallCam
{
    public string CamName;
    public Transform AfkPos;
    public ActionSO Act;
    public Transform CamPos;
    public ActionSO EnterAnim;
    public Transform EnterPos;
}

public class HallShow : BaseActor
{
    public static HallShow instance { get; private set; }
    public Transform MainCameraTrans;
    public PlayableDirector Director;

    [Header("待机机位组")]
    public HallCam NormalIdle;
    public HallCam Setting;
    public HallCam NewGame;
    public HallCam ReadyToPlay;

    [Header("相机过渡参数")]
    public float CameraLerpSpeed = 2.5f;

    public HallCam CurHallCam;
    public HallCam PendingEnterTarget;
    public Transform TargetCamTransform;

    public List<HallCam> lst;
    private int _curIndex;
    public int CurIndex
    {
        get => _curIndex;
        set
        {
            _curIndex = value;
            UpdatePlayButtonVisible();
        }
    }

    public GameObject PlayButtons;
    public GameObject Arrows;

    public void Awake()
    {
        Director = GetComponent<PlayableDirector>();
        GameObject mainCamObj = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCamObj)
        {
            MainCameraTrans = mainCamObj.transform;
        }
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        lst = new List<HallCam> { NewGame, Setting , NormalIdle };
        BasePanel[] panels = GameObject.FindObjectsOfType<BasePanel>();
        foreach(var i in panels)
        {
            i.gameObject.SetActive(true);
            if (i.ConstantShow)
            {
                PlayButtons = i.gameObject;
            }
            else
            {
                Arrows = i.gameObject;
            }
        }
        UpdatePlayButtonVisible();
    }

    public void Start()
    {
        LoadingMgr.instance.StartOpeningTransition();
        PlayAc(NormalIdle);
    }

    public void Update()
    {
        if (!Arrows.activeSelf && CurHallCam != ReadyToPlay)
        {
            Arrows.SetActive(true);
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            CurIndex--;
            if (CurIndex < 0)
            {
                CurIndex = lst.Count - 1;
            }
            RequestEnter(lst[CurIndex]);
            ArrowUI.instance.RefreshArrowUI();
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            CurIndex++;
            if (CurIndex >= lst.Count)
            {
                CurIndex = 0;
            }
            RequestEnter(lst[CurIndex]);
            ArrowUI.instance.RefreshArrowUI();
        }
        else if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (CurHallCam != NormalIdle)
            {
                RequestEnter(NormalIdle);
            }
            ArrowUI.instance.RefreshArrowUI();
        }

        ProcessCameraLerp();
    }
    public void RequestEnter(HallCam targetHallCam)
    {
        transform.SetPositionAndRotation(targetHallCam.EnterPos.position, targetHallCam.EnterPos.rotation);
        Director.Stop();
        CurHallCam = null;
        PendingEnterTarget = targetHallCam;
        if (targetHallCam.CamPos)
        {
            TargetCamTransform = targetHallCam.CamPos;
        }
        TimelineAsset timeline = targetHallCam.EnterAnim.timeline;
        Director.Play(timeline);
        if (targetHallCam == ReadyToPlay)
        {
            CanvasGroup a = Arrows.GetComponentInParent<CanvasGroup>();
            CanvasGroup p = PlayButtons.GetComponentInParent<CanvasGroup>();
            a.DOFade(0, 1.5f);
            p.DOFade(0, 1.5f);
        }
    }
    public void PlayAc(HallCam hallCam)
    {
        if (hallCam == null || !hallCam.Act)
        {
            return;
        }
        Director.Stop();
        PendingEnterTarget = null;
        CurHallCam = hallCam;
        if (hallCam != ReadyToPlay)
        {
            CurIndex = lst.IndexOf(CurHallCam);
        }
        TimelineAsset timeline = hallCam.Act.timeline;
        Director.Play(timeline);
        if (hallCam.CamPos)
        {
            TargetCamTransform = hallCam.CamPos;
        }
    }

    public void ProcessCameraLerp()
    {
        if (!MainCameraTrans|| !TargetCamTransform)
        {
            return;
        }

        MainCameraTrans.position = Vector3.Lerp(MainCameraTrans.position, TargetCamTransform.position, CameraLerpSpeed * Time.unscaledDeltaTime);
        MainCameraTrans.rotation = Quaternion.Lerp(MainCameraTrans.rotation, TargetCamTransform.rotation, CameraLerpSpeed * Time.unscaledDeltaTime);
    }
    public void AcEnd()
    {
        if (PendingEnterTarget != null)
        {
            transform.SetPositionAndRotation(PendingEnterTarget.AfkPos.position, PendingEnterTarget.AfkPos.rotation);
            PlayAc(PendingEnterTarget);
            return;
        }
        if (CurHallCam != null && CurHallCam.Act != null)
        {
            PlayAc(CurHallCam);
            return;
        }
    }
    public void UpdatePlayButtonVisible()
    {
        bool isNormalIdle = false;
        if (_curIndex >= 0 && _curIndex < lst.Count)
        {
            isNormalIdle = lst[_curIndex] == NormalIdle;
        }

        PlayButtons.SetActive(isNormalIdle);
    }
}