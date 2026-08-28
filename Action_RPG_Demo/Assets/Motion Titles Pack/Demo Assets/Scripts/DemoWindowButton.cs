using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Michsky.UI.MTP
{
    [RequireComponent(typeof(Animator))]
    public class DemoWindowButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public float hoverAnimationLength = 0.2f;

        Animator buttonAnimator;
        Coroutine disableCoroutine;
        bool isHovering = false;

        void OnEnable()
        {
            if (buttonAnimator == null)
            {
                buttonAnimator = gameObject.GetComponent<Animator>();
            }
        }

        void OnDisable()
        {
            if (disableCoroutine != null)
            {
                StopCoroutine(disableCoroutine);
                disableCoroutine = null;
            }
            isHovering = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovering = true;

            if (disableCoroutine != null)
            {
                StopCoroutine(disableCoroutine);
                disableCoroutine = null;
            }
            if (!buttonAnimator.enabled) { buttonAnimator.enabled = true; }
            if (!buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hover to Pressed")
                && !buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Normal to Pressed"))
            {
                buttonAnimator.Play("Normal to Hover");
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovering = false;

            // Enable animator if it's not already
            if (!buttonAnimator.enabled) { buttonAnimator.enabled = true; }
            if (!buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hover to Pressed")
                && !buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Normal to Pressed"))
            {
                buttonAnimator.Play("Hover to Normal");

                if (disableCoroutine != null) { StopCoroutine(disableCoroutine); }
                disableCoroutine = StartCoroutine(DisableAnimatorAfterDelay(hoverAnimationLength));
            }
        }

        IEnumerator DisableAnimatorAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (!isHovering && buttonAnimator != null) { buttonAnimator.enabled = false; }
            disableCoroutine = null;
        }
    }
}