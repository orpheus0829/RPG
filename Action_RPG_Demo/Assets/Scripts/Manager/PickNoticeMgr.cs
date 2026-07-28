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
    private string _missionUpdate;
    public string MissionSuccess;
    public float CenterFadeDuration;
    public float CenterAnimDuration;
    public float CenterSlideOffsetx;
    public bool CenterAnimRunning;
    [Header("字段提示/对话Tip")]
    public GameObject TextNote;
    public Transform TextNoteParent;
    public float FieldEnterTime = 0.4f;
    public float FieldStayMoveTime = 0.6f;
    public float FieldExitTime = 0.35f;
    public float EnterOffsetY = -400f;
    public float StayMoveY = 120f;
    public float ExitExtraMoveY = 300f;

    [Header("对话配置")]
    public float DialogueItemSpacing = 60f;
    public float DialogueLayoutAnimTime = 0.5f;
    public float DialogueFadeOutTime = 0.25f;
    [System.Serializable]
    public class ActiveDialogueItem
    {
        public string speaker;
        public GameObject obj;
        public Transform trans;
        public CanvasGroup cg;
        public TextMeshProUGUI txt;
        public TimeMgr.TimerTask timerTask;
    }
    public List<ActiveDialogueItem> ActiveDialogueList = new List<ActiveDialogueItem>();

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
        // 完全移除UpdateActiveDialogues，不再手动倒计时
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
        enterSeq.SetUpdate(UpdateType.Normal, true);

        Notices.Enqueue(n);
        // 替换协程，改用TimeMgr真实时间计时器
        TimeMgr.instance.CreateTimer(TimeMgr.TimerMode.RealTimeUnscaled, 0, FadeDuration, null, () =>
        {
            Sequence exitSeq = DOTween.Sequence();
            exitSeq.Join(noteTrans.DOLocalMoveX(-SlideOffsetx, AnimDuration).SetEase(Ease.InQuad));
            exitSeq.Join(cg.DOFade(0, AnimDuration));
            exitSeq.SetUpdate(UpdateType.Normal, true);
            exitSeq.OnComplete(() => DelNote(n));
        });
    }

    public void DelNote(GameObject obj)
    {
        if (Notices.Count > 0 && Notices.Peek() == obj)
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
        _missionUpdate = $"下一幕:{Story_Mgr.instance.CurQuest.Quest_Title}";
        float delay = CenterAnimRunning ? (CenterAnimDuration * 2 + CenterFadeDuration) : 0f;
        EnqueueCenterText(_missionUpdate, delay);
    }
    private void EnqueueCenterText(string sourceText, float delay = 0f)
    {
        // 替换协程延时，使用TimeMgr真实时间
        TimeMgr.instance.CreateTimer(TimeMgr.TimerMode.RealTimeUnscaled, 0, delay, null, () =>
        {
            GameObject obj = CreateCenterTextObj(sourceText);
            CenterAnounce.Enqueue(obj);
            if (!CenterAnimRunning)
            {
                ProcessCenterQueue();
            }
        });
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

    private void ProcessCenterQueue()
    {
        if (CenterAnounce.Count <= 0)
        {
            CenterAnimRunning = false;
            return;
        }
        CenterAnimRunning = true;
        GameObject curObj = CenterAnounce.Dequeue();
        Transform trans = curObj.transform;
        CanvasGroup cg = curObj.GetComponent<CanvasGroup>();
        cg.alpha = 0;
        trans.localPosition = new Vector3(CenterSlideOffsetx, 0, 0);
        Sequence enterSeq = DOTween.Sequence();
        enterSeq.Join(trans.DOLocalMoveX(0, CenterAnimDuration).SetEase(Ease.OutQuad));
        enterSeq.Join(cg.DOFade(1, CenterAnimDuration));
        enterSeq.SetUpdate(UpdateType.Normal, true);
        enterSeq.OnComplete(() =>
        {
            TimeMgr.instance.CreateTimer(TimeMgr.TimerMode.RealTimeUnscaled, 0, CenterFadeDuration, null, () =>
            {
                Sequence exitSeq = DOTween.Sequence();
                exitSeq.Join(trans.DOLocalMoveX(-CenterSlideOffsetx, CenterAnimDuration).SetEase(Ease.InQuad));
                exitSeq.Join(cg.DOFade(0, CenterAnimDuration));
                exitSeq.SetUpdate(UpdateType.Normal, true);
                exitSeq.OnComplete(() =>
                {
                    RecycleCenterObj(curObj);
                    ProcessCenterQueue();
                });
            });
        });
    }
    private void RecycleCenterObj(GameObject obj)
    {
        TextMeshProUGUI mt = obj.GetComponent<TextMeshProUGUI>();
        mt.text = string.Empty;
        ObjectPoolMgr.instance.PushObj(obj);
    }
    #endregion
    #region 字段提示
    public void ShowFieldTip(string Content)
    {
        GameObject TipObj = CreateFieldTipObj(Content);
        PlaySingleFieldTipAnim(TipObj);
    }
    public GameObject CreateFieldTipObj(string TextContent)
    {
        GameObject Obj = ObjectPoolMgr.instance.GetObj(TextNote, TextNoteParent);
        Obj.name = "FieldTip";
        TextMeshProUGUI Txt = Obj.GetComponent<TextMeshProUGUI>();
        Txt.text = TextContent;

        CanvasGroup Cg = Obj.GetComponent<CanvasGroup>();
        if (Cg == null)
        {
            Cg = Obj.AddComponent<CanvasGroup>();
        }

        Cg.alpha = 0;
        Obj.transform.localPosition = new Vector3(0, EnterOffsetY, 0);
        return Obj;
    }
    private void PlaySingleFieldTipAnim(GameObject CurTip)
    {
        Transform TipTrans = CurTip.transform;
        CanvasGroup Cg = CurTip.GetComponent<CanvasGroup>();

        Sequence EnterSeq = DOTween.Sequence();
        EnterSeq.Join(TipTrans.DOLocalMoveY(0, FieldEnterTime).SetEase(Ease.OutExpo));
        EnterSeq.Join(Cg.DOFade(1, FieldEnterTime));
        EnterSeq.SetUpdate(UpdateType.Normal, true);
        EnterSeq.OnComplete(() =>
        {
            Sequence StaySeq = DOTween.Sequence();
            StaySeq.Join(TipTrans.DOLocalMoveY(StayMoveY, FieldStayMoveTime).SetEase(Ease.Linear));
            StaySeq.SetUpdate(UpdateType.Normal, true);
            StaySeq.OnComplete(() =>
            {
                Sequence ExitSeq = DOTween.Sequence();
                ExitSeq.Join(TipTrans.DOLocalMoveY(StayMoveY + ExitExtraMoveY, FieldExitTime).SetEase(Ease.InExpo));
                ExitSeq.Join(Cg.DOFade(0, FieldExitTime));
                ExitSeq.SetUpdate(UpdateType.Normal, true);
                ExitSeq.OnComplete(() =>
                {
                    RecycleFieldTipObj(CurTip);
                });
            });
        });
    }

    public void RecycleFieldTipObj(GameObject Obj)
    {
        TextMeshProUGUI Txt = Obj.GetComponent<TextMeshProUGUI>();
        Txt.text = string.Empty;
        Obj.transform.DOKill();
        ObjectPoolMgr.instance.PushObj(Obj);
    }
    #endregion
    #region 台词显示
    public void ShowDialogueTip(string speakerName, string dialogueContent, float displayTime)
    {
        ActiveDialogueItem existItem = null;
        foreach (var item in ActiveDialogueList)
        {
            if (item.speaker == speakerName)
            {
                existItem = item;
                break;
            }
        }
        if (existItem != null)
        {
            string prefix = string.IsNullOrEmpty(speakerName) ? "" : $"{speakerName}:";
            existItem.txt.text = $"{prefix}{dialogueContent}";
            if (existItem.timerTask != null)
            {
                TimeMgr.instance.StopTimer(existItem.timerTask);
            }
            CreateDialogueTimer(existItem, displayTime);
            return;
        }
        GameObject tipObj = ObjectPoolMgr.instance.GetObj(TextNote, TextNoteParent);
        tipObj.name = "DialogueTip";
        TextMeshProUGUI txt = tipObj.GetComponent<TextMeshProUGUI>();
        string n = string.IsNullOrEmpty(speakerName) ? "" : $"{speakerName}:";
        txt.text = $"{n}{dialogueContent}";

        CanvasGroup cg = tipObj.GetComponent<CanvasGroup>();
        if (!cg)
        {
            cg = tipObj.AddComponent<CanvasGroup>();
        }
        cg.alpha = 0;
        Transform trans = tipObj.transform;
        trans.localPosition = new Vector3(0, -200, 0);

        ActiveDialogueItem newItem = new ActiveDialogueItem()
        {
            speaker = speakerName,
            obj = tipObj,
            trans = trans,
            cg = cg,
            txt = txt,
            timerTask = null
        };
        ActiveDialogueList.Add(newItem);
        CreateDialogueTimer(newItem, displayTime);

        Sequence enterSeq = DOTween.Sequence();
        enterSeq.Join(trans.DOLocalMoveY(0, DialogueLayoutAnimTime).SetEase(Ease.OutQuad));
        enterSeq.Join(cg.DOFade(1, DialogueLayoutAnimTime));
        enterSeq.SetUpdate(true);
        enterSeq.OnComplete(() => RefreshDialogueLayout());
        RefreshDialogueLayout();
    }

    private void CreateDialogueTimer(ActiveDialogueItem item, float duration)
    {
        item.timerTask = TimeMgr.instance.CreateTimer(TimeMgr.TimerMode.RealTimeUnscaled, 0, duration, null, () =>
        {
            if (!ActiveDialogueList.Contains(item)) return;
            Sequence exitSeq = DOTween.Sequence();
            exitSeq.Join(item.cg.DOFade(0, DialogueFadeOutTime));
            exitSeq.SetUpdate(true);
            exitSeq.OnComplete(() =>
            {
                RecycleFieldTipObj(item.obj);
                ActiveDialogueList.Remove(item);
                RefreshDialogueLayout();
            });
        });
    }

    private void RefreshDialogueLayout()
    {
        float startY = 0;
        for (int i = 0; i < ActiveDialogueList.Count; i++)
        {
            var item = ActiveDialogueList[i];
            float targetY = startY - i * DialogueItemSpacing;
            item.trans.DOLocalMoveY(targetY, DialogueLayoutAnimTime).SetEase(Ease.OutQuad).SetUpdate(true);
        }
    }

    // 清理全部对话（场景切换调用）
    public void ClearAllDialogue()
    {
        foreach (var item in ActiveDialogueList)
        {
            if (item.timerTask != null)
            {
                TimeMgr.instance.StopTimer(item.timerTask);
            }
            item.obj.transform.DOKill();
            RecycleFieldTipObj(item.obj);
        }
        ActiveDialogueList.Clear();
    }
    #endregion
}