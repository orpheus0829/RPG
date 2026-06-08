using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Chapter_Data", menuName = "Story/Chapter_Data")]
public class Chapter_SO : ScriptableObject
{
    public List<Episode_SO> Chapters = new List<Episode_SO>();
}
