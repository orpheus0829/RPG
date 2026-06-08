using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

[CustomEditor(typeof(AllCrafting_Maps))]
public class All_Map_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        AllCrafting_Maps allMap_ = target as AllCrafting_Maps;
        EditorGUILayout.Space(10);
        if (GUILayout.Button("获取所有可获取物品数据盒", GUILayout.Height(35)))
        {
            Collect_MapDta(allMap_);
            EditorUtility.DisplayDialog("完成", "已成功读取文件夹内所有数据", "确定");
        }
    }
    private void Collect_MapDta(AllCrafting_Maps allData_Map)
    {
        allData_Map.Crafting_Maps.Clear();
        if (!Directory.Exists(allData_Map.Allmap_FolderPath))
        {
            Debug.Log($"文件夹不存在:{allData_Map.Allmap_FolderPath}");
            return;
        }
        string[] guids = AssetDatabase.FindAssets("t:Crafting_SO", new[] { allData_Map.Allmap_FolderPath });
        foreach (string guid in guids)
        {
            string assetpath = AssetDatabase.GUIDToAssetPath(guid);
            Crafting_SO map_Data = AssetDatabase.LoadAssetAtPath<Crafting_SO>(assetpath);
            if (map_Data)
            {
                allData_Map.Crafting_Maps.Add(map_Data);
            }
        }
        EditorUtility.SetDirty(allData_Map);
        AssetDatabase.SaveAssets();
    }
}
