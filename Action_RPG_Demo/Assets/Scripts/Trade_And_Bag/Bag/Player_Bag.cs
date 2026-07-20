using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public class Bag_SingleSlot
{
    public int item_ID;
    public int Start_x;
    public int Start_y;
    public bool Have_Item;

    public int json_x;
    public int json_y;

    public int real_width;
    public int real_height;
}
[System.Serializable]
public class Bag_Save_Data
{
    public List<Bag_SingleSlot> slots = new List<Bag_SingleSlot>();
    public int row;
    public int col;
}
public class Player_Bag : MonoBehaviour
{
    //public Item_Data sword;
    public AllData_Item allData_Item;
    public RectTransform Images;
    [Header("格子与图标全局配置")]
    [Tooltip("单个背包格子长宽尺寸")]
    public float cellSize = 80f;
    [Tooltip("图标整体缩放大小")]
    public float iconScale = 1f;
    [Header("行列独立间隔控制")]
    public float horizontalSpace = 0f;
    public float verticalSpace = 0f;
    [Header("物品对齐方式")]
    [Tooltip("图标左上角偏移")]
    public Vector2 iconOffset = Vector2.zero;
    [Header("背包大小")]
    public int Bag_Row;
    public int Bag_Col;
    [Header("背包")]
    public Bag_SingleSlot[,] bag;
    [Header("道具栏")]
    public List<Item_Data> Armed = new List<Item_Data>();
    public QuickEquip quick;
    [Header("整理")]
    public List<Item_Data> resort_list;
    public Bag_SingleSlot[,] bagBackup;
    public Item_Dragger currentDraggingItem;
    public bool IsDragging;
    [Header("丢弃")]
    public float DropRadius = 5f;
    public float objXZ = 1f;
    public float objH = 2f;
    public int SearchCount = 100;
    [HideInInspector]
    [Header("地址")] public string path = "Bag_Data";
    public void Awake()
    {
        Init_Bag();
    }
    public void Start()
    {
        Load_Data(path);
        ReClean_Bag_Display();
        Refresh_Bag_Display();
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
            Panel_Mgr.instance.ShowComfirmPanel("存款不足", true, null);
            Debug.Log("存款不足");
            return;
        }
        bool is_Ok = Pick_Up(item);
        if (!is_Ok)
        {
            Game_Event.instance.Send_Real_BuyItem(is_Ok);
            Panel_Mgr.instance.ShowComfirmPanel("背包容量不足，清腾出空间后再购买", true, null);
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
                Panel_Mgr.instance.ShowComfirmPanel("材料不足:" + matItem.Material.item_name + "需" + matItem.Number + "个，仅有" + haveNum + "个", true, null);
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
            Panel_Mgr.instance.ShowComfirmPanel("背包容量不足，已返还材料", true, null);
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
        Panel_Mgr.instance.ShowComfirmPanel("合成成功", true, null);
        Debug.Log("合成完成");
    }
    #endregion
    public void Init_Bag()
    {
        bag = new Bag_SingleSlot[Bag_Row, Bag_Col];
        for (int y = 0; y < Bag_Row; y++)
        {
            for (int x = 0; x < Bag_Col; x++)
            {
                bag[y, x] = new Bag_SingleSlot();
                bag[y, x].item_ID = 0;
                bag[y, x].Have_Item = false;

                bag[y, x].json_x = x;
                bag[y, x].json_y = y;
            }
        }
        ReClean_Bag_Display();
        Refresh_Bag_Display();
    }
    #region 保存与读取
    public void Save_Bag(string path)
    {
        Bag_Save_Data save_Data = new Bag_Save_Data();
        save_Data.row = Bag_Row;
        save_Data.col = Bag_Col;
        for (int y = 0; y < Bag_Row; y++)
        {
            for (int x = 0; x < Bag_Col; x++)
            {
                save_Data.slots.Add(bag[y, x]);
            }
        }
        string json_bag_data = JsonUtility.ToJson(save_Data);
        File.WriteAllText(Application.persistentDataPath + "/" + path + ".json", json_bag_data);
        Debug.Log("背包保存成功，地址为" + Application.persistentDataPath + "/" + path + ".json");
    }
    public void Load_Data(string path)
    {
        string json_bag_data = Application.persistentDataPath + "/" + path + ".json";
        if (File.Exists(json_bag_data))
        {
            string json = File.ReadAllText(json_bag_data);
            Bag_Save_Data save_Data = JsonUtility.FromJson<Bag_Save_Data>(json);
            //Bag_Row = save_Data.row;
            //Bag_Col = save_Data.col;
            Init_Bag();
            int index = 0;
            for (int y = 0; y < Bag_Row; y++)
            {
                for (int x = 0; x < Bag_Col; x++)
                {
                    bag[y, x] = save_Data.slots[index];
                    index++;
                }
            }
            Debug.Log("背包已读取");
        }
        else
        {
            Init_Bag();
            Debug.Log("找不到存档数据，新建立背包");
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
    public void PlaceItem(Item_Data item, int x, int y)
    {
        for (int yy = y; yy < y + item.Height; yy++)
        {
            for (int xx = x; xx < x + item.Width; xx++)
            {
                //在物品占地面积里全部存储物品数据
                bag[yy, xx].item_ID = item.item_id;
                bag[yy, xx].Start_x = -1;
                bag[yy, xx].Start_y = -1;
                bag[yy, xx].Have_Item = true;
            }
        }
        bag[y, x].Start_x = x;
        bag[y, x].Start_y = y;
        Save_Bag(path);
    }
    public bool RemoveItemInData(Item_Data data)
    {
        int startX = -1;
        int startY = -1;
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
                    findItem = true;
                    break;
                }
            }
            if (findItem)
            {
                break;
            }
        }
        if (!findItem)
        {
            Debug.Log($"背包内没有{data.item_name}");
            return false;
        }
        RemoveItem(startX, startY, data.Width, data.Height);
        for (int i = resort_list.Count - 1; i >= 0; i--)
        {
            if (resort_list[i] == data)
            {
                resort_list.RemoveAt(i);
                break;
            }
        }
        ReClean_Bag_Display();
        Refresh_Bag_Display();
        Save_Bag(path);
        return true;
    }
    public void RemoveItem(int x, int y, int w, int h)
    {
        for (int yy = y; yy < y + h; yy++)
        {
            for (int xx = x; xx < x + w; xx++)
            {
                bag[yy, xx].Have_Item = false;
                bag[yy, xx].item_ID = 0;
                bag[yy, xx].Start_x = -1;
                bag[yy, xx].Start_y = -1;
            }
        }
        Save_Bag(path);
    }
    #endregion
    #region 寻找与刷新
    public void Find_Image_By_id(Item_Data item, int posX, int posY)
    {
        int w = item.Width;
        int h = item.Height;
        GameObject iconObj = new GameObject("ItemIcon");
        iconObj.transform.SetParent(Images, false);

        Image image = iconObj.AddComponent<Image>();
        image.sprite = item.Display_In_Backpacks;

        iconObj.AddComponent<Item_Dragger>();
        var drag = iconObj.GetComponent<Item_Dragger>();
        drag.data = item;
        drag.startPos = new Vector2Int(posX, posY);
        drag.Player_Bag = this;

        float cellW = cellSize + horizontalSpace;
        float cellH = cellSize + verticalSpace;

        float centerX = posX * cellW + (w * cellSize) / 2f;
        float centerY = -posY * cellH - (h * cellSize) / 2f;


        float finalW = w * cellSize * iconScale;
        float finalH = h * cellSize * iconScale;

        centerX += iconOffset.x;
        centerY += iconOffset.y;

        image.rectTransform.anchoredPosition = new Vector2(centerX, centerY);
        image.rectTransform.sizeDelta = new Vector2(finalW, finalH);
    }
    public void Refresh_Bag_Display()
    {
        for (int i = 0; i < Bag_Row; i++)
        {
            for (int j = 0; j < Bag_Col; j++)
            {
                if (bag[i, j].Have_Item == true && bag[i, j].Start_x != -1 && bag[i, j].Start_y != -1)
                {
                    int id = bag[i, j].item_ID;
                    int sx = bag[i, j].Start_x;
                    int sy = bag[i, j].Start_y;
                    Item_Data tempItem = allData_Item.Data_List.Find(t => t.item_id == bag[i, j].item_ID);
                    if (tempItem == null)
                    {
                        continue;
                    }
                    Find_Image_By_id(tempItem, sx, sy);
                }
            }
        }
    }
    public void ReClean_Bag_Display()
    {
        for (int i = Images.childCount - 1; i >= 0; i--)
        {
            Destroy(Images.GetChild(i).gameObject);
        }
    }
    #endregion
    #region 是否有可以储存的位置
    public void Find_Empty_Location(int _height, int _width, out int result_x, out int result_y)
    {
        for (int i = 0; i < Bag_Row; i++)
        {
            for (int j = 0; j < Bag_Col; j++)
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
        Debug.Log("找不到可容纳物品的位置");
        result_x = -1;
        result_y = -1;
    }
    public bool Empty_Check(int x, int y, int h, int w)
    {
        if (y + h > Bag_Row || x + w > Bag_Col)
        {
            return false;
        }
        for (int a = y; a < y + h; a++)
        {
            for (int b = x; b < x + w; b++)
            {
                if (bag[a, b].Have_Item == true)
                {
                    return false;
                }
            }
        }
        return true;
    }
    #endregion
    public bool Pick_Up(Item_Data data)
    {
        int data_height = data.Height;
        int data_width = data.Width;
        int res_x = -1;
        int res_y = -1;
        Find_Empty_Location(data_height, data_width, out res_x, out res_y);
        if (res_x == -1 && res_y == -1)
        {
            //写拒绝捡起来
            data = null;
            return false;
        }
        PlaceItem(data, res_x, res_y);
        ReClean_Bag_Display();
        Refresh_Bag_Display();
        Save_Bag(path);
        Game_Event.instance.Refresh_Sell_List();
        Debug.Log("存入背包，存放在" + res_x + "," + res_y + "位置");
        return true;
    }
    #region 清空背包
    // 删除背包存档文件
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
                    Vector3 drop_pos = new Vector3(gameObject.transform.position.x + Random.Range(0, 5), gameObject.transform.position.y + Random.Range(0, 5), gameObject.transform.position.z + Random.Range(0, 5));
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
    #region 整理用列表
    public void Init_Resort_List()
    {
        resort_list.Clear();
        for (int i = 0; i < Bag_Row; i++)
        {
            for (int j = 0; j < Bag_Col; j++)
            {
                if (bag[i, j].Have_Item == true && bag[i, j].Start_x != -1 && bag[i, j].Start_y != -1)
                {
                    Item_Data item = allData_Item.Data_List.Find(t => t.item_id == bag[i, j].item_ID);
                    resort_list.Add(item);
                }
            }
        }
    }
    public void Resort_By_Value()
    {
        Init_Resort_List();
        BackupBag();

        DeleteBagSaveFile();
        ReClean_Bag_Display();
        resort_list.Sort((a, b) => a.PriceValue.CompareTo(b.PriceValue));
        RePlace_AfterSort();
    }
    public void Resort_By_Size()
    {
        Init_Resort_List();
        BackupBag();

        DeleteBagSaveFile();
        ReClean_Bag_Display();
        resort_list.Sort((a, b) => (b.Height * b.Width).CompareTo(a.Height * a.Width));
        RePlace_AfterSort();
    }
    public void RePlace_AfterSort()
    {
        bool arrangeSuccess = true;
        foreach (var resort_item in resort_list)
        {
            if (!Pick_Up(resort_item))
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
                    json_y = bag[y, x].json_y
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