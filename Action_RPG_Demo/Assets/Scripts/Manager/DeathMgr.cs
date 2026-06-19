using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathMgr : Base_mgr<DeathMgr>
{
    public GameObject DeathFadePanel;
    public Image FadeImage;
    public float FadeTime;
    public float Duaring;
    public float UnFadeTime;
    protected override void Awake()
    {
        base.Awake();
        if (instance == this)
        {
            DontDestroyOnLoad(this.gameObject);
        }
        Canvas canvas = GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            DeathFadePanel = canvas.gameObject;
            FadeImage = canvas.GetComponentInChildren<Image>();
            PanelReset();
        }
    }
    public void PanelReset()
    {
        Color color = Color.black;
        color.a = 0;
        FadeImage.color = color;
        DeathFadePanel.SetActive(false);
    }
    public void DearhFade()
    {
        DeathFadePanel.SetActive(true);
        StartCoroutine(TurnBlackAndBorn(FadeImage.color.a));
        Debug.Log("¿ªÊ¼Ð¯³Ì");
    }
    public IEnumerator TurnBlackAndBorn(float c)
    {
        float speed = 1 / FadeTime;
        while (c < 1f)
        {
            c += speed * Time.deltaTime;
            c = Mathf.Clamp01(c);
            Color t = FadeImage.color;
            t.a = c;
            FadeImage.color = t;
            yield return null;
        }
        float time = 0f;
        Game_Event.instance.DeadState();
        while (time < Duaring)
        {
            time += Time.fixedDeltaTime;
            yield return null;
        }
        Game_Event.instance.DeadSecState();
        float speed_ = 1 / UnFadeTime;
        while (c > 0f)
        {
            c -= speed_ * Time.deltaTime;
            c = Mathf.Clamp01(c);
            Color t = FadeImage.color;
            t.a = c;
            FadeImage.color = t;
            yield return null;
        }
        PanelReset();
    }

}
