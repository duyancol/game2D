
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skills/Earth Hammer Basic Attack")]
public class EarthHammerBasicAttack : WeaponSkill
{
    [Header("Prefabs Hit 1")]
    public GameObject hit1RightPrefab;
    public GameObject hit1LeftPrefab;

    [Header("Prefabs Hit 2")]
    public GameObject hit2RightPrefab;
    public GameObject hit2LeftPrefab;

    [Header("Hit 3 (Slam)")]
    public GameObject slamPrefab;

    [Header("Combo")]
    public int maxCombo = 3;
    public float comboResetTime = 1.2f;

    private int comboIndex = 0;
    private float lastAttackTime = 0f;

    [Header("Timing")]
    public float attackDelay = 0.15f;
    public float lifeTime = 0.2f;

    [Header("Range")]
    public float range = 1.4f;
    public float aoeRadius = 2.2f;

    [Header("Combat")]
    public int damage = 80;
    public float atkScale = 1.2f;
    public float smptScale = 0.3f;
    public DamageType damageType = DamageType.Physical;
    public bool canCrit = true;
    [Header("HP Scaling")]
    public float hpScale = 0.0001f;
   
    //protected override void OnUse(SkillContext ctx)
    //{
    //    if (Time.time - lastAttackTime > comboResetTime)
    //        comboIndex = 0;

    //    comboIndex++;
    //    if (comboIndex > maxCombo)
    //        comboIndex = 1;

    //    lastAttackTime = Time.time;

    //    var owner = ctx.owner;

    //    var anim = owner.GetComponentInChildren<Animator>();
    //    if (anim != null)
    //        anim.SetTrigger("ATK_" + comboIndex);

    //    var runner = owner.GetComponent<SkillCoroutineRunner>();
    //    if (runner == null)
    //        runner = owner.gameObject.AddComponent<SkillCoroutineRunner>();

    //    runner.Run(DoAttack(ctx, comboIndex));
    //}
    protected override void OnUse(SkillContext ctx)
    {
        if (Time.time - lastAttackTime > comboResetTime)
            comboIndex = 0;

        comboIndex++;
        if (comboIndex > maxCombo)
            comboIndex = 1;

        lastAttackTime = Time.time;

        var owner = ctx.owner;
        var playerMove = owner.GetComponent<PlayerMove2D>();

        // ===== LẤY HƯỚNG =====
        float sign = 1f;
        if (playerMove != null)
            sign = playerMove.FacingDirection;

        // ===== LẬT VISUAL =====
        Transform visual = owner.Find("Visual_Skill");
        if (visual != null)
        {
            visual.localScale = new Vector3(
                Mathf.Abs(visual.localScale.x) * sign,
                visual.localScale.y,
                visual.localScale.z
            );
        }

        if (playerMove != null)
            playerMove.SetSkillVisual();

        // ===== ANIMATION BOOL =====
        var anim = owner.Find("Visual_Skill")?.GetComponent<Animator>();
        if (anim != null)
            anim.SetBool("isAttacking", true);

        var runner = owner.GetComponent<SkillCoroutineRunner>();
        if (runner == null)
            runner = owner.gameObject.AddComponent<SkillCoroutineRunner>();

        runner.Run(DoAttack(ctx, comboIndex, playerMove, anim));
    }
    //IEnumerator DoAttack(SkillContext ctx, int comboStep)
    //{
    //    yield return new WaitForSeconds(attackDelay);

    //    var owner = ctx.owner;

    //    float sign = 1f;
    //    var move = owner.GetComponent<PlayerMove2D>();
    //    if (move != null)
    //        sign = move.FacingDirection;

    //    Vector3 dir = new Vector3(sign, 0, 0);

    //    // =========================
    //    // HIT 1 & 2
    //    // =========================
    //    if (comboStep == 1 || comboStep == 2)
    //    {
    //        Vector3 pos = owner.position + dir * range;

    //        GameObject prefab = null;

    //        if (comboStep == 1)
    //            prefab = (sign > 0) ? hit1RightPrefab : hit1LeftPrefab;
    //        else if (comboStep == 2)
    //            prefab = (sign > 0) ? hit2RightPrefab : hit2LeftPrefab;

    //        if (prefab == null) yield break;

    //        var go = Object.Instantiate(prefab, pos, Quaternion.identity);

    //        ApplyDamage(go, owner);

    //        Destroy(go, lifeTime);
    //    }
    //    // =========================
    //    // HIT 3 (SLAM)
    //    // =========================
    //    else
    //    {
    //        var go = Object.Instantiate(slamPrefab, owner.position, Quaternion.identity);

    //        ApplyDamage(go, owner);

    //        go.transform.localScale = Vector3.one * aoeRadius;

    //        Destroy(go, lifeTime);
    //    }
    //}
    IEnumerator DoAttack(SkillContext ctx, int comboStep, PlayerMove2D playerMove, Animator anim)
{
    yield return new WaitForSeconds(attackDelay);

    var owner = ctx.owner;

    float sign = 1f;
    if (playerMove != null)
        sign = playerMove.FacingDirection;

    Vector3 dir = new Vector3(sign, 0, 0);

    // ===== HIT =====
    if (comboStep == 1 || comboStep == 2)
    {
        Vector3 pos = owner.position + dir * range;

        GameObject prefab = (comboStep == 1)
            ? (sign > 0 ? hit1RightPrefab : hit1LeftPrefab)
            : (sign > 0 ? hit2RightPrefab : hit2LeftPrefab);

        if (prefab == null) yield break;

        var go = Object.Instantiate(prefab, pos, Quaternion.identity);
        ApplyDamage(go, owner);
        Destroy(go, lifeTime);
    }
        else
        {
            // 💥 HIT 1 (yếu)
            var go1 = Object.Instantiate(slamPrefab, owner.position, Quaternion.identity);
            go1.transform.localScale = Vector3.one * aoeRadius;
            ApplyDamage(go1, owner, 0.6f); // 👈 yếu hơn
            Destroy(go1, lifeTime);

            yield return new WaitForSeconds(0.1f);

            // 💥 HIT 2 (mạnh - impact)
            var go2 = Object.Instantiate(slamPrefab, owner.position, Quaternion.identity);
            go2.transform.localScale = Vector3.one * aoeRadius;
            ApplyDamage(go2, owner, 1.2f); // 👈 mạnh hơn
            Destroy(go2, lifeTime);
        }

        // ===== CHỜ ANIM CHẠY =====
        yield return new WaitForSeconds(0.3f);

    // ===== TẮT ANIMATION =====
    if (anim != null)
        anim.SetBool("isAttacking", false);

    // ===== TRẢ VISUAL =====
    if (playerMove != null)
        playerMove.SetNormalVisual();
}
    void ApplyDamage(GameObject go, Transform owner, float multiplier = 1f)
    {
        var dmgComp = go.GetComponent<ProjectileDamage>();
        if (dmgComp != null)
        {
            var stats = owner.GetComponent<PlayerStatsMono>();

            float finalAtkScale = atkScale;

            if (stats != null)
            {
                float hpBonus = stats.maxHP * hpScale;
                finalAtkScale += hpBonus;
            }

            EnergyShieldArea shield = owner.GetComponentInChildren<EnergyShieldArea>();
            if (shield != null && shield.IsActive)
            {
                finalAtkScale *= 1.25f;
            }

            // 🔥 ÁP MULTIPLIER Ở ĐÂY
            finalAtkScale *= multiplier;

            dmgComp.InitScaled(
                stats,
                damage,
                finalAtkScale,
                smptScale,
                damageType,
                canCrit,
                owner.gameObject
            );
        }
    }
}