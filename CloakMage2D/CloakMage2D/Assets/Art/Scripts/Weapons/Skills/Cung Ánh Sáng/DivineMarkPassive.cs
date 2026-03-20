using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skills/Bow/Divine Mark Passive")]
public class DivineMarkPassive : WeaponSkill
{
    [Header("Mark Settings")]
    public int maxStack = 5;
    public float bonusAtkScale = 1.2f;

    [Header("Mini Rain")]
    public FireBirdProjectile miniRainPrefab;
    public float spawnHeight = 6f;

    DivineMarkRuntime runtime;

    protected override void OnUse(SkillContext ctx) { }

    public override void OnEquip(SkillContext ctx)
    {
        runtime = ctx.owner.GetComponent<DivineMarkRuntime>();

        if (runtime == null)
            runtime = ctx.owner.gameObject.AddComponent<DivineMarkRuntime>();

        runtime.Init(this);
    }

    public override void OnUnequip(SkillContext ctx)
    {
        if (runtime != null)
            Destroy(runtime);
    }
}