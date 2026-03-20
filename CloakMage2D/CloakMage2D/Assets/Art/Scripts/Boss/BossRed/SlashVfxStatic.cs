using UnityEngine;

public class SlashVfxStatic : MonoBehaviour
{
    [Header("Life")]
    public float lifeTime = 0.25f;

    [Header("Appear Punch (optional)")]
    public bool punchScale = true;
    public float punchTime = 0.08f;
    public float punchScaleMul = 1.15f;

    [Header("Fade (optional)")]
    public bool fadeOut = true;

    SpriteRenderer sr;
    Vector3 baseScale;
    float t;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        baseScale = transform.localScale;
    }

    public void Setup(int dir)
    {
        // dir=1: phải, dir=-1: trái
        var s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (dir >= 0 ? 1f : -1f);
        transform.localScale = s;

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        t += Time.deltaTime;

        if (punchScale)
        {
            float p = Mathf.Clamp01(t / Mathf.Max(0.001f, punchTime));
            float k = Mathf.Sin(p * Mathf.PI); // 0->1->0
            transform.localScale = Vector3.Lerp(baseScale, baseScale * punchScaleMul, k);
        }

        if (fadeOut && sr != null)
        {
            float a = 1f - Mathf.Clamp01(t / Mathf.Max(0.001f, lifeTime));
            var c = sr.color;
            c.a = a;
            sr.color = c;
        }
    }
}
