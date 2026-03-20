using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skills/Ultimate Burst")]
public class UltimateBurstSkill : WeaponSkill
{
    public GameObject burstFxPrefab;
    public float fxLifeTime = 1.5f;

    protected override void OnUse(SkillContext ctx)
    {
        if (burstFxPrefab == null || ctx.owner == null) return;

        var fx = Instantiate(burstFxPrefab, ctx.owner.position, Quaternion.identity);
        Destroy(fx, fxLifeTime);
    }
}
