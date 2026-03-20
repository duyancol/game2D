using UnityEngine;


using System.Collections;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

[RequireComponent(typeof(Rigidbody2D))]
public class BossAntChaseTrip : MonoBehaviour
{
    Animator anim;
    [Header("Refs")]
    public Transform target;                 // Player
    public Transform flipRoot;               // ROOT boss/body để flip (Transform thường)
    public Transform weaponRoot;             // object sẽ đâm (Arm_R / Weapons / kiếm)
    public Transform stabSpawnPoint;         // điểm spawn VFX (WeaponPoint / tip)
    public Transform centerPoint;            // đặt ở thân boss (optional). Nếu null dùng rb.worldCenterOfMass
    [Header("Pull Animation Parts")]
    public Transform body;
    public Transform head;
    public Transform armL;
    public Transform armR;
    [Header("Disable When Pull")]
    private BossBuffolMoveAndIdle2D moveIdle;

    bool isDashing;
    Collider2D bossCol;

    [Header("Shield Skill")]
    public float shieldCooldown = 6f;
    public float shieldDuration = 4f;
    public float bonusDef = 50f;
    [Header("Shield Icon")]
    public GameObject shieldIcon;
    float lastShieldTime;
    bool isShielding;
    [Header("Spear Rain Skill")]
    public float spearCooldown = 8f;
    public float spearWarningTime = 1.2f;
    public GameObject warningCirclePrefab;
    public GameObject spearPrefab;
    public float spearSpawnHeight = 8f;
    public float spearFallSpeed = 15f;
    public int spearDamage = 35;
    [SerializeField] int spearCount = 3;
    [SerializeField] float delayBetweenSpears = 0.3f;
    [SerializeField] float randomRange = 2f; // nếu muốn lệch trái phải
    float lastSpearTime;
    PlayerStatsMono bossStats;

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
        return (flipRoot.lossyScale.x < 0f) ? 1 : -1;
    }

    void Awake()
    {
        stun = GetComponent<BossStun>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rb.freezeRotation = true;
        moveIdle = GetComponent<BossBuffolMoveAndIdle2D>();
        bossCol = GetComponent<Collider2D>();
        bossStats = GetComponent<PlayerStatsMono>();
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
        if (attackRange > 50f)
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
        float atkRange = Mathf.Min(Mathf.Max(attackRange, stopDistance + 0.05f), 20f);


        if (dx <= atkRange && Time.time >= nextAttackTime)
        {
            int dir = DirToTarget();
            ApplyFlip(dir);

            // 🛑 DỪNG DI CHUYỂN
            if (moveIdle != null)
            {
                moveIdle.disableMovement = true;
                moveIdle.enabled = false;
            }
            rb.linearVelocity = Vector2.zero;
            // ===== ƯU TIÊN SPEAR =====
            if (Time.time >= lastSpearTime + spearCooldown)
            {
                lastSpearTime = Time.time;
                StartCoroutine(SpearRainRoutine());
                return;
            }
            // ===== ƯU TIÊN SHIELD =====
            if (Time.time >= lastShieldTime + shieldCooldown)
            {
                lastShieldTime = Time.time;
                StartCoroutine(ShieldRoutine());
                return;
            }

            // ===== ĐÁNH THƯỜNG =====
            nextAttackTime = Time.time + attackCooldown;
            StartCoroutine(StabRoutine(dir));
        }
    }
    IEnumerator SpearRainRoutine()
    {
        isAttacking = true;
        moveIdle?.LockMovement();
        rb.linearVelocity = Vector2.zero;

        anim?.SetTrigger("Cast");

        for (int i = 0; i < spearCount; i++)
        {
            Vector3 targetPos = target.position;

            // random nhẹ cho né khó hơn
            float offsetX = Random.Range(-randomRange, randomRange);
            targetPos.x += offsetX;

            // 🔴 Warning
            GameObject warning = null;
            if (warningCirclePrefab)
                warning = Instantiate(warningCirclePrefab, targetPos, Quaternion.identity);

            yield return new WaitForSeconds(spearWarningTime);

            if (warning) Destroy(warning);

            // ⚔ Spawn spear
            Vector3 spawnPos = new Vector3(
                targetPos.x,
                targetPos.y + spearSpawnHeight,
                0f);

            GameObject spear = Instantiate(spearPrefab, spawnPos, Quaternion.identity);
            StartCoroutine(DropSpear(spear, targetPos));

            yield return new WaitForSeconds(delayBetweenSpears);
        }

        yield return new WaitForSeconds(0.3f);

        moveIdle?.UnlockMovement();
        isAttacking = false;
    }
    IEnumerator DropSpear(GameObject spear, Vector3 targetPos)
    {
        while (spear != null && spear.transform.position.y > targetPos.y)
        {
            spear.transform.position += Vector3.down * spearFallSpeed * Time.deltaTime;
            yield return null;
        }

        // 💥 Check damage
        Collider2D[] hits = Physics2D.OverlapCircleAll(targetPos, 4.2f, hitMask);

        foreach (var h in hits)
        {
            var d = h.GetComponentInParent<IDamageable>();
            if (d != null)
                d.TakeDamage(spearDamage);
        }

        if (spear) Destroy(spear);
    }
    IEnumerator ShieldRoutine()
    {
        isAttacking = true;
        isShielding = true;

        moveIdle?.LockMovement();
        rb.linearVelocity = Vector2.zero;

        anim?.SetTrigger("ShieldT");

        yield return new WaitForSeconds(3.0f);

        // 🛡️ Buff stat
        bossStats?.AddDefense(bonusDef);
        bossStats?.AddMDefense(bonusDef);

        // 🔥 Bật icon
        if (shieldIcon != null)
            shieldIcon.SetActive(true);

        yield return new WaitForSeconds(shieldDuration);

        // ❌ Remove buff
        bossStats?.RemoveDefense(bonusDef);
        

        // 🔥 Tắt icon
        if (shieldIcon != null)
            shieldIcon.SetActive(false);

        moveIdle?.UnlockMovement();

        isShielding = false;
        isAttacking = false;
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

        // 🛑 Dừng di chuyển
        if (moveIdle != null)
        {
            moveIdle.LockMovement();
        }

        rb.linearVelocity = Vector2.zero;
        // 🎬 PLAY ANIMATION
        if (anim) anim.SetTrigger("at");
        ApplyFlip(atkDir);


        CacheWeapon();

        yield return new WaitForSeconds(0.6f); // windup

        SpawnStabVfx(atkDir);
        DealDamage(atkDir);

        yield return new WaitForSeconds(0.25f);

        // Reset vũ khí
        if (weaponRoot)
        {
            weaponRoot.localRotation = weaponStartLocalRot;
            weaponRoot.localPosition = weaponStartLocalPos;
        }

        // ✅ Bật lại di chuyển
        moveIdle.UnlockMovement();

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
        Vector2 origin = Origin2D();

        // Vùng quét phía trước mặt boss
        Vector2 center = origin + new Vector2(2.5f * dir, 0.5f);
        Vector2 size = new Vector2(8f, 8f);


        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f, hitMask);

        for (int i = 0; i < hits.Length; i++)
        {
            var d = hits[i].GetComponentInParent<IDamageable>();
            if (d != null)
            {
                d.TakeDamage(damage);

                // Đẩy nhẹ cho có lực chém
                Rigidbody2D prb = hits[i].GetComponentInParent<Rigidbody2D>();
                if (prb != null)
                {
                    Vector2 push = new Vector2(dir * 6f, 2f);
                    prb.AddForce(push, ForceMode2D.Impulse);
                }
            }
        }
    }


    void SpawnStabVfx(int dir)
    {
        if (!stabVfxPrefab) return;

        Transform sp = stabSpawnPoint ? stabSpawnPoint : (weaponRoot ? weaponRoot : transform);

        Vector3 pos = sp.position + new Vector3(vfxOffset.x * dir, vfxOffset.y, 0f);
        float z = (dir == -1 ? 0f : 180f) + vfxAngleOffset;
        //float z = vfxAngleOffset;
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
        Gizmos.DrawWireSphere(o, Mathf.Min(Mathf.Max(attackRange, stopDistance), 20f));

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



