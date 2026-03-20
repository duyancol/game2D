using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class Arm3PartBowAttack : MonoBehaviour
{
    [Header("Refs")]
    public Transform armPivot;      // ArmPivot
    public Transform aimPoint;      // PlayerAimPoint (optional)
    public Camera cam;             // null = Camera.main

    [Header("Aim / Flip")]
    public bool enableFlip = true;
    public bool flipByScaleX = true;

    [Header("Bow Shoot (Hold -> Release)")]
    public KeyCode attackKey = KeyCode.Mouse0;

    [Tooltip("Góc kéo cung (độ). Âm = lùi về sau một chút.")]
    public float drawDeg = -12f;

    [Tooltip("Góc giật khi thả (độ). Dương = hất nhẹ về trước.")]
    public float releaseKickDeg = 8f;

    [Tooltip("Thời gian kéo cung tới drawDeg")]
    public float drawTime = 0.08f;

    [Tooltip("Thời gian giật khi thả")]
    public float releaseTime = 0.05f;

    [Tooltip("Thời gian hồi về aim")]
    public float recoverTime = 0.06f;

    [Tooltip("Giữ tối đa bao lâu để charge đầy (0..1)")]
    public float maxChargeTime = 0.6f;

    [System.Serializable] public class FloatEvent : UnityEvent<float> { }
    [Header("Event")]
    public FloatEvent OnShoot; // charge01 (0..1)

    bool drawing;
    bool attacking;
    float baseAimZ;
    bool facingLeft;
    Vector3 baseScale;

    float chargeT; // thời gian đang giữ

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (armPivot == null) armPivot = transform;
        baseScale = transform.localScale;
    }

    void Update()
    {
        if (cam == null) cam = Camera.main;

        Vector3 target = GetTargetWorld();
        facingLeft = target.x < armPivot.position.x;

        // 1) Flip cả cụm
        if (enableFlip)
        {
            var s = baseScale;
            if (flipByScaleX) s.x = facingLeft ? -Mathf.Abs(s.x) : Mathf.Abs(s.x);
            else s.y = facingLeft ? -Mathf.Abs(s.y) : Mathf.Abs(s.y);
            transform.localScale = s;
        }

        // 2) Aim
        baseAimZ = GetAimAngleZ();
        float aimZ = facingLeft ? baseAimZ + 180f : baseAimZ;

        // Không attack thì luôn aim theo chuột
        if (!attacking && !drawing)
            armPivot.localRotation = Quaternion.Euler(0, 0, aimZ);

        // 3) Hold to draw
        if (Input.GetKeyDown(attackKey) && !attacking)
        {
            drawing = true;
            chargeT = 0f;
            StopAllCoroutines(); // tránh kẹt routine cũ
            StartCoroutine(DrawRoutine(facingLeft, aimZ));
        }

        if (drawing)
        {
            chargeT += Time.deltaTime;
            // Trong lúc giữ vẫn cho aim theo chuột “mềm”
            QBowAimFollow(aimZ);
        }

        // 4) Release to shoot
        if (drawing && Input.GetKeyUp(attackKey))
        {
            drawing = false;
            if (!attacking)
            {
                float charge01 = maxChargeTime <= 0f ? 1f : Mathf.Clamp01(chargeT / maxChargeTime);
                StartCoroutine(ReleaseRoutine(facingLeft, aimZ, charge01));
            }
        }
    }

    void QBowAimFollow(float aimZ)
    {
        // giữ bow hơi “dính” theo chuột để không giật
        float cur = NormalizeAngle(armPivot.localEulerAngles.z);
        float next = Mathf.LerpAngle(cur, aimZ, 18f * Time.deltaTime);
        armPivot.localRotation = Quaternion.Euler(0, 0, next);
    }

    IEnumerator DrawRoutine(bool left, float aimZ)
    {
        // Kéo cung: lùi nhẹ theo hướng ngược lại (đảo dấu khi qua trái)
        float signedDraw = left ? -drawDeg : drawDeg;
        yield return RotateTo(aimZ + signedDraw, drawTime);
        // Sau khi kéo xong, Update() sẽ tiếp tục follow aim trong lúc giữ
    }

    IEnumerator ReleaseRoutine(bool left, float aimZ, float charge01)
    {
        attacking = true;

        // Giật khi thả: đẩy nhẹ về trước (đảo dấu khi qua trái)
        float signedKick = left ? -releaseKickDeg : releaseKickDeg;

        // 1) giật release
        yield return RotateTo(aimZ + signedKick, releaseTime);

        // 2) bắn (gọi event)
        if (OnShoot != null) OnShoot.Invoke(charge01);

        // 3) hồi về aim
        yield return RotateTo(aimZ, recoverTime);

        attacking = false;
    }

    IEnumerator RotateTo(float targetZ, float time)
    {
        if (time <= 0f)
        {
            armPivot.localRotation = Quaternion.Euler(0, 0, targetZ);
            yield break;
        }

        float startZ = NormalizeAngle(armPivot.localEulerAngles.z);
        targetZ = NormalizeAngle(targetZ);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / time;
            float z = Mathf.LerpAngle(startZ, targetZ, t);
            armPivot.localRotation = Quaternion.Euler(0, 0, z);
            yield return null;
        }
    }

    float GetAimAngleZ()
    {
        Vector3 target = GetTargetWorld();
        Vector2 dir = target - armPivot.position;
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

    Vector3 GetTargetWorld()
    {
        if (aimPoint != null) return aimPoint.position;
        if (cam == null) return armPivot.position;

        Vector3 w = cam.ScreenToWorldPoint(Input.mousePosition);
        w.z = 0f;
        return w;
    }

    float NormalizeAngle(float a)
    {
        while (a > 180f) a -= 360f;
        while (a < -180f) a += 360f;
        return a;
    }
}
