using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossBuffolMoveAndIdle2D : MonoBehaviour
{
    Animator anim;
    [Header("Parts")]
    public Transform body;
    public Transform head;
    public Transform armL;
    public Transform armR;

    public Transform feet;
    public Transform feetL;
    public Transform feetR;
    public bool disableMovement = false;

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
    public int facingSign = 1;

    [Header("Anti Push (không bị player đẩy)")]
    public bool lockPushedByPlayer = true;
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
    public float attackSwingAngle = 65f;       // dùng làm tốc độ charge
    public float attackSwingDuration = 0.6f;   // thời gian charge
    public bool swingArmRight = true;          // giữ cho đúng input

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
        anim = GetComponent<Animator>();
        rb.freezeRotation = true;

        if (!flipRoot) flipRoot = transform;

        if (!lockPushedByPlayer)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }
    }

    void Start()
    {
        rootStartPos = rb.position;

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
        //if (!isSwinging)
        //{
        //    if (body) body.localPosition = bodyStartPos + Vector3.up * sin * bodyBobAmount;
        //    if (head) head.localRotation = headStartRot * Quaternion.Euler(0, 0, sin * headRotateAmount);
        //    if (armL)
        //        armL.localRotation = armLStartRot * Quaternion.Euler(0, 0, sin * armRotateAmount);

        //    if (armR)
        //        armR.localRotation = armRStartRot * Quaternion.Euler(0, 0, -sin * armRotateAmount);
        //}
        if (!isSwinging && false) // TẮT idle procedural motion
        {
            if (body) body.localPosition = bodyStartPos + Vector3.up * sin * bodyBobAmount;
            if (head) head.localRotation = headStartRot * Quaternion.Euler(0, 0, sin * headRotateAmount);
            if (armL)
                armL.localRotation = armLStartRot * Quaternion.Euler(0, 0, sin * armRotateAmount);

            if (armR)
                armR.localRotation = armRStartRot * Quaternion.Euler(0, 0, -sin * armRotateAmount);
        }
        // ===== TRÂU HÚC (CHARGE) =====
        //if (isSwinging && !disableMovement)

        //{
        //    swingTimer += Time.deltaTime;
        //    float p = swingTimer / attackSwingDuration;

        //    // 0 → 0.25 : lùi lại lấy đà
        //    if (p < 0.25f)
        //    {
        //        rb.MovePosition(rb.position - Vector2.right * patrolDir * 0.03f);
        //        if (body)
        //            body.localScale = Vector3.one * 0.95f;
        //    }
        //    else
        //    {
        //        // charge mạnh
        //        float speed = attackSwingAngle * 0.15f;
        //        rb.MovePosition(rb.position + Vector2.right * patrolDir * speed * Time.deltaTime);
        //    }

        //    if (p >= 1f)
        //    {
        //        isSwinging = false;
        //        swingTimer = 0f;

        //        shakeTimer = shakeDuration;
        //        shakeSeed = Random.Range(0f, 999f);

        //        if (body)
        //            body.localScale = Vector3.one;
        //    }
        //}

        // ===== SHAKE =====
        //if (shakeTimer > 0f)
        //{
        //    shakeTimer -= Time.deltaTime;
        //    float k = Mathf.Clamp01(shakeTimer / shakeDuration);

        //    float tt = Time.time * shakeFrequency + shakeSeed;
        //    float dx = (Mathf.PerlinNoise(tt, 0.1f) - 0.5f) * 2f;
        //    float dy = (Mathf.PerlinNoise(0.2f, tt) - 0.5f) * 2f;

        //    if (body)
        //        body.localPosition += new Vector3(dx, dy, 0f) * (shakePosAmount * k);
        //}
    }

    void FixedUpdate()
    {
        if (!allowIdleMove || disableMovement) return;

        if (!patrol || isSwinging) return;

        ApplyFlipByDir();

        if (patrolWaitTimer > 0f)
        {
            patrolWaitTimer -= Time.fixedDeltaTime;
            StopHorizontal();
            return;
        }

        float minX = rootStartPos.x - patrolDistance;
        float maxX = rootStartPos.x + patrolDistance;

        Vector2 pos = rb.position;
        float nextX = pos.x + patrolDir * patrolSpeed * Time.fixedDeltaTime;
        nextX = Mathf.Clamp(nextX, minX, maxX);

        rb.MovePosition(new Vector2(nextX, pos.y));
        if (anim)
        {
            anim.SetFloat("MoveX", patrolDir);
        }
        if ((patrolDir == 1 && nextX >= maxX - 0.001f) ||
            (patrolDir == -1 && nextX <= minX + 0.001f))
        {
            TurnAround();
        }
    }

    public void StopHorizontal()
    {
        if (rb.bodyType == RigidbodyType2D.Dynamic)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        if (anim)
        {
            anim.SetFloat("MoveX", 0f);
        }
    }
    public void LockMovement()
    {
        disableMovement = true;
        allowIdleMove = false;
        StopHorizontal();
    }

    public void UnlockMovement()
    {
        disableMovement = false;
        allowIdleMove = true;
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
        //if (!faceMoveDir || !flipRoot) return;

        //float sx = Mathf.Abs(flipRoot.localScale.x);
        //var sc = flipRoot.localScale;
        //sc.x = (patrolDir == 1) ? (-sx * facingSign) : (sx * facingSign);
        //flipRoot.localScale = sc;
    }

    public void TriggerAttack()
    {
        isSwinging = true;
        swingTimer = 0f;
    }

    public void ResetPatrolOrigin()
    {
        rootStartPos = rb.position;
    }
}
