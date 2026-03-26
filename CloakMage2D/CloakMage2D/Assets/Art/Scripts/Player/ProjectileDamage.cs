
//using UnityEngine;

//public class ProjectileDamage : MonoBehaviour
//{
//    [Header("Fallback (nếu quên set)")]
//    public int damage = 10;              // base damage mặc định
//    public GameObject owner;             // tránh tự bắn trúng mình (optional)

//    [Header("Stats Scaling (được set từ Skill)")]
//    public PlayerStatsMono attackerStats;          // stats người bắn (Player)
//    public int baseDamage = 10;                   // damage riêng của skill
//    public float atkScale = 0f;                   // hệ số ăn theo ATK
//    public float smptScale = 0f;                  // hệ số ăn theo SMPT
//    public DamageType damageType = DamageType.Physical;
//    public bool canCrit = true;

//    // === API cũ: vẫn xài được như trước ===
//    public void Init(int dmg, GameObject ownerGo = null)
//    {
//        // giữ tương thích: Init(dmg) vẫn set baseDamage
//        damage = dmg;
//        baseDamage = dmg;
//        owner = ownerGo;
//    }

//    // === API mới: khuyên dùng ===
//    public void InitScaled(
//        PlayerStatsMono attacker,
//        int skillBaseDamage,
//        float atkScale,
//        float smptScale,
//        DamageType type,
//        bool canCrit,
//        GameObject ownerGo = null)
//    {
//        attackerStats = attacker;
//        baseDamage = skillBaseDamage;
//        damage = skillBaseDamage; // fallback
//        this.atkScale = atkScale;
//        this.smptScale = smptScale;
//        damageType = type;
//        this.canCrit = canCrit;
//        owner = ownerGo;
//    }

//    void OnTriggerEnter2D(Collider2D other)
//    {
//        // 1) chống tự bắn trúng mình
//        if (owner != null && other.transform.IsChildOf(owner.transform)) return;

//        // 2) tìm boss health (m đang dùng GetComponentInParent)
//        var bossHp = other.GetComponentInParent<BossHealth>();
//        if (bossHp == null) return;

//        // 3) lấy stats của mục tiêu để tính DEF/MDEF


//        var targetStats = bossHp.GetComponent<PlayerStatsMono>();
//        if (targetStats == null)
//        {
//            // Nếu boss chưa gắn stats, vẫn trừ theo baseDamage cho khỏi “không ăn damage”
//            bossHp.TakeDamage(baseDamage > 0 ? baseDamage : damage);
//            Destroy(gameObject);
//            return;
//        }

//        // 4) tính damage cuối
//        bool isCrit;
//        int finalDamage = DamageCore.Compute(
//            attackerStats,
//            targetStats,
//            (baseDamage > 0 ? baseDamage : damage),
//            atkScale,
//            smptScale,
//            damageType,
//            canCrit,
//            out isCrit
//        );

//        // 5) trừ máu (giữ đúng hàm của m)
//        Vector3 popupPos = bossHp.head != null
//     ? bossHp.head.position
//     : bossHp.transform.position;

//        DamagePopupSpawner.I?.Show(popupPos, finalDamage, isCrit);



//        bossHp.TakeDamage(finalDamage);
//        if (isCrit && damageType == DamageType.Magic)
//        {
//            var absoluteZero = owner != null
//                ? owner.GetComponent<AbsoluteZeroFlag>()
//                : null;

//            if (absoluteZero != null && absoluteZero.passive != null)
//            {
//                absoluteZero.passive.OnMagicCrit();
//            }
//        }
//        // === KÍCH HOẠT NỘI TẠI ===
//        var markPassive = owner != null
//            ? owner.GetComponent<DivineMarkRuntime>()
//            : null;

//        if (markPassive != null)
//        {
//            markPassive.OnHit(bossHp.transform.position, bossHp);
//        }

//        Destroy(gameObject);
//    }
//}
using UnityEngine;

public class ProjectileDamage : MonoBehaviour
{
    [Header("Fallback (nếu quên set)")]
    public int damage = 10;
    public GameObject owner;

    [Header("Stats Scaling")]
    public PlayerStatsMono attackerStats;
    public int baseDamage = 10;
    public float atkScale = 0f;
    public float smptScale = 0f;
    public DamageType damageType = DamageType.Physical;
    public bool canCrit = true;

    [Header("After Hit")]
    public bool destroyOnHit = false;      // ❗ tắt mặc định
    public float destroyDelay = 0.3f;      // thời gian cho VFX chạy xong

    private bool hasHit = false;

    // ================= INIT =================

    public void Init(int dmg, GameObject ownerGo = null)
    {
        damage = dmg;
        baseDamage = dmg;
        owner = ownerGo;
    }

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

    // ================= HIT =================

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return; // tránh hit nhiều lần
        if (owner != null && other.transform.IsChildOf(owner.transform)) return;

        var bossHp = other.GetComponentInParent<BossHealth>();
        if (bossHp == null) return;

        hasHit = true;

        var targetStats = bossHp.GetComponent<PlayerStatsMono>();

        int finalDamage;
        bool isCrit = false;

        if (targetStats == null)
        {
            finalDamage = baseDamage > 0 ? baseDamage : damage;
        }
        else
        {
            finalDamage = DamageCore.Compute(
                attackerStats,
                targetStats,
                (baseDamage > 0 ? baseDamage : damage),
                atkScale,
                smptScale,
                damageType,
                canCrit,
                out isCrit
            );
        }

        // popup
        Vector3 popupPos = bossHp.head != null
            ? bossHp.head.position
            : bossHp.transform.position;

        DamagePopupSpawner.I?.Show(popupPos, finalDamage, isCrit);

        bossHp.TakeDamage(finalDamage);

        // ===== PASSIVE =====
        if (isCrit && damageType == DamageType.Magic)
        {
            var absoluteZero = owner != null
                ? owner.GetComponent<AbsoluteZeroFlag>()
                : null;

            absoluteZero?.passive?.OnMagicCrit();
        }

        var markPassive = owner != null
            ? owner.GetComponent<DivineMarkRuntime>()
            : null;

        markPassive?.OnHit(bossHp.transform.position, bossHp);

        // =========================
        // ❗ QUAN TRỌNG: KHÔNG DESTROY NGAY
        // =========================

        DisableDamageOnly();

        if (destroyOnHit)
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    // ================= HELPER =================

    void DisableDamageOnly()
    {
        // tắt collider để không hit lại
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // tắt rigidbody nếu có
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // ❗ KHÔNG tắt renderer / particle → để VFX chạy tiếp
    }
}
