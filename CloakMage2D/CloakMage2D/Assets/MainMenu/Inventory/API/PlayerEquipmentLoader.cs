using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerEquipmentLoader : MonoBehaviour
{
    public PlayerStatsMono playerStats;
    public ItemSO[] allItems;
    CharacterEquipmentUI equipmentUI;
    void Start()
    {
        equipmentUI = FindObjectOfType<CharacterEquipmentUI>();
        StartCoroutine(LoadEquipped());
    }
    IEnumerator LoadEquipped()
    {
        yield return null;   // đợi UI init xong
        string url = "https://userservice-production-fd72.up.railway.app/api/equipment/equipped?userId=" + PlayerSession.UserId;

        UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Load equipped error: " + request.error);
            yield break;
        }

        string json = request.downloadHandler.text;

        // Fix JsonUtility root array
        json = "{ \"items\": " + json + "}";

        EquipmentList list = JsonUtility.FromJson<EquipmentList>(json);

        foreach (var item in list.items)
        {
            ItemSO itemSO = System.Array.Find(allItems, x => x.itemId == item.itemId);

            if (itemSO == null)
            {
                Debug.LogError("ItemSO not found: " + item.itemId);
                continue;
            }

            EquipSlot slot = GetSlotFromItemId(item.itemId);

            EquipmentBonus bonus = ConvertToBonus(item, slot);

            playerStats.EquipArmor(bonus, itemSO.icon, item.quality);

            if (equipmentUI != null)
            {
                equipmentUI.SetEquip(slot, itemSO.icon, ParseRarity(item.quality));
            }
        }
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

        Debug.LogWarning("Unknown slot for itemId: " + itemId);

        return EquipSlot.Helm;
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