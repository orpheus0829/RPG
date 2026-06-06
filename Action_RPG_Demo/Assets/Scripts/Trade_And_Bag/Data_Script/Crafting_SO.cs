using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Crafting_Map_Data", menuName = "Data/Crafting_Map_Data")]
public class Crafting_SO : ScriptableObject
{
    [Serializable]
    public class Crafting_Material
    {
        public Item_Data Material;
        public int Number;
    }
    [Serializable]
    public class Crafting_Result
    {
        public Item_Data Product;
        public int Res_Number;
    }
    public string Map_Name;
    public List<Crafting_Material> crafting_Materials;
    public List<Crafting_Result> crafting_Results;
}
