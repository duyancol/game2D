
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skills/Ultimate Cube Meteor Skill")]
public class UltimateCubeMeteorSkill : WeaponSkill
{
    [Header("Meteor Prefab (has CubeMeteorProjectile)")]
    public CubeMeteorProjectile meteorPrefab;

    [Header("Diagonal Flight (meteor lao chéo)")]
    public bool fromLeftToRight = true;
    public float spawnOffsetX = 12f;
    public float spawnOffsetY = 10f;

    [Header("Charge VFX (trên nhân vật)")]
    public GameObject auraPrefab;
    public Vector3 auraLocalOffset = new Vector3(0f, 0.15f, 0f);
    public float auraLife = 1.0f;

    [Header("Ultimate Splash (POW)")]
    public GameObject ultimatePrefab;
    public Vector3 ultimateLocalOffset = new Vector3(0f, 1.0f, 0f);
    public float ultimateLife = 0.6f;

    [Header("Charge Timing")]
    public float chargeDelay = 0.6f;

    [Header("VFX (optional override)")]
    public GameObject impactVfx;
    public GameObject explosionVfx;

    [Header("Damage (Base + Scaling)")]
    public int damage = 40;
    public DamageType damageType = DamageType.Physical;
    public float atkScale = 1.2f;
    public float smptScale = 0f;
    public bool canCrit = true;

    [Header("AOE")]
    public float radius = 2.2f;        // AOE damage của mỗi cục
    public LayerMask hitMask;

    [Header("Meteor Timing")]
    public float flyTime = 0.6f;

    [Header("Triple Meteors")]
    public int count = 3;              // 3 cục
    public float interval = 0.18f;     // delay giữa các cục
    public float areaRadius = 2.5f;    // khu vực rơi quanh điểm target

    protected override void OnUse(SkillContext ctx)
    {
        if (meteorPrefab == null || ctx.owner == null) return;

        var runner = ctx.owner.GetComponent<SkillCoroutineRunner>();
        if (runner == null) runner = ctx.owner.gameObject.AddComponent<SkillCoroutineRunner>();

        runner.Run(UltRoutine(ctx));
    }

    IEnumerator UltRoutine(SkillContext ctx)
    {
        // 1) Aura + ultimate splash
        SpawnOnOwner(ctx, auraPrefab, auraLocalOffset, auraLife);
        SpawnOnOwner(ctx, ultimatePrefab, ultimateLocalOffset, ultimateLife);

        // 2) Charge delay
        yield return new WaitForSeconds(Mathf.Max(0f, chargeDelay));

        var ownerGo = ctx.owner.gameObject;
        var stats = ownerGo.GetComponent<PlayerStatsMono>();

        // 3) center target (khu vực)
        Vector3 center = ctx.mouseWorld;
        center.z = 0f;

        float dirX = fromLeftToRight ? -1f : 1f;

        for (int i = 0; i < Mathf.Max(1, count); i++)
        {
            // random điểm rơi trong khu vực
            Vector2 rnd = Random.insideUnitCircle * areaRadius;
            Vector3 target = new Vector3(center.x + rnd.x, center.y + rnd.y, 0f);

            // spawn chéo theo target (giữ đúng offset X/Y)
            Vector3 spawnPos = new Vector3(
                target.x + dirX * spawnOffsetX,
                target.y + spawnOffsetY,
                0f
            );

            Vector2 flyDir = (target - spawnPos).normalized;
            float angle = Mathf.Atan2(flyDir.y, flyDir.x) * Mathf.Rad2Deg;

            var meteor = Instantiate(meteorPrefab, spawnPos, Quaternion.Euler(0, 0, angle));

            // === config movement/aoe/vfx ===
            meteor.flyTime = flyTime;
            meteor.damageRadius = radius;
            meteor.hitMask = hitMask;

            if (impactVfx) meteor.impactVfx = impactVfx;
            if (explosionVfx) meteor.explosionVfx = explosionVfx;

            // === owner + scaled damage ===
            meteor.owner = ownerGo;

            meteor.InitScaled(
                stats,
                damage,
                atkScale,
                smptScale,
                damageType,
                canCrit,
                ownerGo
            );

            // launch
            meteor.LaunchTo(target);

            if (i < count - 1)
                yield return new WaitForSeconds(Mathf.Max(0.01f, interval));
        }
    }

    void SpawnOnOwner(SkillContext ctx, GameObject prefab, Vector3 localOffset, float life)
    {
        if (!prefab || ctx.owner == null) return;

        Vector3 pos = ctx.owner.position + localOffset;
        pos.z = 0f;

        var go = Instantiate(prefab, pos, Quaternion.identity);
        go.transform.SetParent(ctx.owner, worldPositionStays: true);

        if (life > 0f) Destroy(go, life);
    }
}
