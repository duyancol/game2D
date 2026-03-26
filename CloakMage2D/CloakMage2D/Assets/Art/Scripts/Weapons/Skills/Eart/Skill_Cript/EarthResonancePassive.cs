using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skills/Earth Resonance Passive")]
public class EarthResonancePassive : WeaponSkill
{
    [Header("Damage Bonus")]
    [Range(0f, 2f)]
    public float damageMultiplier = 0.25f; // +25% damage khi shield còn

    [Header("Slow on EarthCrack")]
    [Range(0f, 1f)]
    public float slowPercent = 0.3f; // giảm tốc 30%
    public float slowDuration = 2f;  // 2 giây

    protected override void OnUse(SkillContext ctx)
    {
        // Passive, nên gọi khi skill active
        if (ctx.owner == null) return;

        var runner = ctx.owner.GetComponent<SkillCoroutineRunner>();
        if (runner == null)
            runner = ctx.owner.gameObject.AddComponent<SkillCoroutineRunner>();

        runner.Run(DoPassive(ctx));
    }

    IEnumerator DoPassive(SkillContext ctx)
    {
        Transform owner = ctx.owner;

        while (true)
        {
            // check xem shield còn sống không
            EnergyShieldArea shield = owner.GetComponentInChildren<EnergyShieldArea>();
            if (shield != null && shield.IsActive)
            {
                // Tăng damage cho Skill1 & Skill3
                var skills = owner.GetComponents<WeaponSkill>();

                foreach (var skill in skills)
                {
                    if (skill == this) continue;

                    // Skill1 – Basic Attack
                    if (skill is EarthHammerBasicAttack basicAttack)
                    {
                        basicAttack.damage = Mathf.RoundToInt(basicAttack.damage * (1f + damageMultiplier));
                    }

                    // Skill3 – EarthCrack
                    if (skill is EarthAxeSlamSkill axeSkill)
                    {
                        // Hook vào TickRoutine để slow
                        if (axeSkill.crackPrefab != null)
                            axeSkill.crackPrefab.ApplySlow(slowPercent, slowDuration);
                    }
                }
            }

            yield return new WaitForSeconds(0.1f); // check mỗi 0.1s
        }
    }
}