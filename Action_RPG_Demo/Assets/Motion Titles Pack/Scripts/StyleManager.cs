using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Michsky.UI.MTP
{
    [RequireComponent(typeof(Animator))]
    public class StyleManager : MonoBehaviour
    {
        // Content
        public List<TextItem> textItems = new();
        public List<ImageItem> imageItems = new();

        // Animation
        public Animator styleAnimator;
        public AnimationClip inAnim;
        public AnimationClip outAnim;
        public bool playOnEnable = true;
        [SerializeField, Range(0.1f, 2.5f)] private float animationSpeed = 1;
        public float showFor = 2.5f;

        // Settings
        public bool forceUpdate = true;
        public bool customContent = false;
        public bool customizableWidth = false;
        public bool customizableHeight = false;
        public bool playOutAnimation = true;
        public bool disableOnOut = true;
        public bool loopAnimations = false;
        [SerializeField] private bool useUnscaledTime = false;

        // Events
        public UnityEvent onEnable = new();
        public UnityEvent onDisable = new();

#if UNITY_EDITOR
        public bool editMode = false;
        public bool inspectAnim = false;
        public float tempAnimTime = 0;
#endif

        Coroutine timerCoroutine;
        Coroutine disableCoroutine;
        const string animSpeed = "Anim Speed";
        const string inStateName = "In";
        const string outStateName = "Out";
        const string finishedStateName = "Finished";

        public bool IsPlaying
        {
            get
            {
                if (styleAnimator == null || !gameObject.activeInHierarchy) { return false; }
                return !IsInState(finishedStateName);
            }
        }

        public bool UseUnscaledTime
        {
            get { return useUnscaledTime; }
            set
            {
                useUnscaledTime = value;
                UpdateAnimatorTimeMode();
            }
        }

        public float AnimationSpeed
        {
            get { return animationSpeed; }
            set
            {
                animationSpeed = value;
                if (styleAnimator != null) { styleAnimator.SetFloat(animSpeed, animationSpeed); }
            }
        }

        void Awake()
        {
            if (styleAnimator == null) { styleAnimator = GetComponent<Animator>(); }
            UpdateAnimatorTimeMode();
        }

        void OnEnable()
        {
            if (playOnEnable)
            {
                Play();
            }
        }

        void OnDisable()
        {
            onDisable?.Invoke();
            StopAllCoroutinesInternal();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (styleAnimator != null)
            {
                UpdateAnimatorTimeMode();
            }
        }
#endif

        public void Play()
        {
            // Enable the object
            gameObject.SetActive(true);

            // Check in case it's disabled by parent
            if (!gameObject.activeInHierarchy)
                return;

            // Initialize the speed and time mode
            InitializeSpeedInternal();
            UpdateAnimatorTimeMode();

            // Call Play function
            PlayIn();

            // Process OnEnable events
            onEnable?.Invoke();

            // Start out timer if enabled
            if (playOutAnimation)
            {
                StopAllCoroutinesInternal();
                timerCoroutine = StartCoroutine(StartTimer());
                disableCoroutine = StartCoroutine(DisableStyle());
            }
        }

        public void Stop()
        {
            gameObject.SetActive(false);
        }

        public void PlayIn()
        {
            // Enable the object
            gameObject.SetActive(true);

            // Check in case it's disabled by parent
            if (!gameObject.activeInHierarchy)
                return;

            // Stop previous coroutines
            StopAllCoroutinesInternal();

            // Play the animation
            styleAnimator.StopPlayback();
            styleAnimator.Play(inStateName);
        }

        public void PlayOut()
        {
            // Check for animation states and play out
            if (gameObject.activeInHierarchy && !IsInState(finishedStateName))
            {
                styleAnimator.Play(outStateName);
            }
        }

        void InitializeSpeedInternal()
        {
            if (styleAnimator != null)
            {
                styleAnimator.SetFloat(animSpeed, animationSpeed);
            }
        }

        public void InitializeSpeed(float speed)
        {
            AnimationSpeed = speed;
        }

        void UpdateAnimatorTimeMode()
        {
            if (styleAnimator != null)
            {
                styleAnimator.updateMode = useUnscaledTime
                    ? AnimatorUpdateMode.UnscaledTime
                    : AnimatorUpdateMode.Normal;
            }
        }

        void StopAllCoroutinesInternal()
        {
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
            if (disableCoroutine != null)
            {
                StopCoroutine(disableCoroutine);
                disableCoroutine = null;
            }
        }

        bool IsInState(string stateName)
        {
            return styleAnimator != null && styleAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
        }

        IEnumerator StartTimer()
        {
            if (useUnscaledTime) { yield return new WaitForSecondsRealtime(showFor); }
            else { yield return new WaitForSeconds(showFor); }

            PlayOut();

            if (loopAnimations) { disableCoroutine = StartCoroutine(DisableStyle()); }
            timerCoroutine = null;
        }

        IEnumerator DisableStyle()
        {
            // Wait until we're in the Finished state
            while (!IsInState(finishedStateName))
                yield return null;

            if (loopAnimations)
            {
                styleAnimator.Play(inStateName);
                timerCoroutine = StartCoroutine(StartTimer());
            }
            else if (disableOnOut)
            {
                gameObject.SetActive(false);
            }

            disableCoroutine = null;
        }
    }
}