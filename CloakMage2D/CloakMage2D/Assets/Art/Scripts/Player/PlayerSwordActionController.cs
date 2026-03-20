using System.Collections;
using UnityEngine;

public class PlayerSwordActionController : MonoBehaviour
{
    [Header("Refs")]
    public Rigidbody2D rb;
    public Transform weaponPivot;     // Visual/arms/WeaponPivot
    public Transform offhandArm;      // Visual/arms/ArmRight (optional)

    [Header("Feel Curves")]
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve rotCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Offhand")]
    public bool alsoMoveOffhand = true;
    [Range(0f, 1f)] public float offhandScale = 0.35f;

    [Header("Lock")]
    public bool lockWhileActing = true;
    public bool IsActing { get; private set; }

    Coroutine _co;
    Quaternion _weaponBase;
    Quaternion _offhandBase;

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        CacheBase();
    }

    void CacheBase()
    {
        if (weaponPivot != null) _weaponBase = weaponPivot.localRotation;
        if (offhandArm != null) _offhandBase = offhandArm.localRotation;
    }

    public void Cancel()
    {
        if (_co != null) StopCoroutine(_co);
        _co = null;
        IsActing = false;

        if (weaponPivot != null) weaponPivot.localRotation = _weaponBase;
        if (offhandArm != null) offhandArm.localRotation = _offhandBase;
    }

    // ===== PUBLIC API =====
    public void PlayHit1_DashStab(bool faceRight, float dashDist, float dashTime,
        float windup, float stabTime, float recover)
    {
        StartAction(Hit1(faceRight, dashDist+0.5f, dashTime + 0.2f, windup, stabTime, recover));
    }

    public void PlayHit2_Slash(bool faceRight, float stepDist, float stepTime,
        float windup, float slashTime, float recover)
    {
        StartAction(Hit2(faceRight, stepDist, stepTime, windup, slashTime + 0.2f, recover));
    }

    public void PlayHit3_JumpSlam(bool faceRight, float jumpUp, float jumpUpTime, float slamDownTime,
        float windup, float slamTime, float recover)
    {
        StartAction(Hit3(faceRight, jumpUp+5, jumpUpTime, slamDownTime, windup, slamTime + 0.2f, recover));
    }

    void StartAction(IEnumerator routine)
    {
        CacheBase();
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(routine);
    }

    // ===== ACTIONS =====

    IEnumerator Hit1(bool faceRight, float dashDist, float dashTime, float windup, float stabTime, float recover)
    {
        IsActing = true;

        float dir = faceRight ? 5f : -5f;

        // 1) windup: kéo tay về chút (anticipation)
        if (windup > 0f)
            yield return RotateWeaponPhased(dir, fromDeg: 0f, toDeg: -18f, windup);

        // 2) dash + stab: body lao + tay “đâm” ra
        Vector2 start = rb.position;
        Vector2 end = start + new Vector2(dir * dashDist, 0f);

        // chạy đồng thời: move + rotate (đâm)
        float t = 0f;
        float time = Mathf.Max(0.01f, dashTime);
        while (t < 1f)
        {
            t += Time.deltaTime / time;
            float e = moveCurve.Evaluate(Mathf.Clamp01(t));
            rb.MovePosition(Vector2.Lerp(start, end, e));

            // đâm: -18 -> +12
            float re = rotCurve.Evaluate(Mathf.Clamp01(t));
            ApplyWeaponRot(dir, Mathf.Lerp(-18f, 12f, re));

            yield return null;
        }

        // 3) hold stabTime (giữ mũi đâm một nhịp)
        if (stabTime > 0f)
        {
            float ht = 0f;
            while (ht < stabTime)
            {
                ht += Time.deltaTime;
                ApplyWeaponRot(dir, 12f);
                yield return null;
            }
        }

        // 4) recover về idle
        if (recover > 0f)
            yield return RotateWeaponPhased(dir, fromDeg: 12f, toDeg: 0f, recover);

        ResetRot();
        IsActing = false;
        _co = null;
    }

    IEnumerator Hit2(bool faceRight, float stepDist, float stepTime, float windup, float slashTime, float recover)
    {
        IsActing = true;
        float dir = faceRight ? 1f : -1f;

        // 1) windup: kéo tay về sâu
        if (windup > 0f)
            yield return RotateWeaponPhased(dir, fromDeg: 0f, toDeg: -60f, windup);

        // 2) step forward nhẹ cho “có lực”
        Vector2 start = rb.position;
        Vector2 end = start + new Vector2(dir * stepDist, 0f);

        float mt = 0f;
        float mtime = Mathf.Max(0.01f, stepTime);
        while (mt < 1f)
        {
            mt += Time.deltaTime / mtime;
            float e = moveCurve.Evaluate(Mathf.Clamp01(mt));
            rb.MovePosition(Vector2.Lerp(start, end, e));
            yield return null;
        }

        // 3) slash: quét ngang -60 -> +90 (arc)
        float t = 0f;
        float time = Mathf.Max(0.01f, slashTime);
        while (t < 1f)
        {
            t += Time.deltaTime / time;
            float e = rotCurve.Evaluate(Mathf.Clamp01(t));
            ApplyWeaponRot(dir, Mathf.Lerp(-60f, 90f, e));
            yield return null;
        }

        // 4) recover: +90 -> 0
        if (recover > 0f)
            yield return RotateWeaponPhased(dir, fromDeg: 90f, toDeg: 0f, recover);

        ResetRot();
        IsActing = false;
        _co = null;
    }

    IEnumerator Hit3(bool faceRight, float jumpUp, float jumpUpTime, float slamDownTime,
        float windup, float slamTime, float recover)
    {
        IsActing = true;
        float dir = faceRight ? 1f : -1f;

        // 1) windup: đưa vũ khí lên cao (charge)
        if (windup > 0f)
            yield return RotateWeaponPhased(dir, fromDeg: 0f, toDeg: 95f, windup);

        Vector2 start = rb.position;
        Vector2 up = start + Vector2.up * jumpUp;

        // 2) jump up: body bay lên + giữ vũ khí cao
        float t1 = 0f;
        float upTime = Mathf.Max(0.01f, jumpUpTime);
        while (t1 < 1f)
        {
            t1 += Time.deltaTime / upTime;
            float e = moveCurve.Evaluate(Mathf.Clamp01(t1));
            rb.MovePosition(Vector2.Lerp(start, up, e));
            ApplyWeaponRot(dir, 95f);
            yield return null;
        }

        // 3) slam down: rơi xuống + bổ -95
        Vector2 down = start + new Vector2(dir * 0.25f, 0f); // nhích nhẹ theo hướng nhìn
        float t2 = 0f;
        float downTime = Mathf.Max(0.01f, slamDownTime);
        while (t2 < 1f)
        {
            t2 += Time.deltaTime / downTime;
            float e = moveCurve.Evaluate(Mathf.Clamp01(t2));
            rb.MovePosition(Vector2.Lerp(up, down, e));

            // bổ: 95 -> -95
            float re = rotCurve.Evaluate(Mathf.Clamp01(t2));
            ApplyWeaponRot(dir, Mathf.Lerp(95f, -95f, re));

            yield return null;
        }

        // 4) giữ một nhịp ở cuối cú bổ
        if (slamTime > 0f)
        {
            float ht = 0f;
            while (ht < slamTime)
            {
                ht += Time.deltaTime;
                ApplyWeaponRot(dir, -95f);
                yield return null;
            }
        }

        // 5) recover về 0
        if (recover > 0f)
            yield return RotateWeaponPhased(dir, fromDeg: -95f, toDeg: 0f, recover);

        ResetRot();
        IsActing = false;
        _co = null;
    }

    // ===== ROT HELPERS =====
    IEnumerator RotateWeaponPhased(float dir, float fromDeg, float toDeg, float time)
    {
        if (weaponPivot == null) yield break;

        time = Mathf.Max(0.01f, time);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / time;
            float e = rotCurve.Evaluate(Mathf.Clamp01(t));
            float deg = Mathf.Lerp(fromDeg, toDeg, e);
            ApplyWeaponRot(dir, deg);
            yield return null;
        }
        ApplyWeaponRot(dir, toDeg);
    }

    void ApplyWeaponRot(float dir, float deg)
    {
        if (weaponPivot != null)
            weaponPivot.localRotation = _weaponBase * Quaternion.Euler(0f, 0f, dir * deg);

        if (alsoMoveOffhand && offhandArm != null)
            offhandArm.localRotation = _offhandBase * Quaternion.Euler(0f, 0f, dir * deg * offhandScale);
    }

    void ResetRot()
    {
        if (weaponPivot != null) weaponPivot.localRotation = _weaponBase;
        if (offhandArm != null) offhandArm.localRotation = _offhandBase;
    }
}
