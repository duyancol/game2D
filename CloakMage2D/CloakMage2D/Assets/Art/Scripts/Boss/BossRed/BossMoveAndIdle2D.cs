using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossMoveAndIdle2D : MonoBehaviour
{
    [Header("Parts")]
    public Transform body;
    public Transform head;
    public Transform armL;
    public Transform armR;

    public Transform feet;
    public Transform feetL;
    public Transform feetR;

    public Transform weaponPoint;

    [Header("Idle Motion")]
    public float bodyBobAmount = 0.03f;
    public float bodyBobSpeed = 2f;
    public float headRotateAmount = 2f;
    public float armRotateAmount = 12f;

    [Header("Feet Motion")]
    public float feetSquashAmount = 0.015f;
    public float feetRotateAmount = 1f;

    [Header("Patrol")]
    public bool patrol = true;
    public float patrolSpeed = 1.2f;
    public float patrolDistance = 2.0f;
    public float waitAtEdge = 0.25f;

    [Header("Flip")]
    public bool faceMoveDir = true;
    public Transform flipRoot;
    public int facingSign = 1; // sai hướng thì đổi 1 <-> -1

    [Header("Anti Push (không bị player đẩy)")]
    [Tooltip("Bật cái này để boss không bị đẩy (set Kinematic). Khuyên dùng nếu boss chỉ đi qua lại.")]
    public bool lockPushedByPlayer = true;

    [Tooltip("Nếu không dùng Kinematic, vẫn giảm bị đẩy bằng cách tăng mass + lock X velocity.")]
    public float massWhenDynamic = 999f;

    [Header("Edge/Wall Check (chống té)")]
    public bool useEdgeCheck = true;
    public Transform edgeCheck;
    public float edgeCheckDistance = 0.35f;
    public float edgeCheckForwardOffset = 0.35f;

    public bool useWallCheck = true;
    public Transform wallCheck;
    public float wallCheckDistance = 0.15f;
    public float wallCheckForwardOffset = 0.30f;

    public LayerMask groundMask;

    [Header("Debug")]
    public bool debugLog = false;

    [Header("Attack Swing")]
    public float attackSwingAngle = 65f;
    public float attackSwingDuration = 0.10f;
    public bool swingArmRight = true;

    [Header("Attack Shake")]
    public float shakeDuration = 0.15f;
    public float shakePosAmount = 0.08f;
    public float shakeRotAmount = 8f;
    public float shakeFrequency = 40f;
    public bool allowIdleMove = true;

    Rigidbody2D rb;
    Vector2 rootStartPos;

    Vector3 bodyStartPos;
    Quaternion headStartRot, armLStartRot, armRStartRot;

    Vector3 feetStartPos, feetStartScale;
    Quaternion feetStartRot;

    Vector3 feetLStartPos, feetRStartPos;
    Vector3 feetLStartScale, feetRStartScale;
    Quaternion feetLStartRot, feetRStartRot;

    float shakeTimer, shakeSeed;
    float swingTimer;
    bool isSwinging;

    int patrolDir = 1;
    float patrolWaitTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        if (!flipRoot) flipRoot = transform;

        // Chống bị đẩy
        if (lockPushedByPlayer)
        {
            //rb.bodyType = RigidbodyType2D.Kinematic;   // không bị push bởi rigidbody khác
            //rb.gravityScale = 0f;                      // kinematic + platformer thường không cần gravity
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.mass = Mathf.Max(1f, massWhenDynamic);
        }
    }

    void Start()
    {
        rootStartPos = rb.position;

        if (groundMask == 0 && (useEdgeCheck || useWallCheck))
        {
            Debug.LogWarning("[BossMoveAndIdle2D] groundMask đang = Nothing. Raycast sẽ không hit đất => boss không patrol được.");
        }

        if (body) bodyStartPos = body.localPosition;

        if (head) headStartRot = head.localRotation;
        if (armL) armLStartRot = armL.localRotation;
        if (armR) armRStartRot = armR.localRotation;

        if (feet)
        {
            feetStartPos = feet.localPosition;
            feetStartRot = feet.localRotation;
            feetStartScale = feet.localScale;
        }

        if (feetL)
        {
            feetLStartPos = feetL.localPosition;
            feetLStartRot = feetL.localRotation;
            feetLStartScale = feetL.localScale;
        }
        if (feetR)
        {
            feetRStartPos = feetR.localPosition;
            feetRStartRot = feetR.localRotation;
            feetRStartScale = feetR.localScale;
        }

        ApplyFlipByDir();
    }

    void Update()
    {
        float t = Time.time;
        float sin = Mathf.Sin(t * bodyBobSpeed);

        // ===== IDLE =====
        if (body) body.localPosition = bodyStartPos + Vector3.up * sin * bodyBobAmount;
        if (head) head.localRotation = headStartRot * Quaternion.Euler(0, 0, sin * headRotateAmount);
        if (armL) armL.localRotation = armLStartRot * Quaternion.Euler(0, 0, sin * armRotateAmount);
        if (armR) armR.localRotation = armRStartRot * Quaternion.Euler(0, 0, Mathf.Sin(t * bodyBobSpeed + Mathf.PI) * armRotateAmount);

        if (feet)
        {
            float s = Mathf.Sin(t * bodyBobSpeed);
            feet.localRotation = feetStartRot * Quaternion.Euler(0, 0, s * feetRotateAmount);

            var sc = feetStartScale;
            sc.y = feetStartScale.y - Mathf.Abs(s) * feetSquashAmount;
            feet.localScale = sc;
        }
        else
        {
            float s = Mathf.Sin(t * bodyBobSpeed);
            if (feetL)
            {
                feetL.localRotation = feetLStartRot * Quaternion.Euler(0, 0, s * feetRotateAmount);
                var sc = feetLStartScale;
                sc.y = feetLStartScale.y - Mathf.Abs(s) * feetSquashAmount;
                feetL.localScale = sc;
                feetL.localPosition = feetLStartPos;
            }
            if (feetR)
            {
                feetR.localRotation = feetRStartRot * Quaternion.Euler(0, 0, -s * feetRotateAmount);
                var sc = feetRStartScale;
                sc.y = feetRStartScale.y - Mathf.Abs(s) * feetSquashAmount;
                feetR.localScale = sc;
                feetR.localPosition = feetRStartPos;
            }
        }

        // ===== ATTACK SWING =====
        if (isSwinging)
        {
            swingTimer += Time.deltaTime;
            float p = Mathf.Clamp01(swingTimer / attackSwingDuration);

            float curve = (p < 0.5f)
                ? Mathf.SmoothStep(0f, 1f, p * 2f)
                : Mathf.SmoothStep(1f, 0f, (p - 0.5f) * 2f);

            float ang = attackSwingAngle * curve;

            Transform arm = swingArmRight ? armR : armL;
            if (arm)
            {
                var baseRot = swingArmRight ? armRStartRot : armLStartRot;
                arm.localRotation = baseRot * Quaternion.Euler(0, 0, -ang);
            }

            if (p >= 1f)
            {
                isSwinging = false;
                swingTimer = 0f;
            }
        }

        // ===== SHAKE =====
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            float k = Mathf.Clamp01(shakeTimer / shakeDuration);

            float tt = Time.time * shakeFrequency + shakeSeed;
            float dx = (Mathf.PerlinNoise(tt, 0.1f) - 0.5f) * 2f;
            float dy = (Mathf.PerlinNoise(0.2f, tt) - 0.5f) * 2f;
            float dr = (Mathf.PerlinNoise(tt, tt) - 0.5f) * 2f;

            Vector3 posOff = new Vector3(dx, dy, 0f) * (shakePosAmount * k);
            float rotOff = dr * (shakeRotAmount * k);

            if (body) body.localPosition += posOff;
            if (head) head.localRotation *= Quaternion.Euler(0, 0, rotOff);

            if (armR) armR.localRotation *= Quaternion.Euler(0, 0, rotOff * 0.7f);
            if (armL) armL.localRotation *= Quaternion.Euler(0, 0, rotOff * 0.4f);
        }
    }

    void FixedUpdate()
    {
         if (!allowIdleMove) return;
        if (!patrol) return;

        ApplyFlipByDir();

        if (patrolWaitTimer > 0f)
        {
            patrolWaitTimer -= Time.fixedDeltaTime;
            StopHorizontal();
            return;
        }

        float minX = rootStartPos.x - patrolDistance;
        float maxX = rootStartPos.x + patrolDistance;

        // Edge check
        if (useEdgeCheck && edgeCheck != null)
        {
            Vector3 origin = edgeCheck.position + Vector3.right * (patrolDir * edgeCheckForwardOffset);
            bool hasGroundAhead = Physics2D.Raycast(origin, Vector2.down, edgeCheckDistance, groundMask);

            if (debugLog) Debug.Log($"EdgeCheck dir={patrolDir} hasGround={hasGroundAhead}");

            if (!hasGroundAhead)
            {
                TurnAround();
                return;
            }
        }

        // Wall check
        if (useWallCheck && wallCheck != null)
        {
            Vector3 origin = wallCheck.position + Vector3.right * (patrolDir * wallCheckForwardOffset);
            Vector2 dir = (patrolDir == 1) ? Vector2.right : Vector2.left;
            bool hitWall = Physics2D.Raycast(origin, dir, wallCheckDistance, groundMask);

            if (debugLog) Debug.Log($"WallCheck dir={patrolDir} hitWall={hitWall}");

            if (hitWall)
            {
                TurnAround();
                return;
            }
        }

        // Move (Kinematic: MovePosition / Dynamic: velocity)
        Vector2 pos = rb.position;
        float nextX = pos.x + patrolDir * patrolSpeed * Time.fixedDeltaTime;

        // Clamp theo patrolDistance để chắc chắn đi 2 phía
        nextX = Mathf.Clamp(nextX, minX, maxX);

        rb.MovePosition(new Vector2(nextX, pos.y)); // ổn định, không bị player đẩy

        // chạm biên => quay đầu
        if ((patrolDir == 1 && nextX >= maxX - 0.001f) || (patrolDir == -1 && nextX <= minX + 0.001f))
        {
            TurnAround();
            return;
        }
    }

    void StopHorizontal()
    {
        if (rb.bodyType == RigidbodyType2D.Dynamic)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        // Kinematic không cần set velocity
    }

    void TurnAround()
    {
        patrolDir *= -1;
        patrolWaitTimer = waitAtEdge;
        ApplyFlipByDir();
        StopHorizontal();
    }

    void ApplyFlipByDir()
    {
        if (!faceMoveDir || !flipRoot) return;

        float sx = Mathf.Abs(flipRoot.localScale.x);
        var sc = flipRoot.localScale;
        sc.x = (patrolDir == 1) ? (sx * facingSign) : (-sx * facingSign);
        flipRoot.localScale = sc;
    }

    public void TriggerAttack()
    {
        if (weaponPoint != null && armR != null)
            swingArmRight = weaponPoint.IsChildOf(armR);

        isSwinging = true;
        swingTimer = 0f;

        shakeTimer = shakeDuration;
        shakeSeed = Random.Range(0f, 999f);
    }

    public void ResetPatrolOrigin()
    {
        rootStartPos = rb.position;
    }

    void OnDrawGizmosSelected()
    {
        if (edgeCheck)
        {
            Gizmos.color = Color.red;
            Vector3 a = edgeCheck.position + Vector3.right * (Application.isPlaying ? (patrolDir * edgeCheckForwardOffset) : edgeCheckForwardOffset);
            Gizmos.DrawLine(a, a + Vector3.down * edgeCheckDistance);
        }

        if (wallCheck)
        {
            Gizmos.color = Color.yellow;
            int d = Application.isPlaying ? patrolDir : 1;
            Vector3 a = wallCheck.position + Vector3.right * (d * wallCheckForwardOffset);
            Gizmos.DrawLine(a, a + Vector3.right * (d * wallCheckDistance));
        }
    }
}
