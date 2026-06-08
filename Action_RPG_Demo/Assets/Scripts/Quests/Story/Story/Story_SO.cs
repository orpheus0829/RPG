using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Story_Data", menuName = "Story/Story_Data")]
public class Story_SO : ScriptableObject
{
    public List<Chapter_SO> Chapters = new List<Chapter_SO>();
}
