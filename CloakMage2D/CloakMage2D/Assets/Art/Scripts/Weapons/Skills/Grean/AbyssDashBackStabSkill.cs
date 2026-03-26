using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skills/Abyss Dash Back Stab")]
public class AbyssDashBackStabSkill : WeaponSkill
{
    [Header("Dash")]
    public float dashDistance = 4f;
    public float dashDuration = 0.2f;
    public float returnDuration = 0.18f;

    [Header("Dash Hit")]
    public float dashRadius = 1.6f;
    public LayerMask hitMask;

    [Header("Prefab (đâm xuyên)")]
    public GameObject stabPrefab;
    public float stabOffset = 0.8f;

    [Header("Combat")]
    public int dashDamage = 40;
    public float dashAtkScale = 0.8f;

    public int stabDamage = 90;
    public float stabAtkScale = 1.4f;

    public DamageType damageType = DamageType.Physical;
    public float smptScale = 0f;
    public bool canCrit = true;

    class State { public bool busy; }
    static readonly Dictionary<int, State> states = new();

    protected override void OnUse(SkillContext ctx)
    {
        if (ctx.owner == null) return;

        int id = ctx.owner.GetInstanceID();
        if (!states.TryGetValue(id, out var st))
            states[id] = st = new State();

        if (st.busy) return;

        var runner = ctx.owner.GetComponent<SkillCoroutineRunner>();
        if (runner == null)
            runner = ctx.owner.gameObject.AddComponent<SkillCoroutineRunner>();

        runner.Run(DoSkill(ctx, st));
    }

    IEnumerator DoSkill(SkillContext ctx, State st)
    {
        st.busy = true;

        Transform owner = ctx.owner;

        // ✅ LẤY HƯỚNG GIỐNG IceTornado
        PlayerMove2D move = owner.GetComponent<PlayerMove2D>();
        float face = move != null ? move.FacingDirection : 1f;

        Vector2 dir = Vector2.right * face;

        Vector3 start = owner.position;
        Vector3 end = start + (Vector3)(dir * dashDistance);

        // ===== DASH TỚI =====
        yield return DashWithStab(ctx, start, end, dir, dashDuration);

        // ===== DASH VỀ =====
        yield return DashWithStab(ctx, end, start, -dir, returnDuration);

        // ===== ĐÂM CUỐI =====
        FinalStab(ctx, dir);

        st.busy = false;
    }

    IEnumerator DashWithStab(SkillContext ctx, Vector3 from, Vector3 to, Vector2 dir, float duration)
    {
        Transform tr = ctx.owner.transform;
        var attacker = ctx.owner.GetComponent<PlayerStatsMono>();

        HashSet<BossHealth> hit = new();

        // 🔥 spawn spear VFX chạy theo hướng dash
        GameObject spear = Instantiate(stabPrefab,
            tr.position + (Vector3)(dir * stabOffset),
            RotationFromDir(dir));

        EnsureCollider(spear);
        EnsureRB(spear);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            Vector3 pos = Vector3.Lerp(from, to, t);
            tr.position = pos;

            // spear đi theo player
            if (spear != null)
                spear.transform.position = pos + (Vector3)(dir * stabOffset);

            // 🔥 DAMAGE TRÊN ĐƯỜNG LƯỚT
            var hits = Physics2D.OverlapCircleAll(pos, dashRadius, hitMask);
            foreach (var h in hits)
            {
                var boss = h.GetComponentInParent<BossHealth>();
                if (boss == null || hit.Contains(boss)) continue;

                hit.Add(boss);

                var targetStats = boss.GetComponent<PlayerStatsMono>();

                bool crit;
                int dmg = DamageCore.Compute(
                    attacker,
                    targetStats,
                    dashDamage,
                    dashAtkScale,
                    smptScale,
                    damageType,
                    canCrit,
                    out crit
                );

                boss.TakeDamage(dmg);
                DamagePopupSpawner.I?.Show(boss.transform.position, dmg, crit);
            }

            yield return null;
        }

        if (spear != null) Destroy(spear);
    }

    void FinalStab(SkillContext ctx, Vector2 dir)
    {
        Vector3 pos = ctx.owner.position + (Vector3)(dir * stabOffset);

        GameObject go = Instantiate(stabPrefab, pos, RotationFromDir(dir));

        var hb = go.GetComponent<SwordHitbox>();
        if (hb == null) hb = go.AddComponent<SwordHitbox>();

        hb.InitScaled(
            ctx.owner.GetComponent<PlayerStatsMono>(),
            stabDamage,
            stabAtkScale,
            smptScale,
            damageType,
            canCrit,
            ctx.owner.gameObject
        );

        hb.Fire();

        Destroy(go, 0.4f);
    }

    Quaternion RotationFromDir(Vector2 dir)
    {
        float z = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        return Quaternion.Euler(0f, 0f, z);
    }

    void EnsureCollider(GameObject go)
    {
        var col = go.GetComponent<Collider2D>();
        if (col == null)
        {
            var bc = go.AddComponent<BoxCollider2D>();
            bc.isTrigger = true;
        }
        else col.isTrigger = true;
    }

    void EnsureRB(GameObject go)
    {
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb == null) rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }
}