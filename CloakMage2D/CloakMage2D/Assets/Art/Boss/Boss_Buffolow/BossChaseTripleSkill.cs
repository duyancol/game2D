
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

[RequireComponent(typeof(Rigidbody2D))]
public class BossChaseTripleSkill : MonoBehaviour
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

    [Header("Pull Skill")]
    public float pullCooldown = 4f;           // sau 4s mới dùng skill
    public float pullRange = 4f;              // tầm ảnh hưởng phía trước
    public float pullWidth = 2.5f;            // độ rộng vùng hút
    public float pullForce = 20f;             // lực hút
    public float pullWindupTime = 0.8f;       // thời gian hiện vùng đỏ cảnh báo
    public int pullDamage = 15;
[Header("Pull Move")]
public float pullMoveSpeed = 6f;   // tốc độ lao tới điểm hút

    

    float lastPullTime;
    [Header("Pull FX")]
    public GameObject pullWarningPrefab;   // vùng đỏ
    public GameObject dragonPrefab;        // rồng hút
    public float dashSpeed = 8f;
    public float dashDuration = 0.6f;

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

            // ===== ƯU TIÊN SKILL HÚT =====
            if (Time.time >= lastPullTime + pullCooldown)
            {
                lastPullTime = Time.time;
                StartCoroutine(PullRoutine(dir));
                return;
            }

            // ===== ĐÁNH THƯỜNG =====
            nextAttackTime = Time.time + attackCooldown;
            StartCoroutine(StabRoutine(dir));
        }
    }

    IEnumerator PullRoutine(int dir)
    {
        isAttacking = true;

        // 🛑 Tắt di chuyển
        if (moveIdle != null)
        {
            moveIdle.LockMovement();
        }
       
        rb.linearVelocity = Vector2.zero;
        if (anim)
        {
            anim.SetBool("attack_1", true);   // animation gồng
            anim.SetBool("Pull", false);
        }

        yield return new WaitForSeconds(2f);  // ⬅ GỒNG 1 GIÂY

        if (anim)
        {
            anim.SetBool("attack_1", false);  // tắt gồng
            anim.SetBool("Pull", true);       // bật animation hút
        }
        //Physics2D.IgnoreLayerCollision(
        //    LayerMask.NameToLayer("Boss"),
        //    LayerMask.NameToLayer("Player"),
        //    true
        //);
        Collider2D playerCol = target.GetComponent<Collider2D>();

        if (playerCol != null)
        {
            Physics2D.IgnoreCollision(bossCol, playerCol, true);
        }
        // ===== TÍNH VÙNG HÚT =====
        Vector2 origin = Origin2D();
        Vector2 center = origin + new Vector2(dir * pullRange * 0.5f, 0.5f);
        Vector2 size = new Vector2(pullRange, pullWidth);

        // ===== HIỆN VÙNG CẢNH BÁO =====
        GameObject warn = null;
        if (pullWarningPrefab)
        {
            warn = Instantiate(pullWarningPrefab, center, Quaternion.identity);
            warn.transform.localScale = new Vector3(size.x / 8f, size.y / 1f, 1f);
        }

        yield return new WaitForSeconds(pullWindupTime);

        if (warn) Destroy(warn);

        // ===== SPAWN RỒNG =====
        if (dragonPrefab)
        {
            Vector3 spawnPos = origin + new Vector2(dir * 1.2f, 0);
            GameObject dragon = Instantiate(dragonPrefab, spawnPos, Quaternion.identity);

            float angle = dir > 0 ? 0f : 180f;
            dragon.transform.rotation = Quaternion.Euler(0, angle, 0);

            Destroy(dragon, 1.2f);
        }

        // ===== HÚT LIÊN TỤC =====
        float timer = 0f;

        while (timer < dashDuration)
        {
            timer += Time.deltaTime;

            // Boss lao nhẹ về trước
            float newX = rb.position.x + dir * dashSpeed * Time.deltaTime;
            rb.MovePosition(new Vector2(newX, rb.position.y));

            origin = Origin2D();
            center = origin + new Vector2(dir * pullRange * 0.5f, 0.5f);

            Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f, hitMask);

            for (int i = 0; i < hits.Length; i++)
            {
                Rigidbody2D prb = hits[i].GetComponentInParent<Rigidbody2D>();
                IDamageable dmg = hits[i].GetComponentInParent<IDamageable>();

                if (prb != null)
                {
                    Vector2 pullDir = ((Vector2)origin - prb.position).normalized;
                    prb.AddForce(pullDir * pullForce * Time.deltaTime, ForceMode2D.Force);
                }

                if (dmg != null)
                {
                    dmg.TakeDamage(pullDamage);
                }
            }

            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        //Physics2D.IgnoreLayerCollision(
        //    LayerMask.NameToLayer("Boss"),
        //    LayerMask.NameToLayer("Player"),
        //    false
        //);
        if (playerCol != null)
        {
            Physics2D.IgnoreCollision(bossCol, playerCol, false);
        }
        anim.SetBool("Pull", false);
        // ✅ Bật lại di chuyển
        moveIdle.UnlockMovement();

        rb.linearVelocity = Vector2.zero;
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

        ApplyFlip(atkDir);
        // 🎬 PLAY ANIMATION
        if (anim) anim.SetTrigger("Attack");
      
        CacheWeapon();

        yield return new WaitForSeconds(0.2f); // windup

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



