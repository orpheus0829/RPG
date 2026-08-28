using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Michsky.UI.MTP
{
    public class DemoWindowManager : MonoBehaviour
    {
        [Header("List")]
        public List<WindowItem> windows = new();

        [Header("Settings")]
        public int currentWindowIndex = 0;

        [Header("Demo Event Settings")]
        [Tooltip("Time in seconds to wait before triggering the event")]
        public float demoEventDelay = 2.0f;
        public UnityEvent onShowDemo;

        // Constants
        const float animationLength = 1f;
        const string windowFadeIn = "Panel In";
        const string windowFadeOut = "Panel Out";
        const string buttonFadeIn = "Normal to Pressed";
        const string buttonFadeOut = "Pressed to Normal";

        // Cache
        int newWindowIndex;
        GameObject currentWindow;
        GameObject nextWindow;
        GameObject currentButton;
        GameObject nextButton;
        Animator currentWindowAnimator;
        Animator nextWindowAnimator;
        Animator currentButtonAnimator;
        Animator nextButtonAnimator;
        Coroutine demoCoroutine;
        readonly Dictionary<Animator, Coroutine> activeAnimatorCoroutines = new();
        readonly Dictionary<GameObject, Coroutine> activeWindowCoroutines = new();

        [System.Serializable]
        public class WindowItem
        {
            public string windowName = "My Window";
            public GameObject windowObject;
            public GameObject buttonObject;
        }

        void Start()
        {
            // Disable all windows except the current one
            for (int i = 0; i < windows.Count; i++)
            {
                if (windows[i].windowObject != null)
                {
                    windows[i].windowObject.SetActive(i == currentWindowIndex);
                }
            }

            // Initialize current button
            if (windows[currentWindowIndex].buttonObject != null)
            {
                currentButton = windows[currentWindowIndex].buttonObject;
                currentButtonAnimator = currentButton.GetComponent<Animator>();
                if (currentButtonAnimator != null)
                {
                    currentButtonAnimator.enabled = true;
                    currentButtonAnimator.Play(buttonFadeIn);
                    ScheduleAnimatorDisable(currentButtonAnimator, animationLength);
                }
            }

            // Initialize current window
            currentWindow = windows[currentWindowIndex].windowObject;
            if (currentWindow != null)
            {
                currentWindowAnimator = currentWindow.GetComponent<Animator>();
                if (currentWindowAnimator != null)
                {
                    currentWindowAnimator.enabled = true;
                    currentWindowAnimator.Play(windowFadeIn);
                    ScheduleAnimatorDisable(currentWindowAnimator, animationLength);
                }
            }
        }

        void ScheduleAnimatorDisable(Animator animator, float delay)
        {
            if (animator == null)
                return;

            // Cancel existing coroutine for this animator if any
            if (activeAnimatorCoroutines.ContainsKey(animator))
            {
                if (activeAnimatorCoroutines[animator] != null)
                {
                    StopCoroutine(activeAnimatorCoroutines[animator]);
                }
            }

            Coroutine coroutine = StartCoroutine(DisableAnimatorAfterDelay(animator, delay));
            activeAnimatorCoroutines[animator] = coroutine;
        }

        void ScheduleWindowDeactivate(GameObject window, float delay)
        {
            if (window == null)
                return;

            // Cancel existing coroutine for this window if any
            if (activeWindowCoroutines.ContainsKey(window))
            {
                if (activeWindowCoroutines[window] != null)
                {
                    StopCoroutine(activeWindowCoroutines[window]);
                }
            }

            Coroutine coroutine = StartCoroutine(DeactivateWindowAfterDelay(window, delay));
            activeWindowCoroutines[window] = coroutine;
        }

        IEnumerator DisableAnimatorAfterDelay(Animator animator, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (animator != null) { animator.enabled = false; }
            if (activeAnimatorCoroutines.ContainsKey(animator)) { activeAnimatorCoroutines.Remove(animator); }
        }

        IEnumerator DeactivateWindowAfterDelay(GameObject window, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (window != null) { window.SetActive(false); }
            if (activeWindowCoroutines.ContainsKey(window)) { activeWindowCoroutines.Remove(window); }
        }

        public void OpenPanel(string newPanel)
        {
            for (int i = 0; i < windows.Count; i++)
            {
                if (windows[i].windowName == newPanel)
                {
                    newWindowIndex = i;
                }
            }

            if (newWindowIndex != currentWindowIndex)
            {
                currentWindow = windows[currentWindowIndex].windowObject;
                currentButton = windows[currentWindowIndex].buttonObject;

                nextWindow = windows[newWindowIndex].windowObject;
                nextButton = windows[newWindowIndex].buttonObject;

                // Cancel any pending deactivation for the next window
                if (nextWindow != null && activeWindowCoroutines.ContainsKey(nextWindow))
                {
                    if (activeWindowCoroutines[nextWindow] != null)
                    {
                        StopCoroutine(activeWindowCoroutines[nextWindow]);
                        activeWindowCoroutines.Remove(nextWindow);
                    }
                }

                // Activate next window BEFORE getting animator
                if (nextWindow != null) { nextWindow.SetActive(true); }

                // Handle window animations
                if (currentWindow != null)
                {
                    currentWindowAnimator = currentWindow.GetComponent<Animator>();

                    if (currentWindowAnimator != null)
                    {
                        currentWindow.SetActive(true);
                        currentWindowAnimator.enabled = true;
                        currentWindowAnimator.Play(windowFadeOut);
                        ScheduleAnimatorDisable(currentWindowAnimator, animationLength);
                        ScheduleWindowDeactivate(currentWindow, animationLength);
                    }
                    else
                    {
                        // No animator, deactivate immediately
                        currentWindow.SetActive(false);
                    }
                }

                if (nextWindow != null)
                {
                    nextWindowAnimator = nextWindow.GetComponent<Animator>();
                    if (nextWindowAnimator != null)
                    {
                        nextWindowAnimator.enabled = true;
                        nextWindowAnimator.Play(windowFadeIn);
                        ScheduleAnimatorDisable(nextWindowAnimator, animationLength);
                    }
                }

                // Handle button animations
                if (currentButton != null)
                {
                    currentButtonAnimator = currentButton.GetComponent<Animator>();
                    if (currentButtonAnimator != null)
                    {
                        currentButtonAnimator.enabled = true;
                        currentButtonAnimator.Play(buttonFadeOut);
                        ScheduleAnimatorDisable(currentButtonAnimator, animationLength);
                    }
                }

                if (nextButton != null)
                {
                    nextButtonAnimator = nextButton.GetComponent<Animator>();
                    if (nextButtonAnimator != null)
                    {
                        nextButtonAnimator.enabled = true;
                        nextButtonAnimator.Play(buttonFadeIn);
                        ScheduleAnimatorDisable(nextButtonAnimator, animationLength);
                    }
                }

                currentWindowIndex = newWindowIndex;
            }
        }

        IEnumerator DemoRoutine()
        {
            yield return new WaitForSeconds(demoEventDelay);
            onShowDemo?.Invoke();
            demoCoroutine = null;
        }

        public void OnDemoStart()
        {
            if (demoCoroutine != null) { StopCoroutine(demoCoroutine); }
            demoCoroutine = StartCoroutine(DemoRoutine());
        }
    }
}