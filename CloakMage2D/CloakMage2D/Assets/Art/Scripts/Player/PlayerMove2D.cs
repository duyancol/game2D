
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove2D : MonoBehaviour
{
    [Header("Visual Switch")]
    public GameObject visualNormal;     // Player/Visual
    public GameObject visualSkill;      // Player/Visual_Skill
    public Animator animator;           // Animator của player

    bool isUsingSkill = false;
    [Header("Move")]
    public float moveSpeed = 6f;
    public float acceleration = 50f;
    public float deceleration = 60f;
    [Header("Mobile Fly")]
    public float flyForce = 20f;
    private bool isFlyingButton = false;

    [Header("Air Control")]
    public float airMaxSpeed = 6f;
    public float airAcceleration = 28f;
    public float airDeceleration = 18f;
    [Header("Mobile")]
    public FixedJoystick joystick;   // kéo FixedJoystick vào đây trong Inspector

    [Header("Jump")]
    public KeyCode jumpKey = KeyCode.W;
    public float jumpVelocity = 20f;
    public float gravityScale = 3f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.18f;
    public LayerMask groundMask;

    [Header("References Root")]
    public Transform visual; // Player/Visual
    [Header("CRIP - Idle Arms")]
    public float idleArmWaveAmp = 10f;      // độ lắc tay (độ)
    public float idleArmWaveSpeed = 2.2f;   // tốc độ lắc
    public float idleArmWaveOffset = 0.6f;  // lệch pha giữa 2 tay
    public float idleArmSmooth = 16f;       // mượt tay lúc idle

    [Header("Parts")]
    public Transform body;
    public Transform head;
    public Transform footL;
    public Transform footR;

    [Tooltip("Tay phải (tay trống)")]
    public Transform armRightRoot;

    [Tooltip("Tay trái (tay còn lại / tay cầm vũ khí nếu m gắn ở đây)")]
    public Transform armWeaponRoot;

    [Header("Flip")]
    public bool faceRightByDefault = true;

    [Header("CRIP - Run Arms Speed")]
    [Tooltip("Tốc độ đánh tay so với chân. 1 = bằng chân, 0.5 = chậm hơn 2 lần")]
    public float armSpeedMul = 0.45f;
    public float FacingDirection => lastFace;
    [Header("CRIP - Idle")]
    public float idleBobAmp = 0.03f;
    public float idleBobSpeed = 2f;
    public float idleHeadNodAmp = 2.0f;
    public float idleHeadNodSpeed = 1.6f;

    [Header("CRIP - Walk Body")]
    public float walkBobAmp = 0.06f;
    public float walkBodyLean = 6f;

    [Header("CRIP - Walk Feet (Smooth Gait)")]
    [Tooltip("Bước dài. Tăng -> bước chậm, giảm -> bước nhanh.")]
    public float strideLength = 0.55f;

    [Tooltip("Chân tiến/lùi theo X (local).")]
    public float footForward = 0.07f;

    [Tooltip("Nhấc chân theo Y (local).")]
    public float footLift = 0.06f;

    [Tooltip("Đạp xuống nhẹ khi chân chạm đất (local Y âm).")]
    public float footDown = 0.02f;

    [Header("CRIP - Run Arms (Front/Back loop)")]
    [Tooltip("Tay phải đưa ra trước (deg)")]
    public float rightArmFrontAngle = 90f;

    [Tooltip("Tay phải đưa về sau (deg)")]
    public float rightArmBackAngle = -90f;

    [Tooltip("Tay trái đưa ra trước (deg)")]
    public float leftArmFrontAngle = 90f;

    [Tooltip("Tay trái đưa về sau (deg)")]
    public float leftArmBackAngle = -90f;

    [Tooltip("Mượt khi tay đổi trước/sau")]
    public float armSmooth = 18f;

    [Header("CRIP - Jump/Fall Pose")]
    public float jumpBodyUp = 0.08f;
    public float fallBodyDown = 0.04f;
    public float airLean = 10f;

    [Header("CRIP - Jump UP Pose (NEW)")]
    [Tooltip("Độ 'nhún' khi vừa nhảy: toàn thân hạ xuống (local Y âm). Ví dụ 0.08~0.14")]
    public float jumpSquashDown = 0.11f;

    [Tooltip("Thời gian nhún khi vừa rời đất. Ví dụ 0.06~0.10")]
    public float jumpSquashTime = 0.08f;

    [Tooltip("Góc dang tay chữ V khi vừa nhảy. Ví dụ 65~90")]
    public float jumpVArmAngle = 80f;

    [Tooltip("Tốc độ mượt tay khi bay lên")]
    public float jumpArmSmooth = 16f;

    [Tooltip("Tay hạ dần theo % lực bay lên. 1 = hạ hết về base khi gần hết lực bay.")]
    [Range(0f, 1f)]
    public float jumpArmLowerStrength = 1f;

    [Header("CRIP - Air BOTH Arms Pose (Falling)")]
    [Tooltip("Độ dang tay khi rơi (deg). Ví dụ 60~85.")]
    public float fallArmAngle = 70f;

    [Tooltip("Mượt khi dang tay khi rơi.")]
    public float fallArmSmooth = 18f;

    [Header("Smoothing")]
    public float poseSmooth = 14f;

    Rigidbody2D rb;
    Vector2 input;
    bool isGrounded;
    bool wasGrounded;
    float lastFace = 1f;

    // base
    Vector3 baseVisualPos;
    Quaternion baseBodyRot, baseHeadRot;
    Vector3 baseFootLPos, baseFootRPos;
    Quaternion baseArmRightRot, baseArmLeftRot;

    // smooth states
    float curBobY;
    float curLeanZ;
    float gaitPhase;

    // jump timing
    float jumpStartTime;
    public void OnFlyDown()
    {
        isFlyingButton = true;
    }

    public void OnFlyUp()
    {
        isFlyingButton = false;
    }

    void Awake()
    {
        if (visualNormal == null)
            visualNormal = visual.gameObject;

        if (animator == null)
            animator = GetComponent<Animator>();

        SetNormalVisual();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (visual == null) visual = transform;

        baseVisualPos = visual.localPosition;

        baseBodyRot = body ? body.localRotation : Quaternion.identity;
        baseHeadRot = head ? head.localRotation : Quaternion.identity;

        baseFootLPos = footL ? footL.localPosition : Vector3.zero;
        baseFootRPos = footR ? footR.localPosition : Vector3.zero;

        baseArmRightRot = armRightRoot ? armRightRoot.localRotation : Quaternion.identity;
        baseArmLeftRot = armWeaponRoot ? armWeaponRoot.localRotation : Quaternion.identity;

        lastFace = faceRightByDefault ? 1f : -1f;
        ApplyFlip(lastFace);

        wasGrounded = false;
        jumpStartTime = -999f;
    }
    public void SetNormalVisual()
    {
        isUsingSkill = false;

        if (visualNormal) visualNormal.SetActive(true);
        if (visualSkill) visualSkill.SetActive(false);

        if (animator) animator.enabled = false; // TẮT IDLE ANIMATION
    }

    public void SetSkillVisual()
    {
        isUsingSkill = true;

        if (visualNormal) visualNormal.SetActive(false);
        if (visualSkill) visualSkill.SetActive(true);

        if (animator) animator.enabled = true; // bật animator nếu skill dùng animation
    }
    void Update()
    {
        //float x = Input.GetAxisRaw("Horizontal");
        //input = new Vector2(x, 0f);
        //float x = 0f;

        //// Nếu có joystick (mobile) → dùng joystick
        //if (joystick != null)
        //{
        //    x = joystick.Horizontal;
        //}
        //else
        //{
        //    // PC fallback
        //    x = Input.GetAxisRaw("Horizontal");
        //}

        //input = new Vector2(x, 0f);
        float keyboardX = Input.GetAxisRaw("Horizontal");
        float joystickX = 0f;

        if (joystick != null)
        {
            joystickX = joystick.Horizontal;
        }

        // Ưu tiên cái nào đang có input mạnh hơn
        float x = Mathf.Abs(joystickX) > 0.1f ? joystickX : keyboardX;

        input = new Vector2(x, 0f);


        wasGrounded = isGrounded;

        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);

        if (isGrounded && Input.GetKeyDown(jumpKey))
        {
            // mark jump start (for squash + V arms)
            jumpStartTime = Time.time;

            var v = rb.linearVelocity;
            v.y = jumpVelocity;
            rb.linearVelocity = v;
        }

        // Nếu vừa rời ground (ví dụ bước khỏi mép mà vẫn bay lên) thì cũng set jumpStartTime để pose đẹp
        if (wasGrounded && !isGrounded && rb.linearVelocity.y > 0.5f)
        {
            jumpStartTime = Time.time;
        }

        // Face: chỉ theo A/D
        if (Mathf.Abs(input.x) > 0.01f)
        {
            float face = Mathf.Sign(input.x);
            if (Mathf.Abs(face - lastFace) > 0.001f)
            {
                lastFace = face;
                ApplyFlip(lastFace);
            }
        }

        ApplyCripPose();
    }

    void FixedUpdate()
    {
        if (isGrounded)
        {
            float targetX = input.x * moveSpeed;
            float rate = (Mathf.Abs(targetX) > 0.001f) ? acceleration : deceleration;
            float newX = Mathf.MoveTowards(rb.linearVelocity.x, targetX, rate * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
        }
        else
        {
            float targetX = input.x * airMaxSpeed;
            float rate = (Mathf.Abs(targetX) > 0.001f) ? airAcceleration : airDeceleration;
            float newX = Mathf.MoveTowards(rb.linearVelocity.x, targetX, rate * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
        }
        if (isFlyingButton)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, flyForce);
        }

    }

    void ApplyCripPose()
    {
        float t = Time.time;

        float speedAbs = Mathf.Abs(rb.linearVelocity.x);
        bool moving = speedAbs > 0.05f;
        float speed01 = Mathf.Clamp01(speedAbs / Mathf.Max(0.01f, moveSpeed));

        bool isJumpUp = (!isGrounded && rb.linearVelocity.y > 0.5f);
        bool isFalling = (!isGrounded && rb.linearVelocity.y < -0.5f);

        // ===== GAIT PHASE (mượt) =====
        if (isGrounded && moving)
        {
            float stepsPerSec = speedAbs / Mathf.Max(0.01f, strideLength);
            float omega = stepsPerSec * Mathf.PI * 2f;
            gaitPhase += omega * Time.deltaTime;
        }
        else
        {
            gaitPhase = Mathf.Lerp(gaitPhase, 0f, 1f - Mathf.Exp(-poseSmooth * Time.deltaTime));
        }

        // ===== Bob Y =====
        float targetBob = 0f;

        if (isGrounded)
        {
            float amp = moving ? walkBobAmp : idleBobAmp;
            float spd = moving ? Mathf.Lerp(6f, 11f, speed01) : idleBobSpeed;
            targetBob = amp * Mathf.Sin(t * spd);
        }
        else
        {
            if (isJumpUp)
            {
                // NEW: vừa nhảy -> nhún hạ xuống rồi mới lên
                float jt = Mathf.Max(0f, t - jumpStartTime);
                float squash01 = (jumpSquashTime <= 0.001f) ? 1f : Mathf.Clamp01(jt / jumpSquashTime);

                // phase 0..1: từ -jumpSquashDown -> jumpBodyUp
                float bob = Mathf.Lerp(-jumpSquashDown, jumpBodyUp, Smooth01(squash01));
                targetBob = bob;
            }
            else if (rb.linearVelocity.y < -0.5f)
            {
                targetBob = -fallBodyDown;
            }
            else
            {
                targetBob = 0f;
            }
        }

        curBobY = Smooth(curBobY, targetBob, poseSmooth);
        visual.localPosition = baseVisualPos + new Vector3(0f, curBobY, 0f);

        // ===== Lean =====
        float targetLean = 0f;
        if (isGrounded)
        {
            targetLean = -input.x * walkBodyLean * Mathf.Lerp(0.25f, 1f, speed01);
        }
        else
        {
            float vx = rb.linearVelocity.x;
            float u = Mathf.Clamp(vx / Mathf.Max(0.01f, airMaxSpeed), -1f, 1f);
            targetLean = -u * airLean;
        }

        curLeanZ = Smooth(curLeanZ, targetLean, poseSmooth);
        if (body) body.localRotation = Quaternion.Euler(0f, 0f, curLeanZ) * baseBodyRot;

        // ===== Head nod =====
        if (head)
        {
            float nod = idleHeadNodAmp * Mathf.Sin(t * idleHeadNodSpeed);
            float headZ = (isGrounded && !moving) ? nod : nod * 0.15f;
            head.localRotation = Quaternion.Euler(0f, 0f, headZ) * baseHeadRot;
        }

        // ===== Feet =====
        if (isGrounded && moving)
        {
            float amp = Mathf.Lerp(0.35f, 1.0f, speed01);

            float pL = gaitPhase;
            float pR = gaitPhase + Mathf.PI;

            float sinL = Mathf.Sin(pL);
            float sinR = Mathf.Sin(pR);
            float cosL = Mathf.Cos(pL);
            float cosR = Mathf.Cos(pR);

            float xL = cosL * footForward * amp;
            float xR = cosR * footForward * amp;

            float yL = ComputeFootY(sinL, footLift, footDown) * amp;
            float yR = ComputeFootY(sinR, footLift, footDown) * amp;

            if (footL) footL.localPosition = baseFootLPos + new Vector3(xL, yL, 0f);
            if (footR) footR.localPosition = baseFootRPos + new Vector3(xR, yR, 0f);
        }
        else
        {
            float k = 1f - Mathf.Exp(-poseSmooth * Time.deltaTime);
            if (footL) footL.localPosition = Vector3.Lerp(footL.localPosition, baseFootLPos, k);
            if (footR) footR.localPosition = Vector3.Lerp(footR.localPosition, baseFootRPos, k);
        }

        // ===== Arms =====
        if (isFalling)
        {
            // Rơi: CẢ 2 TAY DANG RA
            float baseDir = (lastFace < 0f) ? 0f : 0f;

            float rightAngle = baseDir + fallArmAngle;
            float leftAngle = baseDir - fallArmAngle;

            float k = 1f - Mathf.Exp(-fallArmSmooth * Time.deltaTime);

            if (armRightRoot)
            {
                Quaternion tr = Quaternion.Euler(0f, 0f, rightAngle) * baseArmRightRot;
                armRightRoot.localRotation = Quaternion.Slerp(armRightRoot.localRotation, tr, k);
            }

            if (armWeaponRoot)
            {
                // giữ logic bạn đang dùng cho falling (có 120f)
                Quaternion tl = Quaternion.Euler(120f, 0f, leftAngle) * baseArmLeftRot;
                armWeaponRoot.localRotation = Quaternion.Slerp(armWeaponRoot.localRotation, tl, k);
            }
        }
        else if (isJumpUp)
        {
            // NEW: Bay lên -> tay chữ V lúc đầu, rồi hạ từ từ khi lực bay lên giảm
            float baseDir = (lastFace < 0f) ? 0f : 0f;

            // up01: 0 khi đang bay lên mạnh, 1 khi gần hết lực (vy ~ 0)
            float up01 = 1f - Mathf.Clamp01(rb.linearVelocity.y / Mathf.Max(0.01f, jumpVelocity));
            up01 = Mathf.Clamp01(up01 * jumpArmLowerStrength);

            // V -> về 0
            float vAng = Mathf.Lerp(jumpVArmAngle, 0f, Smooth01(up01));

            float rightAngle = baseDir + vAng;
            float leftAngle = baseDir - vAng;

            float k = 1f - Mathf.Exp(-jumpArmSmooth * Time.deltaTime);

            if (armRightRoot)
            {
                Quaternion tr = Quaternion.Euler(0f, 0f, rightAngle) * baseArmRightRot;
                armRightRoot.localRotation = Quaternion.Slerp(armRightRoot.localRotation, tr, k);
            }

            if (armWeaponRoot)
            {
                // jump up dùng xoay Z thuần để “V” đẹp (đỡ bị xoắn 120f)
                Quaternion tl = Quaternion.Euler(0f, 0f, leftAngle) * baseArmLeftRot;
                armWeaponRoot.localRotation = Quaternion.Slerp(armWeaponRoot.localRotation, tl, k);
            }
        }
        else if (isGrounded && moving)
        {
            // Chạy: tay phải trước<->sau, tay trái ngược lại (chậm hơn bằng armSpeedMul)
            float armPhase = gaitPhase * armSpeedMul;
            float u = (Mathf.Sin(armPhase) + 1f) * 0.5f;

            float rightZ = Mathf.Lerp(rightArmBackAngle, rightArmFrontAngle, u);
            float leftZ = Mathf.Lerp(leftArmFrontAngle, leftArmBackAngle, u);

            float k = 1f - Mathf.Exp(-armSmooth * Time.deltaTime);

            if (armRightRoot)
            {
                Quaternion target = Quaternion.Euler(0f, 0f, rightZ) * baseArmRightRot;
                armRightRoot.localRotation = Quaternion.Slerp(armRightRoot.localRotation, target, k);
            }

            if (armWeaponRoot)
            {
                Quaternion target = Quaternion.Euler(0f, 0f, leftZ) * baseArmLeftRot;
                armWeaponRoot.localRotation = Quaternion.Slerp(armWeaponRoot.localRotation, target, k);
            }
        }
        else if (isGrounded && !moving)
        {
            // IDLE: lắc tay lên xuống
            float wave = Mathf.Sin(t * idleArmWaveSpeed) * idleArmWaveAmp;

            // cho 2 tay lệch pha nhẹ để nhìn tự nhiên hơn
            float waveR = wave;
            float waveL = Mathf.Sin(t * (idleArmWaveSpeed * 1.05f) + idleArmWaveOffset) * (idleArmWaveAmp * 0.9f);

            float k = 1f - Mathf.Exp(-idleArmSmooth * Time.deltaTime);

            if (armRightRoot)
            {
                Quaternion tr = Quaternion.Euler(0f, 0f, waveR) * baseArmRightRot;
                armRightRoot.localRotation = Quaternion.Slerp(armRightRoot.localRotation, tr, k);
            }

            if (armWeaponRoot)
            {
                Quaternion tl = Quaternion.Euler(0f, 0f, waveL) * baseArmLeftRot;
                armWeaponRoot.localRotation = Quaternion.Slerp(armWeaponRoot.localRotation, tl, k);
            }
        }
        else
        {
            // Lơ lửng (không jumpUp/fall) hoặc trạng thái khác: về base
            float k = 1f - Mathf.Exp(-poseSmooth * Time.deltaTime);
            if (armRightRoot) armRightRoot.localRotation = Quaternion.Slerp(armRightRoot.localRotation, baseArmRightRot, k);
            if (armWeaponRoot) armWeaponRoot.localRotation = Quaternion.Slerp(armWeaponRoot.localRotation, baseArmLeftRot, k);
        }

    }

    static float ComputeFootY(float sinVal, float lift, float down)
    {
        float u = Mathf.Clamp01((sinVal + 1f) * 0.5f);
        float smooth = u * u * (3f - 2f * u);

        if (sinVal > 0f) return smooth * lift;
        return -smooth * down;
    }

    void ApplyFlip(float face)
    {
        float sx = Mathf.Abs(visual.localScale.x);
        visual.localScale = new Vector3(sx * face, visual.localScale.y, -visual.localScale.z);
    }

    static float Smooth(float current, float target, float smooth)
    {
        return Mathf.Lerp(current, target, 1f - Mathf.Exp(-smooth * Time.deltaTime));
    }

    static float Smooth01(float x)
    {
        x = Mathf.Clamp01(x);
        return x * x * (3f - 2f * x);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
