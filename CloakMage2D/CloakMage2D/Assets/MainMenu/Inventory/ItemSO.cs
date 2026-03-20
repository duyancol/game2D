
//using UnityEngine;

//public enum ItemType
//{
//    Equipment,   // Trang bị
//    Consumable,  // Vật phẩm (potion...)
//    Material     // Nguyên liệu
//}

//[CreateAssetMenu(menuName = "Game/Item")]
//public class ItemSO : ScriptableObject
//{
//    public string itemId;   // PHẢI trùng backend

//    public string itemName;
//    public Sprite icon;
//    public UseEffectSO useEffect;

//    public ItemType itemType;   // ⭐ THÊM DÒNG NÀY
//    [TextArea(3, 6)]
//    public string description;

//    [Header("Weapon (optional)")]
//    public WeaponProfile weaponProfile;
//}
using UnityEngine;

public enum ItemType
{
    Equipment,
    Consumable,
    Material
}

[CreateAssetMenu(menuName = "Game/Item")]
public class ItemSO : ScriptableObject
{
    public string itemId;
    public string itemName;
    public Sprite icon;
    public UseEffectSO useEffect;

    public ItemType itemType;

    [TextArea(3, 6)]
    public string description;

    [Header("Equipment")]
    public EquipmentProfile equipmentProfile;

    [Header("Weapon Only (Legacy Support)")]
    public WeaponProfile weaponProfile;
}
