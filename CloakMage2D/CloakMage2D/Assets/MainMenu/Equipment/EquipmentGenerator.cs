using UnityEngine;
public static class EquipmentGenerator
{
    public static EquipmentInstance Create(EquipmentProfile profile)
    {
        EquipmentInstance item = new EquipmentInstance();
        item.profile = profile;
        item.mainStat = profile.mainStat;

        int subCount = GetSubCount(profile.rarity);

        for (int i = 0; i < subCount; i++)
        {
            StatType randomStat = profile.possibleSubStats[
                Random.Range(0, profile.possibleSubStats.Count)
            ];

            StatEntry entry = new StatEntry();
            entry.statType = randomStat;
            entry.value = Random.Range(5, 20);

            item.subStats.Add(entry);
        }

        return item;
    }

    static int GetSubCount(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Green: return 1;
            case ItemRarity.Blue: return 2;
            case ItemRarity.Purple: return 3;
            case ItemRarity.Orange: return 4;
        }
        return 1;
    }
}
