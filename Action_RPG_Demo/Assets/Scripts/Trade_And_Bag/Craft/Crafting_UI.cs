using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crafting_UI : MonoBehaviour
{
    public GameObject Crafting_Button;
    public RectTransform content;
    public List<GameObject> CraftButton_List = new List<GameObject>();

    public int Space_Between;
    public int original_offsetx;
    public int original_offsety;
    public RectTransform original;

    public GameObject StartCam;
    public GameObject EndCam;
    public Camera CraftCam;
    public float CameraMoveLerpSpeed = 3f;
    public bool HaveInit;

    private Vector3 cameraTargetPos;
    private Quaternion cameraTargetRot;
    private bool isCameraAnimating;
    private bool hasTriggerInit;
    private Action CompleteCallback;

    public void Awake()
    {
        Camera[] cameras = GameObject.FindObjectsOfType<Camera>(true);
        foreach (var item in cameras)
        {
            if (item.gameObject.layer == LayerMask.NameToLayer("CraftUI"))
            {
                CraftCam = item;
                break;
            }
        }
        HaveInit = false;
    }

    public void Update()
    {
        if (!isCameraAnimating)
        {
            return;
        }
        CraftCam.transform.position = Vector3.Lerp(
            CraftCam.transform.position,
            cameraTargetPos,
            CameraMoveLerpSpeed * Time.unscaledDeltaTime
        );
        CraftCam.transform.rotation = Quaternion.Lerp(
            CraftCam.transform.rotation,
            cameraTargetRot,
            CameraMoveLerpSpeed * Time.unscaledDeltaTime
        );

        float posDistance = Vector3.Distance(CraftCam.transform.position, cameraTargetPos);
        float angleDiff = Quaternion.Angle(CraftCam.transform.rotation, cameraTargetRot);

        if (posDistance < 0.02f && angleDiff < 0.5f)
        {
            isCameraAnimating = false;
            CraftCam.transform.SetPositionAndRotation(cameraTargetPos, cameraTargetRot);

            if (!hasTriggerInit)
            {
                hasTriggerInit = true;
                Game_Event.instance.Init_Crafting();
                CompleteCallback?.Invoke();
                CompleteCallback = null;
            }
        }
    }

    public void OnEnable()
    {
        hasTriggerInit = false;
        isCameraAnimating = false;
        CompleteCallback = null;

        Game_Event.instance.Spawn_Crafting_Button -= Crafting_Spawner;
        Game_Event.instance.Spawn_Crafting_Button += Crafting_Spawner;
    }

    public void OnDisable()
    {
        Game_Event.instance.Spawn_Crafting_Button -= Crafting_Spawner;
        hasTriggerInit = false;
        isCameraAnimating = false;
        CompleteCallback = null;
    }

    public void Crafting_Spawner(Crafting_SO craft, int index)
    {
        GameObject btn = Instantiate(Crafting_Button, content);
        btn.transform.SetParent(content);
        RectTransform btnRect = btn.GetComponent<RectTransform>();
        btnRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 2000);
        btnRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 200);
        CraftButton_List.Add(btn);
        Single_Craft_UI single_Craft = btn.GetComponent<Single_Craft_UI>();
        single_Craft.crafting = craft;
        Debug.Log("生成第" + index + "个按钮，按钮内容是" + craft.Map_Name + ",坐标为" + btnRect.anchoredPosition);
    }

    public void ResetCraftCam(Action onComplete = null)
    {
        if (!HaveInit)
        {
            HaveInit = true;
            hasTriggerInit = false;
            CompleteCallback = onComplete;

            CraftCam.transform.SetPositionAndRotation(
                StartCam.transform.position,
                StartCam.transform.rotation
            );

            cameraTargetPos = EndCam.transform.position;
            cameraTargetRot = EndCam.transform.rotation;
            isCameraAnimating = true;
        }
        else
        {
            CraftCam.transform.SetPositionAndRotation(
                EndCam.transform.position,
                EndCam.transform.rotation
            );
            if (!hasTriggerInit)
            {
                hasTriggerInit = true;
                Game_Event.instance.Init_Crafting();
                onComplete?.Invoke();
            }
        }
    }
}