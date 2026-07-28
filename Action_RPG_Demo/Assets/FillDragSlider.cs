using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FillDragSlider : MonoBehaviour, IBeginDragHandler, IDragHandler, IPointerDownHandler
{
    public RectTransform FillAreaRt;
    public Image fillImage;

    public void Awake()
    {
        fillImage = GetComponent<Image>();
        FillAreaRt = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateValue(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        UpdateValue(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateValue(eventData);
    }

    public void UpdateValue(PointerEventData data)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            FillAreaRt, data.position, data.pressEventCamera, out Vector2 localPos);
        float ratio = Mathf.InverseLerp(FillAreaRt.rect.xMin, FillAreaRt.rect.xMax, localPos.x);
        ratio = Mathf.Clamp01(ratio);
        fillImage.fillAmount = ratio;
        MenuSetting.instance.Vollume = ratio;
    }
}