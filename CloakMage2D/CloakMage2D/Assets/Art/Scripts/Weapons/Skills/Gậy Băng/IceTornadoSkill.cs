using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skills/Ice Tornado Skill")]
public class IceTornadoSkill : WeaponSkill
{
    public IceTornadoProjectile tornadoPrefab;

    [Header("Spawn Offset")]
    public Vector3 spawnOffset = new Vector3(0.6f, 0.1f, 0f);

    [Header("Scaling")]
    public int baseDamage = 40;
    public float atkScale = 0f;
    public float smptScale = 1.2f;
    public DamageType damageType = DamageType.Magic;
    public bool canCrit = true;
    [Header("VFX")]
    public GameObject hitVfxPrefab;
    [Header("Hit")]
    public float radius = 1.8f;
    public LayerMask hitMask;

    protected override void OnUse(SkillContext ctx)
    {
        if (!tornadoPrefab || ctx.owner == null) return;

        Transform owner = ctx.owner;

        PlayerStatsMono stats = owner.GetComponent<PlayerStatsMono>();
        if (stats == null)
        {
            Debug.LogWarning("IceTornadoSkill: Owner không có PlayerStatsMono!");
            return;
        }

        // Lấy hướng
        PlayerMove2D move = owner.GetComponent<PlayerMove2D>();
        float face = move != null ? move.FacingDirection : 1f;

        Vector2 dir = Vector2.right * face;

        Vector3 spawnPos = owner.position +
            new Vector3(spawnOffset.x * face, spawnOffset.y, 0f);

        // =========================
        // CHECK EMPOWER
        // =========================
        AbsoluteZeroFlag flag = owner.GetComponent<AbsoluteZeroFlag>();
        bool empowered = false;

        if (flag != null && flag.isReady && flag.passive != null)
        {
            empowered = true;
        }

        // Spawn tornado
        var tornado = Instantiate(tornadoPrefab, spawnPos, Quaternion.identity);

        tornado.radius = radius;
        tornado.hitMask = hitMask;
        tornado.hitVfxPrefab = hitVfxPrefab;
        tornado.Init(
            dir,
            stats,
            baseDamage,
            atkScale,
            smptScale,
            damageType,
            canCrit,
            owner.gameObject
        );

        // =========================
        // APPLY EMPOWER BUFF
        // =========================
        if (empowered)
        {
            tornado.radius *= flag.passive.sizeMultiplier;

            tornado.baseDamage =
                Mathf.RoundToInt(baseDamage * flag.passive.damageMultiplier);

            tornado.damageTick /= flag.passive.tickMultiplier;

            // 🔥 QUAN TRỌNG: scale visual thật sự
            tornado.transform.localScale *= flag.passive.sizeMultiplier;
            // 🔥 QUAN TRỌNG
            tornado.SetDamageMultiplier(flag.passive.damageMultiplier);
            flag.passive.ConsumeEmpower();
        }
    }
}