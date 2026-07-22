using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    public int stackCount = 1;
}
[System.Serializable]
public class Bag_Save_Data
{
    public List<Bag_SingleSlot> slots = new List<Bag_SingleSlot>();
    public int row;
    public int col;
}

public abstract class BaseBag : MonoBehaviour
{
    [Header("ÊÇ·ñÎª¿É¶Ñµþ±³°ü")]
    public bool IsStackableBag = true;
    [Header("±³°ü³ß´ç")]
    public int Bag_Row;
    public int Bag_Col;
    public float cellSize = 80f;
    public float horizontalSpace;
    public float verticalSpace;
    [Header("ÒýÓÃ")]
    public RectTransform SlotContainer;
    public RectTransform Images;
    public AllData_Item allData_Item;

    [Header("ÎïÆ·ÅäÖÃ")]
    public GameObject SingleItemPrefab;
    public Sprite slotSprite;
    public Vector2 iconOffset;
    public float iconScale = 1f;
    [Header("¶ªÆúÅäÖÃ")]
    public float DropRadius;
    public float objXZ;
    public float objH;
    public int SearchCount;
    [Header("´æµµ")]
    public string path;

    public Bag_SingleSlot[,] bag;
    public Item_Dragger currentDraggingItem;
    public bool IsDragging;
    public virtual void GenerateSlots()
    {
        if (SlotContainer.childCount > 0)
        {
            for (int i = SlotContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(SlotContainer.GetChild(i).gameObject);
            }
        }
        float cellTotalW = cellSize + horizontalSpace;
        float cellTotalH = cellSize + verticalSpace;
        for (int y = 0; y < Bag_Row; y++)
        {
            for (int x = 0; x < Bag_Col; x++)
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
    public virtual void Init_Bag()
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
    public virtual bool Empty_Check(int x, int y, int h, int w)
    {
        if (y + h > Bag_Row || x + w > Bag_Col)
        {
            return false;
        }
        for (int a = y; a < y + h; a++)
        {
            for (int b = x; b < x + w; b++)
            {
                if (bag[a, b].Have_Item)
                {
                    return false;
                }
            }
        }
        return true;
    }
    public virtual void PlaceItem(Item_Data item, int x, int y)
    {
        int w = item.Width;
        int h = item.Height;
        for (int yy = y; yy < y + h; yy++)
        {
            for (int xx = x; xx < x + w; xx++)
            {
                bag[yy, xx].item_ID = item.item_id;
                bag[yy, xx].Start_x = -1;
                bag[yy, xx].Start_y = -1;
                bag[yy, xx].Have_Item = true;
                bag[yy, xx].real_width = w;
                bag[yy, xx].real_height = h;
            }
        }
        bag[y, x].Start_x = x;
        bag[y, x].Start_y = y;
        Save_Bag(path);
        ReClean_Bag_Display();
        Refresh_Bag_Display();
    }
    public virtual void RemoveItem(int x, int y, int w, int h)
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
    public virtual bool Pick_Up(Item_Data data)
    {
        return Pick_Up(data, 1);
    }
    public virtual bool Pick_Up(Item_Data data, int pickNum = 1)
    {
        if (pickNum <= 0)
        {
            return false;
        }
        if (IsStackableBag && data.Stackable)
        {
            for (int y = 0; y < Bag_Row; y++)
            {
                for (int x = 0; x < Bag_Col; x++)
                {
                    Bag_SingleSlot slot = bag[y, x];
                    if (slot.Have_Item && slot.item_ID == data.item_id)
                    {
                        int newTotal = slot.stackCount + pickNum;
                        if (newTotal > 99) newTotal = 99;
                        slot.stackCount = newTotal;

                        ReClean_Bag_Display();
                        Refresh_Bag_Display();
                        Save_Bag(path);
                        return true;
                    }
                }
            }
        }
        Find_Empty_Location(data.Height, data.Width, out int res_x, out int res_y);
        if (res_x == -1)
        {
            return false;
        }

        bag[res_y, res_x].stackCount = pickNum;
        PlaceItem(data, res_x, res_y);
        Save_Bag(path);
        ReClean_Bag_Display();
        Refresh_Bag_Display();
        return true;
    }
    public virtual void Find_Empty_Location(int h, int w, out int x, out int y)
    {
        for (int i = 0; i < Bag_Row; i++)
        {
            for (int j = 0; j < Bag_Col; j++)
            {
                if (!Empty_Check(j, i, h, w))
                {
                    continue;
                }
                else
                {
                    x = j;
                    y = i;
                    return;
                }
            }
        }
        x = -1;
        y = -1;
    }
    public virtual void Refresh_Bag_Display()
    {
        for (int i = 0; i < Bag_Row; i++)
        {
            for (int j = 0; j < Bag_Col; j++)
            {
                if (bag[i, j].Have_Item && bag[i, j].Start_x != -1)
                {
                    Item_Data item = allData_Item.Data_List.Find(t => t.item_id == bag[i, j].item_ID);
                    if (item == null)
                    {
                        continue;
                    }
                    Find_Image_By_id(item, bag[i, j].Start_x, bag[i, j].Start_y);
                }
            }
        }
    }
    public virtual void Find_Image_By_id(Item_Data item, int posX, int posY)
    {
        int w = item.Width;
        int h = item.Height;
        GameObject itemicon = ObjectPoolMgr.instance.GetObj(SingleItemPrefab, Images);
        Image image = itemicon.GetComponent<Image>();
        image.sprite = item.Display_In_Backpacks;
        Item_Dragger drag = itemicon.GetComponent<Item_Dragger>();
        drag.data = item;
        //drag.Count = drag.gameObject.GetComponentInChildren<TextMeshProUGUI>();
        drag.startPos = new Vector2Int(posX, posY);
        drag.Player_Bag = (Player_Bag)this;

        int stackNum = bag[posY, posX].stackCount;
        drag.SetStackCount(stackNum);

        float cellW = cellSize + horizontalSpace;
        float cellH = cellSize + verticalSpace;
        float blockPixelW = w * cellSize + (w - 1) * horizontalSpace;
        float blockPixelH = h * cellSize + (h - 1) * verticalSpace;

        float gridStartX = posX * cellW;
        float gridStartY = -posY * cellH;
        float centerX = gridStartX + blockPixelW / 2f;
        float centerY = gridStartY - blockPixelH / 2f;
        Vector2 finalPos = new Vector2(centerX, centerY) + iconOffset;
        image.rectTransform.anchoredPosition = finalPos;
        image.rectTransform.sizeDelta = new Vector2(blockPixelW * iconScale, blockPixelH * iconScale);

        drag.originalPos = finalPos;
    }
    public virtual void ReClean_Bag_Display()
    {
        for (int i = Images.childCount - 1; i >= 0; i--)
        {
            Destroy(Images.GetChild(i).gameObject);
        }
    }
    public abstract void Save_Bag(string path);
    public abstract void Load_Data(string path);

    protected virtual void Awake()
    {
        GenerateSlots();
        Init_Bag();
    }
    public Bag_SingleSlot[,] DeepCloneBag()
    {
        Bag_SingleSlot[,] newBag = new Bag_SingleSlot[Bag_Row, Bag_Col];
        for (int y = 0; y < Bag_Row; y++)
        {
            for (int x = 0; x < Bag_Col; x++)
            {
                Bag_SingleSlot src = bag[y, x];
                newBag[y, x] = new Bag_SingleSlot()
                {
                    item_ID = src.item_ID,
                    Start_x = src.Start_x,
                    Start_y = src.Start_y,
                    Have_Item = src.Have_Item,
                    json_x = src.json_x,
                    json_y = src.json_y,
                    real_width = src.real_width,
                    real_height = src.real_height,
                    stackCount = src.stackCount
                };
            }
        }
        return newBag;
    }
}