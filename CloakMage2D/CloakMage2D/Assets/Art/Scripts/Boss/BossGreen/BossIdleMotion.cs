//using UnityEngine;

//public class BossIdleMotion : MonoBehaviour
//{
//    [Header("Parts")]
//    public Transform body;
//    public Transform head;
//    public Transform armL;
//    public Transform armR;
//    public Transform feet;

//    [Header("Idle Motion")]
//    public float bodyBobAmount = 0.03f;
//    public float bodyBobSpeed = 2f;

//    public float headRotateAmount = 2f;
//    public float armRotateAmount = 12f;

//    [Header("Feet Motion")]
//    public float feetSquashAmount = 0.015f; // nhún nhẹ
//    public float feetRotateAmount = 1f;     // xoay rất nhỏ

//    Vector3 bodyStartPos;
//    Vector3 feetStartPos;

//    Quaternion headStartRot;
//    Quaternion armLStartRot;
//    Quaternion armRStartRot;
//    Quaternion feetStartRot;

//    void Start()
//    {
//        if (body) bodyStartPos = body.localPosition;
//        if (feet) feetStartPos = feet.localPosition;

//        if (head) headStartRot = head.localRotation;
//        if (armL) armLStartRot = armL.localRotation;
//        if (armR) armRStartRot = armR.localRotation;
//        if (feet) feetStartRot = feet.localRotation;
//    }

//    void Update()
//    {
//        float t = Time.time;
//        float sin = Mathf.Sin(t * bodyBobSpeed);

//        // BODY thở
//        if (body)
//        {
//            body.localPosition =
//                bodyStartPos + Vector3.up * sin * bodyBobAmount;
//        }

//        // HEAD gật
//        if (head)
//        {
//            head.localRotation =
//                headStartRot * Quaternion.Euler(0, 0, sin * headRotateAmount);
//        }

//        // ARM rung
//        if (armL)
//        {
//            armL.localRotation =
//                armLStartRot * Quaternion.Euler(0, 0, sin * armRotateAmount);
//        }

//        if (armR)
//        {
//            armR.localRotation =
//                armRStartRot * Quaternion.Euler(0, 0,
//                    Mathf.Sin(t * bodyBobSpeed + Mathf.PI) * armRotateAmount);
//        }

//        // FEET nhún + xoay cực nhẹ
//        if (feet)
//        {
//            feet.localRotation =
//                 armRStartRot * Quaternion.Euler(0, 0,
//                    Mathf.Sin(t * bodyBobSpeed + Mathf.PI) * armRotateAmount);
//        }
//    }
//}
//using System.Collections;
//using UnityEngine;

//public class BossIdleMotion : MonoBehaviour
//{
//    [Header("Parts")]
//    public Transform body;
//    public Transform head;
//    public Transform armL;
//    public Transform armR;
//    public Transform feet;
//    public Transform weaponPoint;

//    [Header("Idle Motion")]
//    public float bodyBobAmount = 0.03f;
//    public float bodyBobSpeed = 2f;

//    public float headRotateAmount = 2f;
//    public float armRotateAmount = 12f;

//    [Header("Feet Motion")]
//    public float feetSquashAmount = 0.015f; // nhún nhẹ
//    public float feetRotateAmount = 1f;     // xoay rất nhỏ

//    [Header("Attack Swing")]
//    public float attackSwingAngle = 65f;      // quật mạnh
//    public float attackSwingDuration = 0.10f; // quật nhanh
//    public bool swingArmRight = true;         // quật tay phải (armR)

//    [Header("Attack Shake")]
//    public float shakeDuration = 0.15f;
//    public float shakePosAmount = 0.08f;     // rung mạnh hơn: tăng số này
//    public float shakeRotAmount = 8f;        // rung xoay
//    public float shakeFrequency = 40f;       // rung nhanh

//    Vector3 bodyStartPos;
//    Vector3 feetStartPos;
//    Vector3 feetStartScale;

//    Quaternion headStartRot;
//    Quaternion armLStartRot;
//    Quaternion armRStartRot;
//    Quaternion feetStartRot;

//    // layer thêm khi attack
//    float shakeTimer;
//    float shakeSeed;
//    float swingTimer;
//    bool isSwinging;

//    void Start()
//    {
//        if (body) bodyStartPos = body.localPosition;
//        if (feet)
//        {
//            feetStartPos = feet.localPosition;
//            feetStartRot = feet.localRotation;
//            feetStartScale = feet.localScale;
//        }

//        if (head) headStartRot = head.localRotation;
//        if (armL) armLStartRot = armL.localRotation;
//        if (armR) armRStartRot = armR.localRotation;
//    }

//    void Update()
//    {
//        float t = Time.time;
//        float sin = Mathf.Sin(t * bodyBobSpeed);

//        // ===== IDLE =====
//        if (body)
//            body.localPosition = bodyStartPos + Vector3.up * sin * bodyBobAmount;

//        if (head)
//            head.localRotation = headStartRot * Quaternion.Euler(0, 0, sin * headRotateAmount);

//        if (armL)
//            armL.localRotation = armLStartRot * Quaternion.Euler(0, 0, sin * armRotateAmount);

//        if (armR)
//            armR.localRotation = armRStartRot * Quaternion.Euler(0, 0,
//                Mathf.Sin(t * bodyBobSpeed + Mathf.PI) * armRotateAmount);

//        // FEET nhún + xoay nhẹ (FIX: dùng feetStartRot & feetRotateAmount)
//        if (feet)
//        {
//            float s = Mathf.Sin(t * bodyBobSpeed);
//            feet.localRotation = feetStartRot * Quaternion.Euler(0, 0, s * feetRotateAmount);

//            // nhún: scale Y nhỏ xíu
//            var sc = feetStartScale;
//            sc.y = feetStartScale.y - Mathf.Abs(s) * feetSquashAmount;
//            feet.localScale = sc;
//        }

//        // ===== ATTACK SWING =====
//        if (isSwinging)
//        {
//            swingTimer += Time.deltaTime;
//            float p = Mathf.Clamp01(swingTimer / attackSwingDuration);

//            // curve quật: lên nhanh - về nhanh
//            float curve = (p < 0.5f)
//                ? Mathf.SmoothStep(0f, 1f, p * 2f)
//                : Mathf.SmoothStep(1f, 0f, (p - 0.5f) * 2f);

//            float ang = attackSwingAngle * curve;

//            Transform arm = swingArmRight ? armR : armL;
//            if (arm)
//            {
//                var baseRot = swingArmRight ? armRStartRot : armLStartRot;
//                arm.localRotation = baseRot * Quaternion.Euler(0, 0, -ang);
//            }

//            if (p >= 1f)
//            {
//                isSwinging = false;
//                swingTimer = 0f;
//            }
//        }

//        // ===== SHAKE (rung mạnh) =====
//        if (shakeTimer > 0f)
//        {
//            shakeTimer -= Time.deltaTime;
//            float k = Mathf.Clamp01(shakeTimer / shakeDuration); // fade out

//            float tt = Time.time * shakeFrequency + shakeSeed;
//            float dx = (Mathf.PerlinNoise(tt, 0.1f) - 0.5f) * 2f;
//            float dy = (Mathf.PerlinNoise(0.2f, tt) - 0.5f) * 2f;
//            float dr = (Mathf.PerlinNoise(tt, tt) - 0.5f) * 2f;

//            Vector3 posOff = new Vector3(dx, dy, 0f) * (shakePosAmount * k);
//            float rotOff = dr * (shakeRotAmount * k);

//            // rung body + head cho cảm giác “giật”
//            if (body) body.localPosition += posOff;
//            if (head) head.localRotation *= Quaternion.Euler(0, 0, rotOff);

//            // rung nhẹ tay để nhìn lực hơn
//            if (armR) armR.localRotation *= Quaternion.Euler(0, 0, rotOff * 0.7f);
//            if (armL) armL.localRotation *= Quaternion.Euler(0, 0, rotOff * 0.4f);
//        }
//    }

//    // GỌI HÀM NÀY KHI DÙNG SKILL
//    public void TriggerAttack()
//    {
//        // nếu weaponPoint là con của Arm_R thì quật tay phải, ngược lại quật tay trái
//        if (weaponPoint != null)
//            swingArmRight = weaponPoint.IsChildOf(armR);

//        isSwinging = true;
//        swingTimer = 0f;

//        shakeTimer = shakeDuration;
//        shakeSeed = Random.Range(0f, 999f);
//    }

//}
using UnityEngine;

public class BossIdleMotion : MonoBehaviour
{
    [Header("Parts")]
    public Transform body;
    public Transform head;
    public Transform armL;
    public Transform armR;
    public Transform feet;
    public Transform weaponPoint;

    [Header("Idle Motion")]
    public float bodyBobAmount = 0.03f;
    public float bodyBobSpeed = 2f;

    public float headRotateAmount = 2f;
    public float armRotateAmount = 12f;

    [Header("Feet Motion")]
    public float feetSquashAmount = 0.015f;
    public float feetRotateAmount = 1f;

    [Header("Walk Left-Right (Đi qua đi lại)")]
    public bool patrol = true;
    public float patrolDistance = 2.0f;   // đi xa bao nhiêu
    public float patrolSpeed = 0.6f;      // đi nhanh/chậm
    public bool faceMoveDir = false;      // bật nếu bạn muốn lật hướng theo chiều đi
    public Transform flipRoot;            // nếu cần lật sprite: kéo Body hoặc Root vào đây

    [Header("Attack Swing")]
    public float attackSwingAngle = 65f;
    public float attackSwingDuration = 0.10f;
    public bool swingArmRight = true;

    [Header("Attack Shake")]
    public float shakeDuration = 0.15f;
    public float shakePosAmount = 0.08f;
    public float shakeRotAmount = 8f;
    public float shakeFrequency = 40f;

    Vector3 rootStartPos;
    Vector3 bodyStartPos;

    Vector3 feetStartPos;
    Vector3 feetStartScale;

    Quaternion headStartRot;
    Quaternion armLStartRot;
    Quaternion armRStartRot;
    Quaternion feetStartRot;

    float shakeTimer;
    float shakeSeed;
    float swingTimer;
    bool isSwinging;

    void Start()
    {
        rootStartPos = transform.position;

        if (body) bodyStartPos = body.localPosition;

        if (feet)
        {
            feetStartPos = feet.localPosition;
            feetStartRot = feet.localRotation;
            feetStartScale = feet.localScale;
        }

        if (head) headStartRot = head.localRotation;
        if (armL) armLStartRot = armL.localRotation;
        if (armR) armRStartRot = armR.localRotation;

        if (!flipRoot) flipRoot = body ? body : transform;
    }

    void Update()
    {
        float t = Time.time;

        // ===== WALK / PATROL (root đi qua đi lại) =====
        if (patrol)
        {
            float xOff = Mathf.Sin(t * patrolSpeed) * patrolDistance;
            transform.position = rootStartPos + new Vector3(xOff, 0f, 0f);

            if (faceMoveDir && flipRoot)
            {
                // xOff > 0 => đi phải, xOff < 0 => đi trái
                float sx = Mathf.Abs(flipRoot.localScale.x);
                var sc = flipRoot.localScale;
                sc.x = (xOff >= 0f) ? sx : -sx;
                flipRoot.localScale = sc;
            }
        }

        float sin = Mathf.Sin(t * bodyBobSpeed);

        // ===== IDLE =====
        if (body)
            body.localPosition = bodyStartPos + Vector3.up * sin * bodyBobAmount;

        if (head)
            head.localRotation = headStartRot * Quaternion.Euler(0, 0, sin * headRotateAmount);

        if (armL)
            armL.localRotation = armLStartRot * Quaternion.Euler(0, 0, sin * armRotateAmount);

        if (armR)
            armR.localRotation = armRStartRot *
                Quaternion.Euler(0, 0, Mathf.Sin(t * bodyBobSpeed + Mathf.PI) * armRotateAmount);

        if (feet)
        {
            float s = Mathf.Sin(t * bodyBobSpeed);
            feet.localRotation = feetStartRot * Quaternion.Euler(0, 0, s * feetRotateAmount);

            var sc = feetStartScale;
            sc.y = feetStartScale.y - Mathf.Abs(s) * feetSquashAmount;
            feet.localScale = sc;
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

    public void TriggerAttack()
    {
        if (weaponPoint != null && armR != null)
            swingArmRight = weaponPoint.IsChildOf(armR);

        isSwinging = true;
        swingTimer = 0f;

        shakeTimer = shakeDuration;
        shakeSeed = Random.Range(0f, 999f);
    }

    // gọi cái này nếu boss bị teleport/respawn để reset điểm gốc đi qua đi lại
    public void ResetPatrolOrigin()
    {
        rootStartPos = transform.position;
    }
}
