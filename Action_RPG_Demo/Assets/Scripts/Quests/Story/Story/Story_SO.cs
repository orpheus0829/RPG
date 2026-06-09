using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Story_Data", menuName = "Story/Story_Data")]
public class Story_SO : ScriptableObject
{
    public string Story_Title;
    public int Story_ID;
    [TextArea(0, 3)] public string Story_Introduction;
    public List<Chapter_SO> Chapters = new List<Chapter_SO>();
}
