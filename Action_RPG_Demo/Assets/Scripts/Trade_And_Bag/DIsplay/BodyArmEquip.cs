using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BodyArmEquip : MonoBehaviour, IPointerClickHandler
{
    public WeaponKind NowKind;
    public Item_Data Data
    {
        get => _data;
        set
        {
            _data = value;
            if (_data == null)
            {
                Weapon.gameObject.SetActive(false);
            }
            else
            {
                Weapon.sprite = _data.Display_In_Backpacks;
                Color c = Weapon.color;
                c.a = 1f;
                Weapon.color = c;
                Weapon.gameObject.SetActive(true);
            }
        }
    }
    private Item_Data _data;
    public Image Bottom;
    public Image Weapon;
    public ShowIntro ShowData;

    public void Awake()
    {
        Bottom = GetComponent<Image>();
        Image[] Weapons = GetComponentsInChildren<Image>();
        foreach (var i in Weapons)
        {
            if (i.transform.childCount <= 0)
            {
                Weapon = i;
                ShowData = i.GetComponent<ShowIntro>();
                break;
            }
        }
    }
    public void Start()
    {
        if (Data)
        {
            ShowData.data = Data;
        }
    }
    public void OnEnable()
    {
        Game_Event.instance.EquipWeapon += ReceiveEquip;
        Game_Event.instance.RefreshAllArmEquip += OnRefreshFromSave;
    }
    public void OnDisable()
    {
        Game_Event.instance.EquipWeapon -= ReceiveEquip;
        Game_Event.instance.RefreshAllArmEquip -= OnRefreshFromSave;
    }
    private void OnRefreshFromSave(EquipWeaponData saveEquip, AllData_Item allData)
    {
        int targetId = -1;
        switch (NowKind)
        {
            case WeaponKind.Head:
                targetId = saveEquip.HeadData;
                break;
            case WeaponKind.Chest:
                targetId = saveEquip.ChestData;
                break;
            case WeaponKind.Hand:
                targetId = saveEquip.HandData;
                break;
            case WeaponKind.Foot:
                targetId = saveEquip.FootData;
                break;
            case WeaponKind.Armament:
                targetId = saveEquip.OnHandData;
                break;
        }
        if (targetId <= 0)
        {
            Data = null;
            return;
        }
        Item_Data findItem = allData.Data_List.Find(item => item.item_id == targetId);
        Data = findItem;
    }
    public void ReceiveEquip(Item_Data data)
    {

        if (data.EquipmentSlot != NowKind){
            return;
        }
        if (data.EquipmentSlot == NowKind)
        {
            if (Data)
            {
                Game_Event.instance.SendReturnOldEquip(Data);
            }
            Data = data;
            ShowData.data = Data;
            Weapon.sprite = data.Display_In_Backpacks;

            Game_Event.instance.SendBackArmData(this);
        }
        return;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Data)
        {
            return;
        }
        Panel_Mgr.instance.ShowComfirmPanel($"È·¶¨Ð¶ÏÂ{Data.item_name}?", false, () =>
        {
            Game_Event.instance.SendReturnOldEquip(Data);
            Data = null;
            ShowData.data = Data;
            Game_Event.instance.SendBackArmData(this);
        });
    }
}