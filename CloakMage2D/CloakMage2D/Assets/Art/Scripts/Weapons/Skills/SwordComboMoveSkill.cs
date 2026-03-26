
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skills/Sword Combo Move Skill")]
public class SwordComboMoveSkill : WeaponSkill
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
    public GameObject slashSidePrefabR;
    public GameObject slashDownPrefabR;
    [Header("Prefabs Left")]
    public GameObject stabPrefabL;
    public GameObject slashSidePrefabL;
    public GameObject slashDownPrefabL;
    [Header("Offsets")]
    public Vector3 stabOffset = new Vector3(0.9f, 0.15f, 0f);
    public Vector3 slashSideOffset = new Vector3(1.0f, 0.2f, 0f);
    public Vector3 slashDownOffset = new Vector3(0f, -0.25f, 0f);

    [Header("Life")]
    public float stabLife = 0.5f;
    public float slashSideLife = 0.5f;
    public float slamLife = 0.5f;

    [Header("Combat")]
    public DamageType damageType = DamageType.Physical;
    public bool canCrit = true;
    public float smptScale = 0f;

    public int stabDamage = 30;
    public float stabAtkScale = 1f;

    public int slashSideDamage = 50;
    public float slashSideAtkScale = 1.15f;

    public int slamDamage = 80;
    public float slamAtkScale = 1.4f;

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

        // float sign = ctx.mouseWorld.x >= owner.position.x ? 1f : -1f;
        float sign = 1f;
        if (st.playerMove != null)
            sign = st.playerMove.FacingDirection;
        //Transform visual = owner.Find("Visual_Skill");
        //Animator anim = visual != null ? visual.GetComponent<Animator>() : null;
        Transform visual = null;
        Animator anim = null;

        if (st.playerMove != null)
        {
            visual = st.playerMove.visualSkill != null
                ? st.playerMove.visualSkill.transform
                : null;

            if (visual != null)
                anim = visual.GetComponentInChildren<Animator>();
        }
        // ===== BẬT VISUAL =====
        if (st.playerMove != null)
            st.playerMove.SetSkillVisual();

        if (visual != null)
        {
            visual.gameObject.SetActive(true);
            yield return null;
            visual.localScale = new Vector3(
                Mathf.Abs(visual.localScale.x) * sign,
                visual.localScale.y,
                visual.localScale.z
            );
        }

        int hit = st.comboIndex;

        // ===== ANIMATION =====
        if (anim != null)
        {
            anim.SetBool("isATK1", hit == 0);
            anim.SetBool("isATK2", hit == 1);
            anim.SetBool("isATK3", hit == 2);
            if (hit == 0) anim.SetBool("isATK1", true);
            else if (hit == 1) anim.SetBool("isATK2", true);
            else anim.SetBool("isATK3", true);
            Debug.Log("Visual: " + visual);
            Debug.Log("Anim: " + anim);
        }

        st.comboIndex = (st.comboIndex + 1) % 3;

        if (hit == 0) yield return Hit1(ctx, sign);
        else if (hit == 1) yield return Hit2(ctx, sign);
        else yield return Hit3(ctx, sign);

        // ⏱ GIỮ ANIM
        yield return new WaitForSeconds(0.3f);

        // ===== TẮT ANIM =====
        if (anim != null)
        {
            anim.SetBool("isATK1", false);
            anim.SetBool("isATK2", false);
            anim.SetBool("isATK3", false);
        }

        // ===== TẮT VISUAL =====
        if (visual != null)
            visual.gameObject.SetActive(false);

        // ===== TRẢ NORMAL =====
        if (st.playerMove != null)
            st.playerMove.SetNormalVisual();

        st.busy = false;

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

        GameObject prefab = (sign > 0) ? stabPrefabR : stabPrefabL;

        Spawn(prefab, ctx, pos, stabDamage, stabAtkScale, stabLife);
    }

    IEnumerator Hit2(SkillContext ctx, float sign)
    {
        yield return new WaitForSeconds(hit2Windup + 0.2f);

        Vector3 pos = ctx.owner.position + new Vector3(slashSideOffset.x * sign, slashSideOffset.y, 0);

        GameObject prefab = (sign > 0) ? slashSidePrefabR : slashSidePrefabL;

        Spawn(prefab, ctx, pos, slashSideDamage, slashSideAtkScale, slashSideLife);
    }

    IEnumerator Hit3(SkillContext ctx, float sign)
    {
        yield return new WaitForSeconds(hit3Windup + 0.2f);

        Vector3 pos = ctx.owner.position + slashDownOffset;

        GameObject prefab = (sign > 0) ? slashDownPrefabR : slashDownPrefabL;

        Spawn(prefab, ctx, pos, slamDamage, slamAtkScale * 0.6f, slamLife);

        yield return new WaitForSeconds(0.1f);

        Spawn(prefab, ctx, pos, slamDamage, slamAtkScale * 1.2f, slamLife);
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