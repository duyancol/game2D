
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossChaseStab : MonoBehaviour
{
    [Header("Refs")]
    public Transform target;                 // Player
    public Transform flipRoot;               // ROOT boss/body để flip (Transform thường)
    public Transform weaponRoot;             // object sẽ đâm (Arm_R / Weapons / kiếm)
    public Transform stabSpawnPoint;         // điểm spawn VFX (WeaponPoint / tip)
    public Transform centerPoint;            // đặt ở thân boss (optional). Nếu null dùng rb.worldCenterOfMass

    [Header("Optional: Idle/Patrol")]
    public BossMoveAndIdle2D idleMove2D;     // kéo component BossMoveAndIdle2D vào đây (nếu null thì tự GetComponent)

    [Header("Move")]
    public bool lockPushedByPlayer = true;
    public float moveSpeed = 2.0f;

    [Header("Detect / Attack")]
    public float aggroRangeX = 12f;
    public float aggroRangeY = 3f;
    public float attackRange = 1.6f;
    public float attackCooldown = 1.0f;
    public float loseAggroDelay = 0.25f;

    [Header("Crowd (anti-stack)")]
    public LayerMask bossMask;                       // tick layer Boss
    public float separationRadius = 0.6f;
    public float separationStrength = 2.0f;
    public Vector2 stopDistanceRange = new Vector2(1.2f, 2.0f); // mỗi con dừng lệch nhau
    float stopDistance;

    [Header("Flip")]
    public bool faceTarget = true;
    public int facingSign = 1;

    [Header("Stab Motion (thao tác đâm)")]
    public float stabForward = 0.45f;
    public float stabUp = 0.05f;
    public float stabOutTime = 0.06f;
    public float stabBackTime = 0.10f;
    public AnimationCurve stabCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Hit")]
    public int damage = 20;
    public float hitRadius = 0.9f;

    [Tooltip("Nếu có stabSpawnPoint thì ưu tiên hit theo đó. Nếu không có thì dùng offset này theo thân.")]
    public Vector2 hitOffset = new Vector2(1.1f, 0.1f);

    public LayerMask hitMask;

    [Header("VFX")]
    public GameObject stabVfxPrefab;
    public float vfxLife = 0.6f;
    public Vector3 vfxOffset = new Vector3(0.2f, 0.05f, 0f);
    public float vfxAngleOffset = 0f;

    BossStun stun;
    Rigidbody2D rb;

    float nextAttackTime;
    bool isAttacking;

    Vector3 weaponStartLocalPos;
    Quaternion weaponStartLocalRot;

    bool isAggro;
    float loseAggroAt;

    Vector2 Origin2D()
    {
        if (centerPoint) return centerPoint.position;
        if (rb) return rb.worldCenterOfMass;
        return transform.position;
    }

    int FacingDirFromFlip()
    {
        if (!flipRoot) return 1;
        return (flipRoot.lossyScale.x < 0f) ? -1 : 1;
    }

    void Awake()
    {
        stun = GetComponent<BossStun>();
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        if (!flipRoot) flipRoot = transform;
        if (!idleMove2D) idleMove2D = GetComponent<BossMoveAndIdle2D>();

        if (lockPushedByPlayer)
        {
            // tuỳ game bạn, để Dynamic vẫn OK vì có separation + collider
            // rb.bodyType = RigidbodyType2D.Dynamic;
        }

        CacheWeapon();

        stopDistance = Random.Range(stopDistanceRange.x, stopDistanceRange.y);
        nextAttackTime = Time.time + Random.Range(0f, 0.4f); // lệch nhịp, khỏi đồng loạt
    }

    void OnValidate()
    {
        if (attackRange > 8f)
            Debug.LogWarning($"[{name}] BossChaseStab: AttackRange đang khá lớn ({attackRange}). Melee nên ~1.2-2.2 để boss còn chạy lại.");
    }

    void CacheWeapon()
    {
        if (weaponRoot)
        {
            weaponStartLocalPos = weaponRoot.localPosition;
            weaponStartLocalRot = weaponRoot.localRotation;
        }
    }

    bool InSightNow()
    {
        if (!target) return false;
        Vector2 o = Origin2D();
        float dx = Mathf.Abs(target.position.x - o.x);
        float dy = Mathf.Abs(target.position.y - o.y);
        return dx <= aggroRangeX && dy <= aggroRangeY;
    }

    int DirToTarget()
    {
        float ox = Origin2D().x;
        return (target.position.x >= ox) ? 1 : -1;
    }

    void SetIdleAllowed(bool allowed)
    {
        if (idleMove2D != null)
            idleMove2D.allowIdleMove = allowed;
    }

    void Update()
    {
        if (!target) return;
        if (stun != null && stun.IsStunned) return;

        // Aggro with small delay when losing
        bool seen = InSightNow();
        if (seen)
        {
            isAggro = true;
            loseAggroAt = Time.time + loseAggroDelay;
        }
        else
        {
            if (isAggro && Time.time >= loseAggroAt)
                isAggro = false;
        }

        SetIdleAllowed(!isAggro);
        if (!isAggro) return;
        if (isAttacking) return;

        int dirToTarget = DirToTarget();
        if (faceTarget) ApplyFlip(dirToTarget);

        Vector2 o = Origin2D();
        float dx = Mathf.Abs(target.position.x - o.x);

        // Mỗi con có stopDistance riêng -> mấy con sau cũng vào được vùng đánh của nó
        float atkRange = Mathf.Min(Mathf.Max(attackRange, stopDistance + 0.05f), 4f);

        if (dx <= atkRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;

            if (idleMove2D != null) idleMove2D.TriggerAttack();

            // dùng hướng theo flip (ổn khi flip bằng scale)
            int atkDir = FacingDirFromFlip();
            StartCoroutine(StabRoutine(atkDir));
        }
    }

    void FixedUpdate()
    {
        if (!target) return;
        if (stun != null && stun.IsStunned) return;
        if (!isAggro) return;
        if (isAttacking) return;

        Vector2 pos = rb.position;

        // 1) Separation: đẩy nhau ra để khỏi dồn cục
        if (bossMask.value != 0)
        {
            var near = Physics2D.OverlapCircleAll(pos, separationRadius, bossMask);
            Vector2 push = Vector2.zero;

            for (int i = 0; i < near.Length; i++)
            {
                var otherRb = near[i].attachedRigidbody;
                if (otherRb == null || otherRb == rb) continue;

                Vector2 away = pos - (Vector2)near[i].transform.position;
                float d = away.magnitude;
                if (d > 0.0001f) push += away / d;
            }

            if (push != Vector2.zero)
                pos += push.normalized * separationStrength * Time.fixedDeltaTime;
        }

        // 2) Chase theo X nhưng dừng theo stopDistance riêng
        float dxSigned = target.position.x - pos.x;
        float dx = Mathf.Abs(dxSigned);

        float stop = Mathf.Clamp(stopDistance, 0.8f, 4f);

        if (dx > stop)
        {
            int dir = (dxSigned >= 0) ? 1 : -1;
            float nextX = pos.x + dir * moveSpeed * Time.fixedDeltaTime;
            pos = new Vector2(nextX, pos.y);
        }

        rb.MovePosition(pos);
    }

    IEnumerator StabRoutine(int atkDir)
    {
        isAttacking = true;

        CacheWeapon();

        // VFX + damage ngay lúc đâm
        SpawnStabVfx(atkDir);
        DealDamage(atkDir);

        if (stun != null && stun.IsStunned) { isAttacking = false; yield break; }

        if (weaponRoot)
        {
            // Nếu flip bằng scale, local +X sẽ tự mirror, nên dùng atkDir theo flip để thống nhất
            Vector3 outPos = weaponStartLocalPos + new Vector3(stabForward * atkDir, stabUp, 0f);

            float t = 0f;
            while (t < stabOutTime)
            {
                if (stun != null && stun.IsStunned) break;

                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / stabOutTime);
                float k = stabCurve.Evaluate(p);
                weaponRoot.localPosition = Vector3.Lerp(weaponStartLocalPos, outPos, k);
                yield return null;
            }

            t = 0f;
            while (t < stabBackTime)
            {
                if (stun != null && stun.IsStunned) break;

                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / stabBackTime);
                float k = stabCurve.Evaluate(p);
                weaponRoot.localPosition = Vector3.Lerp(outPos, weaponStartLocalPos, k);
                yield return null;
            }

            weaponRoot.localPosition = weaponStartLocalPos;
            weaponRoot.localRotation = weaponStartLocalRot;
        }
        else
        {
            yield return new WaitForSeconds(0.12f);
        }

        isAttacking = false;
    }

    void ApplyFlip(int dir)
    {
        if (!flipRoot) return;

        float sx = Mathf.Abs(flipRoot.localScale.x);
        Vector3 sc = flipRoot.localScale;
        sc.x = (dir == 1) ? (sx * facingSign) : (-sx * facingSign);
        flipRoot.localScale = sc;
    }

    void DealDamage(int dir)
    {
        // Ưu tiên hit theo mũi vũ khí để con nào đứng lệch cũng đánh được
        Vector2 center;
        if (stabSpawnPoint != null)
        {
            center = stabSpawnPoint.position;
        }
        else
        {
            Vector2 o = Origin2D();
            center = o + new Vector2(hitOffset.x * dir, hitOffset.y);
        }

        var hits = Physics2D.OverlapCircleAll(center, hitRadius, hitMask);
        for (int i = 0; i < hits.Length; i++)
        {
            var d = hits[i].GetComponentInParent<IDamageable>();
            if (d != null) d.TakeDamage(damage);
            else
            {
                var hp = hits[i].GetComponentInParent<Health>();
                if (hp != null) hp.TakeDamage(damage);
            }
        }
    }

    void SpawnStabVfx(int dir)
    {
        if (!stabVfxPrefab) return;

        Transform sp = stabSpawnPoint ? stabSpawnPoint : (weaponRoot ? weaponRoot : transform);

        Vector3 pos = sp.position + new Vector3(vfxOffset.x * dir, vfxOffset.y, 0f);
        float z = (dir == 1 ? 0f : 180f) + vfxAngleOffset;

        var go = Instantiate(stabVfxPrefab, pos, Quaternion.Euler(0, 0, z));

        var staticVfx = go.GetComponent<SlashVfxStatic>();
        if (staticVfx != null)
        {
            staticVfx.lifeTime = vfxLife;
            staticVfx.Setup(dir);
        }
        else
        {
            Destroy(go, vfxLife);
            var s = go.transform.localScale;
            s.x = Mathf.Abs(s.x) * (dir >= 0 ? 1f : -1f);
            go.transform.localScale = s;
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector2 o = Application.isPlaying ? Origin2D()
            : (centerPoint ? (Vector2)centerPoint.position : (Vector2)transform.position);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(o, new Vector3(aggroRangeX * 2f, aggroRangeY * 2f, 0.1f));

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(o, Mathf.Min(Mathf.Max(attackRange, stopDistance), 4f));

        // separation zone
        Gizmos.color = new Color(1f, 0f, 1f, 0.8f);
        Gizmos.DrawWireSphere(o, separationRadius);

        // hit zone
        if (target)
        {
            int dir = (target.position.x >= o.x) ? 1 : -1;

            Vector2 center = stabSpawnPoint ? (Vector2)stabSpawnPoint.position
                : o + new Vector2(hitOffset.x * dir, hitOffset.y);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(center, hitRadius);
        }
    }
}

// Nếu project bạn đã có interface/Health thì bỏ phần này
public interface IDamageable { void TakeDamage(int amount); }

public class Health : MonoBehaviour, IDamageable
{
    public int hp = 100;
    public void TakeDamage(int amount)
    {
        hp -= amount;
        if (hp <= 0) Destroy(gameObject);
    }
}
