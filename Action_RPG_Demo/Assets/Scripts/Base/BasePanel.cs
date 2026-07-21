using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class BasePanel : MonoBehaviour
{
    public string BindTag = "";
    public bool HideControl = true;
    public bool ConstantShow = false;

    [Header("滑动动画")]
    public float SlideInDuration = 0.35f;
    public float SlideOutDuration = 0.3f;
    public float SlideOffsetX = 800f;
    public Ease EaseIn = Ease.OutQuad;
    public Ease EaseOut = Ease.InQuad;

    [Header("标记")]
    public bool SkipSlideAnim = false;

    public CanvasGroup _cg;
    public List<RectTransform> _directChildList = new List<RectTransform>();
    public Dictionary<RectTransform, Vector2> _originPosDict = new Dictionary<RectTransform, Vector2>();

    public void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
        if (_cg == null)
        {
            _cg = gameObject.AddComponent<CanvasGroup>();
        }
        _directChildList.Clear();
        _originPosDict.Clear();
        foreach (Transform child in transform)
        {
            RectTransform rt = child.GetComponent<RectTransform>();
            if (rt != null)
            {
                _directChildList.Add(rt);
                _originPosDict[rt] = rt.anchoredPosition;
            }
        }
    }

    public void Start()
    {
        BindTag = gameObject.name;
        Panel_Mgr.instance.AutoBindAllPanel();
        HidePanel();
        if (ConstantShow)
        {
            ShowPanel();
        }
    }

    public void PlayShowAnim()
    {
        if (SkipSlideAnim || _directChildList.Count == 0)
        {
            ShowPanel();
            _cg.alpha = 1;
            return;
        }

        gameObject.SetActive(true);
        _cg.alpha = 1;
        foreach (var rt in _directChildList)
        {
            Vector2 origin = _originPosDict[rt];
            rt.anchoredPosition = new Vector2(origin.x - SlideOffsetX, origin.y);
        }
        Sequence seq = DOTween.Sequence();
        foreach (var rt in _directChildList)
        {
            Vector2 origin = _originPosDict[rt];
            seq.Join(rt.DOAnchorPos(origin, SlideInDuration).SetEase(EaseIn));
        }
        seq.SetUpdate(UpdateType.Normal, true);
    }

    public void PlayHideAnim(TweenCallback completeCall)
    {
        if (SkipSlideAnim || _directChildList.Count == 0)
        {
            HidePanel();
            completeCall?.Invoke();
            return;
        }

        Sequence seq = DOTween.Sequence();
        foreach (var rt in _directChildList)
        {
            Vector2 origin = _originPosDict[rt];
            Vector2 targetOut = new Vector2(origin.x + SlideOffsetX, origin.y);
            seq.Join(rt.DOAnchorPos(targetOut, SlideOutDuration).SetEase(EaseOut));
        }
        seq.SetUpdate(UpdateType.Normal, true);
        seq.OnComplete(() =>
        {
            HidePanel();
            completeCall?.Invoke();
        });
    }

    public virtual void ShowPanel()
    {
        gameObject.SetActive(true);
    }

    public virtual void HidePanel()
    {
        if (this == null)
        {
            return;
        }
        gameObject.SetActive(false);
    }

    public virtual bool IsVisible()
    {
        return gameObject.activeSelf;
    }
}