using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnlineUI : MonoBehaviour, IPointerClickHandler, IPointerMoveHandler, IPointerExitHandler
{
    [Header("点位空物体")]
    public GameObject Menu;
    public GameObject Invitation;
    public GameObject Chat;

    [Header("相机")]
    public Camera OnlineCam;
    public float CamLerpSpeed = 3f;

    [Header("交互UI")]
    public RectTransform OnlineViewUiRect;

    [Header("功能面板")]
    public GameObject InvitationPanel;
    public GameObject ChatPanel;

    [Header("方块颜色速度")]
    public float BlockColorLerpSpeed = 8f;

    private Vector3 _CamTargetPos;
    private Quaternion _CamTargetRot;
    private bool _IsCameraAnimating;
    private GameObject _TargetPointObj;

    private OnlineCastType _LastHoverBlock;
    private Color _LastBlockCurrentColor;

    private bool _IsAtMenu;

    void Awake()
    {
        Camera[] Objs = Object.FindObjectsOfType<Camera>();
        foreach (Camera I in Objs)
        {
            if (I.gameObject.layer == LayerMask.NameToLayer("Online"))
            {
                OnlineCam = I;
                break;
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(2))
        {
            GoToPoint(Menu);
        }
        if (_IsCameraAnimating)
        {
            OnlineCam.transform.position = Vector3.Lerp(OnlineCam.transform.position, _CamTargetPos, CamLerpSpeed * Time.unscaledDeltaTime);
            OnlineCam.transform.rotation = Quaternion.Lerp(OnlineCam.transform.rotation, _CamTargetRot, CamLerpSpeed * Time.unscaledDeltaTime);

            float PosDistance = Vector3.Distance(OnlineCam.transform.position, _CamTargetPos);
            float AngleDiff = Quaternion.Angle(OnlineCam.transform.rotation, _CamTargetRot);
            if (PosDistance < 0.02f && AngleDiff < 0.5f)
            {
                _IsCameraAnimating = false;
                OnlineCam.transform.SetPositionAndRotation(_CamTargetPos, _CamTargetRot);
                OnCameraArriveTarget(_TargetPointObj);
            }
        }
        if (_IsAtMenu == true && _LastHoverBlock != null && _LastHoverBlock.TargetRenderer != null)
        {
            _LastBlockCurrentColor = Color.Lerp(_LastBlockCurrentColor, _LastHoverBlock.HoverColor, BlockColorLerpSpeed * Time.unscaledDeltaTime);
            _LastHoverBlock.TargetRenderer.material.color = _LastBlockCurrentColor;
        }
    }

    void OnEnable()
    {
        HideAllPanels();
        GoToPoint(Menu);
        ClearHoverBlock();
    }

    void OnDisable()
    {
        OnlineCam.transform.SetPositionAndRotation(Menu.transform.position, Menu.transform.rotation);
        HideAllPanels();
        _IsCameraAnimating = false;
        ClearHoverBlock();
    }

    void HideAllPanels()
    {
        InvitationPanel.SetActive(false);
        ChatPanel.SetActive(false);
    }

    public void GoToPoint(GameObject PointObj)
    {
        if (_IsCameraAnimating)
        {
            return;
        }

        _CamTargetPos = PointObj.transform.position;
        _CamTargetRot = PointObj.transform.rotation;
        _TargetPointObj = PointObj;
        _IsCameraAnimating = true;

        HideAllPanels();
    }

    void OnCameraArriveTarget(GameObject PointObj)
    {
        HideAllPanels();

        if (PointObj == Menu)
        {
            _IsAtMenu = true;
        }
        else
        {
            _IsAtMenu = false;
        }
        if (PointObj == Invitation)
        {
            InvitationPanel.SetActive(true);
        }
        if (PointObj == Chat)
        {
            ChatPanel.SetActive(true);
        }
    }

    void ClearHoverBlock()
    {
        if (_LastHoverBlock && _LastHoverBlock.TargetRenderer)
        {
            _LastHoverBlock.TargetRenderer.material.color = _LastHoverBlock.NormalColor;
        }
        _LastHoverBlock = null;
    }

    private bool RayCheckInsideUi(PointerEventData EventData, out RaycastHit Hit)
    {
        Hit = new RaycastHit();
        RectTransform Rect = OnlineViewUiRect;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(Rect, EventData.position, EventData.pressEventCamera, out Vector2 LocalPoint))
        {
            return false;
        }

        Vector2 RectSize = Rect.rect.size;
        float U = (LocalPoint.x + RectSize.x * 0.5f) / RectSize.x;
        float V = (LocalPoint.y + RectSize.y * 0.5f) / RectSize.y;

        if (U < 0 || U > 1 || V < 0 || V > 1)
        {
            return false;
        }

        Ray Ray = OnlineCam.ViewportPointToRay(new Vector3(U, V, 0));
        return Physics.Raycast(Ray, out Hit, 200f);
    }

    private void CheckHoverObject(PointerEventData EventData)
    {
        if (_IsAtMenu == false)
        {
            ClearHoverBlock();
            return;
        }

        if (RayCheckInsideUi(EventData, out RaycastHit Hit))
        {
            OnlineCastType Cast = Hit.collider.GetComponent<OnlineCastType>();
            if (Cast)
            {
                if (_LastHoverBlock != Cast)
                {
                    ClearHoverBlock();
                    _LastHoverBlock = Cast;
                    if (_LastHoverBlock.TargetRenderer == null)
                    {
                        _LastHoverBlock.TargetRenderer = Hit.collider.GetComponent<Renderer>();
                    }
                    _LastBlockCurrentColor = _LastHoverBlock.NormalColor;
                }
                return;
            }
        }
        ClearHoverBlock();
    }

    public void OnPointerClick(PointerEventData EventData)
    {
        if (_IsCameraAnimating)
        {
            return;
        }
        ClearHoverBlock();
        if (RayCheckInsideUi(EventData, out RaycastHit Hit))
        {
            OnlineCastType Cast = Hit.collider.GetComponent<OnlineCastType>();
            if (Cast == null)
            {
                GoToPoint(Menu);
                return;
            }

            switch (Cast.type)
            {
                case OnlinePanelType.Invitation:
                    GoToPoint(Invitation);
                    break;
                case OnlinePanelType.Chat:
                    GoToPoint(Chat);
                    break;
            }
        }
        else
        {
            GoToPoint(Menu);
        }
    }

    public void OnPointerMove(PointerEventData EventData)
    {
        CheckHoverObject(EventData);
    }

    public void OnPointerExit(PointerEventData EventData)
    {
        ClearHoverBlock();
    }
}