using UnityEngine;
using System.Collections;
using DG.Tweening;
using TMPro;

public class DamageNumItem : MonoBehaviour
{
    public TextMeshProUGUI TextMesh;
    private Coroutine FloatFadeCoroutine;
    private Tween ScaleTween;

    public void Awake()
    {
        TextMesh = GetComponentInChildren<TextMeshProUGUI>();
        transform.localScale = Vector3.zero;
    }

    public void Initialize(float damage, Vector3 monsterPos)
    {
        DamageNumberMgr mgr = DamageNumberMgr.instance;
        Transform camTrans = CameraPivot.instance.transform;
        Vector3 dirToCamera = camTrans.position - monsterPos;
        dirToCamera.y = 0;
        dirToCamera.Normalize();
        float forwardDistance = 0.22f;
        Vector3 frontBasePos = monsterPos + dirToCamera * forwardDistance;
        Vector3 horizontalSide = Vector3.Cross(Vector3.up, dirToCamera);
        float sideRandom = Random.Range(-mgr.RandomXRange, mgr.RandomXRange);
        Vector3 finalOffset = horizontalSide * sideRandom;
        finalOffset.y = mgr.BaseSpawnYOffset;

        transform.position = frontBasePos + finalOffset;

        float targetFontSize;
        if (damage >= mgr.HighDamageThreshold)
        {
            TextMesh.text = $"{damage}!";
            TextMesh.color = Color.red;
            targetFontSize = mgr.CritFontSize;
        }
        else
        {
            TextMesh.text = damage.ToString();
            TextMesh.color = damage >= mgr.HighDamageThreshold ? Color.red : Color.yellow;
            targetFontSize = mgr.NormalFontSize;
        }
        TextMesh.fontSize = targetFontSize;

        Color resetColor = TextMesh.color;
        resetColor.a = 1f;
        TextMesh.color = resetColor;
        if (FloatFadeCoroutine != null)
        {
            StopCoroutine(FloatFadeCoroutine);
        }
        if (ScaleTween != null)
        {
            ScaleTween.Kill();
        }
        transform.localScale = Vector3.zero;
        ScaleTween = transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutBack);

        FloatFadeCoroutine = StartCoroutine(FloatAndFadeCoroutine());
    }

    public IEnumerator FloatAndFadeCoroutine()
    {
        DamageNumberMgr mgr = DamageNumberMgr.instance;
        float timer = 0f;
        Color baseTextColor = TextMesh.color;
        Transform mainCameraTrans = CameraPivot.instance.transform;
        float shrinkStartProgress = 0.7f;

        while (timer < mgr.FadeTotalTime)
        {
            timer += Time.deltaTime;
            float progress = timer / mgr.FadeTotalTime;
            transform.position += Vector3.up * mgr.RiseSpeed * Time.deltaTime;
            transform.LookAt(transform.position + mainCameraTrans.forward);
            Color newColor = baseTextColor;
            newColor.a = 1f - progress;
            TextMesh.color = newColor;
            if (progress >= shrinkStartProgress)
            {
                float shrinkT = (progress - shrinkStartProgress) / (1f - shrinkStartProgress);
                transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, shrinkT);
            }

            yield return null;
        }
        if (ScaleTween != null)
        {
            ScaleTween.Kill();
            ScaleTween = null;
        }
        ObjectPoolMgr.instance.PushObj(gameObject);
        FloatFadeCoroutine = null;
    }
}