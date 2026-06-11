using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ID_Data", menuName = "Data/ID_Data")]
public class IdRegistrySO : ScriptableObject
{
    public List<string> AllID = new List<string>();
}
