using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

[CustomEditor(typeof(AllData_Item))]
public class All_Data_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        AllData_Item allData_ = target as AllData_Item;
        EditorGUILayout.Space(10);
        if (GUILayout.Button("获取所有可获取物品数据盒", GUILayout.Height(35)))
        {
            Collect_ItemDta(allData_);
            EditorUtility.DisplayDialog("完成","已成功读取文件夹内所有数据", "确定");
        }
    }
    private void Collect_ItemDta(AllData_Item allData_Item)
    {
        allData_Item.Data_List.Clear();
        if (!Directory.Exists(allData_Item.Allitem_FolderPath))
        {
            Debug.Log($"文件夹不存在:{allData_Item.Allitem_FolderPath}");
            return;
        }
        string[] guids = AssetDatabase.FindAssets("t:Item_Data", new[] { allData_Item.Allitem_FolderPath });
        foreach(string guid in guids)
        {
            string assetpath = AssetDatabase.GUIDToAssetPath(guid);
            Item_Data item_Data = AssetDatabase.LoadAssetAtPath<Item_Data>(assetpath);
            if (item_Data)
            {
                allData_Item.Data_List.Add(item_Data);
            }
        }
        EditorUtility.SetDirty(allData_Item);
        AssetDatabase.SaveAssets();
    }
}
