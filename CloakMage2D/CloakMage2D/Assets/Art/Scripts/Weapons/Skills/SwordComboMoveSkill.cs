
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skills/Sword Combo Move Skill")]
public class SwordComboMoveSkill : WeaponSkill
{
    [Header("Combo")]
    public float comboWindow = 0.45f;
    public float stepCooldown = 0.08f;

    [Tooltip("Bấm sớm vẫn nối hit tiếp theo (mượt).")]
    public float inputBuffer = 0.18f;

    [Header("Timing (feel)")]
    public float hit1Windup = 0.02f;
    public float hit2Windup = 0.02f;
    public float hit3Windup = 0.02f;

    public float hit1Recovery = 0.05f;
    public float hit2Recovery = 0.06f;
    public float hit3Recovery = 0.10f;

    [Header("Movement")]
    [Tooltip("Dash hit 1. 6f là quá xa -> dễ giật. Thử 0.8~1.6")]
    public float dashDistance = 1.1f;
    public float dashTime = 0.10f;

    public float jumpUp = 3f;
    public float jumpUpTime = 0.12f;

    public float slamDownTime = 0.10f;

    [Header("Prefabs (VFX/Hitbox)")]
    public GameObject stabPrefab;
    public GameObject slashSidePrefab;
    public GameObject slashDownPrefab;

    [Header("Spawn Offsets")]
    public Vector3 stabOffset = new Vector3(0.8f, 0.15f, 0f);
    public Vector3 slashSideOffset = new Vector3(0.9f, 0.2f, 0f);
    public Vector3 slashDownOffset = new Vector3(0f, -0.2f, 0f);

    [Header("Life per hit (thời gian hoạt ảnh tồn tại)")]
    public float stabLife = 0.22f;       // hit 1
    public float slashSideLife = 0.26f;  // hit 2
    public float slamLife = 0.32f;       // hit 3

    [Header("Combat")]
    public DamageType damageType = DamageType.Physical;
    public bool canCrit = true;
    public float smptScale = 0f;

    public int stabDamage = 30;
    public float stabAtkScale = 1.0f;

    public int slashSideDamage = 40;
    public float slashSideAtkScale = 1.05f;

    public int slamDamage = 55;
    public float slamAtkScale = 1.2f;

    [Header("Action Controller (player animation/motion)")]
    [Tooltip("Nếu player có PlayerSwordActionController thì sẽ action mượt. Nếu không có vẫn spawn hitbox như cũ (nhưng không move).")]
    public bool usePlayerAction = true;

    [Tooltip("Tỉ lệ thời điểm spawn hitbox trong HIT1 (0..1 theo dashTime). 0.55 = spawn hơi sau nửa dash cho đúng frame đâm.")]
    [Range(0f, 1f)] public float hit1ImpactT = 0.55f;

    [Tooltip("Tỉ lệ thời điểm spawn hitbox trong HIT2 (0..1 theo slashSideLife). 0.15 = spawn đầu cú quét.")]
    [Range(0f, 1f)] public float hit2ImpactT = 0.15f;

    [Tooltip("Tỉ lệ thời điểm spawn hitbox trong HIT3 (0..1 theo slamDownTime). 0.85 = gần chạm đất.")]
    [Range(0f, 1f)] public float hit3ImpactT = 0.85f;

    class State
    {
        public int comboIndex;
        public float lastPress;
        public float nextStepTime;
        public bool busy;

        public bool buffered;
        public float bufferUntil;

        public SkillCoroutineRunner runner;

        // NOTE: đổi class này đúng với file bạn đang có
        public PlayerSwordActionController action;
    }

    static readonly Dictionary<int, State> states = new();

    protected override void OnUse(SkillContext ctx)
    {
        if (ctx.owner == null) return;

        int id = ctx.owner.GetInstanceID();
        if (!states.TryGetValue(id, out var st))
            states[id] = st = new State();

        if (st.runner == null)
        {
            st.runner = ctx.owner.GetComponent<SkillCoroutineRunner>();
            if (st.runner == null) st.runner = ctx.owner.gameObject.AddComponent<SkillCoroutineRunner>();
        }

        if (usePlayerAction && st.action == null)
        {
            st.action = ctx.owner.GetComponent<PlayerSwordActionController>();
            // không tự Add nếu bạn đã có file riêng (đỡ bị trùng settings),
            // nhưng nếu muốn auto add thì mở dòng dưới:
            // if (st.action == null) st.action = ctx.owner.gameObject.AddComponent<PlayerSwordActionController>();
        }

        if (Time.time - st.lastPress > comboWindow)
            st.comboIndex = 0;

        st.lastPress = Time.time;

        if (st.busy)
        {
            st.buffered = true;
            st.bufferUntil = Time.time + inputBuffer;
            return;
        }

        if (Time.time < st.nextStepTime) return;
        st.nextStepTime = Time.time + stepCooldown;

        st.runner.Run(ComboStep(ctx, st));
    }

    IEnumerator ComboStep(SkillContext ctx, State st)
    {
        st.busy = true;

        bool faceRight = ctx.mouseWorld.x >= ctx.owner.position.x;

        int hit = st.comboIndex;
        st.comboIndex = (st.comboIndex + 1) % 3;

        if (hit == 0) yield return Hit1_StabDash(ctx, st, faceRight);
        else if (hit == 1) yield return Hit2_SlashSide(ctx, st, faceRight);
        else yield return Hit3_JumpSlam(ctx, st, faceRight);

        st.busy = false;

        if (st.buffered && Time.time <= st.bufferUntil)
        {
            st.buffered = false;
            st.lastPress = Time.time;

            if (stepCooldown > 0f) yield return new WaitForSeconds(stepCooldown);
            st.runner.Run(ComboStep(ctx, st));
        }
        else st.buffered = false;
    }

    IEnumerator Hit1_StabDash(SkillContext ctx, State st, bool faceRight)
    {
        // ===== ACTION: dash + stab (player làm thật) =====
        if (usePlayerAction && st.action != null)
        {
            // windup / stabHold dùng theo cảm giác: stabHold lấy một phần stabLife cho “đâm giữ”
            float stabHold = Mathf.Clamp(stabLife * 0.35f, 0.04f, 0.18f);
            st.action.PlayHit1_DashStab(faceRight, dashDistance, dashTime, hit1Windup, stabHold, hit1Recovery);

            // spawn hitbox đúng frame đâm (theo dashTime)
            float impactWait = hit1Windup + dashTime * Mathf.Clamp01(hit1ImpactT);
            if (impactWait > 0f) yield return new WaitForSeconds(impactWait);

            SpawnWithDamage(stabPrefab, ctx, stabOffset, faceRight, stabDamage, stabAtkScale, stabLife);

            // chờ phần còn lại để combo không bị cắt
            float remain = (dashTime * (1f - Mathf.Clamp01(hit1ImpactT))) + stabHold + hit1Recovery;
            if (remain > 0f) yield return new WaitForSeconds(remain);
            yield break;
        }

        // ===== FALLBACK: không có action controller -> chỉ spawn (không move) =====
        if (hit1Windup > 0f) yield return new WaitForSeconds(hit1Windup);
        SpawnWithDamage(stabPrefab, ctx, stabOffset, faceRight, stabDamage, stabAtkScale, stabLife);
        if (hit1Recovery > 0f) yield return new WaitForSeconds(hit1Recovery);
    }

    IEnumerator Hit2_SlashSide(SkillContext ctx, State st, bool faceRight)
    {
        if (usePlayerAction && st.action != null)
        {
            float stepDist = dashDistance * 0.35f;
            float stepTime = dashTime * 0.85f;

            float slashTime = Mathf.Clamp(slashSideLife * 0.70f, 0.08f, 0.24f);
            st.action.PlayHit2_Slash(faceRight, stepDist, stepTime, hit2Windup, slashTime, hit2Recovery);

            // spawn lúc bắt đầu quét (theo slashSideLife)
            float impactWait = hit2Windup + slashSideLife * Mathf.Clamp01(hit2ImpactT);
            if (impactWait > 0f) yield return new WaitForSeconds(impactWait);

            SpawnWithDamage(slashSidePrefab, ctx, slashSideOffset, faceRight, slashSideDamage, slashSideAtkScale, slashSideLife);

            // chờ phần còn lại
            float remain = stepTime + (slashTime * (1f - Mathf.Clamp01(hit2ImpactT))) + hit2Recovery;
            if (remain > 0f) yield return new WaitForSeconds(remain);
            yield break;
        }

        if (hit2Windup > 0f) yield return new WaitForSeconds(hit2Windup);
        SpawnWithDamage(slashSidePrefab, ctx, slashSideOffset, faceRight, slashSideDamage, slashSideAtkScale, slashSideLife);
        if (hit2Recovery > 0f) yield return new WaitForSeconds(hit2Recovery);
    }

    IEnumerator Hit3_JumpSlam(SkillContext ctx, State st, bool faceRight)
    {
        if (usePlayerAction && st.action != null)
        {
            float slamHold = Mathf.Clamp(slamLife * 0.25f, 0.05f, 0.18f);
            st.action.PlayHit3_JumpSlam(faceRight, jumpUp, jumpUpTime, slamDownTime, hit3Windup, slamHold, hit3Recovery);

            // spawn gần lúc chạm đất (theo slamDownTime)
            float impactWait = hit3Windup + jumpUpTime + slamDownTime * Mathf.Clamp01(hit3ImpactT);
            if (impactWait > 0f) yield return new WaitForSeconds(impactWait);

            // spawn theo vị trí hiện tại của owner (đã được action controller move tới vị trí slam)
            Vector3 pos = ctx.owner.position;
            pos.z = 0f;
            SpawnAtOwnerOffsetWithDamage(slashDownPrefab, ctx, pos + slashDownOffset, faceRight, slamDamage, slamAtkScale, slamLife);

            float remain = (slamDownTime * (1f - Mathf.Clamp01(hit3ImpactT))) + slamHold + hit3Recovery;
            if (remain > 0f) yield return new WaitForSeconds(remain);
            yield break;
        }

        if (hit3Windup > 0f) yield return new WaitForSeconds(hit3Windup);
        Vector3 pos2 = ctx.owner.position;
        SpawnAtOwnerOffsetWithDamage(slashDownPrefab, ctx, pos2 + slashDownOffset, faceRight, slamDamage, slamAtkScale, slamLife);
        if (hit3Recovery > 0f) yield return new WaitForSeconds(hit3Recovery);
    }

    // ===== Spawn helpers (SwordHitbox) =====

    void SpawnWithDamage(GameObject prefab, SkillContext ctx, Vector3 localOffset, bool faceRight,
        int baseDmg, float atkScale, float life)
    {
        if (!prefab || ctx.owner == null) return;

        float sign = faceRight ? 1f : -1f;
        Vector3 pos = ctx.owner.position + new Vector3(localOffset.x * sign, localOffset.y, 0f);
        pos.z = 0f;

        SpawnAtOwnerOffsetWithDamage(prefab, ctx, pos, faceRight, baseDmg, atkScale, life);
    }

    void SpawnAtOwnerOffsetWithDamage(GameObject prefab, SkillContext ctx, Vector3 worldPos, bool faceRight,
        int baseDmg, float atkScale, float life)
    {
        if (!prefab || ctx.owner == null) return;

        worldPos.z = 0f;
        var go = Instantiate(prefab, worldPos, Quaternion.identity);

        var hb = go.GetComponent<SwordHitbox>();
        if (hb == null) hb = go.GetComponentInChildren<SwordHitbox>();

        if (hb != null)
        {
            var ownerGo = ctx.owner.gameObject;
            var stats = ownerGo.GetComponent<PlayerStatsMono>();

            hb.InitScaled(stats, baseDmg, atkScale, smptScale, damageType, canCrit, ownerGo);
            hb.Fire();
        }
        else
        {
            Debug.LogError("[SwordCombo] Prefab KHÔNG có SwordHitbox!");
        }

        var sr = go.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.flipX = !faceRight;

        Destroy(go, Mathf.Max(0.02f, life));
    }
}

