using UnityEngine;

public class ChargeBarWorld : MonoBehaviour
{
    public SpriteRenderer fill;
    public Transform followTarget;
    public Vector3 worldOffset;

    Vector3 fillBaseScale;

    void Awake()
    {
        if (fill != null) fillBaseScale = fill.transform.localScale;
    }

    void LateUpdate()
    {
        if (followTarget != null)
            transform.position = followTarget.position + worldOffset;
    }

    public void Set01(float t)
    {
        t = Mathf.Clamp01(t);
        if (fill == null) return;

        var s = fillBaseScale;
        s.x = Mathf.Max(0.0001f, fillBaseScale.x * t);
        fill.transform.localScale = s;
    }
}
