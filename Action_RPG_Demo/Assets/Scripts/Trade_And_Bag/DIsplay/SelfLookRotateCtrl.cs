using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class SelfLookRotateCtrl : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerMoveHandler, IPointerExitHandler
{
    public Player Pl;
    public GameObject DisplayModel;
    public GameObject Arms;
    public Camera SelfLookCam;
    public GameObject CamLocation;
    public GameObject TvLocation;
    public TextMeshProUGUI TextMesh;
    [Header("生成位置")]
    public Vector3 SpawOffset;
    [Header("拖拽灵敏度")]
    public float Sensitivity;

    [Header("电视机蒙版")]
    public GameObject TvHoverMaskQuad;
    public Material TvMaskMat;
    public const float HoverAlpha = 0.15f;
    public const float LerpSpeed = 8f;
    public float TargetAlpha;
    public GameObject TvMark;
    public GameObject PlayerAttributes;
    public float WriteSpeed;

    [Header("相机移动参数")]
    public float CameraMoveLerpSpeed = 3f;
    private Vector3 CameraTargetPos;
    private Quaternion CameraTargetRot;
    private bool IsCameraAnimating;
    private bool IsCameraToTv;
    private TimeMgr.TimerTask AttributeTimerTask;


    private Vector2 LastDragPos;
    private bool IsDragging;
    public RectTransform EquipStageUiRect;

    private TimelineAsset IdleTimeline;
    private TimelineAsset AfkTimeline;
    private PlayableDirector ModelDirector;
    private bool IsPlayingAfk;

    public void Awake()
    {
        EquipStageUiRect = GetComponent<RectTransform>();
        Camera[] cameras = GameObject.FindObjectsOfType<Camera>(true);
        foreach (var item in cameras)
        {
            if (item.gameObject.layer == LayerMask.NameToLayer("EquipStage"))
            {
                SelfLookCam = item;
                break;
            }
        }
        Pl = GameObject.FindObjectOfType<Player>();
        TextMesh = PlayerAttributes.GetComponent<TextMeshProUGUI>();

        if (TvHoverMaskQuad)
        {
            Renderer rd = TvHoverMaskQuad.GetComponent<Renderer>();
            TvMaskMat = new Material(rd.material);
            rd.material = TvMaskMat;
            Color initColor = TvMaskMat.color;
            initColor.a = 0f;
            TvMaskMat.color = initColor;
            TargetAlpha = 0f;
        }
        Arms = gameObject.transform.GetChild(0).gameObject;
    }

    public void Start()
    {

    }

    public void Update()
    {
        if (!TvMaskMat)
        {
            return;
        }
        if (IsCameraToTv)
        {
            SetTvHighlight(false);
        }
        Color currentColor = TvMaskMat.color;
        currentColor.a = Mathf.Lerp(currentColor.a, TargetAlpha, LerpSpeed * Time.unscaledDeltaTime);
        TvMaskMat.color = currentColor;

        if (IsCameraAnimating)
        {
            SelfLookCam.transform.position = Vector3.Lerp(SelfLookCam.transform.position, CameraTargetPos, CameraMoveLerpSpeed * Time.unscaledDeltaTime);
            SelfLookCam.transform.rotation = Quaternion.Lerp(SelfLookCam.transform.rotation, CameraTargetRot, CameraMoveLerpSpeed * Time.unscaledDeltaTime);

            float posDistance = Vector3.Distance(SelfLookCam.transform.position, CameraTargetPos);
            float angleDiff = Quaternion.Angle(SelfLookCam.transform.rotation, CameraTargetRot);
            if (posDistance < 0.02f && angleDiff < 0.5f)
            {
                IsCameraAnimating = false;
                SelfLookCam.transform.SetPositionAndRotation(CameraTargetPos, CameraTargetRot);
                if (IsCameraToTv)
                {
                    TimeMgr.instance.CreateTimer(TimeMgr.TimerMode.RealTimeUnscaled, 0, 0.15f, null, () =>
                    {
                        ShowTelevision(true);
                    });
                }
            }
        }
    }

    public void OnEnable()
    {
        Game_Event.instance.ShowArmSlots += ShowArmWhenTv;

        SelfLookCam.transform.SetPositionAndRotation(CamLocation.transform.position, CamLocation.transform.rotation);
        RefreshModle();
        TextMesh.text = string.Empty;
        Arms.SetActive(true);
    }

    public void OnDisable()
    {
        Game_Event.instance.ShowArmSlots -= ShowArmWhenTv;

        BackToMain();
        if (DisplayModel != null)
        {
            if (ModelDirector != null)
            {
                ModelDirector.stopped -= OnPlayableFinished;
                ModelDirector.Stop();
            }
        }
        Destroy(DisplayModel);
        DisplayModel = null;
        ModelDirector = null;
        IdleTimeline = null;
        AfkTimeline = null;
        IsPlayingAfk = false;
        IsDragging = false;

        if (TvMaskMat)
        {
            Color c = TvMaskMat.color;
            c.a = 0;
            TvMaskMat.color = c;
            TargetAlpha = 0f;
        }
    }
    #region 模型展示
    public GameObject GetModel()
    {
        if (Pl.allrole.Count <= 0)
        {
            return null;
        }
        GameObject sourceRole = Pl.allrole[Pl.CurRoleIndex].RoleObj;
        GameObject previewCopy = GameObject.Instantiate(sourceRole);
        DfsModifyLayer(previewCopy, LayerMask.NameToLayer("EquipStage"));
        previewCopy.SetActive(false);
        return previewCopy;
    }

    public void RefreshModle()
    {
        if (DisplayModel != null)
        {
            Destroy(DisplayModel);
            DisplayModel = null;
        }
        DisplayModel = GetModel();
        if (!DisplayModel)
        {
            return;
        }
        TimeMgr.instance.CreateTimer(TimeMgr.TimerMode.RealTimeUnscaled, 0f, 0.5f, null, () =>
        {
            PickNoticeMgr.instance.ShowDialogueTip($"{Pl.allrole[Pl.CurRoleIndex].RoleID}", "又有什么事情?", 3f);
        });
        DisplayModel.GetComponent<Animator>().ApplyBuiltinRootMotion();
        DisplayModel.name = $"展示模型:{Pl.allrole[Pl.CurRoleIndex].RoleID}";
        Vector3 origin = SelfLookCam.gameObject.transform.position;
        DisplayModel.transform.SetPositionAndRotation(origin + SpawOffset, Quaternion.identity);
        origin.y = DisplayModel.transform.position.y;
        DisplayModel.transform.LookAt(origin);
        DisplayModel.SetActive(true);

        ActionControl ctrl = DisplayModel.GetComponent<ActionControl>();
        ModelDirector = DisplayModel.GetComponent<PlayableDirector>();
        IdleTimeline = ctrl.Character.Idle.timeline;
        AfkTimeline = ctrl.Character.AfkIdle.timeline;

        Destroy(ctrl);
        ModelDirector.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;

        ModelDirector.playableAsset = IdleTimeline;
        ModelDirector.stopped -= OnPlayableFinished;
        ModelDirector.stopped += OnPlayableFinished;
        ModelDirector.time = 0;
        ModelDirector.Play();
        IsPlayingAfk = false;
    }

    public void DfsModifyLayer(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
        {
            DfsModifyLayer(child.gameObject, layer);
        }
    }
    public void OnPlayableFinished(PlayableDirector pd)
    {
        if (IsPlayingAfk)
        {
            IsPlayingAfk = false;
            pd.playableAsset = IdleTimeline;
        }
        pd.time = 0;
        pd.Play();
    }
    #endregion
    #region 操作
    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsCameraToTv && !IsCameraAnimating)
        {
            BackToMain();
        }
        if (RayCheckTv(eventData))
        {
            OnClickTelevision();
            return;
        }
        if (!ModelDirector || IsPlayingAfk || !AfkTimeline)
        {
            return;
        }

        IsPlayingAfk = true;
        ModelDirector.playableAsset = AfkTimeline;
        ModelDirector.time = 0;
        ModelDirector.Play();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        IsDragging = true;
        LastDragPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!IsDragging)
        {
            return;
        }
        bool mouseInsideUi = RectTransformUtility.RectangleContainsScreenPoint(EquipStageUiRect, eventData.position, eventData.pressEventCamera);
        if (!mouseInsideUi)
        {
            return;
        }

        Vector2 delta = eventData.position - LastDragPos;
        DisplayModel.transform.Rotate(Vector3.up, delta.x * -Sensitivity, Space.World);
        LastDragPos = eventData.position;

        CheckHoverTv(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        IsDragging = false;
    }
    public void OnPointerMove(PointerEventData eventData)
    {
        CheckHoverTv(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetTvHighlight(false);
    }
    #endregion
    #region 电视机
    public bool RayCheckTv(PointerEventData eventData)
    {
        RectTransform rect = EquipStageUiRect;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out Vector2 LocalPoint))
        {
            return false;
        }

        Vector2 rectSize = rect.rect.size;
        float U = (LocalPoint.x + rectSize.x * 0.5f) / rectSize.x;
        float V = (LocalPoint.y + rectSize.y * 0.5f) / rectSize.y;

        if (U < 0 || U > 1 || V < 0 || V > 1)
        {
            return false;
        }

        Ray ray = SelfLookCam.ViewportPointToRay(new Vector3(U, V, 0));
        if (Physics.Raycast(ray, out RaycastHit HitInfo, 200f))
        {
            if (HitInfo.collider is BoxCollider)
            {
                return true;
            }
        }
        return false;
    }

    public void CheckHoverTv(PointerEventData eventData)
    {
        bool TargetHoverState = RayCheckTv(eventData);
        SetTvHighlight(TargetHoverState);
    }

    public void SetTvHighlight(bool Active)
    {
        TargetAlpha = Active ? HoverAlpha : 0f;
    }

    public void OnClickTelevision()
    {
        Debug.Log("点击电视机");
        ForwardToTelevison();
    }
    public void ForwardToTelevison()
    {
        if (IsCameraAnimating || IsCameraToTv)
        {
            return;
        }
        Arms.SetActive(false);
        TvMark.SetActive(false);
        TextMesh.text = string.Empty;
        PlayerAttributes.SetActive(true);

        CameraTargetPos = TvLocation.transform.position;
        CameraTargetRot = TvLocation.transform.rotation;
        IsCameraAnimating = true;
        IsCameraToTv = true;
    }
    public void BackToMain()
    {
        Arms.SetActive(true);
        if (IsCameraAnimating || !IsCameraToTv)
        {
            return;
        }
        TextMesh.text = string.Empty;
        TvMark.SetActive(true);
        PlayerAttributes.SetActive(false);
        if (AttributeTimerTask != null)
        {
            TimeMgr.instance.StopTimer(AttributeTimerTask);
            AttributeTimerTask = null;
        }

        CameraTargetPos = CamLocation.transform.position;
        CameraTargetRot = CamLocation.transform.rotation;
        IsCameraAnimating = true;
        IsCameraToTv = false;
    }
    public void ShowTelevision(bool showattributes)
    {
        if (showattributes)
        {
            float maxhp = Pl.damageReceiver.maxHp;
            float damage = 15 * Pl.buffReceiver.DamageFactor * (1 + 0.01f * Pl.DamageFac);
            float movespeed = Pl.Speed * Pl.buffReceiver.MoveFactor * (1 + 0.01f * Pl.SpeedFac);
            float defense = Pl.DefenseFac / (Pl.DefenseFac + 100f);
            string defence= defense == 0f ? "0" : defense.ToString("F4");
            float special = Pl.PowerFactor;
            float end = Pl.ChargeFactor;

            string attribute = $"角色属性:\n  最大血量:{maxhp}    装备加成:{Pl.MaxhpFac}%\n  伤害:{damage}    装备加成:{Pl.DamageFac}%\n  移速:{movespeed}    装备加成:{Pl.SpeedFac}%\n  防御力:{defence}    装备加成:{Pl.DefenseFac}%\n  特殊技获取速度:{special}    装备加成:{Pl.SpecialFac}%\n  终结技获取速度:{end}    装备加成:{Pl.EndFac}%";
            ShowAttributesByTick(attribute, TextMesh);
        }
    }
    public void ShowAttributesByTick(string content, TextMeshProUGUI root)
    {
        if (AttributeTimerTask != null)
        {
            TimeMgr.instance.StopTimer(AttributeTimerTask);
            AttributeTimerTask = null;
        }
        root.text = string.Empty;
        int index = 0;
        int totalLength = content.Length;
        int CharPerTick = 5;
        AttributeTimerTask = TimeMgr.instance.CreateTimer(
            TimeMgr.TimerMode.RealTimeUnscaled,
            0,
            9999f,
            null,
            null,
            () =>
            {
                index += CharPerTick;
                if (index > totalLength)
                {
                    index = totalLength;
                }
                string res = content.Substring(0, index);
                root.text = res;
                if (index >= totalLength)
                {
                    TimeMgr.instance.StopTimer(AttributeTimerTask);
                    AttributeTimerTask = null;
                }
            },
            WriteSpeed);
    }
    public void ShowArmWhenTv()
    {
        if (IsCameraToTv || IsCameraAnimating)
        {
            BackToMain();
        }
    }
    #endregion
}