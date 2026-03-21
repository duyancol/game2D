using UnityEngine;

public class IceTornadoProjectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 8f;
    public float rotateSpeed = 360f;
    public float lifetime = 3f;

    [Header("Hit")]
    public float radius = 0.8f;
    public LayerMask hitMask;
    float damageMultiplier = 1f;
    [Header("Scaling (giống ProjectileDamage)")]
    public PlayerStatsMono attackerStats;
    public int baseDamage = 10;
    public float atkScale = 0f;
    public float smptScale = 0f;
    public DamageType damageType = DamageType.Magic;
    public bool canCrit = true;
    public GameObject owner;
    [Header("VFX")]
    public GameObject hitVfxPrefab;
    float lifeTimer;
    float tickTimer;
    public float damageTick = 0.3f;
    Vector2 _dir;

    public void Init(
        Vector2 direction,
        PlayerStatsMono attacker,
        int skillBaseDamage,
        float atkScale,
        float smptScale,
        DamageType type,
        bool canCrit,
        GameObject ownerGo = null)
    {
        _dir = direction.normalized;
        attackerStats = attacker;
        baseDamage = skillBaseDamage;
        this.atkScale = atkScale;
        this.smptScale = smptScale;
        damageType = type;
        this.canCrit = canCrit;
        owner = ownerGo;
    }
    public void SetDamageMultiplier(float value)
    {
        damageMultiplier = value;
    }
    void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        transform.position += (Vector3)(_dir * speed * Time.deltaTime);
        //transform.Rotate(0, rotateSpeed * Time.deltaTime,0 );

        tickTimer += Time.deltaTime;
        if (tickTimer >= damageTick)
        {
            tickTimer = 0f;
            DealDamage();
        }
    }

    void DealDamage()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, radius, hitMask);

        foreach (var h in hits)
        {
            if (!h) continue;
            if (owner != null && h.transform.IsChildOf(owner.transform))
                continue;

            var bossHp = h.GetComponentInParent<BossHealth>();
            if (bossHp == null) continue;

            var targetStats = bossHp.GetComponent<PlayerStatsMono>();

            int finalDamage;
            bool isCrit = false;

            if (targetStats != null)
            {
                finalDamage = DamageCore.Compute(
     attackerStats,
     targetStats,
     baseDamage,
     atkScale,
     smptScale,
     damageType,
     canCrit,
     out isCrit
 );

                // 🔥 Nhân multiplier ở đây
                finalDamage = Mathf.RoundToInt(finalDamage * damageMultiplier);
            }
            else
            {
                finalDamage = baseDamage; // fallback
            }

            // Popup
            Vector3 popupPos = bossHp.head != null
                ? bossHp.head.position
                : bossHp.transform.position;

            DamagePopupSpawner.I?.Show(popupPos, finalDamage, isCrit);

            // Trừ máu
            bossHp.TakeDamage(finalDamage);
            if (hitVfxPrefab != null)
            {
                Vector3 hitPos = bossHp.head != null
                    ? bossHp.head.position
                    : bossHp.transform.position;

                GameObject vfx = Instantiate(hitVfxPrefab, hitPos, Quaternion.identity);
                Destroy(vfx, 1f);
            }
            // ❄️ Absolute Zero – chỉ stack khi Ice Tornado crit phép
            if (isCrit && damageType == DamageType.Magic)
            {
                var absoluteZero = owner != null
                    ? owner.GetComponent<AbsoluteZeroFlag>()
                    : null;

                if (absoluteZero != null && absoluteZero.passive != null)
                {
                    absoluteZero.passive.OnMagicCrit();
                }
            }
            // Passive nếu có
            var markPassive = owner != null
                ? owner.GetComponent<DivineMarkRuntime>()
                : null;

            if (markPassive != null)
                markPassive.OnHit(bossHp.transform.position, bossHp);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}