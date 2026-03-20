
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public enum InventoryTab
    {
        Equipment,
        Item
    }

    [Header("Refs")]
    public GameObject panel;
    public Transform gridRoot;
    public InventorySlotUI slotPrefab;
    [Header("Item UI")]
    public ItemPanelUI itemPanelUI;
    [Header("Equipment UI")]
    public EquipmentPanelUI equipmentPanelUI;

    [Header("Tabs")]
    public Button tabEquipment;
    public Button tabItem;

    [Header("Weapon UI")]
    public WeaponController weaponController;
    public WeaponPanelUI weaponPanelUI;

    InventorySlotUI[] slotUIs;
    List<int> filteredIndexes = new();   // ⭐ lưu index gốc trong InventorySystem
    InventoryTab currentTab = InventoryTab.Equipment;
    public static bool IsOpen { get; private set; }

    int selectedIndex = -1;
    bool built = false;

    void Start()
    {
        if (panel != null)
            panel.SetActive(false);

        TryBuild();
        if (itemPanelUI != null)
            itemPanelUI.Init(this);


        if (tabEquipment != null)
            tabEquipment.onClick.AddListener(() =>
            {
                currentTab = InventoryTab.Equipment;
                Refresh();
            });

        if (tabItem != null)
            tabItem.onClick.AddListener(() =>
            {
                currentTab = InventoryTab.Item;
                Refresh();
            });

        if (weaponPanelUI != null && weaponController != null)
            weaponPanelUI.Init(weaponController);
    }
    public void UseItemFromPanel(int realIndex)
    {
        InventorySystem.I.UseItem(realIndex, weaponController.gameObject);
        Refresh();
    }

    //public void Toggle()
    //{
    //    if (panel == null)
    //        return;

    //    panel.SetActive(!panel.activeSelf);

    //    if (panel.activeSelf)
    //    {
    //        TryBuild();
    //        Refresh();

    //        selectedIndex = -1;

    //        for (int i = 0; i < slotUIs.Length; i++)
    //            slotUIs[i].SetSelected(false);
    //    }
    //}
    public void Toggle()
    {
        if (panel == null)
            return;

        bool isActive = !panel.activeSelf;
        panel.SetActive(isActive);

        // ⭐ Tắt / bật đánh
        //if (weaponController != null)
        //    weaponController.enabled = !isActive;
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            if (weaponController != null)
                weaponController.enabled = !isActive;
        }
        if (!isActive)
            return;

        TryBuild();
        Refresh();

        selectedIndex = -1;

        for (int i = 0; i < slotUIs.Length; i++)
            slotUIs[i].SetSelected(false);
    }


    void TryBuild()
    {
        if (built && slotUIs != null && slotUIs.Length > 0)
            return;

        if (InventorySystem.I == null || gridRoot == null || slotPrefab == null)
            return;

        int count = InventorySystem.I.slotCount;

        for (int i = gridRoot.childCount - 1; i >= 0; i--)
            Destroy(gridRoot.GetChild(i).gameObject);

        slotUIs = new InventorySlotUI[count];

        for (int i = 0; i < count; i++)
        {
            var ui = Instantiate(slotPrefab, gridRoot);
            ui.Init(this, i); // index UI, không phải index inventory
            slotUIs[i] = ui;
        }

        built = true;
    }

    public void Refresh()
    {
        if (InventorySystem.I == null || slotUIs == null)
            return;

        filteredIndexes.Clear();

        for (int i = 0; i < InventorySystem.I.slots.Count; i++)
        {
            var slot = InventorySystem.I.slots[i];

            if (slot == null || slot.IsEmpty)
                continue;

            if (currentTab == InventoryTab.Equipment &&
                slot.item.itemType == ItemType.Equipment)
            {
                filteredIndexes.Add(i);
            }

            if (currentTab == InventoryTab.Item &&
                slot.item.itemType != ItemType.Equipment)
            {
                filteredIndexes.Add(i);
            }
        }

        for (int i = 0; i < slotUIs.Length; i++)
        {
            if (i < filteredIndexes.Count)
            {
                int realIndex = filteredIndexes[i];
                slotUIs[i].Render(InventorySystem.I.slots[realIndex]);
            }
            else
            {
                slotUIs[i].Render(null);
            }
        }
    }


    public void OnClickSlot(int uiIndex)
    {
        if (uiIndex >= filteredIndexes.Count)
            return;

        selectedIndex = uiIndex;

        for (int i = 0; i < slotUIs.Length; i++)
            slotUIs[i].SetSelected(i == selectedIndex);

        int realIndex = filteredIndexes[uiIndex];
        var slot = InventorySystem.I.slots[realIndex];

        if (slot == null || slot.IsEmpty)
            return;

        //if (slot.item.equipmentProfile != null && equipmentPanelUI != null)
        //{
        //    equipmentPanelUI.Show(slot.equipmentInstanceId, slot.item.icon);




        //    return;
        //}

        if (slot.item.equipmentProfile != null && equipmentPanelUI != null)
        {
            equipmentPanelUI.Show(
                slot.equipmentInstanceId,
                slot.item.equipmentProfile,   // ⭐ QUAN TRỌNG
                slot.item.icon
            );

            return;
        }
        // ⭐ Nếu là Weapon
        //if (slot.item.weaponProfile != null && weaponPanelUI != null)
        //{
        //    weaponPanelUI.SetWeapon(slot.item.weaponProfile);
        //    weaponPanelUI.panelWeapon.SetActive(true);
        //    return;
        //}
        if (slot.item.weaponProfile != null && weaponPanelUI != null)
        {
            weaponPanelUI.panelWeapon.SetActive(true);          // ✅ bật trước
            weaponPanelUI.SetWeapon(slot.item.weaponProfile);   // ✅ gọi sau
            return;
        }
        // ⭐ Item thường
        if (itemPanelUI != null)
        {
            itemPanelUI.Show(slot.item, realIndex);
        }
    }

}
