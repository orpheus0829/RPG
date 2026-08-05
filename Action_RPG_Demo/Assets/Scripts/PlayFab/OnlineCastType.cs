using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OnlinePanelType
{
    Invitation,
    Chat,
}
public class OnlineCastType : MonoBehaviour
{
    public OnlinePanelType type;
    [Header("Hover²ÄÖÊÑÕÉ«")]
    public Color NormalColor;
    public Color HoverColor;
    public Renderer TargetRenderer;
    public void Awake()
    {
        TargetRenderer = GetComponent<MeshRenderer>();
    }
}
