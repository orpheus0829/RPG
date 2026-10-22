using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public enum ViewBagType
{
    MainItemBag,
    WeaponBag,
}
public class Player_Bag : BaseBag
{
    [Header("玩家专属")]
    public List<Item_Data> Armed = new List<Item_Data>();
    public QuickEquip quick;
    public List<Item_Data> resort_list;
    private Bag_SingleSlot[,] bagBackup;
    public ViewBagType CurrentViewBagType;
    public TextMeshProUGUI BagStyleMark;

    [Header("武器背包")]
    public int WeaponBag_Row;
    public int WeaponBag_Col;
    public string WeaponBagPath;
    public Bag_SingleSlot[,] WeaponBag;
    private Bag_SingleSlot[,] weaponBagBackup;
    public Bag_SingleSlot[,] CurrentViewBag
    {
        get
        {
            if (CurrentViewBagType == ViewBagType.MainItemBag)
            {
                return bag;
            }
            else
            {
                return WeaponBag;
            }
        }
    }
    public (int Row, int Col) GetCurrentBagSize()
    {
        if (CurrentViewBagType == ViewBagType.MainItemBag)
        {
            return (Bag_Row, Bag_Col);
        }
        else
        {
            return (WeaponBag_Row, WeaponBag_Col);
        }
    }
    protected override void Awake()
    {
        base.Awake();
        BagStyleMark.text = "背包(道具)";
    }
    public void Start()
    {
        Load_Data(path);
        LoadWeaponBag(WeaponBagPath);
        CurrentViewBagType = ViewBagType.MainItemBag;
        GenerateCurrentBagSlots();

        Init_AllResortList();
        Game_Event.instance.Buy_Item += Add_Good;
        Game_Event.instance.Sell_Item += Sell_Good;
        Game_Event.instance.Last_Item_By_ID += Search_By_ID;
        Game_Event.instance.Remove_Sold_Good += Remove_Because_Sold;

        Game_Event.instance.Craft_Check += Craft_Need;
        Game_Event.instance.Crafting_Start += Craft_Add;
        quick = Panel_Mgr.instance.PlayUiPanel.gameObject.GetComponentInChildren<QuickEquip>();

        Game_Event.instance.ReturnOldEquipToBag += ReceiveReturnEquip;
    }
    public void SwitchViewBag()
    {
        int currentIndex = (int)CurrentViewBagType;
        BagStyleMark.text = currentIndex == 1 ? "背包(道具)" : "背包(装备)";
        int totalBagTypes = Enum.GetNames(typeof(ViewBagType)).Length;
        int nextIndex = (currentIndex + 1) % totalBagTypes;
        CurrentViewBagType = (ViewBagType)nextIndex;
        ReClean_Bag_Display();
        GenerateCurrentBagSlots();
        Refresh_Bag_Display();
        Init_AllResortList();
    }
    public void GenerateCurrentBagSlots()
    {
        if (SlotContainer.childCount > 0)
        {
            for (int i = SlotContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(SlotContainer.GetChild(i).gameObject);
            }
        }

        var size = GetCurrentBagSize();
        int rowCount = size.Row;
        int colCount = size.Col;

        float cellTotalW = cellSize + horizontalSpace;
        float cellTotalH = cellSize + verticalSpace;

        for (int y = 0; y < rowCount; y++)
        {
            for (int x = 0; x < colCount; x++)
            {
                GameObject slotObj = new GameObject($"Slot_{x}_{y}");
                slotObj.AddComponent<ClickNull_Checker>();
                slotObj.transform.SetParent(SlotContainer, false);
                RectTransform rt = slotObj.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(cellSize, cellSize);
                rt.anchoredPosition = new Vector2(x * cellTotalW, -y * cellTotalH);
                slotObj.AddComponent<Image>();
                Image slotImg = slotObj.GetComponent<Image>();
                slotImg.sprite = slotSprite;
                slotImg.color = Color.black;
            }
        }
    }
    public void InitWeaponBag()
    {
        WeaponBag = new Bag_SingleSlot[WeaponBag_Row, WeaponBag_Col];
        for (int y = 0; y < WeaponBag_Row; y++)
        {
            for (int x = 0; x < WeaponBag_Col; x++)
            {
                WeaponBag[y, x] = new Bag_SingleSlot();
                WeaponBag[y, x].item_ID = 0;
                WeaponBag[y, x].Have_Item = false;
                WeaponBag[y, x].json_x = x;
                WeaponBag[y, x].json_y = y;
            }
        }
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
        Game_Event.instance.ReturnOldEquipToBag -= ReceiveReturnEquip;
    }
    public void ReceiveReturnEquip(Item_Data oldEquipItem)
    {
        if (!oldEquipItem)
        {
            return;
        }
        bool HaveTemp = Pick_Up(oldEquipItem);
        if (!HaveTemp)
        {
            ObjectPoolMgr.instance.GetObj(oldEquipItem.Drop, transform.position, Quaternion.identity);
        }
    }
    #region 查询剩余某物品
    public int Search_By_ID(Item_Data item)
    {
        int total = 0;
        for (int y = 0; y < Bag_Row; y++) for (int x = 0; x < Bag_Col; x++)
            {
                if (bag[y, x].Have_Item && bag[y, x].item_ID == item.item_id)
                {
                    total += bag[y, x].stackCount;
                }
            }
        for (int y = 0; y < WeaponBag_Row; y++)
        {
            for (int x = 0; x < WeaponBag_Col; x++)
            {
                if (WeaponBag[y, x].Have_Item && WeaponBag[y, x].item_ID == item.item_id)
                {
                    total += WeaponBag[y, x].stackCount;
                }
            }
        }
        return total;
    }
    #endregion
    #region 卖出删除物品
    public void Remove_Because_Sold(Item_Data item)
    {
        bool success = false;
        for (int y = 0; y < Bag_Row && !success; y++)
        {
            for (int x = 0; x < Bag_Col && !success; x++)
            {
                if (bag[y, x].Have_Item && bag[y, x].item_ID == item.item_id)
                {
                    if (bag[y, x].stackCount > 1)
                    {
                        bag[y, x].stackCount--;
                        Save_Bag(path);
                    }
                    else
                    {
                        RemoveMainItem(x, y, item.Width, item.Height);
                    }
                    success = true;
                }
            }
        }
        if (!success)
        {
            for (int y = 0; y < WeaponBag_Row && !success; y++)
            {
                for (int x = 0; x < WeaponBag_Col && !success; x++)
                {
                    if (WeaponBag[y, x].Have_Item && WeaponBag[y, x].item_ID == item.item_id)
                    {
                        if (WeaponBag[y, x].stackCount > 1)
                        {
                            WeaponBag[y, x].stackCount--;
                            SaveWeaponBag(WeaponBagPath);
                        }
                        else
                        {
                            RemoveWeaponItem(x, y, item.Width, item.Height);
                        }
                        success = true;
                    }
                }
            }
        }
        Init_AllResortList();
        ReClean_Bag_Display();
        Refresh_Bag_Display();
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
            PickNoticeMgr.instance.ShowFieldTip($"买入{item.item_name}");
            Debug.Log("买入" + item.item_name);
            Init_AllResortList();
        }
    }
    public void Sell_Good(Item_Data item)
    {
        Game_Event.instance.Send_Real_SellItem(Search_By_ID(item) > 0);
    }
    #endregion
    #region 合成
    public bool Craft_Need(Crafting_SO craft)
    {
        Init_AllResortList();
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
            int need = mat.Number;
            while (need > 0)
            {
                bool removed = false;
                for (int y = 0; y < Bag_Row && !removed; y++)
                {
                    for (int x = 0; x < Bag_Col && !removed; x++)
                    {
                        if (bag[y, x].Have_Item && bag[y, x].item_ID == mat.Material.item_id)
                        {
                            if (bag[y, x].stackCount > 1)
                            {
                                bag[y, x].stackCount--;
                                Save_Bag(path);
                            }
                            else
                            {
                                RemoveMainItem(x, y, mat.Material.Width, mat.Material.Height);
                            }
                            removed = true;
                            need--;
                        }
                    }
                }
                if (!removed)
                {
                    for (int y = 0; y < WeaponBag_Row && !removed; y++)
                    {
                        for (int x = 0; x < WeaponBag_Col && !removed; x++)
                        {
                            if (WeaponBag[y, x].Have_Item && WeaponBag[y, x].item_ID == mat.Material.item_id)
                            {
                                if (WeaponBag[y, x].stackCount > 1)
                                {
                                    WeaponBag[y, x].stackCount--;
                                    SaveWeaponBag(WeaponBagPath);
                                }
                                else
                                {
                                    RemoveWeaponItem(x, y, mat.Material.Width, mat.Material.Height);
                                }
                                removed = true;
                                need--;
                            }
                        }
                    }
                }
                if (!removed)
                {
                    Debug.Log($"合成缺少材料：{mat.Material.item_name}");
                    break;
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
        Init_AllResortList();
        PickNoticeMgr.instance.ShowFieldTip("合成成功");
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
    public void SaveWeaponBag(string filePath)
    {
        string fullPath = Application.persistentDataPath + "/" + filePath + ".json";
        Bag_Save_Data save = new Bag_Save_Data { row = WeaponBag_Row, col = WeaponBag_Col };
        for (int y = 0; y < WeaponBag_Row; y++)
        {
            for (int x = 0; x < WeaponBag_Col; x++)
            {
                save.slots.Add(WeaponBag[y, x]);
            }
        }
        string jsonStr = JsonUtility.ToJson(save);
        File.WriteAllText(fullPath, jsonStr);
    }
    public void LoadWeaponBag(string filePath)
    {
        string pathFull = Application.persistentDataPath + "/" + filePath + ".json";
        if (File.Exists(pathFull))
        {
            string json = File.ReadAllText(pathFull);
            Bag_Save_Data save = JsonUtility.FromJson<Bag_Save_Data>(json);
            InitWeaponBag();
            int index = 0;
            for (int y = 0; y < save.row; y++)
            {
                for (int x = 0; x < save.col; x++)
                {
                    if (index >= save.slots.Count)
                    {
                        break;
                    }
                    if (y < WeaponBag_Row && x < WeaponBag_Col)
                    {
                        WeaponBag[y, x] = save.slots[index];
                    }
                    index++;
                }
            }
        }
        else
        {
            Debug.LogWarning($"武器背包存档不存在:{pathFull},初始化空白武器背包");
            InitWeaponBag();
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
        Init_AllResortList();
        Save_Bag(path);
    }
    public Bag_Save_Data ExportWeaponBagData()
    {
        Bag_Save_Data save = new Bag_Save_Data();
        save.row = WeaponBag_Row;
        save.col = WeaponBag_Col;
        for (int y = 0; y < WeaponBag_Row; y++)
        {
            for (int x = 0; x < WeaponBag_Col; x++)
            {
                save.slots.Add(WeaponBag[y, x]);
            }
        }
        return save;
    }
    public void ImportWeaponBagData(Bag_Save_Data saveData)
    {
        WeaponBag_Row = saveData.row;
        WeaponBag_Col = saveData.col;
        InitWeaponBag();
        int index = 0;
        for (int y = 0; y < WeaponBag_Row; y++)
        {
            for (int x = 0; x < WeaponBag_Col; x++)
            {
                WeaponBag[y, x] = saveData.slots[index];
                index++;
            }
        }
        ReClean_Bag_Display();
        Refresh_Bag_Display();
        Init_AllResortList();
        SaveWeaponBag(WeaponBagPath);
    }
    #endregion
    #region 放置与删除
    public override void PlaceItem(Item_Data item, int x, int y)
    {
        int w = item.Width;
        int h = item.Height;
        Bag_SingleSlot[,] targetBag = CurrentViewBag;

        for (int yy = y; yy < y + h; yy++)
        {
            for (int xx = x; xx < x + w; xx++)
            {
                targetBag[yy, xx].item_ID = item.item_id;
                targetBag[yy, xx].Start_x = -1;
                targetBag[yy, xx].Start_y = -1;
                targetBag[yy, xx].Have_Item = true;
                targetBag[yy, xx].real_width = w;
                targetBag[yy, xx].real_height = h;
            }
        }
        targetBag[y, x].Start_x = x;
        targetBag[y, x].Start_y = y;
        if (CurrentViewBagType == ViewBagType.MainItemBag)
        {
            Save_Bag(path);
        }
        else
        {
            SaveWeaponBag(WeaponBagPath);
        }

        ReClean_Bag_Display();
        Refresh_Bag_Display();
    }
    public bool RemoveItemInData(Item_Data data, int removeCount = 1)
    {
        int startX = -1;
        int startY = -1;
        bool findItem = false;

        for (int y = 0; y < Bag_Row; y++)
        {
            for (int x = 0; x < Bag_Col; x++)
            {
                if (bag[y, x].Have_Item && bag[y, x].Start_x == x && bag[y, x].Start_y == y && bag[y, x].item_ID == data.item_id)
                {
                    startX = x;
                    startY = y;
                    findItem = true;
                    break;
                }
            }
            if (findItem) break;
        }
        if (!findItem)
        {
            Debug.Log($"背包内没有{data.item_name}");
            return false;
        }
        int currentStack = bag[startY, startX].stackCount;
        if (currentStack > removeCount)
        {
            bag[startY, startX].stackCount -= removeCount;
            for (int i = 0; i < removeCount; i++)
            {
                int index = resort_list.FindIndex(it => it.item_id == data.item_id);
                if (index >= 0) resort_list.RemoveAt(index);
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
        Bag_SingleSlot[,] targetBag = CurrentViewBag;
        for (int yy = y; yy < y + h; yy++)
        {
            for (int xx = x; xx < x + w; xx++)
            {
                targetBag[yy, xx].Have_Item = false;
                targetBag[yy, xx].item_ID = 0;
                targetBag[yy, xx].Start_x = -1;
                targetBag[yy, xx].Start_y = -1;
                targetBag[yy, xx].stackCount = 1;
            }
        }
        if (CurrentViewBagType == ViewBagType.MainItemBag)
        {
            Save_Bag(path);
        }
        else
        {
            SaveWeaponBag(WeaponBagPath);
        }
    }
    private void RemoveMainItem(int x, int y, int w, int h)
    {
        for (int yy = y; yy < y + h; yy++)
        {
            for (int xx = x; xx < x + w; xx++)
            {
                bag[yy, xx].Have_Item = false;
                bag[yy, xx].item_ID = 0;
                bag[yy, xx].Start_x = -1;
                bag[yy, xx].Start_y = -1;
                bag[yy, xx].stackCount = 1;
            }
        }
        Save_Bag(path);
    }
    private void RemoveWeaponItem(int x, int y, int w, int h)
    {
        for (int yy = y; yy < y + h; yy++)
        {
            for (int xx = x; xx < x + w; xx++)
            {
                WeaponBag[yy, xx].Have_Item = false;
                WeaponBag[yy, xx].item_ID = 0;
                WeaponBag[yy, xx].Start_x = -1;
                WeaponBag[yy, xx].Start_y = -1;
                WeaponBag[yy, xx].stackCount = 1;
            }
        }
        SaveWeaponBag(WeaponBagPath);
    }
    #endregion
    #region 寻找与刷新
    public override void Find_Image_By_id(Item_Data item, int posX, int posY)
    {
        base.Find_Image_By_id(item, posX, posY);
    }
    public override void Refresh_Bag_Display()
    {
        var size = GetCurrentBagSize();
        int maxRow = size.Row;
        int maxCol = size.Col;
        Bag_SingleSlot[,] targetBag = CurrentViewBag;

        for (int i = 0; i < maxRow; i++)
        {
            for (int j = 0; j < maxCol; j++)
            {
                if (targetBag[i, j].Have_Item && targetBag[i, j].Start_x != -1)
                {
                    Item_Data item = allData_Item.Data_List.Find(t => t.item_id == targetBag[i, j].item_ID);
                    if (item == null)
                    {
                        continue;
                    }
                    Find_Image_By_id(item, targetBag[i, j].Start_x, targetBag[i, j].Start_y);
                }
            }
        }
    }
    public override void ReClean_Bag_Display()
    {
        base.ReClean_Bag_Display();
    }
    #endregion
    #region 是否有可以储存的位置
    public void Init_AllResortList()
    {
        resort_list.Clear();
        for (int y = 0; y < Bag_Row; y++)
        {
            for (int x = 0; x < Bag_Col; x++)
            {
                Bag_SingleSlot slot = bag[y, x];
                if (slot.Have_Item && slot.Start_x == x && slot.Start_y == y)
                {
                    Item_Data item = allData_Item.Data_List.Find(t => t.item_id == slot.item_ID);
                    if (item == null)
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
        for (int y = 0; y < WeaponBag_Row; y++)
        {
            for (int x = 0; x < WeaponBag_Col; x++)
            {
                Bag_SingleSlot slot = WeaponBag[y, x];
                if (slot.Have_Item && slot.Start_x == x && slot.Start_y == y)
                {
                    Item_Data item = allData_Item.Data_List.Find(t => t.item_id == slot.item_ID);
                    if (item == null)
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
    public override void Find_Empty_Location(int _height, int _width, out int result_x, out int result_y)
    {
        var size = GetCurrentBagSize();
        int maxRow = size.Row;
        int maxCol = size.Col;
        Bag_SingleSlot[,] targetBag = CurrentViewBag;

        for (int i = 0; i < maxRow; i++)
        {
            for (int j = 0; j < maxCol; j++)
            {
                if (!Empty_Check(j, i, _height, _width))
                {
                    continue;
                }
                else
                {
                    result_x = j;
                    result_y = i;
                    return;
                }
            }
        }
        result_x = -1;
        result_y = -1;
    }
    public override bool Empty_Check(int x, int y, int h, int w)
    {
        var size = GetCurrentBagSize();
        int maxRow = size.Row;
        int maxCol = size.Col;

        if (y + h > maxRow || x + w > maxCol)
        {
            return false;
        }
        Bag_SingleSlot[,] targetBag = CurrentViewBag;
        for (int a = y; a < y + h; a++)
        {
            for (int b = x; b < x + w; b++)
            {
                if (targetBag[a, b].Have_Item)
                {
                    return false;
                }
            }
        }
        return true;
    }
    private bool EmptyCheckByBag(Bag_SingleSlot[,] bagArr, int rowCount, int colCount, int x, int y, int w, int h)
    {
        if (y + h > rowCount || x + w > colCount)
            return false;
        for (int a = y; a < y + h; a++)
        {
            for (int b = x; b < x + w; b++)
            {
                if (bagArr[a, b].Have_Item)
                    return false;
            }
        }
        return true;
    }
    private bool FindEmptyByBag(Bag_SingleSlot[,] bagArr, int rowCount, int colCount, int w, int h, out int resX, out int resY)
    {
        for (int y = 0; y < rowCount; y++)
        {
            for (int x = 0; x < colCount; x++)
            {
                if (EmptyCheckByBag(bagArr, rowCount, colCount, x, y, w, h))
                {
                    resX = x;
                    resY = y;
                    return true;
                }
            }
        }
        resX = -1;
        resY = -1;
        return false;
    }
    #endregion
    public override bool Pick_Up(Item_Data data, int pickNum = 1)
    {
        if (pickNum <= 0 || data == null)
        {
            return false;
        }
        Bag_SingleSlot[,] targetBag;
        int targetRow, targetCol;
        string savePath;
        bool isWeaponBag = false;
        if (data.item_Kind == Item_Kind.Weapon)
        {
            targetBag = WeaponBag;
            targetRow = WeaponBag_Row;
            targetCol = WeaponBag_Col;
            savePath = WeaponBagPath;
            isWeaponBag = true;
        }
        else
        {
            targetBag = bag;
            targetRow = Bag_Row;
            targetCol = Bag_Col;
            savePath = path;
            isWeaponBag = false;
        }
        int remaining = pickNum;
        if (IsStackableBag && data.Stackable && !isWeaponBag)
        {
            for (int y = 0; y < targetRow && remaining > 0; y++)
            {
                for (int x = 0; x < targetCol && remaining > 0; x++)
                {
                    Bag_SingleSlot slot = targetBag[y, x];
                    if (slot.Have_Item && slot.item_ID == data.item_id)
                    {
                        int canAdd = data.StackMax - slot.stackCount;
                        if (canAdd > 0)
                        {
                            int addCount = Mathf.Min(canAdd, remaining);
                            slot.stackCount += addCount;
                            remaining -= addCount;
                        }
                    }
                }
            }
        }
        while (remaining > 0)
        {
            bool hasEmpty = FindEmptyByBag(targetBag, targetRow, targetCol, data.Width, data.Height, out int res_x, out int res_y);
            if (!hasEmpty)
            {
                Refresh_Bag_Display();
                if (data.item_Kind == Item_Kind.Weapon)
                {
                    SaveWeaponBag(WeaponBagPath);
                }
                else
                {
                    Save_Bag(path);
                }
                return pickNum - remaining > 0;
            }
            int placeCount = Mathf.Min(remaining, data.StackMax);
            targetBag[res_y, res_x].stackCount = placeCount;
            for (int yy = res_y; yy < res_y + data.Height; yy++)
            {
                for (int xx = res_x; xx < res_x + data.Width; xx++)
                {
                    targetBag[yy, xx].item_ID = data.item_id;
                    targetBag[yy, xx].Start_x = -1;
                    targetBag[yy, xx].Start_y = -1;
                    targetBag[yy, xx].Have_Item = true;
                    targetBag[yy, xx].real_width = data.Width;
                    targetBag[yy, xx].real_height = data.Height;
                }
            }
            targetBag[res_y, res_x].Start_x = res_x;
            targetBag[res_y, res_x].Start_y = res_y;

            remaining -= placeCount;
        }
        ReClean_Bag_Display();
        Refresh_Bag_Display();
        if (data.item_Kind == Item_Kind.Weapon)
        {
            SaveWeaponBag(WeaponBagPath);
        }
        else
        {
            Save_Bag(path);
        }

        return true;
    }
    #region 清空背包
    public void DeleteBagSaveFile()
    {
        if (CurrentViewBagType == ViewBagType.MainItemBag)
        {
            string mainPath = Application.persistentDataPath + "/" + path + ".json";
            if (File.Exists(mainPath))
            {
                File.Delete(mainPath);
            }
            Init_Bag();
            Save_Bag(path);
        }
        else
        {
            string weaponPath = Application.persistentDataPath + "/" + WeaponBagPath + ".json";
            if (File.Exists(weaponPath))
            {
                File.Delete(weaponPath);
            }
            InitWeaponBag();
            SaveWeaponBag(WeaponBagPath);
        }

        resort_list.Clear();
        ReClean_Bag_Display();
        Refresh_Bag_Display();
        PickNoticeMgr.instance.ShowFieldTip("已清空背包");
    }
    public void ThrowAll()
    {
        Bag_SingleSlot[,] currentBag = CurrentViewBag;
        int row = GetCurrentBagSize().Row;
        int col = GetCurrentBagSize().Col;
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                if (currentBag[i, j].Have_Item && currentBag[i, j].Start_x != -1)
                {
                    Item_Data item = allData_Item.Data_List.Find(t => t.item_id == currentBag[i, j].item_ID);
                    if (!item)
                    {
                        continue;
                    }
                    int stackCount = currentBag[i, j].stackCount;
                    Vector3 drop_pos = transform.position + new Vector3(UnityEngine.Random.Range(-2f, 2f), 0.5f, UnityEngine.Random.Range(-2f, 2f));
                    for (int k = 0; k < stackCount; k++)
                    {
                        ObjectPoolMgr.instance.GetObj(item.Drop, drop_pos, Quaternion.identity);
                    }
                    currentBag[i, j].Have_Item = false;
                    currentBag[i, j].item_ID = 0;
                    currentBag[i, j].Start_x = -1;
                    currentBag[i, j].Start_y = -1;
                    currentBag[i, j].stackCount = 1;
                }
            }
        }
        if (CurrentViewBagType == ViewBagType.MainItemBag)
        {
            Save_Bag(path);
        }
        else
        {
            SaveWeaponBag(WeaponBagPath);
        }
        resort_list.Clear();
        ReClean_Bag_Display();
        Refresh_Bag_Display();
        PickNoticeMgr.instance.ShowFieldTip("当前背包所有物品已丢弃");
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
        var size = GetCurrentBagSize();
        int row = size.Row;
        int col = size.Col;
        var targetBag = CurrentViewBag;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                Bag_SingleSlot slot = targetBag[i, j];
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
        BackupCurrentBag();
        Dictionary<Item_Data, int> itemCountDic = new Dictionary<Item_Data, int>();
        foreach (var item in resort_list)
        {
            if (itemCountDic.ContainsKey(item))
            {
                itemCountDic[item]++;
            }
            else
            {
                itemCountDic[item] = 1;
            }
        }
        int currentRow = GetCurrentBagSize().Row;
        int currentCol = GetCurrentBagSize().Col;
        int totalNeedCell = 0;

        foreach (var pair in itemCountDic)
        {
            Item_Data item = pair.Key;
            int perCellMax = item.StackMax;
            int group = (pair.Value + perCellMax - 1) / perCellMax;
            totalNeedCell += group * item.Width * item.Height;
        }

        int emptyCell = 0;
        var currentBag = CurrentViewBag;
        for (int y = 0; y < currentRow; y++)
        {
            for (int x = 0; x < currentCol; x++)
            {
                if (!currentBag[y, x].Have_Item)
                {
                    emptyCell++;
                }
            }
        }
        if (emptyCell < totalNeedCell)
        {
            Debug.Log("当前背包空间不足，整理取消");
            RestoreCurrentBag();
            return;
        }
        ReClean_Bag_Display();
        for (int y = 0; y < currentRow; y++)
        {
            for (int x = 0; x < currentCol; x++)
            {
                currentBag[y, x] = new Bag_SingleSlot { stackCount = 1 };
            }
        }
        resort_list.Sort(sortRule);
        bool arrangeSuccess = true;
        foreach (var item in resort_list)
        {
            int remaining = 1;
            Bag_SingleSlot[,] targetBag = CurrentViewBag;
            int targetRow = currentRow;
            int targetCol = currentCol;
            if (IsStackableBag && item.Stackable)
            {
                for (int y = 0; y < targetRow && remaining > 0; y++)
                {
                    for (int x = 0; x < targetCol && remaining > 0; x++)
                    {
                        Bag_SingleSlot slot = targetBag[y, x];
                        if (slot.Have_Item && slot.item_ID == item.item_id)
                        {
                            int canAdd = item.StackMax - slot.stackCount;
                            if (canAdd > 0)
                            {
                                int addCount = Mathf.Min(canAdd, remaining);
                                slot.stackCount += addCount;
                                remaining -= addCount;
                            }
                        }
                    }
                }
            }
            while (remaining > 0)
            {
                Find_Empty_Location(item.Height, item.Width, out int res_x, out int res_y);
                if (res_x == -1 || res_y == -1)
                {
                    arrangeSuccess = false;
                    break;
                }
                int placeCount = Mathf.Min(remaining, item.StackMax);
                targetBag[res_y, res_x].stackCount = placeCount;
                for (int yy = res_y; yy < res_y + item.Height; yy++)
                {
                    for (int xx = res_x; xx < res_x + item.Width; xx++)
                    {
                        targetBag[yy, xx].item_ID = item.item_id;
                        targetBag[yy, xx].Start_x = -1;
                        targetBag[yy, xx].Start_y = -1;
                        targetBag[yy, xx].Have_Item = true;
                        targetBag[yy, xx].real_width = item.Width;
                        targetBag[yy, xx].real_height = item.Height;
                    }
                }
                targetBag[res_y, res_x].Start_x = res_x;
                targetBag[res_y, res_x].Start_y = res_y;
                remaining -= placeCount;
            }
            if (!arrangeSuccess)
            {
                break;
            }
        }

        if (!arrangeSuccess)
        {
            RestoreCurrentBag();
            Debug.Log("整理失败，已还原");
        }
        else
        {
            Refresh_Bag_Display();
            Init_Resort_List();
            if (CurrentViewBagType == ViewBagType.MainItemBag)
            {
                Save_Bag(path);
            }
            else
            {
                SaveWeaponBag(WeaponBagPath);
            }
            Debug.Log("当前背包整理成功");
        }
        PickNoticeMgr.instance.ShowFieldTip("整理成功");
    }
    private void BackupCurrentBag()
    {
        if (CurrentViewBagType == ViewBagType.MainItemBag)
        {
            bagBackup = new Bag_SingleSlot[Bag_Row, Bag_Col];
            for (int y = 0; y < Bag_Row; y++)
            {
                for (int x = 0; x < Bag_Col; x++)
                {
                    bagBackup[y, x] = CloneSlot(bag[y, x]);
                }
            }
        }
        else
        {
            weaponBagBackup = new Bag_SingleSlot[WeaponBag_Row, WeaponBag_Col];
            for (int y = 0; y < WeaponBag_Row; y++)
            {
                for (int x = 0; x < WeaponBag_Col; x++)
                {
                    weaponBagBackup[y, x] = CloneSlot(WeaponBag[y, x]);
                }
            }
        }
    }
    private void RestoreCurrentBag()
    {
        if (CurrentViewBagType == ViewBagType.MainItemBag)
        {
            for (int y = 0; y < Bag_Row; y++)
            {
                for (int x = 0; x < Bag_Col; x++)
                {
                    bag[y, x] = CloneSlot(bagBackup[y, x]);
                }
            }
        }
        else
        {
            for (int y = 0; y < WeaponBag_Row; y++)
            {
                for (int x = 0; x < WeaponBag_Col; x++)
                {
                    WeaponBag[y, x] = CloneSlot(weaponBagBackup[y, x]);
                }
            }
        }

        ReClean_Bag_Display();
        Refresh_Bag_Display();
        if (CurrentViewBagType == ViewBagType.MainItemBag)
        {
            Save_Bag(path);
        }
        else
        {
            SaveWeaponBag(WeaponBagPath);
        }
        PickNoticeMgr.instance.ShowFieldTip("背包空间过于拥挤,无法整理");
    }
    private Bag_SingleSlot CloneSlot(Bag_SingleSlot slot)
    {
        return new Bag_SingleSlot
        {
            item_ID = slot.item_ID,
            Have_Item = slot.Have_Item,
            Start_x = slot.Start_x,
            Start_y = slot.Start_y,
            real_width = slot.real_width,
            real_height = slot.real_height,
            stackCount = slot.stackCount,
            json_x = slot.json_x,
            json_y = slot.json_y
        };
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