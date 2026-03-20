using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InvSlot
{
    public ItemSO item;
    public int amount;
    public EquipmentResponse equipmentData;
    public long serverId;
    public int id;
    public long equipmentInstanceId;




    public bool IsEmpty => item == null || amount <= 0;
}

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem I;

    public int slotCount = 28;
    //public List<InvSlot> slots = new List<InvSlot>();
    [HideInInspector]
    public List<InvSlot> slots = new();

    //void Awake()
    //{
    //    if (I != null)
    //    {
    //        Destroy(gameObject);
    //        return;
    //    }

    //    I = this;
    //    Debug.Log("InventorySystem Awake on " + gameObject.name);

    //    Init();
    //}
    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;

        DontDestroyOnLoad(gameObject); // ⭐ QUAN TRỌNG

        Debug.Log("InventorySystem Awake on " + gameObject.name);

        Init();
    }
    public void AddFromServer(ItemSO item, int amount, long serverId, long equipmentInstanceId)

    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsEmpty)
            {
                slots[i].item = item;
                slots[i].amount = amount;
                slots[i].serverId = serverId;
                slots[i].equipmentInstanceId = equipmentInstanceId;
                return;
            }
        }
    }
 


    void Init()
    {
        slots.Clear();

        for (int i = 0; i < slotCount; i++)
            slots.Add(new InvSlot());
    }

    public void Add(ItemSO item, int amount = 1)
    {
        // 1️⃣ Tìm slot đã có item giống vậy
        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].IsEmpty && slots[i].item == item)
            {
                slots[i].amount += amount;
                Debug.Log("Stacked: " + item.itemName + " x" + slots[i].amount);
                return;
            }
        }

        // 2️⃣ Nếu chưa có → tìm slot trống
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsEmpty)
            {
                slots[i].item = item;
                slots[i].amount = amount;
                Debug.Log("Added new: " + item.itemName);
                return;
            }
        }

        Debug.Log("Inventory Full!");
    }

    public void UseItem(int index, GameObject user)
    {
        var slot = slots[index];
        Debug.Log("UseItem called: " + index);

        if (slot.item.useEffect != null)
        {
            slot.item.useEffect.Execute(user);
        }

        slot.amount--;

        if (slot.amount <= 0)
            slot.item = null;
    }
    public void ClearInventory()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].item = null;
            slots[i].amount = 0;
        }
    }
    //public int GetAmount(ItemSO item)
    //{
    //    int total = 0;

    //    for (int i = 0; i < slots.Count; i++)
    //    {
    //        if (!slots[i].IsEmpty && slots[i].item == item)
    //        {
    //            total += slots[i].amount;
    //        }
    //    }

    //    return total;
    //}
    public int GetAmount(ItemSO item)
    {
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty && slot.item.itemId == item.itemId)
            {
                return slot.amount;
            }
        }
        return 0;
    }
    public void SetAmount(ItemSO item, int amount)
    {
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty && slot.item.itemId == item.itemId)
            {
                slot.amount = amount;
                return;
            }
        }

        // nếu chưa có thì add mới
        Add(item, amount);
    }
    public void Remove(ItemSO item, int amount)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (amount <= 0) return;

            if (!slots[i].IsEmpty && slots[i].item == item)
            {
                int take = Mathf.Min(slots[i].amount, amount);

                slots[i].amount -= take;
                amount -= take;

                if (slots[i].amount <= 0)
                {
                    slots[i].item = null;
                }
            }
        }
    }
    public ItemSO GetItemById(string id)
    {
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty && slot.item.itemId == id)
            {
                return slot.item;
            }
        }
        return null;
    }

}
