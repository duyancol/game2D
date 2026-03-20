
using System.Collections;
using UnityEngine;

public class BossHitFX : MonoBehaviour
{
    [Header("Shake (Run)")]
    public Transform visualRoot;
    public float shakeAmount = 0.08f;
    public float shakeTime = 0.15f;

    [Header("Flash (Overlay Red)")]
    public Color overlayColor = new Color(1f, 0.1f, 0.1f, 0.75f); // đỏ + alpha
    public float flashTime = 0.06f;
    public int flashCount = 3;
    public int overlayOrderOffset = 50; // đè lên trên
    [Header("Stun (interrupt attack)")]
    public float stunTime = 0.25f;     // thời gian ngắt nhịp attack
    BossStun stun;
    SpriteRenderer[] baseR;
    SpriteRenderer[] overlayR;
    Coroutine co;

    void Awake()
    {
        if (visualRoot == null) visualRoot = transform;

        baseR = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);

        // tạo overlay renderers (copy sprite) để chớp đỏ
        overlayR = new SpriteRenderer[baseR.Length];
        for (int i = 0; i < baseR.Length; i++)
        {
            var r = baseR[i];
            if (r == null) continue;

            var go = new GameObject(r.gameObject.name + "_HIT_OVERLAY");
            go.transform.SetParent(r.transform, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            var o = go.AddComponent<SpriteRenderer>();
            o.sprite = r.sprite;
            o.sortingLayerID = r.sortingLayerID;
            o.sortingOrder = r.sortingOrder + overlayOrderOffset;
            o.color = overlayColor;
            o.enabled = false;

            overlayR[i] = o;
        }
        stun = GetComponentInParent<BossStun>();
    }

    void LateUpdate()
    {
        // overlay phải bám sprite hiện tại (nếu boss đổi sprite/animation)
        if (overlayR == null || baseR == null) return;
        for (int i = 0; i < baseR.Length; i++)
        {
            if (baseR[i] == null || overlayR[i] == null) continue;
            overlayR[i].sprite = baseR[i].sprite;
        }
    }

    public void PlayHit()
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(HitRoutine());
        if (stun != null) stun.Stun(stunTime);
    }

    IEnumerator HitRoutine()
    {
        Vector3 basePos = visualRoot.localPosition;
        float shakeEnd = Time.time + shakeTime;

        for (int k = 0; k < flashCount; k++)
        {
            SetOverlay(true);
            yield return FlashAndShake(basePos, shakeEnd);

            SetOverlay(false);
            yield return FlashAndShake(basePos, shakeEnd);
        }

        visualRoot.localPosition = basePos;
        SetOverlay(false);
        co = null;
    }

    IEnumerator FlashAndShake(Vector3 basePos, float shakeEnd)
    {
        float t = 0f;
        while (t < flashTime)
        {
            if (Time.time < shakeEnd)
            {
                float x = Random.Range(-shakeAmount, shakeAmount);
                float y = Random.Range(-shakeAmount, shakeAmount);
                visualRoot.localPosition = basePos + new Vector3(x, y, 0f);
            }
            t += Time.deltaTime;
            yield return null;
        }
    }

    void SetOverlay(bool on)
    {
        for (int i = 0; i < overlayR.Length; i++)
            if (overlayR[i] != null) overlayR[i].enabled = on;
    }
}
