using UnityEngine;

[CreateAssetMenu(menuName = "Game/Character Profile")]
public class CharacterProfile : ScriptableObject
{
    [Header("ID")]
    public string characterId;

    [Header("Info")]
    public string characterName;
    public Sprite icon;

    [Header("Visual")]
    public GameObject visualPrefab;   // 🔥 cái này thay NV1 NV2

    [Header("Base Stats")]
    public int baseHP;
    public int baseATK;
    public float moveSpeed;

    [Header("Weapon Default")]
    public WeaponProfile defaultWeapon;

    [Header("Skills")]
    public WeaponSkill primarySkill;
    public WeaponSkill secondarySkill;
    public WeaponSkill ultimateSkill;
    public WeaponSkill passiveSkill;
}