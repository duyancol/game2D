using UnityEngine;

public class SkillBarUI : MonoBehaviour
{
    public WeaponController weaponController;
    public SkillSlotUI primarySlot;
    public SkillSlotUI ultimateSlot;
    public SkillSlotUI secondarySlot;   // thêm
    public SkillSlotUI passiveSlot;
    void OnEnable()
    {
        if (weaponController != null)
            weaponController.OnEquipped += HandleEquipped;
    }

    void OnDisable()
    {
        if (weaponController != null)
            weaponController.OnEquipped -= HandleEquipped;
    }

    void Start()
    {
        // cập nhật lần đầu khi vào game
        if (weaponController != null && weaponController.CurrentWeapon != null)
            HandleEquipped(weaponController.CurrentWeapon);
    }

    void HandleEquipped(WeaponProfile wp)
    {
        if (primarySlot) primarySlot.SetSkill(wp != null ? wp.primarySkill : null);
        if (secondarySlot) secondarySlot.SetSkill(wp != null ? wp.secondarySkill : null);
        if (ultimateSlot) ultimateSlot.SetSkill(wp != null ? wp.ultimateSkill : null);
        if (passiveSlot) passiveSlot.SetSkill(wp != null ? wp.passiveSkill : null);
    }
}
