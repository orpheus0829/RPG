using DG.Tweening;
using MMD4MecanimInternal;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Item_Dragger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Item_Data data;
    public TextMeshProUGUI Count;
    public int CNum;
    public Vector2Int startPos;
    public Player_Bag Player_Bag;

    public RectTransform rect;
    public Vector2 originalPos;
    private Vector2 dragMouseOffset;

    public void Awake()
    {
        rect = GetComponent<RectTransform>();
        Count = GetComponentInChildren<TextMeshProUGUI>();
    }
    public void OnEnable()
    {
        //Introduction_Mrg.instance.ClickOnItem += Display_Intro;
    }
    public void OnDisable()
    {
        
    }
    public void SetStackCount(int count)
    {
        CNum = count;
        if (!Count)
        {
            return;
        }
        if (CNum <= 1)
        {
            Count.gameObject.SetActive(false);
        }
        else
        {
            Count.gameObject.SetActive(true);
            Count.text = CNum.ToString();
        }
    }
    public void Update()
    {
        if (data.item_Kind == Item_Kind.Weapon)
        {
            SetStackCount(1);
        }
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
        int stackCount = item.CNum;
        Player_Bag.RemoveItem(sx, sy, sw, sh);
        List<Vector3> vaildpoint = FindVaildGround(Player_Bag.gameObject.transform.position, Player_Bag.DropRadius);
        Vector3 drop_pos;
        if (vaildpoint.Count <= 0)
        {
            drop_pos = new Vector3(Player_Bag.gameObject.transform.position.x + UnityEngine.Random.Range(-2f, 2f), Player_Bag.gameObject.transform.position.y + 0.5f, Player_Bag.gameObject.transform.position.z + UnityEngine.Random.Range(-2f, 2f));
            while (Vector3.Distance(drop_pos, Player_Bag.gameObject.transform.position) < 1f)
            {
                drop_pos = new Vector3(Player_Bag.gameObject.transform.position.x + UnityEngine.Random.Range(-2f, 2f), Player_Bag.gameObject.transform.position.y + 0.5f, Player_Bag.gameObject.transform.position.z + UnityEngine.Random.Range(-2f, 2f));
            }
        }
        else
        {
            drop_pos = vaildpoint[UnityEngine.Random.Range(0, vaildpoint.Count)];
        }
        for (int i = 0; i < stackCount; i++)
        {
            ObjectPoolMgr.instance.GetObj(data.Drop, drop_pos, Quaternion.identity);
        }

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
        int stackCount = item.CNum;
        Player_Bag.RemoveItem(sx, sy, sw, sh);
        List<Vector3> vaildpoint = FindVaildGround(Player_Bag.gameObject.transform.position, Player_Bag.DropRadius);
        Vector3 drop_pos;
        if (vaildpoint.Count <= 0)
        {
            drop_pos = new Vector3(Player_Bag.gameObject.transform.position.x + UnityEngine.Random.Range(-2f, 2f), Player_Bag.gameObject.transform.position.y + 0.5f, Player_Bag.gameObject.transform.position.z + UnityEngine.Random.Range(-2f, 2f));
            while (Vector3.Distance(drop_pos, Player_Bag.gameObject.transform.position) < 1f)
            {
                drop_pos = new Vector3(Player_Bag.gameObject.transform.position.x + UnityEngine.Random.Range(-2f, 2f), Player_Bag.gameObject.transform.position.y + 0.5f, Player_Bag.gameObject.transform.position.z + UnityEngine.Random.Range(-2f, 2f));
            }
        }
        else
        {
            drop_pos = vaildpoint[UnityEngine.Random.Range(0, vaildpoint.Count)];
        }
        for (int i = 0; i < stackCount; i++)
        {
            ObjectPoolMgr.instance.GetObj(dropitem.Drop, drop_pos, Quaternion.identity);
        }
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
    //public void Display_Intro(Item_Data _Data)
    //{
    //    if (_Data.item_Kind == Item_Kind.Material)
    //    {
    //        string a = $"类型:制造材料";
    //        string content = $"价值:{_Data.PriceValue}\n{a}";
    //        Introduction_Mrg.instance.Intro_Name.text = $"{_Data.item_name}\n\n\n{content}";
    //    }
    //    else if (_Data.item_Kind == Item_Kind.Consumable)
    //    {
    //        string a = $"类型:消耗品";
    //        string content = $"价值:{_Data.PriceValue}\n{a}";
    //        Introduction_Mrg.instance.Intro_Name.text = $"{_Data.item_name}\n\n\n{content}";
    //    }
    //    Introduction_Mrg.instance.Intro_Image.sprite = _Data.Display_In_Backpacks;
    //    Introduction_Mrg.instance.Intro_Introduce.text = $"物品介绍:\n{_Data.Introduction}";
    //}
    public void OnPointerClick(PointerEventData eventData)
    {
        //左键点击
        if (eventData.button == PointerEventData.InputButton.Left)
        {

        }
        //中键点击
        else if (eventData.button == PointerEventData.InputButton.Middle)
        {
            Introduction_Mrg.instance.StopTrack();
            //后面写使用
            if (data.item_Kind == Item_Kind.Material)
            {
                PickNoticeMgr.instance.ShowFieldTip($"无法将原材料装备至道具栏");
                return;
            }
            else if (data.item_Kind == Item_Kind.Consumable)
            {
                bool HaveArmed = Game_Event.instance.SameEquip(this);
                Panel_Mgr.instance.ShowComfirmPanel($"确定将{data.item_name}{(HaveArmed ? "从道具栏卸下" : "装备至道具栏")}?", false, () =>
                {
                    Debug.Log(HaveArmed);
                    PickNoticeMgr.instance.ShowFieldTip($"已将{data.item_name}{(HaveArmed ? "从道具栏卸下" : "装备至道具栏")}");
                    Game_Event.instance.EquipInQuick(HaveArmed ? null : this);
                    Player_Bag.RefrshArms();
                });
                return;
            }
            else if (data.item_Kind == Item_Kind.Weapon)
            {
                Panel_Mgr.instance.ShowComfirmPanel($"确定将{data.item_name}装备至装备栏",false, () =>
                {
                    Game_Event.instance.ShowArms();

                    PickNoticeMgr.instance.ShowFieldTip($"已将{data.item_name}装备至装备栏");
                    Game_Event.instance.EquipW(data);
                    RemoveSelfItem();
                });
                return;
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {

        }
    }
    public Item_Data GetData(Item_Data data)
    {
        return data;
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
        Player_Bag.ReClean_Bag_Display();
        Player_Bag.Refresh_Bag_Display();
    }
    public void Try_New_Place(int targetX, int targetY)
    {
        Bag_SingleSlot[,] currentBag = Player_Bag.CurrentViewBag;
        int selfStackCount = currentBag[startPos.y, startPos.x].stackCount;

        int selfMainX = startPos.x;
        int selfMainY = startPos.y;
        Item_Data selfItem = data;
        int selfW = data.Width;
        int selfH = data.Height;

        if (GetSlotItemInfo(targetX, targetY, out Item_Data targetItem, out int tMainX, out int tMainY, out int tW, out int tH, out int tStack))
        {
            if (selfItem.item_id == targetItem.item_id && selfItem.Stackable && selfItem.item_Kind != Item_Kind.Weapon)
            {
                if (tStack >= selfItem.StackMax)
                {
                    Back_To_OriginPlace();
                    return;
                }
                if (tStack + selfStackCount > selfItem.StackMax)
                {
                    Back_To_OriginPlace();
                    return;
                }

                Player_Bag.RemoveItem(selfMainX, selfMainY, selfW, selfH);
                int newTotal = currentBag[tMainY, tMainX].stackCount + selfStackCount;
                newTotal = Mathf.Min(newTotal, selfItem.StackMax);
                currentBag[tMainY, tMainX].stackCount = newTotal;

                Destroy(gameObject);
                Player_Bag.Init_Resort_List();
                Player_Bag.ReClean_Bag_Display();
                Player_Bag.Refresh_Bag_Display();
                return;
            }
            if (selfW != tW || selfH != tH)
            {
                Back_To_OriginPlace();
                return;
            }
            Bag_SingleSlot[,] tempBag = Player_Bag.DeepCloneBag();
            Player_Bag.RemoveItem(selfMainX, selfMainY, selfW, selfH);
            Player_Bag.RemoveItem(tMainX, tMainY, tW, tH);
            bool aCanPutAtB = Player_Bag.Empty_Check(tMainX, tMainY, selfH, selfW);
            bool bCanPutAtA = Player_Bag.Empty_Check(selfMainX, selfMainY, tH, tW);
            if (aCanPutAtB && bCanPutAtA)
            {
                currentBag[tMainY, tMainX].stackCount = selfStackCount;
                Player_Bag.PlaceItem(selfItem, tMainX, tMainY);
                currentBag[selfMainY, selfMainX].stackCount = tStack;
                Player_Bag.PlaceItem(targetItem, selfMainX, selfMainY);

                Destroy(gameObject);
                Player_Bag.Init_Resort_List();
                Player_Bag.ReClean_Bag_Display();
                Player_Bag.Refresh_Bag_Display();
                Player_Bag.StartCoroutine(PlaySwapAnimDelay(tMainX, tMainY, selfMainX, selfMainY));
                return;
            }
            else
            {
                Back_To_OriginPlace();
                Player_Bag.bag = tempBag;
                Player_Bag.ReClean_Bag_Display();
                Player_Bag.Refresh_Bag_Display();
                return;
            }
        }
        Player_Bag.RemoveItem(selfMainX, selfMainY, selfW, selfH);
        var bagSize = Player_Bag.GetCurrentBagSize();
        bool canPlace = !(targetX < 0 || targetY < 0 || targetX + selfW > bagSize.Col || targetY + selfH > bagSize.Row || !Player_Bag.Empty_Check(targetX, targetY, selfH, selfW));
        if (!canPlace)
        {
            Back_To_OriginPlace();
            currentBag[selfMainY, selfMainX].stackCount = selfStackCount;
            Player_Bag.PlaceItem(selfItem, selfMainX, selfMainY);
            Player_Bag.ReClean_Bag_Display();
            Player_Bag.Refresh_Bag_Display();
            return;
        }
        currentBag[targetY, targetX].stackCount = selfStackCount;
        Player_Bag.PlaceItem(selfItem, targetX, targetY);
        Player_Bag.Init_Resort_List();
        Player_Bag.ReClean_Bag_Display();
        Player_Bag.Refresh_Bag_Display();
    }
    public bool GetSlotItemInfo(int x, int y, out Item_Data targetItem, out int mainX, out int mainY, out int w, out int h, out int stackNum)
    {
        targetItem = null;
        mainX = -1;
        mainY = -1;
        w = 0;
        h = 0;
        stackNum = 0;
        var bagSize = Player_Bag.GetCurrentBagSize();
        if (x < 0 || y < 0 || x >= bagSize.Col || y >= bagSize.Row)
        {
            return false;
        }
        Bag_SingleSlot slot = Player_Bag.CurrentViewBag[y, x];
        if (!slot.Have_Item)
        {
            return false;
        }
        targetItem = Player_Bag.allData_Item.Data_List.Find(t => t.item_id == slot.item_ID);
        mainX = slot.Start_x;
        mainY = slot.Start_y;
        w = slot.real_width;
        h = slot.real_height;
        stackNum = slot.stackCount;
        return true;
    }
    public void Back_To_OriginPlace()
    {
        rect.anchoredPosition = originalPos;
    }
    public float CanvasScale()
    {
        return GetComponentInParent<Canvas>().scaleFactor;
    }
    private IEnumerator PlaySwapAnimDelay(int oldX, int oldY, int newX, int newY)
    {
        yield return null;
        float cellW = Player_Bag.cellSize + Player_Bag.horizontalSpace;
        float cellH = Player_Bag.cellSize + Player_Bag.verticalSpace;
        Vector2 oldPos = new Vector2(
            oldX * cellW + cellW * 0.5f,
            -(oldY * cellH + cellH * 0.5f)
        );
        Vector2 newPos = new Vector2(
            newX * cellW + cellW * 0.5f,
            -(newY * cellH + cellH * 0.5f)
        );
        Transform imagesRoot = Player_Bag.Images;
        foreach (Transform child in imagesRoot)
        {
            Item_Dragger drag = child.GetComponent<Item_Dragger>();
            if (drag != null && drag.startPos.x == newX && drag.startPos.y == newY)
            {
                drag.rect.anchoredPosition = oldPos;

                drag.rect.DOAnchorPos(newPos, 0.22f)
                    .SetEase(Ease.OutBack, 1.3f)
                    .SetUpdate(UpdateType.Normal, true);
                break;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //Introduction_Mrg.instance.StopTrack();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Introduction_Mrg.instance.SetContent(data);
        Introduction_Mrg.instance.StartTrack();
    }
    public void RemoveSelfItem()
    {
        Bag_SingleSlot[,] curBag = Player_Bag.CurrentViewBag;
        int sx = startPos.x;
        int sy = startPos.y;
        int w = data.Width;
        int h = data.Height;
        Player_Bag.RemoveItem(sx, sy, w, h);
        Player_Bag.Init_AllResortList();
        Player_Bag.ReClean_Bag_Display();
        Player_Bag.Refresh_Bag_Display();
    }
}