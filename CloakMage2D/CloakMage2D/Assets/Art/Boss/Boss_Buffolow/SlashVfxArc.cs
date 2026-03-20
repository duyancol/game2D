using UnityEngine;

public class SlashVfxArc : MonoBehaviour
{
    [Header("Life")]
    public float lifeTime = 0.3f;

    [Header("Scale Expand")]
    public bool expand = true;
    public float expandTime = 0.12f;
    public float expandMulX = 1.3f;   // giãn ngang theo hướng chém
    public float expandMulY = 1.1f;

    [Header("Fade")]
    public bool fadeOut = true;

    [Header("Rotation Follow")]
    public bool autoRotate = true;    // xoay theo hướng chém

    SpriteRenderer sr;

    Vector3 baseScale;
    float timer;
    int facingDir = 1;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        baseScale = transform.localScale;
    }

    public void Setup(int dir, float angleZ = 0f)
    {
        facingDir = (dir >= 0) ? 1 : -1;

        ApplyScale(1f);

        if (autoRotate)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, angleZ);
        }

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        timer += Time.deltaTime;

        float lifePercent = Mathf.Clamp01(timer / Mathf.Max(0.0001f, lifeTime));

        // ===== EXPAND EFFECT =====
        if (expand)
        {
            float p = Mathf.Clamp01(timer / Mathf.Max(0.0001f, expandTime));
            float curve = Mathf.Sin(p * Mathf.PI * 0.5f); // nở mượt

            ApplyScale(curve);
        }

        // ===== FADE OUT =====
        if (fadeOut && sr != null)
        {
            Color c = sr.color;
            c.a = 1f - lifePercent;
            sr.color = c;
        }
    }

    void ApplyScale(float expandPercent)
    {
        float mulX = Mathf.Lerp(1f, expandMulX, expandPercent);
        float mulY = Mathf.Lerp(1f, expandMulY, expandPercent);

        Vector3 s = new Vector3(
            Mathf.Abs(baseScale.x) * mulX * facingDir,
            baseScale.y * mulY,
            baseScale.z
        );

        transform.localScale = s;
    }
}
