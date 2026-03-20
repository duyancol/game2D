
using System;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Refs")]
    public Transform owner;
    public Transform weaponPivot;
    public SpriteRenderer weaponRenderer;
    public ArmWeaponController armController;

    [Header("Mobile Ultimate")]
    private bool isUltimateButtonHeld = false;
    private bool isUltimateButtonDown = false;
    private bool isUltimateButtonUp = false;
    GameObject currentAura;
    public void OnUltimateDown()
    {
        isUltimateButtonHeld = true;
        isUltimateButtonDown = true;

        if (ultimateJoystickRoot != null)
            ultimateJoystickRoot.SetActive(true);
    }

    public void OnUltimateUp()
    {
        isUltimateButtonHeld = false;
        isUltimateButtonUp = true;

        if (ultimateJoystickRoot != null)
            ultimateJoystickRoot.SetActive(false);
    }

    [Header("Loadout")]
    public WeaponProfile[] loadout;
    public int index;

    [Header("Aim Joystick (Mobile)")]
    public FixedJoystick aimJoystick;

    [Header("Ultimate Aim Joystick UI")]
    public GameObject ultimateJoystickRoot;

    [Header("Controls")]
    public KeyCode nextWeaponKey = KeyCode.Z;
    public KeyCode prevWeaponKey = KeyCode.X;

    [Header("Shoot")]
    public bool holdToShoot = true;

    [Header("Optional Anim")]
    public bool playAttackAnimOnPrimary = true;
    public bool playAttackAnimOnUltimate = false;

    WeaponSkill currentPassive;
    WeaponProfile current;

    // 🔥 QUAN TRỌNG
    WeaponInstance currentWeapon;

    public WeaponProfile CurrentWeapon => current;

    public event Action<WeaponProfile> OnEquipped;

    void Start()
    {
        if (owner == null) owner = transform;

        if (!TryEquipSavedWeapon())
            Equip(index);
    }

    [Header("Mobile Attack")]
    private bool isAttackButtonHeld = false;
    private bool isAttackButtonDown = false;

    public void OnAttackDown()
    {
        isAttackButtonHeld = true;
        isAttackButtonDown = true;
    }

    public void OnAttackUp()
    {
        isAttackButtonHeld = false;
    }

    [Header("Mobile Secondary (E Skill)")]
    private bool isSecondaryButtonHeld = false;
    private bool isSecondaryButtonDown = false;

    public void OnSecondaryDown()
    {
        isSecondaryButtonHeld = true;
        isSecondaryButtonDown = true;
    }

    public void OnSecondaryUp()
    {
        isSecondaryButtonHeld = false;
    }

    bool TryEquipSavedWeapon()
    {
        if (loadout == null || loadout.Length == 0) return false;

        string savedId = PlayerPrefs.GetString("EQUIPPED_WEAPON_ID", "");
        if (string.IsNullOrEmpty(savedId)) return false;

        for (int i = 0; i < loadout.Length; i++)
        {
            if (loadout[i] != null && loadout[i].weaponId == savedId)
            {
                Equip(i);
                return true;
            }
        }

        return false;
    }

    void Update()
    {
        if (loadout == null || loadout.Length == 0) return;

        if (Input.GetKeyDown(nextWeaponKey))
            Equip((index + 1) % loadout.Length);

        if (Input.GetKeyDown(prevWeaponKey))
            Equip((index - 1 + loadout.Length) % loadout.Length);

        if (current == null) return;
        if (Input.GetKeyDown(KeyCode.U))
        {
            EnhanceCurrentWeapon();
        }
        // ===== PRIMARY =====
        bool pcPrimaryDown = Input.GetKeyDown(KeyCode.T);
        bool pcPrimaryHeld = Input.GetKey(KeyCode.T);

        bool primaryDown = pcPrimaryDown || isAttackButtonDown;
        bool primaryHeld = pcPrimaryHeld || isAttackButtonHeld;

        bool primaryInput = holdToShoot ? primaryHeld : primaryDown;

        isAttackButtonDown = false;

        if (primaryInput)
        {
            if (playAttackAnimOnPrimary && primaryDown && armController != null)
                armController.PlayAttack();

            if (current.primarySkill != null)
            {
                SkillContext ctx = BuildContext();
                current.primarySkill.TryUse(ctx);
            }
        }

        // ===== SECONDARY =====
        bool pcSecondaryDown = Input.GetKeyDown(KeyCode.E);
        bool pcSecondaryHeld = Input.GetKey(KeyCode.E);

        bool secondaryDown = pcSecondaryDown || isSecondaryButtonDown;
        bool secondaryHeld = pcSecondaryHeld || isSecondaryButtonHeld;

        bool secondaryInput = holdToShoot ? secondaryHeld : secondaryDown;

        isSecondaryButtonDown = false;

        if (secondaryInput && current.secondarySkill != null)
        {
            SkillContext ctx = BuildContext();
            current.secondarySkill.TryUse(ctx);
        }

        // ===== ULT =====
        bool pcUltDown = Input.GetKeyDown(KeyCode.R);
        bool pcUltHeld = Input.GetKey(KeyCode.R);
        bool pcUltUp = Input.GetKeyUp(KeyCode.R);

        bool mobileUltDown = isUltimateButtonDown;
        bool mobileUltHeld = isUltimateButtonHeld;
        bool mobileUltUp = isUltimateButtonUp;

        isUltimateButtonUp = false;
        isUltimateButtonDown = false;

        bool ultDown = pcUltDown || mobileUltDown;
        bool ultHeld = pcUltHeld || mobileUltHeld;
        bool ultUp = pcUltUp || mobileUltUp;

        if (current.ultimateSkill is IChargeSkill chargeSkill)
        {
            SkillContext ctx = BuildContext();

            if (ultDown)
                chargeSkill.ChargeBegin(ctx);

            if (ultHeld)
                chargeSkill.ChargeTick(ctx, Time.deltaTime);

            if (ultUp)
                chargeSkill.ChargeEnd(ctx, true);
        }
        else
        {
            if (ultDown && current.ultimateSkill != null)
            {
                if (current.ultimateSkill.castType == SkillCastType.Directional)
                {
                    SkillContext ctx = BuildContext();
                    current.ultimateSkill.TryUse(ctx);
                }
            }
        }
    }

    // 🔥 CHỖ QUAN TRỌNG NHẤT
    public SkillContext BuildContext()
    {
        Vector3 aimWorld = GetAimWorld();

        Vector2 dir = (aimWorld - owner.position);
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.right;

        dir.Normalize();

        return new SkillContext
        {
            owner = owner,
            weaponPivot = weaponPivot,
            arm = armController != null ? armController.transform : null,
            mouseWorld = aimWorld,
            aimDirection = dir,

            // 🔥 STAT SAU CƯỜNG HÓA
            weaponStat = WeaponCalculator.GetFinalStat(currentWeapon)
        };
    }

    Vector3 GetAimWorld()
    {
        if (mobileAimDirection.sqrMagnitude > 0.001f)
            return owner.position + (Vector3)(mobileAimDirection * 10f);

        if (aimJoystick != null)
        {
            Vector2 joyDir = new Vector2(aimJoystick.Horizontal, aimJoystick.Vertical);

            if (joyDir.sqrMagnitude > 0.01f)
            {
                joyDir.Normalize();
                return owner.position + (Vector3)(joyDir * 10f);
            }
        }

        return owner.position + owner.right * 10f;
    }

    void Equip(int newIndex)
    {
        if (loadout == null || loadout.Length == 0) return;

        // remove passive cũ
        if (current != null && current.passiveSkill != null)
        {
            SkillContext oldCtx = BuildContext();
            current.passiveSkill.OnUnequip(oldCtx);
            currentPassive = null;
        }

        index = Mathf.Clamp(newIndex, 0, loadout.Length - 1);
        current = loadout[index];

        if (current == null) return;

        // 🔥 TẠO INSTANCE (KHÔNG SET LEVEL Ở ĐÂY)
        currentWeapon = new WeaponInstance(current);

        // DEBUG STAT
        var stat = WeaponCalculator.GetFinalStat(currentWeapon);
        Debug.Log("ATK FINAL: " + stat.addAtk);

        // reset cooldown
        current.primarySkill?.ResetCooldown();
        current.secondarySkill?.ResetCooldown();
        current.ultimateSkill?.ResetCooldown();

        // apply passive
        if (current.passiveSkill != null)
        {
            SkillContext ctx = BuildContext();
            current.passiveSkill.OnEquip(ctx);
            currentPassive = current.passiveSkill;
        }
        // ===== AURA =====
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

        if (!string.IsNullOrEmpty(current.weaponId))
        {
            PlayerPrefs.SetString("EQUIPPED_WEAPON_ID", current.weaponId);
            PlayerPrefs.Save();
        }
    }

    public bool EquipByProfile(WeaponProfile wp)
    {
        if (wp == null) return false;

        for (int i = 0; i < loadout.Length; i++)
        {
            if (loadout[i] == wp)
            {
                Equip(i);
                return true;
            }
        }

        return false;
    }
    public void EnhanceCurrentWeapon()
    {
        if (currentWeapon == null) return;

        currentWeapon.enhanceLevel++;

        var stat = WeaponCalculator.GetFinalStat(currentWeapon);

        Debug.Log("ENHANCE +" + currentWeapon.enhanceLevel);
        Debug.Log("ATK: " + stat.addAtk);
        // 🔥 CẬP NHẬT PLAYER
        var stats = owner.GetComponent<PlayerStatsMono>();
        if (stats != null)
        {
            stats.EquipWeapon(currentWeapon);
        }
    }
    private Vector2 mobileAimDirection = Vector2.zero;

    public void SetMobileAimDirection(Vector2 dir)
    {
        mobileAimDirection = dir;
    }

    public void ClearMobileAim()
    {
        mobileAimDirection = Vector2.zero;
    }
}
