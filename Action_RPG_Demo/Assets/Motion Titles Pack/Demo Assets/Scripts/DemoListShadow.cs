using UnityEngine;
using UnityEngine.UI;

namespace Michsky.UI.MTP
{
    public class DemoListShadow : MonoBehaviour
    {
        [SerializeField] private Scrollbar listScrollbar;
        [SerializeField] private CanvasGroup topShadow;
        [SerializeField] private CanvasGroup bottomShadow;
     
        const float lerpSpeed = 0.02f;
        const float threshold = 0.02f;
        const float shadowFadeSpeed = 0.04f;

        bool isAnimating;
        float targetScrollValue;
        float targetTopAlpha;
        float targetBottomAlpha;

        void OnEnable()
        {
            if (listScrollbar != null)
            {
                listScrollbar.onValueChanged.AddListener(OnScrollbarValueChanged);
            }
        }

        void OnDisable()
        {
            if (listScrollbar != null)
            {
                listScrollbar.onValueChanged.RemoveListener(OnScrollbarValueChanged);
            }
        }

        void Start()
        {
            listScrollbar.value = 1f;
            targetTopAlpha = 0f;
            targetBottomAlpha = 1f;

            if (topShadow != null)
            {
                topShadow.alpha = 0f;
                topShadow.interactable = false;
                topShadow.blocksRaycasts = false;
            }
            if (bottomShadow != null)
            {
                bottomShadow.alpha = 1f;
                bottomShadow.interactable = true;
                bottomShadow.blocksRaycasts = true;
            }
        }

        void Update()
        {
            if (isAnimating)
            {
                listScrollbar.value = Mathf.Lerp(listScrollbar.value, targetScrollValue, lerpSpeed);
                if (Mathf.Abs(listScrollbar.value - targetScrollValue) <= threshold)
                {
                    listScrollbar.value = targetScrollValue;
                    isAnimating = false;
                }
            }

            if (topShadow != null)
            {
                topShadow.alpha = Mathf.Lerp(topShadow.alpha, targetTopAlpha, shadowFadeSpeed);
                topShadow.interactable = topShadow.alpha > 0.5f;
                topShadow.blocksRaycasts = topShadow.alpha > 0.5f;
            }
            if (bottomShadow != null)
            {
                bottomShadow.alpha = Mathf.Lerp(bottomShadow.alpha, targetBottomAlpha, shadowFadeSpeed);
                bottomShadow.interactable = bottomShadow.alpha > 0.5f;
                bottomShadow.blocksRaycasts = bottomShadow.alpha > 0.5f;
            }
        }

        void OnScrollbarValueChanged(float value)
        {
            UpdateShadowTargets(value);
        }

        void UpdateShadowTargets(float scrollValue)
        {
            targetTopAlpha = scrollValue < 1f - threshold ? 1f : 0f;
            targetBottomAlpha = scrollValue > threshold ? 1f : 0f;
        }

        public void ScrollUp()
        {
            targetScrollValue = 0f;
            isAnimating = true;
        }

        public void ScrollDown()
        {
            targetScrollValue = 1f;
            isAnimating = true;
        }
    }
}