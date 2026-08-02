using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class IdCollectWindow : EditorWindow
{
    private IdRegistrySO targetRegistry;

    [MenuItem("工具/ID管理/收集场景角色ID")]
    static void OpenWindow()
    {
        IdCollectWindow win = GetWindow<IdCollectWindow>("ID收集工具");
        win.minSize = new Vector2(350, 120);
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        targetRegistry = EditorGUILayout.ObjectField("目标ID注册表", targetRegistry, typeof(IdRegistrySO), false) as IdRegistrySO;
        EditorGUILayout.Space();

        if (GUILayout.Button("扫描当前场景所有Character的ID写入注册表", GUILayout.Height(30)) && targetRegistry != null)
        {
            CollectSceneAllIds();
            EditorUtility.SetDirty(targetRegistry);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("收集完成");
        }
    }

    void CollectSceneAllIds()
    {
        Dialogue_Set[] allChars = Object.FindObjectsOfType<Dialogue_Set>();
        HashSet<string> uniqueIds = new HashSet<string>();

        foreach (var cha in allChars)
        {
            if (!string.IsNullOrWhiteSpace(cha.CharacterId))
            {
                uniqueIds.Add(cha.CharacterId);
            }
        }

        targetRegistry.AllID.Clear();
        targetRegistry.AllID.AddRange(uniqueIds);
    }
}