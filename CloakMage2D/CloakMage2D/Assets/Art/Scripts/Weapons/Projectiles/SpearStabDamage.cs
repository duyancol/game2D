using UnityEngine;

public class SpearStabDamage : MonoBehaviour
{
    [Header("Owner / Anti self-hit")]
    public GameObject owner;

    [Header("Damage config (set từ Skill)")]
    public PlayerStatsMono attackerStats;
    public int baseDamage = 10;
    public float atkScale = 0f;
    public float smptScale = 0f;
    public DamageType damageType = DamageType.Physical;
    public bool canCrit = true;

    [Header("Hit behavior")]
    [Tooltip("Đánh 1 lần rồi thôi (khuyên dùng cho mũi đâm)")]
    public bool hitOnce = true;

    [Tooltip("Sau khi trúng, KHÔNG destroy ngay. Chờ thêm X giây.")]
    public float destroyDelayAfterHit = 0.5f;

    bool _hasHit;
    bool _scheduledDestroy;
    public bool HasHit => _hasHit;

    public void InitScaled(
        PlayerStatsMono attacker,
        int skillBaseDamage,
        float atkScale,
        float smptScale,
        DamageType type,
        bool canCrit,
        GameObject ownerGo = null,
        float destroyDelay = 0.5f,
        bool hitOnce = true
    )
    {
        attackerStats = attacker;
        baseDamage = skillBaseDamage;
        this.atkScale = atkScale;
        this.smptScale = smptScale;
        damageType = type;
        this.canCrit = canCrit;
        owner = ownerGo;

        destroyDelayAfterHit = Mathf.Max(0f, destroyDelay);
        this.hitOnce = hitOnce;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // chống tự đâm trúng mình
        if (owner != null && other.transform.IsChildOf(owner.transform)) return;

        if (hitOnce && _hasHit) return;

        var bossHp = other.GetComponentInParent<BossHealth>();
        if (bossHp == null) return;

        var targetStats = bossHp.GetComponent<PlayerStatsMono>();

        bool isCrit = false;
        int finalDamage;

        if (targetStats == null)
        {
            finalDamage = Mathf.Max(1, baseDamage);
        }
        else
        {
            finalDamage = DamageCore.Compute(
                attackerStats,
                targetStats,
                Mathf.Max(1, baseDamage),
                atkScale,
                smptScale,
                damageType,
                canCrit,
                out isCrit
            );
        }

        Vector3 popupPos = bossHp.head != null ? bossHp.head.position : bossHp.transform.position;
        DamagePopupSpawner.I?.Show(popupPos, finalDamage, isCrit);

        bossHp.TakeDamage(finalDamage);

        _hasHit = true;

        // ✅ KHÔNG destroy ngay -> giữ mũi tồn tại cho “cảm giác đâm lâu”
        if (!_scheduledDestroy)
        {
            _scheduledDestroy = true;
            if (destroyDelayAfterHit <= 0f) Destroy(gameObject);
            else Destroy(gameObject, destroyDelayAfterHit);
        }
    }
}
