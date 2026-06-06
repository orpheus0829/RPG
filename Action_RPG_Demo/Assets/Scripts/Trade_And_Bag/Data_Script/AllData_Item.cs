using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AllItem_Data", menuName = "All_Data/AllItem_Data")]
public class AllData_Item : ScriptableObject
{
    public List<Item_Data> Data_List;
}
