using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skills/Energy Shield Skill")]
public class EnergyShield : WeaponSkill
{
    public EnergyShieldArea shieldPrefab;

    public float hpPercent = 0.3f;
    public float duration = 4f;

    [Header("Explosion")]
    public float radius = 2.5f;
    public LayerMask hitMask;

    [Header("Damage")]
    public int damage = 0;
    public DamageType damageType = DamageType.Physical;
    public float atkScale = 1f;
    public float smptScale = 0f;
    public bool canCrit = true;

    protected override void OnUse(SkillContext ctx)
    {
        if (ctx.owner == null || shieldPrefab == null) return;

        var runner = ctx.owner.GetComponent<SkillCoroutineRunner>();
        if (runner == null)
            runner = ctx.owner.gameObject.AddComponent<SkillCoroutineRunner>();

        runner.Run(DoSkill(ctx));
    }

    IEnumerator DoSkill(SkillContext ctx)
    {
        Transform owner = ctx.owner;
        var stats = owner.GetComponent<PlayerStatsMono>();

        float shieldAmount = stats.maxHP * hpPercent;

        var shield = Instantiate(shieldPrefab, owner.position, Quaternion.identity);

        shield.Init(
            owner.gameObject,
            stats,
            shieldAmount,
            duration,
            damage,
            atkScale,
            smptScale,
            damageType,
            canCrit,
            radius,
            hitMask
        );

        yield return null;
    }
}