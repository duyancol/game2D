using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skills/Staff/Absolute Zero Passive")]
public class AbsoluteZeroPassive : WeaponSkill
{
    [Header("Stack Settings")]
    public int maxStacks = 5;
    public float smptPercentPerStack = 0.05f;
    public float stackDuration = 5f;

    [Header("Empowered Tornado")]
    public float damageMultiplier = 1.6f;
    public float sizeMultiplier = 1.5f;
    public float tickMultiplier = 2f;

    int currentStacks = 0;
    float expireTime = 0f;

    PlayerStatsMono stats;
    AbsoluteZeroFlag flag; // component trung gian báo cho skill biết đang empowered

    protected override void OnUse(SkillContext ctx) { }

    // =========================
    // EQUIP
    // =========================
    public override void OnEquip(SkillContext ctx)
    {
        stats = ctx.owner.GetComponent<PlayerStatsMono>();
        if (stats == null) return;

        // gắn flag runtime vào player
        flag = ctx.owner.GetComponent<AbsoluteZeroFlag>();
        if (flag == null)
            flag = ctx.owner.gameObject.AddComponent<AbsoluteZeroFlag>();

        flag.passive = this;
    }

    // =========================
    // UNEQUIP
    // =========================
    public override void OnUnequip(SkillContext ctx)
    {
        ResetStacks();

        if (flag != null)
            Destroy(flag);
    }

    // =========================
    // CALLED FROM DAMAGE SYSTEM
    // =========================
    public void OnMagicCrit()
    {
        if (currentStacks < maxStacks)
            currentStacks++;

        expireTime = Time.time + stackDuration;

        ApplyBonus();

        if (currentStacks >= maxStacks)
        {
            flag.isReady = true;
        }
    }



    void ApplyBonus()
    {
        stats.passivePercentSmpt = currentStacks * smptPercentPerStack;
        stats.RecalculateStats();
    }

    public void ConsumeEmpower()
    {
        ResetStacks();
        flag.isReady = false;
    }

    void ResetStacks()
    {
        currentStacks = 0;
        stats.passivePercentSmpt = 0f;
        stats.RecalculateStats();
    }
    public void Tick()
    {
        if (currentStacks > 0 && Time.time > expireTime)
        {
            ResetStacks();
        }
    }
}