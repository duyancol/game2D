using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthCrackArea : MonoBehaviour
{
    GameObject owner;
    PlayerStatsMono attackerStats;

    int baseDamage;
    float atkScale;
    float smptScale;
    DamageType damageType;
    bool canCrit;

    float duration;
    float tickInterval;
    float radius;
    LayerMask hitMask;
    // ===== SLOW =====
    float slowPercent = 0f;
    float slowDuration = 0f;

    public GameObject tickVfx;
    public void ApplySlow(float percent, float duration)
    {
        slowPercent = Mathf.Max(slowPercent, percent);
        slowDuration = Mathf.Max(slowDuration, duration);
    }

    public void Init(
        GameObject owner,
        PlayerStatsMono stats,
        int damage,
        float atkScale,
        float smptScale,
        DamageType damageType,
        bool canCrit,
        float duration,
        float tickInterval,
        float radius,
        LayerMask hitMask
    )
    {
        this.owner = owner;
        this.attackerStats = stats;
        this.baseDamage = damage;
        this.atkScale = atkScale;
        this.smptScale = smptScale;
        this.damageType = damageType;
        this.canCrit = canCrit;

        this.duration = duration;
        this.tickInterval = tickInterval;
        this.radius = radius;
        this.hitMask = hitMask;

        StartCoroutine(TickRoutine());
    }

    IEnumerator TickRoutine()
    {
        float time = 0f;

        while (time < duration)
        {
            DealDamage();
            yield return new WaitForSeconds(tickInterval);
            time += tickInterval;
        }

        Destroy(gameObject);
    }

    void DealDamage()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, radius, hitMask);

        var damaged = new HashSet<BossHealth>(); // tránh dính nhiều collider

        int useBase = baseDamage;

        foreach (var col in hits)
        {
            if (!col) continue;

            // ignore self
            if (owner != null && col.transform.IsChildOf(owner.transform)) continue;

            var bossHp = col.GetComponentInParent<BossHealth>();
            if (bossHp == null) continue;

            if (!damaged.Add(bossHp)) continue;

            var targetStats = bossHp.GetComponent<PlayerStatsMono>();

            if (targetStats == null)
            {
                bossHp.TakeDamage(useBase);
                continue;
            }

            bool isCrit;
            int finalDamage = DamageCore.Compute(
                attackerStats,
                targetStats,
                useBase,
                atkScale,
                smptScale,
                damageType,
                canCrit,
                out isCrit
            );

            Vector3 popupPos = bossHp.head != null
                ? bossHp.head.position
                : bossHp.transform.position;

            DamagePopupSpawner.I?.Show(popupPos, finalDamage, isCrit);

            bossHp.TakeDamage(finalDamage);

            if (tickVfx)
                Instantiate(tickVfx, popupPos, Quaternion.identity);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}