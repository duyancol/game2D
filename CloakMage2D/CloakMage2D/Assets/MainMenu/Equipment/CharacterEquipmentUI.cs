
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CharacterEquipmentUI : MonoBehaviour
{
    [System.Serializable]
    public class SlotUI
    {
        public EquipSlot slot;
        public Image icon;
        public Image background;
    }

    [SerializeField] private Sprite greenFrame;
    [SerializeField] private Sprite blueFrame;
    [SerializeField] private Sprite purpleFrame;
    [SerializeField] private Sprite orangeFrame;
    public PlayerStatsMono playerStats;
    [Header("Panel")]
    [SerializeField] private GameObject rootPanel;

    public List<SlotUI> slots;
    public ItemSO[] allItems;
    Dictionary<EquipSlot, SlotUI> _slotMap;
    Dictionary<EquipSlot, long> equippedIds = new Dictionary<EquipSlot, long>();
    InventoryLoader inventoryLoader;
    void Awake()
    {
        _slotMap = new Dictionary<EquipSlot, SlotUI>();
        
        foreach (var s in slots)
        {
            _slotMap[s.slot] = s;
        }
    }

    //void Start()
    //{

    //    inventoryLoader = FindObjectOfType<InventoryLoader>();
    //    StartCoroutine(LoadEquipped());
    //}
    IEnumerator Start()
    {
        yield return new WaitUntil(() => FindObjectOfType<InventoryLoader>() != null);

        inventoryLoader = FindObjectOfType<InventoryLoader>();

        StartCoroutine(LoadEquipped());
    }
    IEnumerator LoadEquipped()
    {
        string url = "https://userservice-production-fd72.up.railway.app/api/equipment/equipped?userId="+PlayerSession.UserId;
      
        equippedIds.Clear();
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("API ERROR: " + request.error);
            yield break;
        }

        string json = request.downloadHandler.text;

        // Unity JsonUtility không đọc được root array
        json = "{ \"items\": " + json + "}";

        EquipmentList list = JsonUtility.FromJson<EquipmentList>(json);
        if (inventoryLoader == null)
        {
            Debug.LogError("InventoryLoader NOT FOUND in scene");
            yield break;
        }

        if (inventoryLoader.allItems == null)
        {
            Debug.LogError("InventoryLoader.allItems NULL");
            yield break;
        }
        foreach (var item in list.items)
        {
            EquipSlot slot = GetSlotFromItemId(item.itemId);

            // ⭐ lưu id item đang equip
            equippedIds[slot] = item.id;
            ItemSO itemSO = System.Array.Find(
     allItems,
     x => x.itemId == item.itemId
 
 );
 

            if (itemSO == null)
            {
                Debug.LogError("ItemSO NOT FOUND: " + item.itemId);
                continue;
            }

            Sprite icon = itemSO.icon;

            ItemRarity rarity = ParseRarity(item.quality);

            SetEquip(slot, icon, rarity);
            EquipmentBonus bonus = ConvertToBonus(item, slot);
            playerStats.EquipArmor(bonus, icon, item.quality);
        }
    }
    public long GetEquippedId(EquipSlot slot)
    {
        if (equippedIds.ContainsKey(slot))
            return equippedIds[slot];

        return -1;
    }
    EquipmentBonus ConvertToBonus(EquipmentResponse data, EquipSlot slot)
    {
        EquipmentBonus b = new EquipmentBonus();
        b.slot = slot;

        ApplyFlatStat(ref b, data.mainStat, data.mainValue);

        if (!string.IsNullOrEmpty(data.subStatsJson))
        {
            List<SubStat> subStats =
                JsonHelper.FromJson<SubStat>(data.subStatsJson);

            foreach (var sub in subStats)
            {
                ApplyPercentStat(ref b, sub.type, sub.value);
            }
        }

        return b;
    }
    void ApplyFlatStat(ref EquipmentBonus b, string type, float value)
    {
        switch (type.ToLower())
        {
            case "hp": b.flatHP += Mathf.RoundToInt(value); break;
            case "atk":
            case "attack": b.flatAtk += value; break;
            case "smpt": b.flatSmpt += value; break;
            case "def": b.flatDef += value; break;
            case "mdef": b.flatMdef += value; break;
            case "crit": b.flatCritChance += value; break;
            case "crit_damage": b.flatCritDamage += value; break;
        }
    }

    void ApplyPercentStat(ref EquipmentBonus b, string type, float value)
    {
        float percent = value / 100f;

        switch (type.ToLower())
        {
            case "hp": b.percentHP += percent; break;
            case "atk":
            case "attack": b.percentAtk += percent; break;
            case "smpt": b.percentSmpt += percent; break;
            case "def": b.percentDef += percent; break;
            case "mdef": b.percentMdef += percent; break;
            case "crit": b.percentCritChance += percent; break;
            case "crit_damage": b.percentCritDamage += percent; break;
        }
    }
    // ⭐ Equip
    public void SetEquip(EquipSlot slot, Sprite icon, ItemRarity rarity)
    {
        if (_slotMap.ContainsKey(slot))
        {
            var s = _slotMap[slot];

            s.icon.sprite = icon;
            s.icon.enabled = true;

            s.background.sprite = GetFrameByRarity(rarity);
        }
    }

    // ⭐ Unequip
    public void RemoveEquip(EquipSlot slot)
    {
        if (_slotMap.ContainsKey(slot))
        {
            var s = _slotMap[slot];

            s.icon.sprite = null;
            s.icon.enabled = false;

            s.background.sprite = greenFrame;
        }
    }

    Sprite GetFrameByRarity(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Green: return greenFrame;
            case ItemRarity.Blue: return blueFrame;
            case ItemRarity.Purple: return purpleFrame;
            case ItemRarity.Orange: return orangeFrame;
        }

        return greenFrame;
    }

    ItemRarity ParseRarity(string q)
    {
        switch (q)
        {
            case "GREEN": return ItemRarity.Green;
            case "BLUE": return ItemRarity.Blue;
            case "PURPLE": return ItemRarity.Purple;
            case "ORANGE": return ItemRarity.Orange;
        }

        return ItemRarity.Green;
    }

    EquipSlot GetSlotFromItemId(string itemId)
    {
        itemId = itemId.ToLower();

        if (itemId.Contains("helm"))
            return EquipSlot.Helm;

        if (itemId.Contains("armor"))
            return EquipSlot.Armor;

        if (itemId.Contains("boots"))
            return EquipSlot.Boots;

        if (itemId.Contains("earring"))
            return EquipSlot.Earring;

        if (itemId.Contains("feather"))
            return EquipSlot.Feather;

        if (itemId.Contains("clock"))
            return EquipSlot.Clock;

        Debug.LogWarning("Unknown itemId slot: " + itemId);

        return EquipSlot.Helm;
    }

    public void Open()
    {
        if (rootPanel != null)
            rootPanel.SetActive(true);
    }

    public void Close()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);
    }

    public void Toggle()
    {
        if (rootPanel != null)
            rootPanel.SetActive(!rootPanel.activeSelf);
    }
    public void ReloadEquip()
    {
        StopAllCoroutines();
        StartCoroutine(LoadEquipped());
    }
    // =========================
    // JSON Classes
    // =========================

    [System.Serializable]
    public class EquipmentList
    {
        public EquipmentResponse[] items;
    }

    [System.Serializable]
    public class EquipmentResponse
    {
        public long id;
        public string itemId;
        public string quality;
        public string mainStat;
        public int mainValue;
        public string subStatsJson;
        public bool equipped;
    }
}