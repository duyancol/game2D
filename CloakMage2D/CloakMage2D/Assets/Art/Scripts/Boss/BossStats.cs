
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class BossStats : MonoBehaviour
//{
//    [Header("Base Stats (không đổi)")]
//    public int baseMaxHP = 200;
//    public float baseAtk = 30f;
//    public float baseSmpt = 20f;
//    [Range(0f, 1f)] public float baseCritChance = 0.15f;
//    public float baseCritDamage = 1.5f;
//    public float baseDef = 10f;
//    public float baseMdef = 8f;
//    public CharacterEquipmentUI equipmentUI;
//    [Header("UI")]
//    public Text combatPowerText;

//    [Header("Runtime Stats (sau khi cộng đồ)")]
//    public int maxHP;
//    public int hp;

//    public float atk;
//    public float smpt;

//    [Range(0f, 1f)] public float critChance;
//    public float critDamage;

//    public float def;
//    public float mdef;
//    // ===== PASSIVE BONUS =====
//    [HideInInspector] public float passiveFlatAtk;
//    [HideInInspector] public float passiveFlatSmpt;
//    [HideInInspector] public float passiveFlatCritChance;

//    [HideInInspector] public float passivePercentAtk;
//    [HideInInspector] public float passivePercentSmpt;
//    [HideInInspector] public float passivePercentCritChance;
//    WeaponProfile _equippedWeapon;

//    // ✅ Đổi sang Dictionary để không stack trùng slot
//    Dictionary<EquipSlot, EquipmentBonus> _equipBonuses
//        = new Dictionary<EquipSlot, EquipmentBonus>();

//    void Awake()
//    {
//        RecalculateStats();
//        hp = maxHP;
//    }

//    // ==================================================
//    // WEAPON
//    // ==================================================
//    public void EquipWeapon(WeaponProfile wp)
//    {
//        _equippedWeapon = wp;

//        int oldMax = maxHP;
//        RecalculateStats();
//        KeepHPPercent(oldMax);
//    }

//    // ==================================================
//    // ARMOR EQUIP (KHÔNG STACK)
//    // ==================================================
//    //public void EquipArmor(EquipmentBonus bonus)
//    //{
//    //    // Nếu đã có món cùng slot → tự ghi đè
//    //    _equipBonuses[bonus.slot] = bonus;

//    //    int oldMax = maxHP;
//    //    RecalculateStats();
//    //    KeepHPPercent(oldMax);
//    //}
//    //public void EquipArmor(EquipmentBonus bonus, Sprite icon)
//    //{
//    //    _equipBonuses[bonus.slot] = bonus;

//    //    equipmentUI.SetEquip(bonus.slot, icon);

//    //    int oldMax = maxHP;
//    //    RecalculateStats();
//    //    KeepHPPercent(oldMax);
//    //}
//    public void EquipArmor(EquipmentBonus bonus, Sprite icon, string quality)
//    {
//        _equipBonuses[bonus.slot] = bonus;

//        // Convert string -> enum
//        ItemRarity rarity;
//        if (!System.Enum.TryParse(quality, true, out rarity))
//        {
//            rarity = ItemRarity.Green; // fallback
//        }

//        equipmentUI.SetEquip(bonus.slot, icon, rarity);

//        int oldMax = maxHP;
//        RecalculateStats();
//        KeepHPPercent(oldMax);
//    }
//    // ==================================================
//    // ARMOR REMOVE
//    // ==================================================
//    //public void RemoveArmor(EquipSlot slot)
//    //{
//    //    if (_equipBonuses.ContainsKey(slot))
//    //    {
//    //        _equipBonuses.Remove(slot);

//    //        int oldMax = maxHP;
//    //        RecalculateStats();
//    //        KeepHPPercent(oldMax);
//    //    }
//    //}
//    public void RemoveArmor(EquipSlot slot)
//    {
//        if (_equipBonuses.ContainsKey(slot))
//        {
//            _equipBonuses.Remove(slot);

//            equipmentUI.RemoveEquip(slot);

//            int oldMax = maxHP;
//            RecalculateStats();
//            KeepHPPercent(oldMax);
//        }
//    }

//    // ==================================================
//    // GIỮ % HP
//    // ==================================================
//    void KeepHPPercent(int oldMax)
//    {
//        if (oldMax > 0)
//        {
//            float pct = (float)hp / oldMax;
//            hp = Mathf.Clamp(Mathf.RoundToInt(pct * maxHP), 0, maxHP);
//        }
//        else hp = maxHP;
//    }

//    // ==================================================
//    // RECALCULATE (CHUẨN FLAT → %)
//    // ==================================================
//    public void RecalculateStats()
//    {
//        // 1️⃣ Reset base
//        maxHP = baseMaxHP;
//        atk = baseAtk;
//        smpt = baseSmpt;
//        critChance = baseCritChance;
//        critDamage = baseCritDamage;
//        def = baseDef;
//        mdef = baseMdef;

//        // 2️⃣ Weapon (flat)
//        if (_equippedWeapon != null)
//        {
//            var b = _equippedWeapon.bonus;

//            maxHP += b.addMaxHP;
//            atk += b.addAtk;
//            smpt += b.addSmpt;
//            critChance += b.addCritChance;
//            critDamage += b.addCritDamage;
//            def += b.addDef;
//            mdef += b.addMdef;
//        }

//        // 3️⃣ Armor flat + gom %
//        float totalPercentHP = 0f;
//        float totalPercentAtk = 0f;
//        float totalPercentSmpt = 0f;
//        float totalPercentCritChance = 0f;
//        float totalPercentCritDamage = 0f;
//        float totalPercentDef = 0f;
//        float totalPercentMdef = 0f;

//        foreach (var b in _equipBonuses.Values)
//        {
//            // ⭐ Flat trước
//            maxHP += b.flatHP;
//            atk += b.flatAtk;
//            smpt += b.flatSmpt;
//            critChance += b.flatCritChance / 100;
//            critDamage += b.flatCritDamage / 100;
//            def += b.flatDef;
//            mdef += b.flatMdef;

//            // ⭐ Gom %
//            totalPercentHP += b.percentHP;
//            totalPercentAtk += b.percentAtk;
//            totalPercentSmpt += b.percentSmpt;
//            totalPercentCritChance += b.percentCritChance;
//            totalPercentCritDamage += b.percentCritDamage;
//            totalPercentDef += b.percentDef;
//            totalPercentMdef += b.percentMdef;
//        }

//        // 4️⃣ Apply %
//        //maxHP = Mathf.RoundToInt(maxHP * (1f + totalPercentHP));
//        //atk *= (1f + totalPercentAtk);
//        //smpt *= (1f + totalPercentSmpt);
//        // 4️⃣ Apply %
//        maxHP = Mathf.RoundToInt(maxHP * (1f + totalPercentHP));

//        atk = (atk + passiveFlatAtk) * (1f + totalPercentAtk + passivePercentAtk);
//        smpt = (smpt + passiveFlatSmpt) * (1f + totalPercentSmpt + passivePercentSmpt);

//        critChance = Mathf.Clamp01(
//            (critChance + passiveFlatCritChance) *
//            (1f + totalPercentCritChance + passivePercentCritChance)
//        );

//        critDamage *= (1f + totalPercentCritDamage);
//        def *= (1f + totalPercentDef);
//        mdef *= (1f + totalPercentMdef);
//        critChance = Mathf.Clamp01(critChance * (1f + totalPercentCritChance));
//        critDamage *= (1f + totalPercentCritDamage);
//        def *= (1f + totalPercentDef);
//        mdef *= (1f + totalPercentMdef);

//        maxHP = Mathf.Max(1, maxHP);
//        critDamage = Mathf.Max(1f, critDamage);
//        CalculateCombatPower();
//    }
//    public int combatPower;

//    void CalculateCombatPower()
//    {
//        float power = 0f;

//        power += maxHP * 0.2f;
//        power += atk * 5f;
//        power += smpt * 4f;
//        power += critChance * 1000f;
//        power += (critDamage - 1f) * 800f;
//        power += def * 3f;
//        power += mdef * 3f;

//        combatPower = Mathf.RoundToInt(power);

//        UpdateCombatPowerUI();
//    }
//    void UpdateCombatPowerUI()
//    {
//        if (combatPowerText != null)
//        {
//            combatPowerText.text = combatPower.ToString("N0");
//        }
//    }
//    public void AddDefense(float value)
//    {
//        def += value;
//    }

//    public void RemoveDefense(float value)
//    {
//        def -= value;
//        def = Mathf.Max(def, baseDef);
//    }
//}
