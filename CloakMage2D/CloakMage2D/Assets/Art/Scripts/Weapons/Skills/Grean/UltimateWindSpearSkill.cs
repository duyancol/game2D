
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skills/Ultimate Wind Spear Skill")]
public class UltimateWindSpearSkill : WeaponSkill
{
    [Header("Spear Projectile")]
    public WindSpearProjectile spearPrefab;

    [Header("Dash + Stab VFX")]
    public GameObject stabPrefab;       // prefab riêng để hiển thị stab
    public float stabMuzzleOffset = 0.6f;
    public float stabThrustDistance = 2.2f;
    public float stabThrustDuration = 0.18f;
    public float stabHitLife = 0.5f;
    [Header("Wind Ball")]
    public GameObject windBallPrefab;
    [Header("Vortex")]
    public GameObject vortexVfx;
    public float vortexDuration = 2.5f;
    public float vortexRadius = 3f;
    public float pullForce = 8f;
    public LayerMask hitMask;

    [Header("Dash + Stab")]
    public float dashSpeed = 25f;
    public float stabDuration = 0.8f;
    public float stabInterval = 0.1f;

    [Header("Rain Spear")]
    public int rainCount = 5;
    public float rainInterval = 0.2f;
    public float rainHeight = 6f;
    public float rainRadius = 2.5f;

    [Header("Damage")]
    public int damage = 50;
    public float atkScale = 1.2f;
    public float smptScale = 0f;
    public DamageType damageType = DamageType.Physical;
    public bool canCrit = true;

    [Header("Recast")]
    public float recastWindow = 2.5f;
    public override bool IsActive() => isActive;
    bool isActive = false;
    bool canRecast = false;
    Vector3 vortexPos;
    [Header("Dash + Stab VFX Fan")]
    public float stabFanAngle = 20f; // mỗi stab lệch ±20° so với chính giữa
    public int stabFanCount = 3;     // số lần stab xòe
                                     //protected override void OnUse(SkillContext ctx)
                                     //{
                                     //    var runner = ctx.owner.GetComponent<SkillCoroutineRunner>();
                                     //    if (runner == null) runner = ctx.owner.gameObject.AddComponent<SkillCoroutineRunner>();

    //    if (!isActive)
    //        runner.Run(Phase1(ctx));
    //    else if (canRecast)
    //        runner.Run(Phase2(ctx));
    //}
    protected override void OnUse(SkillContext ctx)
    {
        var runner = ctx.owner.GetComponent<SkillCoroutineRunner>();
        if (runner == null) runner = ctx.owner.gameObject.AddComponent<SkillCoroutineRunner>();

        if (!isActive)
        {
            runner.Run(Phase1(ctx));
        }
        else if (canRecast)
        {
            runner.Run(Phase2(ctx));
        }
        else
        {
            return; // ❗ thêm dòng này để tránh spam
        }
    }
    IEnumerator Phase1(SkillContext ctx)
    {
        isActive = true;
        canRecast = true;

        var owner = ctx.owner;
        var stats = owner.GetComponent<PlayerStatsMono>();

        Vector3 target = ctx.mouseWorld;
        target.z = 0f;

        Vector3 dir = (target - owner.position).normalized;

        // Spawn spear projectile
        var spear = Instantiate(spearPrefab, owner.position, Quaternion.identity);
        spear.transform.right = dir;
        spear.InitScaled(stats, damage, atkScale, smptScale, damageType, canCrit, owner.gameObject);

        spear.hitMask = hitMask;
        spear.vortexDuration = vortexDuration;
        spear.vortexRadius = vortexRadius;
        spear.pullForce = pullForce;
        spear.vortexVfx = vortexVfx;

        spear.LaunchTo(target);

        vortexPos = target;

        float t = 0f;
        while (t < recastWindow)
        {
            t += Time.deltaTime;
            yield return null;
        }
        // Hết recast window mà không nhấn Phase2 → bắt cooldown
        if (isActive)
            nextTime = Time.time + cooldown;

        isActive = false;
        canRecast = false;
    }

    //IEnumerator Phase2(SkillContext ctx)
    //{
    //    canRecast = false;

    //    var owner = ctx.owner;
    //    var stats = owner.GetComponent<PlayerStatsMono>();

    //    // ===== DASH =====
    //    while (Vector3.Distance(owner.position, vortexPos) > 0.05f)
    //    {
    //        owner.position = Vector3.MoveTowards(owner.position, vortexPos, dashSpeed * Time.deltaTime);
    //        yield return null;
    //    }
    //    owner.position = vortexPos;

    //    // ===== MULTI STAB =====
    //    float t = 0f;
    //    while (t < stabDuration)
    //    {
    //        t += stabInterval;

    //        if (stabPrefab != null)
    //        {
    //            Vector3 baseDir = (vortexPos - owner.position).normalized;
    //            float[] angles;
    //            if (stabFanCount > 1)
    //            {
    //                angles = new float[stabFanCount];
    //                float step = stabFanAngle * 2 / (stabFanCount - 1);
    //                for (int i = 0; i < stabFanCount; i++)
    //                    angles[i] = -stabFanAngle + step * i;
    //            }
    //            else angles = new float[] { 0f };

    //            // spawn mỗi stab theo các góc
    //            foreach (var angle in angles)
    //            {
    //                Vector3 fanDir = Rotate(baseDir, angle);
    //                yield return StabThrust(ctx, owner.position, owner.position + fanDir * stabThrustDistance, fanDir);
    //            }
    //        }

    //        // AOE damage
    //        var hits = Physics2D.OverlapCircleAll(vortexPos, vortexRadius, hitMask);
    //        foreach (var col in hits)
    //        {
    //            var boss = col.GetComponentInParent<BossHealth>();
    //            if (boss == null) continue;

    //            bool isCrit;
    //            int finalDamage = DamageCore.Compute(stats, boss.GetComponent<PlayerStatsMono>(),
    //                damage, atkScale, smptScale, damageType, canCrit, out isCrit);

    //            boss.TakeDamage(finalDamage);
    //            DamagePopupSpawner.I?.Show(boss.transform.position, finalDamage, isCrit);
    //        }

    //        yield return new WaitForSeconds(stabInterval);
    //    }


    //    // ===== FINISHER =====
    //    var finalHits = Physics2D.OverlapCircleAll(vortexPos, vortexRadius, hitMask);
    //    foreach (var col in finalHits)
    //    {
    //        var boss = col.GetComponentInParent<BossHealth>();
    //        if (boss == null) continue;
    //        boss.TakeDamage(damage * 2);
    //    }

    //    isActive = false;
    //    nextTime = Time.time + cooldown; // cooldown bắt khi skill thực sự xong

    //}
    IEnumerator Phase2(SkillContext ctx)
    {
        canRecast = false;

        var owner = ctx.owner;
        var stats = owner.GetComponent<PlayerStatsMono>();
        // ===== ẨN PLAYER =====
        Transform visual = owner.Find("Visual");
        if (visual != null)
            visual.gameObject.SetActive(false);
        var colliders = owner.GetComponentsInChildren<Collider2D>(true);
        foreach (var c in colliders)
            c.enabled = false;
        // ===== LẤY HƯỚNG =====
        PlayerMove2D move = owner.GetComponent<PlayerMove2D>();
        float face = move != null ? move.FacingDirection : 1f;
        Vector2 dir = Vector2.right * face;

        // ===== SPAWN WIND BALL =====
        GameObject ball = null;
        if (windBallPrefab != null)
        {
            ball = Instantiate(windBallPrefab, owner.position, Quaternion.identity);
            ball.transform.localScale = owner.localScale;
        }

        // ===== DASH + DAMAGE TRÊN ĐƯỜNG =====
        float hitTick = 0.05f;
        float tickTimer = 0f;

        while (Vector3.Distance(owner.position, vortexPos) > 0.05f)
        {
            Vector3 prevPos = owner.position;

            owner.position = Vector3.MoveTowards(
                owner.position,
                vortexPos,
                dashSpeed * Time.deltaTime
            );

            // ball follow player
            if (ball != null)
                ball.transform.position = owner.position;

            // ===== DAMAGE TRÊN ĐƯỜNG =====
            tickTimer -= Time.deltaTime;
            if (tickTimer <= 0f)
            {
                tickTimer = hitTick;

                var hits = Physics2D.OverlapCircleAll(owner.position, 1.5f, hitMask);
                foreach (var col in hits)
                {
                    var boss = col.GetComponentInParent<BossHealth>();
                    if (boss == null) continue;

                    bool isCrit;
                    int finalDamage = DamageCore.Compute(
                        stats,
                        boss.GetComponent<PlayerStatsMono>(),
                        damage,
                        atkScale,
                        smptScale,
                        damageType,
                        canCrit,
                        out isCrit
                    );

                    boss.TakeDamage(finalDamage);
                    DamagePopupSpawner.I?.Show(boss.transform.position, finalDamage, isCrit);
                }
            }

            yield return null;
        }

        owner.position = vortexPos;

        // ===== HUỶ BALL =====
        if (ball != null) Destroy(ball);
        // ===== HIỆN LẠI PLAYER =====
        if (visual != null)
            visual.gameObject.SetActive(true);

        foreach (var c in colliders)
            c.enabled = true;
        // ===== STAB BURST =====
        float t = 0f;
        while (t < stabDuration)
        {
            t += stabInterval;

            // spawn stab theo hướng mặt
            Vector2 baseDir = Vector2.right * face;

            float[] angles;
            if (stabFanCount > 1)
            {
                angles = new float[stabFanCount];
                float step = stabFanAngle * 2 / (stabFanCount - 1);
                for (int i = 0; i < stabFanCount; i++)
                    angles[i] = -stabFanAngle + step * i;
            }
            else angles = new float[] { 0f };

            foreach (var angle in angles)
            {
                Vector3 fanDir = Rotate(baseDir, angle);
                yield return StabThrust(ctx, owner.position, owner.position + fanDir * stabThrustDistance, fanDir);
            }

            // ===== AOE =====
            var hits = Physics2D.OverlapCircleAll(vortexPos, vortexRadius, hitMask);
            foreach (var col in hits)
            {
                var boss = col.GetComponentInParent<BossHealth>();
                if (boss == null) continue;

                bool isCrit;
                int finalDamage = DamageCore.Compute(
                    stats,
                    boss.GetComponent<PlayerStatsMono>(),
                    damage,
                    atkScale,
                    smptScale,
                    damageType,
                    canCrit,
                    out isCrit
                );

                boss.TakeDamage(finalDamage);
                DamagePopupSpawner.I?.Show(boss.transform.position, finalDamage, isCrit);
            }

            yield return new WaitForSeconds(stabInterval);
        }

        // ===== FINISH =====
        var finalHits = Physics2D.OverlapCircleAll(vortexPos, vortexRadius, hitMask);
        foreach (var col in finalHits)
        {
            var boss = col.GetComponentInParent<BossHealth>();
            if (boss == null) continue;

            boss.TakeDamage(damage * 2);
        }

        isActive = false;
        nextTime = Time.time + cooldown;
    }
    Vector3 Rotate(Vector3 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector3(v.x * cos - v.y * sin, v.x * sin + v.y * cos, 0f).normalized;
    }
    IEnumerator StabThrust(SkillContext ctx, Vector3 startPos, Vector3 targetPos, Vector3 dir)
    {
        dir.Normalize();
        Vector3 stabStart = startPos + dir * stabMuzzleOffset;
        Vector3 stabEnd = stabStart + dir * stabThrustDistance;

        var go = Instantiate(stabPrefab, stabStart, Quaternion.identity);
        go.transform.right = dir;

        ApplySortingLikeOwner(ctx, go);
        EnsureTriggerCollider(go);
        EnsureKinematicRB(go);

        var stabDmg = go.GetComponent<SpearStabDamage>();
        if (stabDmg == null) stabDmg = go.AddComponent<SpearStabDamage>();

        var stats = ctx.owner.GetComponent<PlayerStatsMono>();
        stabDmg.InitScaled(stats, damage, atkScale, smptScale, damageType, canCrit, ctx.owner.gameObject,
            destroyDelay: stabHitLife, hitOnce: true);

        float t = 0f;
        while (t < 1f)
        {
            if (go == null) yield break;
            if (stabDmg.HasHit) break;

            t += Time.deltaTime / stabThrustDuration;
            go.transform.position = Vector3.Lerp(stabStart, stabEnd, t);
            yield return null;
        }

        if (go != null && !stabDmg.HasHit)
        {
            if (stabHitLife > 0f) yield return new WaitForSeconds(stabHitLife);
            if (go != null) Destroy(go);
        }
    }
    IEnumerator StabThrust(SkillContext ctx, Vector3 startPos, Vector3 targetPos)
    {
        Vector3 dir = (targetPos - startPos).normalized;
        Vector3 stabStart = startPos + dir * stabMuzzleOffset;
        Vector3 stabEnd = stabStart + dir * stabThrustDistance;

        var go = Instantiate(stabPrefab, stabStart, Quaternion.identity);
        go.transform.right = dir;

        ApplySortingLikeOwner(ctx, go);
        EnsureTriggerCollider(go);
        EnsureKinematicRB(go);

        var stabDmg = go.GetComponent<SpearStabDamage>();
        if (stabDmg == null) stabDmg = go.AddComponent<SpearStabDamage>();

        var stats = ctx.owner.GetComponent<PlayerStatsMono>();
        stabDmg.InitScaled(stats, damage, atkScale, smptScale, damageType, canCrit, ctx.owner.gameObject,
            destroyDelay: stabHitLife, hitOnce: true);

        float t = 0f;
        while (t < 1f)
        {
            if (go == null) yield break;
            if (stabDmg.HasHit) break;

            t += Time.deltaTime / stabThrustDuration;
            go.transform.position = Vector3.Lerp(stabStart, stabEnd, t);
            yield return null;
        }

        if (go != null && !stabDmg.HasHit)
        {
            if (stabHitLife > 0f) yield return new WaitForSeconds(stabHitLife);
            if (go != null) Destroy(go);
        }
    }

    void ApplySortingLikeOwner(SkillContext ctx, GameObject go)
    {
        var sr = go.GetComponentInChildren<SpriteRenderer>();
        if (sr == null) return;

        var ownerSr = ctx.owner.GetComponentInChildren<SpriteRenderer>();
        if (ownerSr != null)
        {
            sr.sortingLayerID = ownerSr.sortingLayerID;
            sr.sortingOrder = ownerSr.sortingOrder + 10;
        }
        else
        {
            sr.sortingLayerName = "VFX";
            sr.sortingOrder = 50;
        }
    }

    void EnsureTriggerCollider(GameObject go)
    {
        Collider2D col = go.GetComponent<Collider2D>();
        if (col == null)
        {
            var bc = go.AddComponent<BoxCollider2D>();
            bc.size = new Vector2(2.6f, 1.2f);
            bc.isTrigger = true;
        }
        else col.isTrigger = true;
    }

    void EnsureKinematicRB(GameObject go)
    {
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb == null) rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
    }
}
