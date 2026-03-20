using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skills/Rain Of Judgment")]
public class RainOfJudgmentSkill : WeaponSkill
{
    [Header("Arrow Prefab (use FireBirdProjectile)")]
    public FireBirdProjectile arrowPrefab;

    [Header("Cast VFX")]
    public GameObject auraPrefab;
    public Vector3 auraLocalOffset = new Vector3(0f, 0.2f, 0f);
    public float auraLife = 1.0f;

    [Header("Rain Config")]
    public int arrowCount = 10;
    public float rainRadius = 3f;
    public float forwardDistance = 4f;
    public float arrowHeight = 8f;
    public float arrowFallTime = 0.6f;
    public float delayBetweenArrows = 0.05f;

    [Header("Damage (Base + Scaling)")]
    public int baseDamage = 0;
    public float atkScale = 0.6f;
    public float smptScale = 0f;
    public DamageType damageType = DamageType.Physical;
    public bool canCrit = true;

    [Header("AOE")]
    public float radius = 1.2f;
    public LayerMask hitMask;

    [Header("Invincible")]
    public float invincibleTime = 0.5f;

    protected override void OnUse(SkillContext ctx)
    {
        if (!arrowPrefab || ctx.owner == null) return;

        var runner = ctx.owner.GetComponent<SkillCoroutineRunner>();
        if (runner == null)
            runner = ctx.owner.gameObject.AddComponent<SkillCoroutineRunner>();

        runner.Run(UltRoutine(ctx));
    }

    IEnumerator UltRoutine(SkillContext ctx)
    {
        var owner = ctx.owner;
        var ownerGo = owner.gameObject;

        var playerMove = owner.GetComponent<PlayerMove2D>();
        if (playerMove != null)
            playerMove.SetSkillVisual();

        var anim = owner.transform.Find("Visual_Skill")?.GetComponent<Animator>();
        if (anim != null)
            anim.SetTrigger("R_AL");
        yield return new WaitForSeconds(0.3f);
        SpawnOnOwner(ctx, auraPrefab, auraLocalOffset, auraLife);

        // ===== INVINCIBLE =====
        var stats = ownerGo.GetComponent<PlayerStatsMono>();
        //if (stats != null)
        //    stats.SetInvincible(invincibleTime);

        yield return new WaitForSeconds(invincibleTime);

        // ===== XÁC ĐỊNH VÙNG PHÍA TRƯỚC =====
        Vector2 facingDir = owner.right;
        Vector3 center = owner.position + (Vector3)(facingDir * forwardDistance);
        center.z = 0f;
        if (playerMove != null)
            playerMove.SetNormalVisual();
        // ===== SPAWN RAIN =====
        for (int i = 0; i < arrowCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * rainRadius;
            Vector3 targetPos = center + (Vector3)randomOffset;
            targetPos.z = 0f;

            Vector3 spawnPos = targetPos + Vector3.up * arrowHeight;

            // mũi tên quay xuống
            float angle = -90f;
            var arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.Euler(0, 0, angle));

            arrow.owner = ownerGo;
            arrow.radius = radius;
            arrow.hitMask = hitMask;
            arrow.flyTime = arrowFallTime;
            arrow.rotateToVelocity = true;
            arrow.spriteAngleOffset = 0f;

            arrow.InitScaled(
                stats,
                baseDamage,
                atkScale,
                smptScale,
                damageType,
                canCrit,
                hitMask,
                ownerGo
            );

            // bay thẳng xuống
            arrow.LaunchTo(targetPos);

            yield return new WaitForSeconds(delayBetweenArrows);
        }

        yield return new WaitForSeconds(0.4f);

       
    }

    void SpawnOnOwner(SkillContext ctx, GameObject prefab, Vector3 localOffset, float life)
    {
        if (!prefab || ctx.owner == null) return;

        Vector3 pos = ctx.owner.position + localOffset;
        pos.z = 0f;

        var go = Instantiate(prefab, pos, Quaternion.identity);
        go.transform.SetParent(ctx.owner, worldPositionStays: true);

        if (life > 0f)
            Destroy(go, life);
    }
}