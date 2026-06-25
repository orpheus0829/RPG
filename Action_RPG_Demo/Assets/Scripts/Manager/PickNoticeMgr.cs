using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PickNoticeMgr : Base_mgr<PickNoticeMgr>
{
    public Queue<GameObject> Notices = new Queue<GameObject>();
    public GameObject Note;
    public Transform NoteParent;
    public float FadeDuration;
    public float AnimDuration;
    public float SlideOffsetx;
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
    public void AddNote(Item_Data data)
    {
        GameObject n = ObjectPoolMgr.instance.GetObj(Note, NoteParent);
        Transform noteTrans = n.transform;
        CanvasGroup cg = n.GetComponent<CanvasGroup>();
        if (cg == null)
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
        //DOmove
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
    }
}
