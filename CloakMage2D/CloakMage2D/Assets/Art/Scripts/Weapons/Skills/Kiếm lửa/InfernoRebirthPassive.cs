
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Game/Skills/Sword/Inferno Rebirth Passive")]
public class InfernoRebirthPassive : WeaponSkill
{
    [Header("Stat Bonus")]
    public float atkPercent = 0.60f;
    public float smptPercent = 0.10f;
    public float critPercent = 0.05f;

    [Header("Rebirth")]
    public float triggerHpPercent = 0.3f;     // 30%
    public float eggDuration = 5f;
    public float reviveHpPercent = 0.3f;

    public GameObject eggVFXPrefab;
    [Header("Cooldown")]
   // public float cooldown = 60f;
    float bonusAtk;
    float bonusSmpt;
    float bonusCrit;
    private float nextTime;
    bool isRebirthing = false;
    bool hasTriggered = false;
    bool buffApplied = false;
    UnityAction<int, int> hpListener;
    bool applied = false;
    protected override void OnUse(SkillContext ctx) { }
    public bool CanUse() => Time.time >= nextTime;
    // =========================
    // EQUIP
    // =========================
    //public override void OnEquip(SkillContext ctx)
    //{
    //    var stats = ctx.owner.GetComponent<PlayerStatsMono>();
    //    var health = ctx.owner.GetComponent<PlayerHealth>();

    //    if (stats == null || health == null) return;

    //    // ===== APPLY STAT BONUS =====
    //    bonusAtk = stats.atk * atkPercent;
    //    bonusSmpt = stats.smpt * smptPercent;
    //    bonusCrit = critPercent;
    //    Debug.Log("Inferno Passive ATK befor ;" + stats.atk);
    //    stats.passivePercentAtk += atkPercent;
    //    stats.passivePercentSmpt += smptPercent;
    //    stats.passiveFlatCritChance += critPercent;

    //    stats.RecalculateStats();
    //    Debug.Log("Inferno Passive ATK after ;" + stats.atk);
    //    Debug.Log("Inferno Passive: STAT APPLIED ;" + bonusAtk +"; " + bonusCrit + "; " + bonusSmpt);

    //    // ===== HP LISTENER =====
    //    hpListener = (current, max) =>
    //    {
    //        if (isRebirthing) return;

    //        float percent = (float)current / max;

    //        if (percent <= triggerHpPercent && !hasTriggered && CanUse())
    //        {
    //            hasTriggered = true;

    //            var runner = ctx.owner.GetComponent<SkillCoroutineRunner>();
    //            if (runner == null)
    //                runner = ctx.owner.gameObject.AddComponent<SkillCoroutineRunner>();

    //            runner.Run(RebirthRoutine(ctx));
    //        }

    //        if (percent > 0.4f)
    //        {
    //            hasTriggered = false;
    //        }
    //    };

    //    health.onHpChanged.AddListener(hpListener);
    //}
    public override void OnEquip(SkillContext ctx)
    {
        if (applied) return; // ❗ CHẶN APPLY NHIỀU LẦN
        applied = true;

        var stats = ctx.owner.GetComponent<PlayerStatsMono>();
        var health = ctx.owner.GetComponent<PlayerHealth>();

        if (stats == null || health == null) return;

        // ❗ CHỈ CỘNG % (KHÔNG TÍNH bonusAtk NỮA)
        Debug.Log("Inferno Passive ATK before: " + stats.atk);

        stats.passivePercentAtk += atkPercent;
        stats.passivePercentSmpt += smptPercent;
        stats.passiveFlatCritChance += critPercent;

        stats.RecalculateStats();

        Debug.Log("Inferno Passive ATK after: " + stats.atk);

        // ===== HP LISTENER =====
        hpListener = (current, max) =>
        {
            if (isRebirthing) return;

            float percent = (float)current / max;

            if (percent <= triggerHpPercent && !hasTriggered && CanUse())
            {
                hasTriggered = true;

                var runner = ctx.owner.GetComponent<SkillCoroutineRunner>();
                if (runner == null)
                    runner = ctx.owner.gameObject.AddComponent<SkillCoroutineRunner>();

                runner.Run(RebirthRoutine(ctx));
            }

            if (percent > 0.4f)
            {
                hasTriggered = false;
            }
        };

        health.onHpChanged.AddListener(hpListener);
    }
    // =========================
    // UNEQUIP
    // =========================
    //public override void OnUnequip(SkillContext ctx)
    //{
    //    var stats = ctx.owner.GetComponent<PlayerStatsMono>();
    //    var health = ctx.owner.GetComponent<PlayerHealth>();
    //    nextTime = 0f;
    //    if (stats != null)
    //    {
    //        stats.atk -= bonusAtk;
    //        stats.smpt -= bonusSmpt;
    //        stats.critChance -= bonusCrit;
    //    }

    //    if (health != null && hpListener != null)
    //    {
    //        health.onHpChanged.RemoveListener(hpListener);
    //    }

    //    isRebirthing = false;
    //    hasTriggered = false;
    //}
    public override void OnUnequip(SkillContext ctx)
    {
        if (!applied) return; // ❗ tránh trừ 2 lần

        var stats = ctx.owner.GetComponent<PlayerStatsMono>();
        var health = ctx.owner.GetComponent<PlayerHealth>();

        nextTime = 0f;

        if (stats != null)
        {
            // ❗ TRỪ ĐÚNG % (KHÔNG ĐỤNG stats.atk trực tiếp nữa)
            stats.passivePercentAtk -= atkPercent;
            stats.passivePercentSmpt -= smptPercent;
            stats.passiveFlatCritChance -= critPercent;

            stats.RecalculateStats();
        }

        if (health != null && hpListener != null)
        {
            health.onHpChanged.RemoveListener(hpListener);
        }

        isRebirthing = false;
        hasTriggered = false;
        applied = false;
    }
    // =========================
    // REBIRTH
    // =========================
    IEnumerator RebirthRoutine(SkillContext ctx)
    {
        isRebirthing = true;

        var stats = ctx.owner.GetComponent<PlayerStatsMono>();
        var health = ctx.owner.GetComponent<PlayerHealth>();
        var weaponController = ctx.owner.GetComponent<WeaponController>();
        var rb = ctx.owner.GetComponent<Rigidbody2D>();

        Debug.Log("🔥 INFERNO REBIRTH ACTIVATED");
        nextTime = Time.time + cooldown;

        // ===== NGĂN CHẾT =====
        stats.hp = 1;
        health.onHpChanged?.Invoke(stats.hp, stats.maxHP);

        // ===== SPAWN TRỨNG =====
        GameObject egg = null;
        if (eggVFXPrefab != null)
            egg = Instantiate(eggVFXPrefab, ctx.owner.position, Quaternion.identity);

        // ===== ẨN VISUAL =====
        Transform visual = ctx.owner.transform.Find("Visual");
        if (visual != null)
            visual.gameObject.SetActive(false);

        // ===== DISABLE CONTROL =====
        if (weaponController != null)
            weaponController.enabled = false;

        // ===== DISABLE COLLIDER =====
        var colliders = ctx.owner.GetComponentsInChildren<Collider2D>(true);
        foreach (var c in colliders)
            c.enabled = false;

        // ===== FREEZE PHYSICS =====
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        yield return new WaitForSeconds(eggDuration);

        // ===== REVIVE =====
        stats.hp = Mathf.RoundToInt(stats.maxHP * reviveHpPercent);
        health.onHpChanged?.Invoke(stats.hp, stats.maxHP);

        // ===== BẬT LẠI VISUAL =====
        if (visual != null)
            visual.gameObject.SetActive(true);

        if (weaponController != null)
            weaponController.enabled = true;

        foreach (var c in colliders)
            c.enabled = true;

        if (rb != null)
            rb.simulated = true;

        if (egg != null)
            Destroy(egg);

        isRebirthing = false;

        Debug.Log("🔥 INFERNO REBIRTH COMPLETE");
    }
}