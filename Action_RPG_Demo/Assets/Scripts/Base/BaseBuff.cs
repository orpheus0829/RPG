using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseBuff : ScriptableObject
{
    public List<string> SuitableTags = new List<string>();
    public string BuffName;
    public Sprite BuffIcon;

    public float Duration;
    public float ActiveInterval;
}
