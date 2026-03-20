using UnityEngine;
using System.Collections;

public class Arm3PartAttack : MonoBehaviour
{
    [Header("Refs")]
    public Transform armPivot;      // kéo ArmPivot vào
    public Transform aimPoint;      // kéo PlayerAimPoint (optional)
    public Camera cam;             // để trống = Camera.main

    [Header("Aim / Flip")]
    [Tooltip("Flip cả cụm tay theo trái/phải")]
    public bool enableFlip = true;

    [Tooltip("Dùng Scale X để flip (khuyên dùng)")]
    public bool flipByScaleX = true;

    [Header("Attack Swing")]
    public KeyCode attackKey = KeyCode.Mouse0;
    public float windupDeg = -15f;
    public float swingDeg = 75f;
    public float windupTime = 0.06f;
    public float swingTime = 0.10f;
    public float recoverTime = 0.08f;

    bool attacking;
    float baseAimZ;
    bool facingLeft;
    Vector3 baseScale;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (armPivot == null) armPivot = transform;
        baseScale = transform.localScale;
    }

    void Update()
    {
        if (cam == null) cam = Camera.main;

        // target + hướng trái/phải
        Vector3 target = GetTargetWorld();
        facingLeft = target.x < armPivot.position.x;

        // 1) Flip cả cụm
        if (enableFlip)
        {
            var s = baseScale;
            if (flipByScaleX)
                s.x = facingLeft ? -Mathf.Abs(s.x) : Mathf.Abs(s.x);
            else
                s.y = facingLeft ? -Mathf.Abs(s.y) : Mathf.Abs(s.y);

            transform.localScale = s;
        }

        // 2) Aim angle
        baseAimZ = GetAimAngleZ();
        float aimZ = facingLeft ? baseAimZ + 180f : baseAimZ;

        // 3) nếu không đang đánh -> luôn hướng theo chuột
        if (!attacking)
            armPivot.localRotation = Quaternion.Euler(0, 0, aimZ);

        // 4) Attack
        if (Input.GetKeyDown(attackKey) && !attacking)
            StartCoroutine(SwingRoutine(facingLeft, aimZ));
    }

    IEnumerator SwingRoutine(bool left, float aimZ)
    {
        attacking = true;

        // Khi flip qua trái, hướng vung cần đảo dấu để nhìn đúng
        float swingSigned = left ? -swingDeg : swingDeg;
        float windupSigned = left ? -windupDeg : windupDeg; // optional, giúp windup cũng tự nhiên

        yield return RotateTo(aimZ + windupSigned, windupTime);
        yield return RotateTo(aimZ + swingSigned, swingTime);
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
