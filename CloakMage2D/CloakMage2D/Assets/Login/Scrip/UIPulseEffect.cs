using UnityEngine;

public class UIPulseEffect : MonoBehaviour
{
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.05f;

    private Vector3 startScale;

    void Start()
    {
        startScale = transform.localScale;
    }

    void Update()
    {
        float scale = 1 + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = startScale * scale;
    }
}
