using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class DataSum
{
    public bool HaveData;
    public string datatip;
    public MainStoryData storyData;
    public string bagJson;
    public string weaponJson;
    public string equipJson;
    public int money;
}
public class SaveLoadDataCtrl : MonoBehaviour, IPointerClickHandler
{
    public int SaveSlotIndex;
    public TextMeshProUGUI DataText;
    public Button Btn;
    public DataSum CurData = new DataSum();

    public const string path = "AllDataPath";
    private string FullSavePath => $"{Application.persistentDataPath}/{path}_{SaveSlotIndex}.json";
    public void Awake()
    {
        Btn = GetComponent<Button>();
        DataText = GetComponentInChildren<TextMeshProUGUI>();
        SaveSlotIndex = transform.GetSiblingIndex() + 1;
    }
    public void Start()
    {
        LoadAllData();
        RefreshSlotText();
    }
    private void RefreshSlotText()
    {
        if (!CurData.HaveData)
        {
            DataText.text = $"存档{SaveSlotIndex}：无存档";
        }
        else
        {
            DataText.text = CurData.datatip;
        }
    }
    public void SaveAllData()
    {
        string json = JsonUtility.ToJson(CurData);
        File.WriteAllText(FullSavePath, json);
        Debug.Log($"存档{SaveSlotIndex}保存成功,路径:{FullSavePath}");
        RefreshSlotText();
    }
    public void LoadAllData()
    {
        if (!File.Exists(FullSavePath))
        {
            CurData = new DataSum();
            CurData.HaveData = false;
            return;
        }
        string jsonText = File.ReadAllText(FullSavePath);
        CurData = JsonUtility.FromJson<DataSum>(jsonText);
        Debug.Log($"读取存档{SaveSlotIndex}成功");
    }
    public void DeleteSaveFile()
    {
        if (File.Exists(FullSavePath))
        {
            File.Delete(FullSavePath);
            Debug.Log($"存档{SaveSlotIndex}文件已删除");
        }
        CurData = new DataSum();
        CurData.HaveData = false;
        RefreshSlotText();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            //覆盖存档
            Action SaveCurrentProgress = () =>
            {
                Player player = FindObjectOfType<Player>();
                Player_Bag bag = player.bag;

                Bag_Save_Data mainBag = bag.ExportBagData();
                Bag_Save_Data weaponBag = bag.ExportWeaponBagData();
                CurData.bagJson = JsonUtility.ToJson(mainBag);
                CurData.weaponJson = JsonUtility.ToJson(weaponBag);

                EquipWeaponData equipSave = player.ExportWeaponData();
                CurData.equipJson = JsonUtility.ToJson(equipSave);

                CurData.money = PlayerPrefs.GetInt("Money", 0);
                string questTitle = "无任务";
                if (Story_Mgr.instance.CurQuest != null)
                {
                    questTitle = Story_Mgr.instance.CurQuest.Quest_Title;
                }

                CurData.storyData = new MainStoryData()
                {
                    ChapterID = Story_Mgr.instance.CurStory.ChapterID,
                    EpisodeID = Story_Mgr.instance.CurStory.EpisodeID,
                    QuestID = Story_Mgr.instance.CurStory.QuestID
                };
                CurData.HaveData = true;
                DateTime now = DateTime.Now;
                CurData.datatip = $"{now.Year}/{now.Month}/{now.Day}  {now.Hour}:{now.Minute}:{now.Second}——{questTitle}";
                SaveAllData();
            };

            if (!CurData.HaveData)
            {
                Panel_Mgr.instance.ShowComfirmPanel($"存档{SaveSlotIndex}无存档,是否存入当前进度?", false, SaveCurrentProgress);
            }
            else
            {
                Panel_Mgr.instance.ShowComfirmPanel($"存档{SaveSlotIndex}已有存档,是否覆盖?", false, SaveCurrentProgress);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Middle)
        {
            //删除存档
            if (!CurData.HaveData)
            {
                Panel_Mgr.instance.ShowComfirmPanel("当前无存档，请先存入存档再进行删除", true, null);
            }
            else
            {
                Action DeleteCurrentProgress = () =>
                {
                    DeleteSaveFile();
                };
                Panel_Mgr.instance.ShowComfirmPanel($"确定删除当前存档{SaveSlotIndex}({CurData.datatip})?", false, DeleteCurrentProgress);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            //加载存档
            if (!CurData.HaveData)
            {
                Panel_Mgr.instance.ShowComfirmPanel("当前无存档,请先存入存档再进行加载", true, null);
                return;
            }
            Panel_Mgr.instance.ShowComfirmPanel("是否载入此存档?当前未保存进度会丢失", false, () =>
            {
                StartCoroutine(LoadWholeData());
            });
        }
    }
    public IEnumerator LoadWholeData()
    {
        Panel_Mgr.instance.HideAllPanel();
        Panel_Mgr.instance.ConfirmPanel.HidePanel();
        yield return null;
        Story_Mgr.instance.CurStory = new MainStoryData()
        {
            ChapterID = CurData.storyData.ChapterID,
            EpisodeID = CurData.storyData.EpisodeID,
            QuestID = CurData.storyData.QuestID
        };
        Story_Mgr.instance.CurEnemys.Clear();
        Story_Mgr.instance.CurDrops.Clear();
        Story_Mgr.instance.CurActor = null;
        PlayerPrefs.SetInt("Money", CurData.money);
        PlayerPrefs.Save();
        if (GameObject.FindGameObjectWithTag("Player").TryGetComponent(out Player pl))
        {
            Player_Bag bag = pl.bag;
            Bag_Save_Data mainData = JsonUtility.FromJson<Bag_Save_Data>(CurData.bagJson);
            Bag_Save_Data weaponData = JsonUtility.FromJson<Bag_Save_Data>(CurData.weaponJson);
            bag.CurrentViewBagType = ViewBagType.MainItemBag;
            bag.ImportBagData(mainData);
            bag.ImportWeaponBagData(weaponData);

            yield return null;

            bag.ReClean_Bag_Display();
            bag.GenerateCurrentBagSlots();
            bag.Refresh_Bag_Display();
            bag.Init_AllResortList();

            EquipWeaponData loadEquip = JsonUtility.FromJson<EquipWeaponData>(CurData.equipJson);
            pl.ImportWeaponData(loadEquip);
            pl.SaveWeaponData();
            Game_Event.instance.BroadcastRefreshAllArmEquip(pl.EquipData, bag.allData_Item);
        }
        yield return null;
        TimeMgr.instance.CreateTimer(TimeMgr.TimerMode.RealTimeUnscaled, 0, 1f, null, () =>
        {
            LoadingMgr.instance.StartTransition("City", true);
        });
        yield return new WaitForSecondsRealtime(0.15f);
        if (GameObject.FindGameObjectWithTag("Player").TryGetComponent(out Player p))
        {
            CameraPivot.instance.target = p.transform;
            Game_Event.instance.BroadcastRefreshAllArmEquip(p.EquipData, p.bag.allData_Item);
        }
    }
}
