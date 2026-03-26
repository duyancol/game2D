
using UnityEngine;
using System.Collections.Generic;

public class SwordHitbox : MonoBehaviour
{
    [Header("Hitbox Shape")]
    public LayerMask hitMask;
    public float radius = 2.8f;

    [Header("Fallback (nếu quên set)")]
    public int damage = 10;
    public GameObject owner;

    [Header("Stats Scaling (được set từ Skill)")]
    public PlayerStatsMono attackerStats;
    public int baseDamage = 10;
    public float atkScale = 0f;
    public float smptScale = 0f;
    public DamageType damageType = DamageType.Physical;
    public bool canCrit = true;

    [Header("Life / Behavior")]
    [Tooltip("Sau khi Fire, chỉ giữ HITBOX thêm X giây rồi tắt (không huỷ VFX).")]
    public float disableHitboxAfterFire = 0.05f;

    [Tooltip("Nếu bật: sẽ huỷ luôn GameObject (cả VFX). TẮT nếu muốn skill control thời gian tồn tại.")]
    public bool destroyWholeObjectOnFire = false;

    HashSet<BossHealth> damaged = new();
    bool _fired;
    [Header("Direction")]
    public float offsetX = 1.5f; // khoảng cách từ player tới hitbox
    // === API cũ ===
    public void Init(int dmg, GameObject ownerGo = null)
    {
        damage = dmg;
        baseDamage = dmg;
        owner = ownerGo;
    }

    // === API mới ===
    public void InitScaled(
        PlayerStatsMono attacker,
        int skillBaseDamage,
        float atkScale,
        float smptScale,
        DamageType type,
        bool canCrit,
        GameObject ownerGo = null)
    {
        attackerStats = attacker;
        baseDamage = skillBaseDamage;
        damage = skillBaseDamage;
        this.atkScale = atkScale;
        this.smptScale = smptScale;
        damageType = type;
        this.canCrit = canCrit;
        owner = ownerGo;
    }

    // GỌI HÀM NÀY SAU KHI Init/InitScaled xong
    public void Fire()
    {
        if (_fired) return;
        _fired = true;

        DoDamageOnce();

        // ✅ QUAN TRỌNG: Đừng destroy cả prefab nếu muốn VFX sống theo stabLife/slamLife trong skill
        if (destroyWholeObjectOnFire)
        {
            Destroy(gameObject, Mathf.Max(0f, disableHitboxAfterFire));
        }
        else
        {
            // chỉ tắt hitbox để khỏi đánh lại lần nữa
            float t = Mathf.Max(0f, disableHitboxAfterFire);
            if (t <= 0f) DisableSelf();
            else Invoke(nameof(DisableSelf), t);
        }
    }

    void DisableSelf()
    {
        // cách 1: tắt component (nhẹ nhất)
        enabled = false;

        // nếu prefab có collider trigger dùng OnTrigger thì có thể tắt collider ở đây (của m đang OverlapCircle nên ko cần)
        // var col = GetComponent<Collider2D>(); if (col) col.enabled = false;
    }

    void DoDamageOnce()
    {
        Vector2 center = transform.position;

        if (owner != null)
        {
            float dir = Mathf.Sign(owner.transform.localScale.x);
            center = (Vector2)owner.transform.position + new Vector2(offsetX * dir, 0);
        }
        var hits = Physics2D.OverlapCircleAll(center, radius, hitMask);

        foreach (var h in hits)
        {
            if (!h) continue;

            if (owner != null && h.transform.IsChildOf(owner.transform))
                continue;

            var bossHp = h.GetComponentInParent<BossHealth>();
            if (bossHp == null) continue;

            if (!damaged.Add(bossHp)) continue;

            int useBase = (baseDamage > 0 ? baseDamage : damage);

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

            Vector3 popupPos = bossHp.head != null ? bossHp.head.position : bossHp.transform.position;
            DamagePopupSpawner.I?.Show(popupPos, finalDamage, isCrit);

            bossHp.TakeDamage(finalDamage);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
