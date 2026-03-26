using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skills/Spear Combo Move Skill")]
public class SpearComboMoveSkill : WeaponSkill
{
    [Header("Combo")]
    public float comboWindow = 0.65f;
    public float stepCooldown = 0.45f;
    public float inputBuffer = 0.25f;

    [Header("Timing")]
    public float hit1Windup = 0.05f;
    public float hit2Windup = 0.05f;
    public float hit3Windup = 0.05f;

    [Header("Prefabs Right")]
    public GameObject stabPrefabR;
    public GameObject sweepPrefabR;
    public GameObject spinPrefabR;
    [Header("Prefabs Left")]
    public GameObject stabPrefabL;
    public GameObject sweepPrefabL;
    public GameObject spinPrefabL;

    [Header("Offsets")]
    public Vector3 stabOffset = new Vector3(0.9f, 0.15f, 0f);
    public Vector3 sweepOffset = new Vector3(1.0f, 0.2f, 0f);
    public Vector3 spinOffset = new Vector3(0f, 0f, 0f);

    [Header("Life")]
    public float stabLife = 0.5f;
    public float sweepLife = 0.5f;
    public float spinLife = 0.5f;

    [Header("Combat")]
    public DamageType damageType = DamageType.Physical;
    public bool canCrit = true;
    public float smptScale = 0f;

    public int stabDamage = 30; // Hit 1 – nhanh, yếu
    public float stabAtkScale = 1f;

    public int sweepDamage = 50; // Hit 2 – quét, trung
    public float sweepAtkScale = 1.15f;

    public int spinDamage = 80; // Hit 3 – xoáy 2 tick
    public float spinAtkScale = 1.4f;

    class State
    {
        public int comboIndex;
        public float lastPress;
        public float nextStepTime;
        public bool busy;

        public bool buffered;
        public float bufferUntil;

        public SkillCoroutineRunner runner;
        public PlayerMove2D playerMove;
    }

    static readonly Dictionary<int, State> states = new();

    protected override void OnUse(SkillContext ctx)
    {
        if (ctx.owner == null) return;

        int id = ctx.owner.GetInstanceID();
        if (!states.TryGetValue(id, out var st))
            states[id] = st = new State();

        if (st.runner == null)
            st.runner = ctx.owner.GetComponent<SkillCoroutineRunner>()
                     ?? ctx.owner.gameObject.AddComponent<SkillCoroutineRunner>();

        if (st.playerMove == null)
            st.playerMove = ctx.owner.GetComponent<PlayerMove2D>();

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
        var owner = ctx.owner;
        float sign = st.playerMove != null ? st.playerMove.FacingDirection : 1f;

        Transform visual = st.playerMove?.visualSkill?.transform;
        Animator anim = visual != null ? visual.GetComponentInChildren<Animator>() : null;

        // Bật visual
        if (st.playerMove != null)
            st.playerMove.SetSkillVisual();
        if (visual != null)
        {
            visual.gameObject.SetActive(true);
            yield return null;
            visual.localScale = new Vector3(Mathf.Abs(visual.localScale.x) * sign,
                                            visual.localScale.y,
                                            visual.localScale.z);
        }

        int hit = st.comboIndex;

        // Animation bool
        if (anim != null)
        {
            anim.SetBool("isATR_GREEN1", hit == 0);
            anim.SetBool("isATR_GREEN2", hit == 1);
            anim.SetBool("isATR_GREEN3", hit == 2);
        }

        st.comboIndex = (st.comboIndex + 1) % 3;

        if (hit == 0) yield return Hit1(ctx, sign);
        else if (hit == 1) yield return Hit2(ctx, sign);
        else yield return Hit3(ctx, sign);

        // Giữ anim một lúc
        yield return new WaitForSeconds(0.3f);

        // Tắt anim
        if (anim != null)
        {
            anim.SetBool("isATR_GREEN1", false);
            anim.SetBool("isATR_GREEN2", false);
            anim.SetBool("isATR_GREEN3", false);
        }

        // Tắt visual
        if (visual != null)
            visual.gameObject.SetActive(false);

        // Trả normal visual
        if (st.playerMove != null)
            st.playerMove.SetNormalVisual();

        st.busy = false;

        // Xử lý buffer
        if (st.buffered && Time.time <= st.bufferUntil)
        {
            st.buffered = false;
            st.lastPress = Time.time;
            yield return new WaitForSeconds(stepCooldown);
            st.runner.Run(ComboStep(ctx, st));
        }
    }

    IEnumerator Hit1(SkillContext ctx, float sign)
    {
        yield return new WaitForSeconds(hit1Windup + 0.2f);
        Vector3 pos = ctx.owner.position + new Vector3(stabOffset.x * sign, stabOffset.y, 0);
        GameObject prefab = sign > 0 ? stabPrefabR : stabPrefabL;
        Spawn(prefab, ctx, pos, stabDamage, stabAtkScale, stabLife);
    }

    IEnumerator Hit2(SkillContext ctx, float sign)
    {
        yield return new WaitForSeconds(hit2Windup + 0.2f);
        Vector3 pos = ctx.owner.position + new Vector3(sweepOffset.x * sign, sweepOffset.y, 0);
        GameObject prefab = sign > 0 ? sweepPrefabR : sweepPrefabL;
        Spawn(prefab, ctx, pos, sweepDamage, sweepAtkScale, sweepLife);
    }

    IEnumerator Hit3(SkillContext ctx, float sign)
    {
        yield return new WaitForSeconds(hit3Windup);
        Vector3 pos = ctx.owner.position + spinOffset;
        GameObject prefab = sign > 0 ? spinPrefabR : spinPrefabL;

        // Tick 1 – yếu
        Spawn(prefab, ctx, pos, spinDamage, spinAtkScale * 0.2f, spinLife);
        yield return new WaitForSeconds(0.1f);
        // Tick 2 – mạnh
        Spawn(prefab, ctx, pos, spinDamage, spinAtkScale * 0.3f, spinLife);
    }

    void Spawn(GameObject prefab, SkillContext ctx, Vector3 pos,
               int dmg, float scale, float life)
    {
        var go = Instantiate(prefab, pos, Quaternion.identity);
        var hb = go.GetComponentInChildren<SwordHitbox>();
        if (hb != null)
        {
            var stats = ctx.owner.GetComponent<PlayerStatsMono>();
            hb.InitScaled(stats, dmg, scale, smptScale, damageType, canCrit, ctx.owner.gameObject);
            hb.Fire();
        }
        Destroy(go, life);
    }
}