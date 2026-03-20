using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game/Equipment Profile")]
public class EquipmentProfile : ScriptableObject
{
    [Header("Basic Info")]
    public string equipmentId;
    public string equipmentName;
    public EquipSlot slot;
    public ItemRarity rarity;

    [Header("Main Stat")]
    public StatEntry mainStat;   // ⭐ 1 chỉ số chính

    [Header("Sub Stats (Random Pool)")]
    public List<StatType> possibleSubStats;  // Danh sách có thể random

    [Header("Visual")]
    public Sprite uiIcon;

    [Header("Weapon Only (optional)")]
    public WeaponProfile weaponProfile;
}
