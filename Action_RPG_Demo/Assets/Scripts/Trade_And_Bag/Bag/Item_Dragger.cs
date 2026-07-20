using MMD4MecanimInternal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Item_Dragger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Item_Data data;
    public Vector2Int startPos;
    public Player_Bag Player_Bag;

    public RectTransform rect;
    public Vector2 originalPos;
    private Vector2 dragMouseOffset;

    public void Awake()
    {
        rect = GetComponent<RectTransform>();
    }
    public void OnEnable()
    {
        Introduction_Mrg.instance.ClickOnItem += Display_Intro;
    }
    public void OnDisable()
    {
        Introduction_Mrg.instance.ClickOnItem -= Display_Intro;
    }
    #region 扔东西
    public void Throw_Item()
    {
        Item_Dragger item = Player_Bag.currentDraggingItem;
        if (item == null || item.data == null)
        {
            return;
        }

        int sx = Player_Bag.currentDraggingItem.startPos.x;
        int sy = Player_Bag.currentDraggingItem.startPos.y;
        int sw = Player_Bag.currentDraggingItem.data.Width;
        int sh = Player_Bag.currentDraggingItem.data.Height;
        Player_Bag.RemoveItem(sx, sy, sw, sh);
        List<Vector3> vaildpoint = FindVaildGround(Player_Bag.gameObject.transform.position, Player_Bag.DropRadius);
        Debug.Log("位置有" + vaildpoint.Count);
        Vector3 drop_pos;
        if (vaildpoint.Count <= 0)
        {
            drop_pos = new Vector3(Player_Bag.gameObject.transform.position.x + Random.Range(-2f, 2f), Player_Bag.gameObject.transform.position.y + 0.5f, Player_Bag.gameObject.transform.position.z + Random.Range(-2f, 2f));
            while (Vector3.Distance(drop_pos, Player_Bag.gameObject.transform.position) < 1f)
            {
                drop_pos = new Vector3(Player_Bag.gameObject.transform.position.x + Random.Range(-2f, 2f), Player_Bag.gameObject.transform.position.y + 0.5f, Player_Bag.gameObject.transform.position.z + Random.Range(-2f, 2f));
            }
        }
        else
        {
            drop_pos = vaildpoint[Random.Range(0, vaildpoint.Count)];
        }
        ObjectPoolMgr.instance.GetObj(data.Drop, drop_pos, Quaternion.identity);

        Player_Bag.currentDraggingItem = null;
        Player_Bag.IsDragging = false;
        Player_Bag.resort_list.Remove(data);
        Destroy(this.gameObject);
    }
    public void Throw_Item(Item_Data dropitem)
    {
        Item_Dragger item = Player_Bag.currentDraggingItem;
        if (item == null || item.data == null)
        {
            return;
        }

        int sx = Player_Bag.currentDraggingItem.startPos.x;
        int sy = Player_Bag.currentDraggingItem.startPos.y;
        int sw = Player_Bag.currentDraggingItem.data.Width;
        int sh = Player_Bag.currentDraggingItem.data.Height;
        Player_Bag.RemoveItem(sx, sy, sw, sh);
        List<Vector3> vaildpoint = FindVaildGround(Player_Bag.gameObject.transform.position, Player_Bag.DropRadius);
        Debug.Log("位置有" + vaildpoint.Count);
        Vector3 drop_pos;
        if (vaildpoint.Count <= 0)
        {
            drop_pos = new Vector3(Player_Bag.gameObject.transform.position.x + Random.Range(-2f, 2f), Player_Bag.gameObject.transform.position.y + 0.5f, Player_Bag.gameObject.transform.position.z + Random.Range(-2f, 2f));
            while (Vector3.Distance(drop_pos, Player_Bag.gameObject.transform.position) < 1f)
            {
                drop_pos = new Vector3(Player_Bag.gameObject.transform.position.x + Random.Range(-2f, 2f), Player_Bag.gameObject.transform.position.y + 0.5f, Player_Bag.gameObject.transform.position.z + Random.Range(-2f, 2f));
            }
        }
        else
        {
            drop_pos = vaildpoint[Random.Range(0, vaildpoint.Count)];
        }
        ObjectPoolMgr.instance.GetObj(dropitem.Drop, drop_pos, Quaternion.identity);

        Player_Bag.currentDraggingItem = null;
        Player_Bag.IsDragging = false;
    }
    public List<Vector3> FindVaildGround(Vector3 center, float radius)
    {
        float xz = Player_Bag.objXZ;
        float h = Player_Bag.objH;
        int count = Player_Bag.SearchCount;
        float r = Player_Bag.DropRadius;
        List<Vector3> res = new List<Vector3>();
        Vector3 halfBox = new Vector3(xz / 2, h / 2, xz / 2);

        for (int i = 0; i < count; i++)
        {
            float angle = Mathf.Deg2Rad * (360f / count * i);
            float x = Mathf.Cos(angle) * r;
            float z = Mathf.Sin(angle) * r;
            Vector3 horPos = center + new Vector3(x, 0, z);
            float minY = center.y;
            float maxY = center.y + h;

            Vector3 boxCenter = new Vector3(horPos.x, (minY + maxY) / 2f, horPos.z);
            bool hasObstacle = Physics.CheckBox(boxCenter, halfBox);
            if (!hasObstacle)
            {
                Vector3 validPos = new Vector3(horPos.x, minY, horPos.z);
                res.Add(validPos);
            }
        }
        return res;
    }
    #endregion
    #region 点击
    public void Display_Intro(Item_Data _Data)
    {
        Introduction_Mrg.instance.Intro_Name.text = $"{_Data.item_name}";
        Introduction_Mrg.instance.Intro_Image.sprite = _Data.Display_In_Backpacks;
        Introduction_Mrg.instance.Intro_Value.text = $"价值:{_Data.PriceValue}";
        if (_Data.item_Kind == Item_Kind.Material)
        {
            Introduction_Mrg.instance.Intro_Kind.text = $"类型:制造材料";
        }
        else if (_Data.item_Kind == Item_Kind.Consumable)
        {
            Introduction_Mrg.instance.Intro_Kind.text = $"类型:消耗品";
        }
        Introduction_Mrg.instance.Intro_Introduce.text = $"物品介绍:\n{_Data.Introduction}";
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        //左键点击
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Introduction_Mrg.instance.gameObject.SetActive(true);
            Introduction_Mrg.instance.OnItem(data);
        }
        //右键点击
        else if (eventData.button == PointerEventData.InputButton.Middle)
        {
            Debug.Log("右键点击");
            //后面写使用
            if (data.item_Kind != Item_Kind.Consumable)
            {
                Panel_Mgr.instance.ShowComfirmPanel("该物品无法放进快捷装备栏", true, null);
                return;
            }
            bool HaveArmed = Game_Event.instance.SameEquip(this);
            Panel_Mgr.instance.ShowComfirmPanel($"确定将{data.item_name}{(HaveArmed ? "从装备栏卸下" : "装备至装备栏")}?", false, () =>
            {
                Debug.Log(HaveArmed);
                Game_Event.instance.EquipInQuick(HaveArmed ? null : this);
                Player_Bag.RefrshArms();
            });
        }
    }
    #endregion
    public void OnBeginDrag(PointerEventData eventData)
    {
        Player_Bag.IsDragging = true;
        originalPos = rect.anchoredPosition;
        Player_Bag.currentDraggingItem = this;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out dragMouseOffset);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Player_Bag.IsDragging = true;

        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Player_Bag.Images, eventData.position, eventData.pressEventCamera, out localMousePos);
        rect.anchoredPosition = localMousePos - dragMouseOffset;

        float cellW = Player_Bag.cellSize + Player_Bag.horizontalSpace;
        float cellH = Player_Bag.cellSize + Player_Bag.verticalSpace;

        int gridX = Mathf.RoundToInt(localMousePos.x / cellW);
        int gridY = Mathf.RoundToInt(-localMousePos.y / cellH);

        Debug.Log($"拖拽格子 X:{gridX} Y:{gridY}");
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Player_Bag.Images, eventData.position, eventData.pressEventCamera, out localPoint);

        float cellW = Player_Bag.cellSize + Player_Bag.horizontalSpace;
        float cellH = Player_Bag.cellSize + Player_Bag.verticalSpace;

        float fixOffsetX = data.Width * cellW * 0.5f;
        float fixOffsetY = data.Height * cellH * 0.5f;

        int targetX = Mathf.RoundToInt((localPoint.x - fixOffsetX) / cellW);
        int targetY = Mathf.RoundToInt((-localPoint.y - fixOffsetY) / cellH);

        Try_New_Place(targetX, targetY);
        Player_Bag.currentDraggingItem = null;
        Player_Bag.IsDragging = false;
    }
    public void Try_New_Place(int x, int y)
    {
        Player_Bag.RemoveItem(startPos.x, startPos.y, data.Width, data.Height);
        if (x < 0 || y < 0 || x + data.Width > Player_Bag.Bag_Col || y + data.Height > Player_Bag.Bag_Row || !Player_Bag.Empty_Check(x, y, data.Height, data.Width))
        {
            Player_Bag.PlaceItem(data, startPos.x, startPos.y);
            Back_To_OriginPlace();
            return;
        }
        Player_Bag.PlaceItem(data, x, y);

        Player_Bag.ReClean_Bag_Display();
        Player_Bag.Refresh_Bag_Display();
    }
    public void Back_To_OriginPlace()
    {
        rect.anchoredPosition = originalPos;
    }
    public float CanvasScale()
    {
        return GetComponentInParent<Canvas>().scaleFactor;
    }
}