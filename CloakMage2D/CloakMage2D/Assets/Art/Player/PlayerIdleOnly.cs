using UnityEngine;

public class PlayerIdleOnly : MonoBehaviour
{
    [Header("References Root")]
    public Transform visual; // Player/Visual

    [Header("Parts")]
    public Transform body;
    public Transform head;

    [Tooltip("Tay phải (tay trống)")]
    public Transform armRightRoot;

    [Tooltip("Tay trái (tay còn lại / tay cầm vũ khí nếu m gắn ở đây)")]
    public Transform armWeaponRoot;

    [Header("Flip")]
    public bool faceRightByDefault = true;

    [Header("Idle Bob")]
    public float idleBobAmp = 0.03f;
    public float idleBobSpeed = 2f;

    [Header("Idle Head Nod")]
    public float idleHeadNodAmp = 2.0f;
    public float idleHeadNodSpeed = 1.6f;

    [Header("Idle Arms Wave")]
    public float idleArmWaveAmp = 10f;      // độ lắc tay (độ)
    public float idleArmWaveSpeed = 2.2f;   // tốc độ lắc
    public float idleArmWaveOffset = 0.6f;  // lệch pha giữa 2 tay
    public float idleArmSmooth = 16f;       // mượt tay lúc idle

    [Header("Smoothing")]
    public float poseSmooth = 14f;

    // base cache
    Vector3 baseVisualPos;
    Quaternion baseBodyRot, baseHeadRot;
    Quaternion baseArmRightRot, baseArmLeftRot;
    float lastFace = 1f;

    void Awake()
    {
        if (visual == null) visual = transform;

        baseVisualPos = visual.localPosition;
        baseBodyRot = body ? body.localRotation : Quaternion.identity;
        baseHeadRot = head ? head.localRotation : Quaternion.identity;

        baseArmRightRot = armRightRoot ? armRightRoot.localRotation : Quaternion.identity;
        baseArmLeftRot = armWeaponRoot ? armWeaponRoot.localRotation : Quaternion.identity;

        lastFace = faceRightByDefault ? 1f : -1f;
        ApplyFlip(lastFace);
    }

    void Update()
    {
        ApplyIdlePose();
    }

    void ApplyIdlePose()
    {
        float t = Time.time;

        // === Bob ===
        float targetBob = idleBobAmp * Mathf.Sin(t * idleBobSpeed);
        float curBobY = Smooth(visual.localPosition.y - baseVisualPos.y, targetBob, poseSmooth);
        visual.localPosition = baseVisualPos + new Vector3(0f, curBobY, 0f);

        // === Head nod ===
        if (head)
        {
            float nod = idleHeadNodAmp * Mathf.Sin(t * idleHeadNodSpeed);
            head.localRotation = Quaternion.Euler(0f, 0f, nod) * baseHeadRot;
        }

        // === Arms wave ===
        float waveR = Mathf.Sin(t * idleArmWaveSpeed) * idleArmWaveAmp;
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

        // (tuỳ chọn) body về base nếu bạn sợ lệch
        if (body)
        {
            float kk = 1f - Mathf.Exp(-poseSmooth * Time.deltaTime);
            body.localRotation = Quaternion.Slerp(body.localRotation, baseBodyRot, kk);
        }
    }

    public void SetFace(bool faceRight)
    {
        lastFace = faceRight ? 1f : -1f;
        ApplyFlip(lastFace);
    }

    void ApplyFlip(float face)
    {
        float sx = Mathf.Abs(visual.localScale.x);
        visual.localScale = new Vector3(sx * face, visual.localScale.y, visual.localScale.z);
    }

    static float Smooth(float current, float target, float smooth)
    {
        return Mathf.Lerp(current, target, 1f - Mathf.Exp(-smooth * Time.deltaTime));
    }
}
