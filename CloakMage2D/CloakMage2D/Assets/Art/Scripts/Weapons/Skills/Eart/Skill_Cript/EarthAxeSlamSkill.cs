using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skills/Earth Axe Slam Skill")]
public class EarthAxeSlamSkill : WeaponSkill
{
    [Header("Jump")]
    public float jumpTime = 0.6f;
    public AnimationCurve jumpCurve; // optional cho bay cong

    [Header("Landing Effect (??t n?t)")]
    public EarthCrackArea crackPrefab;

    [Header("VFX")]
    public GameObject jumpVfx;
    public GameObject landVfx;

    [Header("Crack Settings")]
    public float duration = 6f;
    public float tickInterval = 0.5f;
    public float radius = 2.5f;
    public LayerMask hitMask;

    [Header("Damage")]
    public int damage = 20;
    public DamageType damageType = DamageType.Physical;
    public float atkScale = 1.0f;
    public float smptScale = 0f;
    public bool canCrit = true;
    [Header("HP Scaling")]
    public float hpScale = 0.0002f;
    protected override void OnUse(SkillContext ctx)
    {
        if (ctx.owner == null || crackPrefab == null) return;

        var runner = ctx.owner.GetComponent<SkillCoroutineRunner>();
        if (runner == null)
            runner = ctx.owner.gameObject.AddComponent<SkillCoroutineRunner>();

        runner.Run(DoSkill(ctx));
    }

    IEnumerator DoSkill(SkillContext ctx)
    {
        Transform owner = ctx.owner;
        Vector3 start = owner.position;
        Vector3 target = ctx.mouseWorld;
        target.z = 0f;

        var stats = owner.GetComponent<PlayerStatsMono>();

        // ===== 1. Jump VFX =====
        if (jumpVfx)
            Instantiate(jumpVfx, start, Quaternion.identity);

        // ===== 2. NHẢY LÊN TẠI CHỖ =====
        float halfTime = jumpTime * 0.5f;
        float t = 0f;

        while (t < halfTime)
        {
            t += Time.deltaTime;
            float progress = t / halfTime;

            Vector3 pos = start;

            // bay lên cao
            float height = jumpCurve != null
                ? jumpCurve.Evaluate(progress)
                : Mathf.Sin(progress * Mathf.PI); // fallback

            pos.y += height * 10f; // chỉnh độ cao ở đây

            owner.position = pos;

            yield return null;
        }

        // ===== 3. TELEPORT TRÊN KHÔNG → TARGET =====
        Vector3 airPos = target + Vector3.up * 2f; // ở trên đầu target
        owner.position = airPos;

        // ===== 4. RƠI XUỐNG =====
        t = 0f;
        while (t < halfTime)
        {
            t += Time.deltaTime;
            float progress = t / halfTime;

            Vector3 pos = Vector3.Lerp(airPos, target, progress);

            owner.position = pos;

            yield return null;
        }

        owner.position = target;

        // ===== 5. LAND VFX =====
        if (landVfx)
            Instantiate(landVfx, target, Quaternion.identity);

        // ===== 6. CRACK =====
        var crack = Instantiate(crackPrefab, target, Quaternion.identity);

        float finalAtkScale = atkScale;

        // =========================
        // ✅ SCALE THEO MAX HP
        // =========================
        if (stats != null)
        {
            float hpBonus = stats.maxHP * hpScale;
            finalAtkScale += hpBonus;
        }

        // =========================
        // ✅ BONUS KHI CÓ SHIELD (giống skill kia nếu muốn)
        // =========================
        EnergyShieldArea shield = owner.GetComponentInChildren<EnergyShieldArea>();
        if (shield != null && shield.IsActive)
        {
            finalAtkScale *= 1.25f;
        }

        crack.Init(
            owner.gameObject,
            stats,
            damage,
            finalAtkScale, // 👈 dùng cái đã scale
            smptScale,
            damageType,
            canCrit,
            duration,
            tickInterval,
            radius,
            hitMask
        );
    }
}