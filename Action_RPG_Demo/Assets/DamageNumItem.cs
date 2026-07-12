using UnityEngine;
using System.Collections;
using DG.Tweening;

public class DamageNumItem : MonoBehaviour
{
    public TextMesh TextMesh;
    private Coroutine FloatFadeCoroutine;
    private Tween ScaleTween;

    public void Awake()
    {
        TextMesh = GetComponentInChildren<TextMesh>();
        transform.localScale = Vector3.zero;
    }

    public void Initialize(float damage, Vector3 monsterPos)
    {
        DamageNumberMgr mgr = DamageNumberMgr.instance;
        float randomX = Random.Range(-mgr.RandomXRange, mgr.RandomXRange);
        float randomZ = Random.Range(-mgr.RandomZRange, mgr.RandomZRange);
        Vector3 spawnOffset = new Vector3(randomX, mgr.BaseSpawnYOffset, randomZ);
        transform.position = monsterPos + spawnOffset;

        float targetCharSize;
        if (damage >= mgr.HighDamageThreshold)
        {
            TextMesh.text = $"{damage}!";
            TextMesh.color = Color.red;
            targetCharSize = mgr.CritFontSize;
        }
        else
        {
            TextMesh.text = damage.ToString();
            TextMesh.color = Color.yellow;
            targetCharSize = mgr.NormalFontSize;
        }
        TextMesh.characterSize = targetCharSize;

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

    private IEnumerator FloatAndFadeCoroutine()
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
            transform.forward = mainCameraTrans.forward;
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