using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PickNoticeMgr : Base_mgr<PickNoticeMgr>
{
    public Queue<GameObject> CenterAnounce = new Queue<GameObject>();
    [Header("拾取提示")]
    public Queue<GameObject> Notices = new Queue<GameObject>();
    public Queue<Item_Data> NoticeWaitQueue = new Queue<Item_Data>();
    public int ActiveNoticeCount = 0;
    public int MaxNoticeCount = 5;
    public GameObject Note;
    public Transform NoteParent;
    public float FadeDuration;
    public float AnimDuration;
    public float SlideOffsetx;
    [Header("任务提示")]
    public GameObject CenterText;
    public Transform CenterParent;
    private string MissionUpdate;
    public string MissionSuccess;
    public float CenterFadeDuration;
    public float CenterAnimDuration;
    public float CenterSlideOffsetx;
    public bool CenterAnimRunning;
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

    }
    public void Update()
    {

    }
    #region 拾取
    public void AddNote(Item_Data data)
    {
        if (ActiveNoticeCount < MaxNoticeCount)
        {
            SpawnNotice(data);
        }
        else
        {
            NoticeWaitQueue.Enqueue(data);
        }
    }
    private void SpawnNotice(Item_Data data)
    {
        ActiveNoticeCount++;
        GameObject n = ObjectPoolMgr.instance.GetObj(Note, NoteParent);
        Transform noteTrans = n.transform;
        CanvasGroup cg = n.GetComponent<CanvasGroup>();
        if (!cg)
        {
            cg = n.AddComponent<CanvasGroup>();
        }

        Image image = n.GetComponentInChildren<Image>();
        TextMeshProUGUI NoteText = n.GetComponentInChildren<TextMeshProUGUI>();
        NoteText.text = $"{data.item_name} * 1";
        image.sprite = data.Display_In_Backpacks;
        cg.alpha = 0;
        noteTrans.localPosition = new Vector3(SlideOffsetx, 0, 0);

        Sequence enterSeq = DOTween.Sequence();
        enterSeq.Join(noteTrans.DOLocalMoveX(0, AnimDuration).SetEase(Ease.OutQuad));
        enterSeq.Join(cg.DOFade(1, AnimDuration));

        Notices.Enqueue(n);
        StartCoroutine(WaitExitAnim(n, noteTrans, cg));
    }
    public IEnumerator WaitExitAnim(GameObject obj, Transform trans, CanvasGroup cg)
    {
        yield return new WaitForSeconds(FadeDuration);

        Sequence exitSeq = DOTween.Sequence();
        exitSeq.Join(trans.DOLocalMoveX(-SlideOffsetx, AnimDuration).SetEase(Ease.InQuad));
        exitSeq.Join(cg.DOFade(0, AnimDuration));
        exitSeq.OnComplete(() => DelNote(obj));
    }
    public void DelNote(GameObject obj)
    {
        if (Notices.Peek() == obj)
        {
            Notices.Dequeue();
        }
        Image image = obj.GetComponentInChildren<Image>();
        TextMeshProUGUI NoteText = obj.GetComponentInChildren<TextMeshProUGUI>();
        image = null;
        NoteText.text = string.Empty;
        ObjectPoolMgr.instance.PushObj(obj);

        ActiveNoticeCount--;
        if (NoticeWaitQueue.Count > 0)
        {
            Item_Data next = NoticeWaitQueue.Dequeue();
            SpawnNotice(next);
        }
    }
    #endregion
    #region 任务
    public void MissionDone()
    {
        Debug.Log("任务完成");
        EnqueueCenterText(MissionSuccess, 0f);
    }
    public void MissionNext()
    {
        Debug.Log("进入下一个任务");
        MissionUpdate = $"下一幕:{Story_Mgr.instance.CurQuest.Quest_Title}";
        float delay = CenterAnimRunning ? (CenterAnimDuration * 2 + CenterFadeDuration) : 0f;
        EnqueueCenterText(MissionUpdate, delay);
    }
    private void EnqueueCenterText(string sourceText, float delay = 0f)
    {
        StartCoroutine(DelayAddToQueue(sourceText, delay));
    }
    private IEnumerator DelayAddToQueue(string text, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        GameObject obj = CreateCenterTextObj(text);
        CenterAnounce.Enqueue(obj);
        if (!CenterAnimRunning)
        {
            StartCoroutine(ProcessCenterQueue());
        }
    }

    private GameObject CreateCenterTextObj(string t)
    {
        GameObject obj = ObjectPoolMgr.instance.GetObj(CenterText, CenterParent);
        TextMeshProUGUI mt = obj.GetComponent<TextMeshProUGUI>();
        mt.text = t;
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (!cg)
        {
            cg = obj.AddComponent<CanvasGroup>();
        }
        cg.alpha = 0;
        obj.transform.localPosition = new Vector3(CenterSlideOffsetx, 0, 0);
        return obj;
    }

    private IEnumerator ProcessCenterQueue()
    {
        CenterAnimRunning = true;
        while (CenterAnounce.Count > 0)
        {
            GameObject curObj = CenterAnounce.Dequeue();
            Transform trans = curObj.transform;
            CanvasGroup cg = curObj.GetComponent<CanvasGroup>();
            cg.alpha = 0;
            trans.localPosition = new Vector3(CenterSlideOffsetx, 0, 0);
            Sequence enterSeq = DOTween.Sequence();
            enterSeq.Join(trans.DOLocalMoveX(0, CenterAnimDuration).SetEase(Ease.OutQuad));
            enterSeq.Join(cg.DOFade(1, CenterAnimDuration));
            yield return enterSeq.WaitForCompletion();
            yield return new WaitForSeconds(CenterFadeDuration);
            Sequence exitSeq = DOTween.Sequence();
            exitSeq.Join(trans.DOLocalMoveX(-CenterSlideOffsetx, CenterAnimDuration).SetEase(Ease.InQuad));
            exitSeq.Join(cg.DOFade(0, CenterAnimDuration));
            yield return exitSeq.WaitForCompletion();
            RecycleCenterObj(curObj);
        }
        CenterAnimRunning = false;
    }

    private void RecycleCenterObj(GameObject obj)
    {
        TextMeshProUGUI mt = obj.GetComponent<TextMeshProUGUI>();
        mt.text = string.Empty;
        ObjectPoolMgr.instance.PushObj(obj);
    }
    #endregion
}
