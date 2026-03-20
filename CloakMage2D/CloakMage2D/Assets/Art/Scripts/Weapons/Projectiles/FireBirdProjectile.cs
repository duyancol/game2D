
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBirdProjectile : MonoBehaviour
{
    [Header("Move")]
    public float flyTime = 0.45f;
    public bool rotateToVelocity = true;
    public float spriteAngleOffset = 0f;
    SpriteRenderer _sr;

    [Header("Sprite Facing Fix")]
    public bool useFlipXInsteadOf180 = true;   // bật để khỏi bị ảnh ngược khi bay trái
    public bool spriteFacesRight = true;       // nếu ảnh gốc nhìn sang phải

    [Header("Owner (optional)")]
    public GameObject owner;

    [Header("Impact")]
    public GameObject impactVfx;
    public float impactLife = 0.8f;

    [Header("AOE")]
    public float radius = 2.2f;
    public LayerMask hitMask;

    [Header("Fallback (nếu quên set)")]
    public int damage = 40;

    [Header("Stats Scaling (được set từ Skill)")]
    public PlayerStatsMono attackerStats;
    public int baseDamage = 40;
    public float atkScale = 0f;
    public float smptScale = 0f;
    public DamageType damageType = DamageType.Physical;
    public bool canCrit = true;

    Vector3 _target;
    bool _launched;
    Coroutine _co;
    bool _impacted;
    Collider2D _col;



    public void Init(int dmg, LayerMask mask, GameObject ownerGo = null)
    {
        damage = dmg;
        baseDamage = dmg;
        hitMask = mask;
        owner = ownerGo;
    }

    public void InitScaled(
        PlayerStatsMono attacker,
        int skillBaseDamage,
        float atkScale,
        float smptScale,
        DamageType type,
        bool canCrit,
        LayerMask mask,
        GameObject ownerGo = null)
    {
        attackerStats = attacker;
        baseDamage = skillBaseDamage;
        damage = skillBaseDamage;
        this.atkScale = atkScale;
        this.smptScale = smptScale;
        damageType = type;
        this.canCrit = canCrit;

        hitMask = mask;
        owner = ownerGo;
    }

    // ===== (GIỮ CŨ) bay thẳng =====
    public void LaunchTo(Vector3 target)
    {
        if (_launched) return;
        _launched = true;

        target.z = 0f;
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(FlyLinearRoutine(target, Mathf.Max(0.01f, flyTime), impactAtEnd: true, destroyAfterImpact: true));
    }

    // ===== NEW: bay CONG (Bezier) =====
    // arcHeight > 0 => cong lên
    public void LaunchArcTo(Vector3 target, float arcHeight)
    {
        if (_launched) return;
        _launched = true;

        target.z = 0f;
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(FlyArcRoutine(target, Mathf.Max(0.01f, flyTime), arcHeight, impactAtEnd: true, destroyAfterImpact: true));
    }

    // ===== NEW: 2 chặng cong: ra trước rồi cong lên trời =====
    public void LaunchArcTwoStep(
        Vector3 firstPoint, float firstTime, float arcHeight1,
        Vector3 secondPoint, float secondTime, float arcHeight2,
        bool impactAtFirstPoint = true)
    {
        if (_launched) return;
        _launched = true;

        firstPoint.z = 0f;
        secondPoint.z = 0f;

        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(ArcTwoStepRoutine(firstPoint, Mathf.Max(0.01f, firstTime), arcHeight1,
                                              secondPoint, Mathf.Max(0.01f, secondTime), arcHeight2,
                                              impactAtFirstPoint));
    }
    public void LaunchLateRise(Vector3 endPoint, float time, float riseHeight, float risePower, bool destroyAtEnd = true)
    {
        if (_launched) return;
        _launched = true;

        endPoint.z = 0f;

        StopAllCoroutines();
        StartCoroutine(LateRiseRoutine(endPoint, Mathf.Max(0.01f, time), riseHeight, Mathf.Max(1.01f, risePower), destroyAtEnd));
    }

    IEnumerator LateRiseRoutine(Vector3 endPoint, float time, float riseHeight, float risePower, bool destroyAtEnd)
    {
        Vector3 start = transform.position; start.z = 0f;
        Vector3 end = endPoint; end.z = 0f;
        _target = endPoint;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / time;
            float tt = Mathf.Clamp01(t);

            // base: đi thẳng từ start -> end
            Vector3 p = Vector3.Lerp(start, end, tt);

            // cong lên: đầu phẳng, cuối vút lên (giống hình)
            float yAdd = riseHeight * Mathf.Pow(tt, risePower);
            p.y += yAdd;

            RotateByVelocity(p);

            transform.position = p;
            yield return null;
        }

        if (destroyAtEnd) Destroy(gameObject);


    }

    IEnumerator ArcTwoStepRoutine(Vector3 p1, float t1, float h1, Vector3 p2, float t2, float h2, bool impactAtP1)
    {
        yield return FlyArcRoutine(p1, t1, h1, impactAtEnd: impactAtP1, destroyAfterImpact: false);
        yield return FlyArcRoutine(p2, t2, h2, impactAtEnd: false, destroyAfterImpact: false);
        Destroy(gameObject);
    }

    IEnumerator FlyLinearRoutine(Vector3 target, float time, bool impactAtEnd, bool destroyAfterImpact)
    {
        _target = target;
        Vector3 start = transform.position; start.z = 0f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / time;
            float tt = Mathf.Clamp01(t);

            Vector3 newPos = Vector3.Lerp(start, _target, tt);
            RotateByVelocity(newPos);

            transform.position = newPos;
            yield return null;
        }

        transform.position = _target;
        if (impactAtEnd) ImpactAt(_target, destroyAfterImpact);
    }

    IEnumerator FlyArcRoutine(Vector3 target, float time, float arcHeight, bool impactAtEnd, bool destroyAfterImpact)
    {
        _target = target;
        Vector3 start = transform.position; start.z = 0f;

        // Control point: nằm giữa và nâng lên
        Vector3 mid = (start + _target) * 0.5f;
        Vector3 control = mid + Vector3.up * arcHeight;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / time;
            float tt = Mathf.Clamp01(t);

            Vector3 newPos = Bezier2(start, control, _target, tt);
            RotateByVelocity(newPos);

            transform.position = newPos;
            yield return null;
        }

        transform.position = _target;
        if (impactAtEnd) ImpactAt(_target, destroyAfterImpact);
    }

    void RotateByVelocity(Vector3 newPos)
    {
        if (!rotateToVelocity) return;

        Vector2 v = (newPos - transform.position);
        if (v.sqrMagnitude <= 0.000001f) return;

        if (useFlipXInsteadOf180 && _sr != null)
        {
            // Lật ảnh theo chiều bay
            bool left = v.x < 0f;

            // Nếu sprite gốc nhìn sang phải => bay trái thì flipX = true
            // Nếu sprite gốc nhìn sang trái => đảo ngược
            _sr.flipX = spriteFacesRight ? left : !left;

            // Góc chỉ dựa trên abs(x) để không quay 180° (tránh “ảnh ngược”)
            float ang = Mathf.Atan2(v.y, Mathf.Abs(v.x)) * Mathf.Rad2Deg + spriteAngleOffset;
            transform.rotation = Quaternion.Euler(0, 0, ang);
        }
        else
        {
            // Cách cũ: quay full 360 theo velocity
            float ang = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg + spriteAngleOffset;
            transform.rotation = Quaternion.Euler(0, 0, ang);
        }
    }


    static Vector3 Bezier2(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        // (1-t)^2 a + 2(1-t)t b + t^2 c
        float u = 1f - t;
        return (u * u) * a + (2f * u * t) * b + (t * t) * c;
    }

    void ImpactAt(Vector3 point, bool destroyAfter)
    {
        if (impactVfx)
        {
            var v = Instantiate(impactVfx, point, Quaternion.identity);
            Destroy(v, Mathf.Max(0.05f, impactLife));
        }

        //if (hitMask.value != 0)
        {
            Debug.Log("Mask value: " + hitMask.value);
            var hits = Physics2D.OverlapCircleAll(point, radius, hitMask);
            HashSet<BossHealth> damaged = new HashSet<BossHealth>();

            int useBase = (baseDamage > 0 ? baseDamage : damage);

            foreach (var h in hits)
            {
                if (!h) continue;

                if (owner != null && h.transform.IsChildOf(owner.transform))
                    continue;

                var bossHp = h.GetComponentInParent<BossHealth>();
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

                Vector3 popupPos = bossHp.head != null ? bossHp.head.position : bossHp.transform.position;
                DamagePopupSpawner.I?.Show(popupPos, finalDamage, isCrit);

                bossHp.TakeDamage(finalDamage);
                //// ===== PASSIVE HOOK =====
                //var markPassive = owner != null
                //    ? owner.GetComponent<DivineMarkRuntime>()
                //    : null;

                //if (markPassive != null)
                //{
                //    markPassive.OnHit(bossHp.transform.position, bossHp);
                //}
            }
        }

        if (destroyAfter) Destroy(gameObject);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        TryImpact(other);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col != null) TryImpact(col.collider);
    }

    void TryImpact(Collider2D other)
    {
        if (_impacted) return;
        if (!other) return;

        // ignore owner
        if (owner != null && other.transform.IsChildOf(owner.transform))
            return;

        // chỉ nổ khi đụng BossHealth
        var bossHp = other.GetComponentInParent<BossHealth>();
        if (bossHp == null) return;

        _impacted = true;

        // nổ + damage tại vị trí chim, NHƯNG KHÔNG DESTROY chim
        Vector3 p = transform.position; p.z = 0f;
        ImpactAt(p, destroyAfter: false);

        // tắt collider để bay xuyên boss, khỏi trigger liên tục
        if (_col != null) _col.enabled = false;
    }

    void Awake()
    {
        _col = GetComponent<Collider2D>();
        _sr = GetComponentInChildren<SpriteRenderer>();
    }


#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Application.isPlaying ? _target : transform.position, radius);
    }
#endif
}
