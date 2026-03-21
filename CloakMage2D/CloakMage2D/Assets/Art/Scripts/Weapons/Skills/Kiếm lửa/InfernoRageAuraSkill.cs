using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skills/Sword/Inferno Rage Aura")]
public class InfernoRageAuraSkill : WeaponSkill
{
    [Header("Buff Settings")]
    public float duration = 7f;
    public float attackPercentBonus = 0.3f;   // 30%
    public float healPercentPerSecond = 0.07f; // 7%
    //public float skillCooldown = 32f;
    //private float nextTime;
    [Header("VFX")]
    public GameObject auraVFXPrefab;
    [Header("Heal VFX")]
    public GameObject healVFXPrefab;
    [Header("Aura Area")]
    public float auraRadius = 3f;
    [Header("Damage Over Time")]
    public LayerMask hitMask;
    public int baseDamagePerTick = 20;
    public float atkScale = 0.5f;
    public float smptScale = 0f;
    public DamageType damageType = DamageType.Physical;
    public bool canCrit = false;
    PlayerHealthBarUI barUI;
    GameObject healVfxInstance = null;
    protected override void OnUse(SkillContext ctx)
    {
        if (ctx.owner == null) return;

        var runner = ctx.owner.GetComponent<SkillCoroutineRunner>();
        if (runner == null)
            runner = ctx.owner.gameObject.AddComponent<SkillCoroutineRunner>();

        runner.Run(AuraRoutine(ctx));
    }

   // public bool CanUse() => Time.time >= nextTime;
    //IEnumerator AuraRoutine(SkillContext ctx)
    //{
    //    var playerMove = ctx.owner.GetComponent<PlayerMove2D>();
    //    if (playerMove != null)
    //    {
    //        playerMove.SetSkillVisual();
    //    }
    //    var stats = ctx.owner.GetComponent<PlayerStatsMono>();
    //    if (stats == null) yield break;
    //    var anim = ctx.owner.transform.Find("Visual_Skill")?.GetComponent<Animator>();
    //    if (anim != null)
    //    {
    //        anim.SetBool("E_WB", true);
    //    }

    //    Vector3 spawnPos = ctx.owner.position;

    //    // Spawn vòng lửa KHÔNG parent
    //    GameObject aura = null;
    //    if (auraVFXPrefab != null)
    //    {
    //        aura = Instantiate(auraVFXPrefab, spawnPos, Quaternion.identity);
    //    }

    //    float timer = 0f;
    //    bool buffApplied = false;
    //    float bonusAtk = stats.atk * attackPercentBonus;


    //    while (timer < duration)
    //    {
    //        // ===== DAMAGE LUÔN CHẠY TẠI spawnPos =====
    //        DoAuraDamage(spawnPos, stats);

    //        // ===== BUFF & HEAL chỉ khi player đứng trong vùng =====
    //        float distance = Vector3.Distance(ctx.owner.position, spawnPos);

    //        if (distance <= auraRadius)
    //        {
    //            if (!buffApplied)
    //            {
    //                stats.atk += bonusAtk;
    //                buffApplied = true;
    //            }

    //            int healAmount = Mathf.RoundToInt(stats.maxHP * healPercentPerSecond);
    //            stats.hp = Mathf.Clamp(stats.hp + healAmount, 0, stats.maxHP);

    //            if (healVFXPrefab != null)
    //            {
    //                GameObject healVfx = Instantiate(healVFXPrefab, ctx.owner);
    //                healVfx.transform.localPosition = Vector3.zero;
    //                Destroy(healVfx, 0.5f);
    //            }

    //        }
    //        else
    //        {
    //            if (buffApplied)
    //            {
    //                stats.atk -= bonusAtk;
    //                buffApplied = false;
    //            }
    //        }

    //        yield return new WaitForSeconds(1f);
    //        timer += 1f;
    //    }
    //    if (playerMove != null)
    //    {
    //        playerMove.SetNormalVisual();
    //    }
    //    // Hết thời gian → remove buff nếu còn
    //    if (buffApplied)
    //        stats.atk -= bonusAtk;

    //    if (aura != null)
    //        Destroy(aura);
    //}
    IEnumerator AuraRoutine(SkillContext ctx)
    {
        var playerMove = ctx.owner.GetComponent<PlayerMove2D>();
        var stats = ctx.owner.GetComponent<PlayerStatsMono>();
       
        if (stats == null) yield break;

        var anim = ctx.owner.transform.Find("Visual_Skill")?.GetComponent<Animator>();

        // ===== BẬT VISUAL SKILL =====
        if (playerMove != null)
            playerMove.SetSkillVisual();

        if (anim != null)
            anim.SetBool("E_WB", true);

        // ===== ĐỢI animation đánh xong (VD 0.8s) =====
        yield return new WaitForSeconds(0.8f);

        // ===== TRẢ VỀ VISUAL THƯỜNG NGAY =====
        if (anim != null)
            anim.SetBool("E_WB", false);

        if (playerMove != null)
            playerMove.SetNormalVisual();


        // ===== SPAWN AURA =====
        Vector3 spawnPos = ctx.owner.position;

        GameObject aura = null;
        if (auraVFXPrefab != null)
            aura = Instantiate(auraVFXPrefab, spawnPos, Quaternion.identity);

        float timer = 0f;
        bool buffApplied = false;
        float bonusAtk = stats.atk * attackPercentBonus;

        while (timer < duration)
        {
            DoAuraDamage(spawnPos, stats);

            float distance = Vector3.Distance(ctx.owner.position, spawnPos);

            //if (distance <= auraRadius)
            //{
            //    if (!buffApplied)
            //    {
            //        stats.atk += bonusAtk;
            //        buffApplied = true;
            //    }

            //    int healAmount = Mathf.RoundToInt(stats.maxHP * healPercentPerSecond);
            //    //stats.hp = Mathf.Clamp(stats.hp + healAmount, 0, stats.maxHP);
            //    var playerHp = ctx.owner.GetComponent<PlayerHealth>();
            //    if (playerHp != null)
            //    {
            //        playerHp.Heal(healAmount);
            //    }
            //    if (healVFXPrefab != null)
            //    {
            //        GameObject healVfx = Instantiate(healVFXPrefab, ctx.owner);
            //        healVfx.transform.localPosition = Vector3.zero;
            //        Destroy(healVfx, 0.5f);
            //    }

            //}

            //else
            //{
            //    if (buffApplied)
            //    {
            //        stats.atk -= bonusAtk;
            //        buffApplied = false;
            //    }
            //}
            if (distance <= auraRadius)
            {
                if (!buffApplied)
                {
                    stats.atk += bonusAtk;
                    buffApplied = true;
                }

                int healAmount = Mathf.RoundToInt(stats.maxHP * healPercentPerSecond);

                var playerHp = ctx.owner.GetComponent<PlayerHealth>();
                if (playerHp != null)
                {
                    playerHp.Heal(healAmount);
                }

                // ✅ CHỈ SPAWN 1 LẦN
                if (healVfxInstance == null && healVFXPrefab != null)
                {
                    healVfxInstance = Instantiate(healVFXPrefab, ctx.owner);
                    healVfxInstance.transform.localPosition = Vector3.zero;
                }
            }
            else
            {
                if (buffApplied)
                {
                    stats.atk -= bonusAtk;
                    buffApplied = false;
                }

                // ❌ RA KHỎI VÙNG → TẮT HEAL VFX
                if (healVfxInstance != null)
                {
                    Destroy(healVfxInstance);
                    healVfxInstance = null;
                }
            }

            yield return new WaitForSeconds(1f);
            timer += 1f;
        }

        // Hết thời gian → remove buff nếu còn
        if (buffApplied)
            stats.atk -= bonusAtk;
        if (healVfxInstance != null)
        {
            Destroy(healVfxInstance);
            healVfxInstance = null;
        }
        if (aura != null)
            Destroy(aura);
    }
    private void OnEnable()
    {
        // cooldown = skillCooldown; // sync cooldown với WeaponSkill
        cooldown = 32f;
    }
    void DoAuraDamage(Vector2 center, PlayerStatsMono attackerStats)
    {
        var hits = Physics2D.OverlapCircleAll(center, auraRadius, hitMask);

        foreach (var h in hits)
        {
            if (!h) continue;

            var targetHp = h.GetComponentInParent<BossHealth>();
            if (targetHp == null) continue;

            var targetStats = targetHp.GetComponent<PlayerStatsMono>();

            int finalDamage = baseDamagePerTick;

            if (targetStats != null)
            {
                bool isCrit;
                finalDamage = DamageCore.Compute(
                    attackerStats,
                    targetStats,
                    baseDamagePerTick,
                    atkScale,
                    smptScale,
                    damageType,
                    canCrit,
                    out isCrit
                );

                Vector3 popupPos = targetHp.head != null
                    ? targetHp.head.position
                    : targetHp.transform.position;

                DamagePopupSpawner.I?.Show(popupPos, finalDamage, isCrit);
            }

            targetHp.TakeDamage(finalDamage);
        }
    }
}