

using UnityEngine;

[CreateAssetMenu(menuName = "Game/Weapon Profile")]
public class WeaponProfile : ScriptableObject
{
    [Header("Save Id")]
    public string weaponId; // ví dụ: "wp_sword_blazing"

    [Header("Info")]
    public string weaponName;

    [Header("UI Icon (Inventory/Panel)")]
    public Sprite uiIcon;

    [Header("Weapon Visual")]
    public Sprite weaponSprite;
    public Vector3 localPos;
    public float localRotZ;
    public Vector3 localScale = Vector3.one;
    [Header("Aura Effect")]
    public GameObject auraPrefab;
    public Vector3 auraLocalPos;
    public Vector3 auraLocalRot;
    public Vector3 auraLocalScale = Vector3.one;
    [Header("Arm Visual (Prefab)")]
    public GameObject armPrefab;

    [Header("Stats Bonus")]
    public StatBonus bonus;
   // public int level;

    public int level;
    public int enhanceLevel;
    public int exp;
    public int ascend;

    [Header("Skills")]
    public WeaponSkill primarySkill;      // Q
    public WeaponSkill secondarySkill;    // E (skill nhỏ)
    public WeaponSkill ultimateSkill;     // R
    public WeaponSkill passiveSkill;      // Nội tại
}