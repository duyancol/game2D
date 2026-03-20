using System;
using UnityEngine;

public class WeaponController_SwitchOnly : MonoBehaviour
{
    [Header("Refs")]
    public Transform owner;
    public Transform weaponPivot;
    public SpriteRenderer weaponRenderer;
    public ArmWeaponController armController;

    [Header("Loadout")]
    public WeaponProfile[] loadout;
    public int index;

    [Header("Controls")]
    public KeyCode nextWeaponKey = KeyCode.E;
    public KeyCode prevWeaponKey = KeyCode.Q;

    WeaponProfile current;
    public WeaponProfile CurrentWeapon => current;
    GameObject currentAura;
    // ✅ Event khi đổi vũ khí (nếu UI cần cập nhật icon/name)
    public event Action<WeaponProfile> OnEquipped;

    void Start()
    {
        if (owner == null) owner = transform;

        // auto clamp & equip
        Equip(index);
    }

    void Update()
    {
        if (loadout == null || loadout.Length == 0) return;

        // ===== SWITCH WEAPON ONLY =====
        if (Input.GetKeyDown(nextWeaponKey))
            Equip((index + 1) % loadout.Length);

        if (Input.GetKeyDown(prevWeaponKey))
            Equip((index - 1 + loadout.Length) % loadout.Length);

        // ❌ KHÔNG có bắn primary/ultimate ở đây nữa
    }

    //void Equip(int newIndex)
    //{
    //    if (loadout == null || loadout.Length == 0) return;

    //    index = Mathf.Clamp(newIndex, 0, loadout.Length - 1);
    //    current = loadout[index];

    //    Debug.Log("EQUIP: " + (current != null ? current.weaponName : "NULL"));
    //    if (current == null) return;

    //    // ❌ KHÔNG reset cooldown skill
    //    // current.primarySkill?.ResetCooldown();
    //    // current.ultimateSkill?.ResetCooldown();

    //    // ✅ Chỉ apply sprite/offset/scale của vũ khí
    //    // ===== AURA =====
    //    if (currentAura != null)
    //    {
    //        Destroy(currentAura);
    //        currentAura = null;
    //    }

    //    if (current.auraPrefab != null && weaponPivot != null)
    //    {
    //        currentAura = Instantiate(current.auraPrefab, weaponPivot);

    //        currentAura.transform.localPosition = current.auraLocalPos;
    //        currentAura.transform.localRotation = Quaternion.Euler(current.auraLocalRot);
    //        currentAura.transform.localScale = current.auraLocalScale;
    //    }
    //    if (weaponRenderer != null)
    //    {
    //        weaponRenderer.sprite = current.weaponSprite;
    //        weaponRenderer.transform.localPosition = current.localPos;
    //        weaponRenderer.transform.localRotation = Quaternion.Euler(0, 0, current.localRotZ);
    //        weaponRenderer.transform.localScale = current.localScale;
    //    }

    //    // ✅ Apply arm/holder (đổi tư thế tay/cụm vũ khí)
    //    if (armController != null)
    //        armController.ApplyWeapon(current);

    //    // ✅ báo cho UI (nếu bạn vẫn muốn UI đổi icon/name vũ khí)
    //    OnEquipped?.Invoke(current);
    //}
    void Equip(int newIndex)
    {
        if (loadout == null || loadout.Length == 0) return;

        // 🔥 THÊM ĐOẠN NÀY
        if (current != null && current.passiveSkill != null)
        {
            SkillContext ctx = new SkillContext
            {
                owner = owner,
                weaponPivot = weaponPivot,
                arm = armController != null ? armController.transform : null
            };

            current.passiveSkill.OnUnequip(ctx);
        }

        index = Mathf.Clamp(newIndex, 0, loadout.Length - 1);
        current = loadout[index];

        if (current == null) return;

        // ===== AURA (controller) =====
        if (currentAura != null)
        {
            Destroy(currentAura);
            currentAura = null;
        }

        if (current.auraPrefab != null && weaponPivot != null)
        {
            currentAura = Instantiate(current.auraPrefab, weaponPivot);

            currentAura.transform.localPosition = current.auraLocalPos;
            currentAura.transform.localRotation = Quaternion.Euler(current.auraLocalRot);
            currentAura.transform.localScale = current.auraLocalScale;
        }

        // 🔥 THÊM luôn OnEquip
        if (current.passiveSkill != null)
        {
            SkillContext ctx = new SkillContext
            {
                owner = owner,
                weaponPivot = weaponPivot,
                arm = armController != null ? armController.transform : null
            };

            current.passiveSkill.OnEquip(ctx);
        }

        // visual
        if (weaponRenderer != null)
        {
            weaponRenderer.sprite = current.weaponSprite;
            weaponRenderer.transform.localPosition = current.localPos;
            weaponRenderer.transform.localRotation = Quaternion.Euler(0, 0, current.localRotZ);
            weaponRenderer.transform.localScale = current.localScale;
        }

        if (armController != null)
            armController.ApplyWeapon(current);

        OnEquipped?.Invoke(current);
    }
}
