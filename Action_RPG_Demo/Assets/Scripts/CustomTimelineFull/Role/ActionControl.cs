using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class ActionControl : MonoBehaviour, INotificationReceiver
{
    [Header("组件绑定")]
    public Animator roleAnimator;
    public AudioSource audioSource;
    public Camera mainCamera;
    public PlayableDirector timelineDirector;

    [Header("默认动作")]
    public ActionSO idleAction;

    private ActionSO currentAction;

    [Header("动作窗口（由 Timeline 信号控制）")]
    public bool canCombo;
    public bool canInterrupt;

    [Header("攻击范围盒调试用")]
    private SphereCollider hitCollider;

    public Vector3 debugBoxOffset;
    public float debugBoxRadius;
    public bool debugDrawHitBox;

    private void Awake()
    {
        hitCollider = gameObject.AddComponent<SphereCollider>();
        hitCollider.isTrigger = true;
        hitCollider.enabled = false;
    }

    public void OnNotify(Playable origin, INotification notification, object context)
    {
        if (notification is SignalEmitter emitter)
        {
            // 拿出你真正的信号 SO
            ActionWindowSignal sig = emitter.asset as ActionWindowSignal;

            if (sig != null)
            {
                canCombo = sig.allowCombo;
                canInterrupt = sig.allowInterrupt;
                //Debug.Log($"收到信号: canCombo={canCombo}, canInterrupt={canInterrupt}");
                return;
            }
        }

        Debug.Log($"收到未知信号: {notification?.GetType().Name}");
    }

    public void PlayAction(ActionSO action)
    {
        if (action == null || action.timeline == null)
        {
            return;
        }
        Debug.Log("切换为" + action.actionName);
        currentAction = action;

        timelineDirector.Stop();
        timelineDirector.playableAsset = action.timeline;
        timelineDirector.Play();
    }

    public void OnActionEnd()
    {
        if (currentAction == null) return;

        if (currentAction.nextAction != null)
        {
            PlayAction(currentAction.nextAction);
        }
        else
        {
            PlayAction(idleAction);
        }
    }


    #region 攻击判定
    public void OpenHitBox(Vector3 offset, float radius)
    {
        hitCollider.center = offset;
        hitCollider.radius = radius;
        hitCollider.enabled = true;

        debugDrawHitBox = true;
        debugBoxOffset = offset;
        debugBoxRadius = radius;
    }
    public void CloseHitBox()
    {
        hitCollider.enabled = false;

        debugDrawHitBox = false;
    }
    public void OnDrawGizmos()
    {
        if (!debugDrawHitBox)
        {
            return;
        }

        Gizmos.color = Color.red;
        Vector3 worldPos = transform.TransformPoint(debugBoxOffset);
        Gizmos.DrawWireSphere(worldPos, debugBoxRadius);
    }
    #endregion

    #region 动画/音效/特效
    public void PlayAnimation(AnimationClip clip)
    {
        if (clip == null) return;
        roleAnimator.Play(clip.name);
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip);
    }

    public void SpawnEffect(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (prefab == null) return;
        Instantiate(prefab, pos, rot);
    }
    #endregion

    #region 相机
    public CameraMotion GetCameraMotion()
    {
        return mainCamera.GetComponent<CameraMotion>();
    }
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IDamageable target))
        {
            target.TakeDamage(20f, transform.forward);
        }
    }
}