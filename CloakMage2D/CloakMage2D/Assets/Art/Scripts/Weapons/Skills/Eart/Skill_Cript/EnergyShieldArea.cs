using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyShieldArea : MonoBehaviour
{
    GameObject owner;
    PlayerStatsMono attackerStats;

    float currentShield;
    float maxShield;

    float duration;

    int baseDamage;
    float atkScale;
    float smptScale;
    DamageType damageType;
    bool canCrit;

    float radius;
    LayerMask hitMask;

    public GameObject explodeVfx;

    bool isActive = false;
    public bool IsActive => isActive;
    // ===== INIT =====
    public void Init(
        GameObject owner,
        PlayerStatsMono stats,
        float shieldAmount,
        float duration,
        int baseDamage,
        float atkScale,
        float smptScale,
        DamageType damageType,
        bool canCrit,
        float radius,
        LayerMask hitMask
    )
    {
        this.owner = owner;
        this.attackerStats = stats;

        this.maxShield = shieldAmount;
        this.currentShield = shieldAmount;

        this.duration = duration;

        this.baseDamage = baseDamage;
        this.atkScale = atkScale;
        this.smptScale = smptScale;
        this.damageType = damageType;
        this.canCrit = canCrit;

        this.radius = radius;
        this.hitMask = hitMask;

        isActive = true;

        transform.SetParent(owner.transform);
        transform.localPosition = Vector3.zero;

        StartCoroutine(LifeRoutine());
    }

    // ===== LIFE =====
    IEnumerator LifeRoutine()
    {
        float time = 0f;

        while (time < duration)
        {
            if (currentShield <= 0)
            {
                isActive = false;
                Destroy(gameObject); // <-- destroy ngay khi shield hết
                yield break;
            }

            time += Time.deltaTime;
            yield return null;
        }

        // ===== NỔ =====
        if (currentShield > 0)
        {
            Explode(currentShield);
        }

        Destroy(gameObject); // <-- destroy prefab sau khi nổ
    }

    // ===== ABSORB DAMAGE =====
    public int AbsorbDamage(int dmg)
    {
        if (!isActive) return dmg;

        float absorbed = Mathf.Min(currentShield, dmg);
        currentShield -= absorbed;

        int remain = dmg - Mathf.RoundToInt(absorbed);

        return remain;
    }

    // ===== EXPLODE =====
    void Explode(float dmgAmount)
    {
        if (explodeVfx)
            Instantiate(explodeVfx, transform.position, Quaternion.identity);

        var hits = Physics2D.OverlapCircleAll(transform.position, radius, hitMask);
        var damaged = new HashSet<BossHealth>();

        foreach (var col in hits)
        {
            if (!col) continue;

            if (owner != null && col.transform.IsChildOf(owner.transform)) continue;

            var boss = col.GetComponentInParent<BossHealth>();
            if (boss == null) continue;

            if (!damaged.Add(boss)) continue;

            var targetStats = boss.GetComponent<PlayerStatsMono>();

            bool isCrit;
            int finalDamage = DamageCore.Compute(
                attackerStats,
                targetStats,
                Mathf.RoundToInt(dmgAmount),
                atkScale,
                smptScale,
                damageType,
                canCrit,
                out isCrit
            );

            Vector3 popupPos = boss.transform.position;

            DamagePopupSpawner.I?.Show(popupPos, finalDamage, isCrit);
            boss.TakeDamage(finalDamage);
        }
    }
}