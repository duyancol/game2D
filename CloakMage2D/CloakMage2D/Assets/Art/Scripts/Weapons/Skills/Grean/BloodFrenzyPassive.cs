using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skills/Spear/Blood Rage Passive")]
public class BloodFrenzyPassive : WeaponSkill
{
    [Header("Scaling")]
    [Tooltip("Max % ATK khi HP = 0 (1 = 100%)")]
    public float maxAtkBonus = 1.0f;

    [Tooltip("Chỉ bắt đầu scale khi HP thấp hơn mức này")]
    [Range(0f, 1f)]
    public float startThreshold = 1f; // 1 = luôn active

    [Tooltip("Giảm độ gắt (curve)")]
    public AnimationCurve scalingCurve = AnimationCurve.Linear(0, 0, 1, 1);

    PlayerStatsMono stats;
    PlayerHealth health;

    protected override void OnUse(SkillContext ctx) { }

    // =========================
    // EQUIP
    // =========================
    public override void OnEquip(SkillContext ctx)
    {
        stats = ctx.owner.GetComponent<PlayerStatsMono>();
        health = ctx.owner.GetComponent<PlayerHealth>();

        if (stats == null || health == null) return;

        health.onHpChanged.AddListener(OnHpChanged);

        ApplyBonus();
    }

    // =========================
    // UNEQUIP
    // =========================
    public override void OnUnequip(SkillContext ctx)
    {
        if (health != null)
            health.onHpChanged.RemoveListener(OnHpChanged);

        if (stats != null)
        {
            stats.passivePercentAtk = 0f;
            stats.RecalculateStats();
        }
    }

    // =========================
    // HP CHANGE
    // =========================
    void OnHpChanged(int hp, int maxHp)
    {
        ApplyBonus();
    }

    void ApplyBonus()
    {
        if (stats == null) return;

        float hpPercent = (float)stats.hp / stats.maxHP;
        float missing = 1f - hpPercent;

        float denom = (1f - startThreshold);

        float normalized;

        if (denom <= 0.0001f)
            normalized = missing;
        else
            normalized = missing / denom;

        normalized = Mathf.Clamp01(normalized);

        float curved = scalingCurve.Evaluate(normalized);

        stats.passivePercentAtk = curved * maxAtkBonus;

        stats.RecalculateStats();
    }
}