using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
public class Player_Bag : BaseBag
{
    [Header("玩家专属")]
    public List<Item_Data> Armed = new List<Item_Data>();
    public QuickEquip quick;
    public List<Item_Data> resort_list;
    private Bag_SingleSlot[,] bagBackup;
    protected override void Awake()
    {
        base.Awake();
    }
    public void Start()
    {
        Load_Data(path);
        //ReClean_Bag_Display();
        //Refresh_Bag_Display();
        Init_Resort_List();
        //DeleteBagSaveFile();
        //if (sword)
        //{
        //    PlaceItem(sword, 0, 0);
        //    Debug.Log("测试：背包里硬塞了一把铁剑！");
        //}
        Game_Event.instance.Buy_Item += Add_Good;
        Game_Event.instance.Sell_Item += Sell_Good;
        Game_Event.instance.Last_Item_By_ID += Search_By_ID;
        Game_Event.instance.Remove_Sold_Good += Remove_Because_Sold;

        Game_Event.instance.Craft_Check += Craft_Need;
        Game_Event.instance.Crafting_Start += Craft_Add;
        quick = Panel_Mgr.instance.PlayUiPanel.gameObject.GetComponentInChildren<QuickEquip>();
    }
    public void OnEnable()
    {

    }
    public void OnDisable()
    {
        Game_Event.instance.Buy_Item -= Add_Good;
        Game_Event.instance.Sell_Item -= Sell_Good;
        Game_Event.instance.Last_Item_By_ID -= Search_By_ID;
        Game_Event.instance.Remove_Sold_Good -= Remove_Because_Sold;

        Game_Event.instance.Craft_Check -= Craft_Need;
        Game_Event.instance.Crafting_Start -= Craft_Add;
    }
    #region 查询剩余某物品
    public int Search_By_ID(Item_Data item)
    {
        List<Item_Data> Find_Item = resort_list.FindAll(it => it.item_id == item.item_id);
        int count = 0;
        foreach(var i in Find_Item)
        {
            count++;
        }
        return count;
    }
    #endregion
    #region 卖出删除物品
    public void Remove_Because_Sold(Item_Data item)
    {
        Item_Data data = resort_list.Find(it => it.item_id == item.item_id);
        resort_list.Remove(data);
        for (int i = 0; i < Bag_Row; i++)
        {
            for (int j = 0; j < Bag_Col; j++)
            {
                if (bag[i, j].Have_Item == true && bag[i, j].Start_x != -1 && bag[i, j].Start_y != -1 && bag[i,j].item_ID == item.item_id)
                {
                    RemoveItem(bag[i, j].Start_x, bag[i, j].Start_y, item.Width, item.Height);
                    ReClean_Bag_Display();
                    Refresh_Bag_Display();
                    return;
                }
            }
        }
    }
    #endregion
    #region 商店买卖
    public void Add_Good(Item_Data item)
    {
        int Coin_Now = PlayerPrefs.GetInt("Money", 0);
        if (Coin_Now < item.PriceValue)
        {
            Game_Event.instance.Send_Real_BuyItem(false);
            PickNoticeMgr.instance.ShowFieldTip("存款不足");
            Debug.Log("存款不足");
            return;
        }
        bool is_Ok = Pick_Up(item);
        if (!is_Ok)
        {
            Game_Event.instance.Send_Real_BuyItem(is_Ok);
            PickNoticeMgr.instance.ShowFieldTip("背包容量不足，清腾出空间后再购买");
            Debug.Log("背包容量不足，清腾出空间后再购买");
        }
        else
        {
            Game_Event.instance.Send_Real_BuyItem(is_Ok);
            Panel_Mgr.instance.ShowComfirmPanel($"买入{item.item_name}", true, null);
            Debug.Log("买入" + item.item_name);
            Init_Resort_List();
        }
    }
    public void Sell_Good(Item_Data item)
    {
        Item_Data data = resort_list.Find(it => it.item_id == item.item_id);
        if (data)
        {
            Game_Event.instance.Send_Real_SellItem(true);
        }
        else
        {
            Game_Event.instance.Send_Real_SellItem(false);
        }
    }
    #endregion
    #region 合成
    public bool Craft_Need(Crafting_SO craft)
    {
        Init_Resort_List();
        foreach (var matItem in craft.crafting_Materials)
        {
            int haveNum = Search_By_ID(matItem.Material);
            if (haveNum < matItem.Number)
            {
                PickNoticeMgr.instance.ShowFieldTip("材料不足:" + matItem.Material.item_name + "需" + matItem.Number + "个，仅有" + haveNum + "个");
                Debug.Log("材料不足:" + matItem.Material.item_name + "需" + matItem.Number + "个，仅有" + haveNum + "个");
                return false;
            }
        }
        List<Bag_SingleSlot> slotBackup = new List<Bag_SingleSlot>();
        List<Vector2Int> clearPosList = new List<Vector2Int>();

        foreach (var matItem in craft.crafting_Materials)
        {
            int needFind = matItem.Number;
            for (int y = 0; y < Bag_Row && needFind > 0; y++)
            {
                for (int x = 0; x < Bag_Col && needFind > 0; x++)
                {
                    Bag_SingleSlot slot = bag[y, x];
                    if (slot.Have_Item && slot.Start_x == x && slot.Start_y == y)
                    {
                        Item_Data curItem = allData_Item.Data_List.Find(d => d.item_id == slot.item_ID);
                        if (curItem == matItem.Material)
                        {
                            for (int yy = y; yy < y + curItem.Height; yy++)
                            {
                                for (int xx = x; xx < x + curItem.Width; xx++)
                                {
                                    slotBackup.Add(bag[yy, xx]);
                                    clearPosList.Add(new Vector2Int(xx, yy));
                                    bag[yy, xx].Have_Item = false;
                                    bag[yy, xx].item_ID = 0;
                                }
                            }
                            needFind--;
                        }
                    }
                }
            }
        }
        bool canAllPlace = true;
        List<Item_Data> tempProductList = new List<Item_Data>();
        foreach (var res in craft.crafting_Results)
        {
            for (int i = 0; i < res.Res_Number; i++)
            {
                tempProductList.Add(res.Product);
            }
        }
        foreach (var product in tempProductList)
        {
            Find_Empty_Location(product.Height, product.Width, out int px, out int py);
            if (px == -1 && py == -1)
            {
                canAllPlace = false;
                break;
            }
        }
        for (int i = 0; i < clearPosList.Count; i++)
        {
            Vector2Int pos = clearPosList[i];
            bag[pos.y, pos.x] = slotBackup[i];
        }
        if (!canAllPlace)
        {
            PickNoticeMgr.instance.ShowFieldTip("背包容量不足，已返还材料");
            Debug.Log("材料足够，但腾空后背包空间放不下合成产物");
            return false;
        }

        Debug.Log("材料充足且空间充足，可以合成");
        return true;
    }
    public void Craft_Add(Crafting_SO craft)
    {
        foreach (var mat in craft.crafting_Materials)
        {
            int needRemove = mat.Number;
            for (int y = 0; y < Bag_Row && needRemove > 0; y++)
            {
                for (int x = 0; x < Bag_Col && needRemove > 0; x++)
                {
                    var slot = bag[y, x];
                    if (slot.Have_Item && slot.Start_x == x && slot.Start_y == y)
                    {
                        Item_Data item = allData_Item.Data_List.Find(i => i.item_id == slot.item_ID);
                        if (item == mat.Material)
                        {
                            RemoveItem(x, y, item.Width, item.Height);
                            needRemove--;
                        }
                    }
                }
            }
        }
        foreach (var res in craft.crafting_Results)
        {
            for (int i = 0; i < res.Res_Number; i++)
            {
                Pick_Up(res.Product);
            }
        }
        ReClean_Bag_Display();
        Refresh_Bag_Display();
        Init_Resort_List();
        Save_Bag(path);
        PickNoticeMgr.instance.ShowFieldTip("合成成功");
        Debug.Log("合成完成");
    }
    #endregion
    public override void Init_Bag()
    {
        base.Init_Bag();
    }
    #region 保存与读取
    public override void Save_Bag(string path)
    {
        string fullPath = Application.persistentDataPath + "/" + path + ".json";
        Bag_Save_Data save = new Bag_Save_Data { row = Bag_Row, col = Bag_Col };
        for (int y = 0; y < Bag_Row; y++)
        {
            for (int x = 0; x < Bag_Col; x++)
            {
                save.slots.Add(bag[y, x]);
            }
        }
        string jsonStr = JsonUtility.ToJson(save);
        File.WriteAllText(fullPath, jsonStr);
        Debug.Log($"存档路径:{fullPath}");
    }
    public override void Load_Data(string path)
    {
        string pathFull = Application.persistentDataPath + "/" + path + ".json";
        Debug.Log($"文件地址：{pathFull}");
        if (File.Exists(pathFull))
        {
            string json = File.ReadAllText(pathFull);
            Bag_Save_Data save = JsonUtility.FromJson<Bag_Save_Data>(json);
            Init_Bag();
            int index = 0;
            for (int y = 0; y < save.row; y++)
            {
                for (int x = 0; x < save.col; x++)
                {
                    if (index >= save.slots.Count)
                    {
                        Debug.Log("存档列表长度不足，停止读取");
                        break;
                    }
                    if (y < Bag_Row && x < Bag_Col)
                    {
                        bag[y, x] = save.slots[index];
                    }
                    index++;
                }
            }
        }
        else
        {
            Debug.LogWarning($"存档文件不存在，{pathFull},初始化空白背包");
            Init_Bag();
        }
    }
    public Bag_Save_Data ExportBagData()
    {
        Bag_Save_Data save = new Bag_Save_Data();
        save.row = Bag_Row;
        save.col = Bag_Col;
        for (int y = 0; y < Bag_Row; y++)
        {
            for (int x = 0; x < Bag_Col; x++)
            {
                save.slots.Add(bag[y, x]);
            }
        }
        return save;
    }
    public void ImportBagData(Bag_Save_Data saveData)
    {
        Bag_Row = saveData.row;
        Bag_Col = saveData.col;
        Init_Bag();
        int index = 0;
        for (int y = 0; y < Bag_Row; y++)
        {
            for (int x = 0; x < Bag_Col; x++)
            {
                bag[y, x] = saveData.slots[index];
                index++;
            }
        }
        ReClean_Bag_Display();
        Refresh_Bag_Display();
        Init_Resort_List();
        Save_Bag(path);
    }
    #endregion
    #region 放置与删除
    public override void PlaceItem(Item_Data item, int x, int y)
    {
        base.PlaceItem(item, x, y);
    }
    public bool RemoveItemInData(Item_Data data, int removeCount = 1)
    {
        int startX = -1;
        int startY = -1;
        Bag_SingleSlot targetSlot = null;
        bool findItem = false;
        for (int y = 0; y < Bag_Row; y++)
        {
            for (int x = 0; x < Bag_Col; x++)
            {
                Bag_SingleSlot slot = bag[y, x];
                if (slot.Have_Item && slot.Start_x == x && slot.Start_y == y && slot.item_ID == data.item_id)
                {
                    startX = x;
                    startY = y;
                    targetSlot = slot;
                    findItem = true;
                    break;
                }
            }
            if (findItem)
            {
                break;
            }
        }
        if (!findItem || targetSlot == null)
        {
            Debug.Log($"背包内没有{data.item_name}");
            return false;
        }
        int currentStack = targetSlot.stackCount;
        if (currentStack > removeCount)
        {
            targetSlot.stackCount -= removeCount;
            for (int i = 0; i < removeCount; i++)
            {
                int index = resort_list.FindIndex(it => it.item_id == data.item_id);
                if (index >= 0)
                {
                    resort_list.RemoveAt(index);
                }
            }
        }
        else
        {
            RemoveItem(startX, startY, data.Width, data.Height);
            resort_list.RemoveAll(it => it.item_id == data.item_id);
        }

        ReClean_Bag_Display();
        Refresh_Bag_Display();
        Save_Bag(path);
        return true;
    }
    public override void RemoveItem(int x, int y, int w, int h)
    {
        base.RemoveItem(x, y, w, h);
    }
    #endregion
    #region 寻找与刷新
    public override void Find_Image_By_id(Item_Data item, int posX, int posY)
    {
        base.Find_Image_By_id(item, posX, posY);
    }
    public override void Refresh_Bag_Display()
    {
        base.Refresh_Bag_Display();
    }
    public override void ReClean_Bag_Display()
    {
        base.ReClean_Bag_Display();
    }
    #endregion
    #region 是否有可以储存的位置
    public override void Find_Empty_Location(int _height, int _width, out int result_x, out int result_y)
    {
        base.Find_Empty_Location(_height, _width, out result_x, out result_y);
    }
    public override bool Empty_Check(int x, int y, int h, int w)
    {
        return base.Empty_Check(x, y, h, w);
    }
    #endregion
    public override bool Pick_Up(Item_Data data,int c)
    {
        return base.Pick_Up(data,c);
    }
    #region 清空背包
    public void DeleteBagSaveFile()
    {
        string filePath = Application.persistentDataPath + "/" + path + ".json";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("旧背包存档已删除");
        }
        Init_Bag();
        Save_Bag(path);
        Debug.Log("已重新生成全新空白背包存档");
    }
    public void ThrowAll()
    {
        for(int i = 0; i < Bag_Row; i++)
        {
            for(int j = 0; j < Bag_Col; j++)
            {
                if (bag[i, j].Have_Item == true && bag[i, j].Start_x != -1 && bag[i, j].Start_y != -1) {
                    Item_Data item = allData_Item.Data_List.Find(t => t.item_id == bag[i, j].item_ID);
                    if (!item)
                    {
                        continue;
                    }
                    Vector3 drop_pos = new Vector3(gameObject.transform.position.x + UnityEngine.Random.Range(0, 5), gameObject.transform.position.y + UnityEngine.Random.Range(0, 5), gameObject.transform.position.z + UnityEngine.Random.Range(0, 5));
                    ObjectPoolMgr.instance.GetObj(item.Drop, drop_pos, Quaternion.identity);
                    RemoveItem(j, i, item.Width, item.Height);
                }
            }
        }
        ReClean_Bag_Display();
        Refresh_Bag_Display();
    }
    public void Throw_And_Delete_All()
    {
        ThrowAll();
        DeleteBagSaveFile();
        resort_list.Clear();
    }
    #endregion
    #region 整理
    public void Init_Resort_List()
    {
        resort_list.Clear();
        for (int i = 0; i < Bag_Row; i++)
        {
            for (int j = 0; j < Bag_Col; j++)
            {
                Bag_SingleSlot slot = bag[i, j];
                if (slot.Have_Item == true && slot.Start_x == j && slot.Start_y == i)
                {
                    Item_Data item = allData_Item.Data_List.Find(t => t.item_id == slot.item_ID);
                    if (!item)
                    {
                        continue;
                    }
                    for (int cnt = 0; cnt < slot.stackCount; cnt++)
                    {
                        resort_list.Add(item);
                    }
                }
            }
        }
    }
    public void Resort_By_Value()
    {
        SortBagCommon((a, b) => a.PriceValue.CompareTo(b.PriceValue));
    }

    public void Resort_By_Size()
    {
        SortBagCommon((a, b) =>
        {
            int sizeA = a.Width * a.Height;
            int sizeB = b.Width * b.Height;
            return sizeB.CompareTo(sizeA);
        });
    }
    private void SortBagCommon(Comparison<Item_Data> sortRule)
    {
        Init_Resort_List();
        BackupBag();
        Dictionary<Item_Data, int> itemCountDic = new Dictionary<Item_Data, int>();
        foreach (var item in resort_list)
        {
            if (itemCountDic.ContainsKey(item))
                itemCountDic[item]++;
            else
                itemCountDic[item] = 1;
        }
        int totalNeedCell = 0;
        int perCellMax = 99;
        foreach (var pair in itemCountDic)
        {
            Item_Data item = pair.Key;
            int total = pair.Value;
            int group = (total + perCellMax - 1) / perCellMax;
            totalNeedCell += group * item.Width * item.Height;
        }
        int emptyCell = 0;
        for (int y = 0; y < Bag_Row; y++)
        {
            for (int x = 0; x < Bag_Col; x++)
            {
                if (!bagBackup[y, x].Have_Item)
                    emptyCell++;
            }
        }
        if (emptyCell < totalNeedCell)
        {
            Debug.Log("背包空间不足，整理取消");
            RestoreBag();
            return;
        }
        ReClean_Bag_Display();
        for (int y = 0; y < Bag_Row; y++)
        {
            for (int x = 0; x < Bag_Col; x++)
            {
                bag[y, x] = new Bag_SingleSlot { stackCount = 1 };
            }
        }
        resort_list.Sort(sortRule);
        bool arrangeSuccess = true;
        foreach (var item in resort_list)
        {
            bool ok = Pick_Up(item, 1);
            if (!ok)
            {
                arrangeSuccess = false;
                break;
            }
        }

        if (!arrangeSuccess)
        {
            RestoreBag();
            Debug.Log("背包空间不足，整理失败，已复原原来布局");
        }
        else
        {
            Refresh_Bag_Display();
            Init_Resort_List();
            Save_Bag(path);
            Debug.Log("整理成功");
        }
    }
    public void BackupBag()
    {
        bagBackup = new Bag_SingleSlot[Bag_Row, Bag_Col];
        for (int y = 0; y < Bag_Row; y++)
        {
            for (int x = 0; x < Bag_Col; x++)
            {
                bagBackup[y, x] = new Bag_SingleSlot
                {
                    item_ID = bag[y, x].item_ID,
                    Start_x = bag[y, x].Start_x,
                    Start_y = bag[y, x].Start_y,
                    Have_Item = bag[y, x].Have_Item,
                    json_x = bag[y, x].json_x,
                    json_y = bag[y, x].json_y,
                    real_width = bag[y, x].real_width,
                    real_height = bag[y, x].real_height,
                    stackCount = bag[y, x].stackCount
                };
            }
        }
    }
    public void RestoreBag()
    {
        for (int y = 0; y < Bag_Row; y++)
        {
            for (int x = 0; x < Bag_Col; x++)
            {
                bag[y, x] = bagBackup[y, x];
            }
        }
        ReClean_Bag_Display();
        Refresh_Bag_Display();
        Save_Bag(path);
    }
    #endregion
    #region 装备栏道具
    public void RefrshArms()
    {
        Item_Data d = quick.Tool;
        Armed.Clear();
        if (d)
        {
            foreach (var i in resort_list)
            {
                if (i.item_id == d.item_id)
                {
                    Armed.Add(i);
                }
            }
        }
        if (Armed.Count <= 0)
        {
            Game_Event.instance.EquipInQuick(null);
            Panel_Mgr.instance.BagPanel.GetComponentInChildren<QuickEquip>().EquipToHotbar(null);
        }
    }
    #endregion
}