using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Chapter_Data", menuName = "Story/Chapter_Data")]
public class Chapter_SO : ScriptableObject
{
    public string Chapter_Title;
    public int Chapter_ID;
    [TextArea(0, 3)] public string Chapter_Introduction;
    public List<Episode_SO> Episodes = new List<Episode_SO>();
}
