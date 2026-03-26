using System.Collections;
using UnityEngine;

public class WindSpearProjectile : MonoBehaviour
{
    public float flyTime = 0.4f;

    [Header("Vortex")]
    public float vortexDuration;
    public float vortexRadius;
    public float pullForce;
    public LayerMask hitMask;
    public GameObject vortexVfx;

    [Header("Damage")]
    public PlayerStatsMono attackerStats;
    public int baseDamage;
    public float atkScale;
    public float smptScale;
    public DamageType damageType;
    public bool canCrit;
    [Header("Vortex Damage")]
    public float tickInterval = 0.8f;
    public float tickDamageMultiplier = 0.2f;
    public GameObject owner;

    Vector3 targetPos;

    public void InitScaled(
        PlayerStatsMono attacker,
        int baseDamage,
        float atkScale,
        float smptScale,
        DamageType type,
        bool canCrit,
        GameObject ownerGo
    )
    {
        attackerStats = attacker;
        this.baseDamage = baseDamage;
        this.atkScale = atkScale;
        this.smptScale = smptScale;
        damageType = type;
        this.canCrit = canCrit;
        owner = ownerGo;
    }

    public void LaunchTo(Vector3 target)
    {
        targetPos = target;
        StartCoroutine(Fly());
    }

    IEnumerator Fly()
    {
        Vector3 start = transform.position;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / flyTime;
            transform.position = Vector3.Lerp(start, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;

        StartCoroutine(Vortex());
    }

    IEnumerator Vortex()
    {
        if (vortexVfx)
        {
            var vfx = Instantiate(vortexVfx, targetPos, Quaternion.identity);
            Destroy(vfx, vortexDuration);
        }

        float t = 0f;

        float tickTimer = 0f;

        while (t < vortexDuration)
        {
            float dt = Time.deltaTime;
            t += dt;
            tickTimer += dt;

            var hits = Physics2D.OverlapCircleAll(targetPos, vortexRadius, hitMask);

            foreach (var col in hits)
            {
                // ===== HÚT =====
                var rb = col.attachedRigidbody;
                if (rb != null)
                {
                    Vector2 dir = (targetPos - col.transform.position).normalized;
                    rb.AddForce(dir * pullForce);
                }

                // ===== DAMAGE THEO TICK =====
                if (tickTimer >= tickInterval)
                {
                    var boss = col.GetComponentInParent<BossHealth>();
                    if (boss == null) continue;
                    
                    bool isCrit;
                    int finalDamage = DamageCore.Compute(
                        attackerStats,
                        boss.GetComponent<PlayerStatsMono>(),
                        Mathf.RoundToInt(baseDamage * tickDamageMultiplier), // 👈 damage nhỏ hơn
                        atkScale/4,
                        smptScale,
                        damageType,
                        canCrit,
                        out isCrit
                    );

                    boss.TakeDamage(finalDamage);
                    DamagePopupSpawner.I?.Show(boss.transform.position, finalDamage, isCrit);
                }
            }

            // reset timer sau khi tick
            if (tickTimer >= tickInterval)
                tickTimer = 0f;

            yield return null;
        }

        Destroy(gameObject);
    }
}