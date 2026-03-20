
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class CubeMeteorProjectile : MonoBehaviour
{
    [Header("Move")]
    public float flyTime = 0.45f;

    [Tooltip("Bù góc cho sprite. Thử: 0, 90, -90, 180 cho đúng hướng.")]
    public float spriteAngleOffset = 0f;

    [Header("Owner (optional)")]
    public GameObject owner; // để ignore self

    [Header("VFX")]
    public GameObject impactVfx;
    public float impactVfxLife = 0.6f;

    public GameObject explosionVfx;
    public float explosionVfxLife = 0.9f;

    [Header("Damage Area")]
    public float damageRadius = 2.2f;
    public LayerMask hitMask;

    [Header("Fallback (nếu quên set)")]
    public int damage = 40; // base damage mặc định

    [Header("Stats Scaling (được set từ Skill)")]
    public PlayerStatsMono attackerStats; // stats người gọi skill
    public int baseDamage = 40;           // base damage riêng của skill
    public float atkScale = 0f;           // ăn theo ATK
    public float smptScale = 0f;          // ăn theo SMPT
    public DamageType damageType = DamageType.Physical;
    public bool canCrit = true;

    [Header("Life")]
    public float destroyAfter = 0.05f;

    Vector3 _targetPos;
    bool _launched;
    bool _exploded;

    // === API cũ (nếu bạn muốn dùng nhanh) ===
    public void Init(int dmg, GameObject ownerGo = null)
    {
        damage = dmg;
        baseDamage = dmg;
        owner = ownerGo;
    }

    // === API mới: giống ProjectileDamage ===
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
        damage = skillBaseDamage; // fallback
        this.atkScale = atkScale;
        this.smptScale = smptScale;
        damageType = type;
        this.canCrit = canCrit;
        owner = ownerGo;
    }

    public void LaunchTo(Vector3 targetPos)
    {
        if (_launched) return;
        _launched = true;

        _targetPos = targetPos;
        _targetPos.z = 0f;

        StartCoroutine(FlyRoutine());
    }

    IEnumerator FlyRoutine()
    {
        Vector3 startPos = transform.position;
        startPos.z = 0f;

        RotateTowards(_targetPos - startPos);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, flyTime);

            float eased = 1f - Mathf.Pow(1f - t, 2f);
            Vector3 newPos = Vector3.Lerp(startPos, _targetPos, eased);

            Vector3 vel = newPos - transform.position;
            if (vel.sqrMagnitude > 0.000001f)
                RotateTowards(vel);

            transform.position = newPos;
            yield return null;
        }

        transform.position = _targetPos;
        ExplodeOnce();
    }

    void RotateTowards(Vector3 dir)
    {
        dir.z = 0f;
        if (dir.sqrMagnitude < 0.000001f) return;

        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, ang + spriteAngleOffset);
    }

    void ExplodeOnce()
    {
        if (_exploded) return;
        _exploded = true;

        if (impactVfx)
        {
            var go = Instantiate(impactVfx, _targetPos, Quaternion.identity);
            Destroy(go, Mathf.Max(0.05f, impactVfxLife));
        }

        // ====== DAMAGE BOSS (scaled like normal) ======
        var hits = Physics2D.OverlapCircleAll(_targetPos, damageRadius, hitMask);
        var damaged = new HashSet<BossHealth>(); // tránh boss nhiều collider ăn nhiều lần

        int useBase = (baseDamage > 0 ? baseDamage : damage);

        for (int i = 0; i < hits.Length; i++)
        {
            var col = hits[i];
            if (!col) continue;

            // ignore self
            if (owner != null && col.transform.IsChildOf(owner.transform)) continue;

            var bossHp = col.GetComponentInParent<BossHealth>();
            if (bossHp == null) continue;

            if (!damaged.Add(bossHp)) continue;

            var targetStats = bossHp.GetComponent<PlayerStatsMono>();
            if (targetStats == null)
            {
                // boss chưa có stats => trừ thẳng base
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

        if (explosionVfx)
        {
            var go = Instantiate(explosionVfx, _targetPos, Quaternion.identity);
            Destroy(go, Mathf.Max(0.05f, explosionVfxLife));
        }

        Destroy(gameObject, Mathf.Max(0.01f, destroyAfter));
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? _targetPos : transform.position, damageRadius);
    }
#endif
}
