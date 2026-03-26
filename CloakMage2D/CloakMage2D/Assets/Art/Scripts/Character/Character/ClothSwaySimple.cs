using UnityEngine;

public class ClothSwaySimple : MonoBehaviour
{
    [Header("Target")]
    public Transform root; // nhân vật (player)

    [Header("Sway Settings")]
    public float swayAmount = 15f;     // độ lắc
    public float swaySpeed = 3f;       // tốc độ lắc

    [Header("Follow Movement")]
    public float followSpeed = 5f;     // độ trễ theo hướng di chuyển

    private Vector3 lastPos;
    private float swayOffset;

    void Start()
    {
        if (root == null) root = transform.root;
        lastPos = root.position;
        swayOffset = Random.Range(0f, 100f); // lệch pha cho tự nhiên
    }

    void Update()
    {
        // tính velocity
        Vector3 velocity = (root.position - lastPos) / Time.deltaTime;
        lastPos = root.position;

        // sway idle
        float sway = Mathf.Sin(Time.time * swaySpeed + swayOffset) * swayAmount;

        // sway theo movement (ngược hướng chạy)
        float movementInfluence = Mathf.Clamp(velocity.magnitude, 0, 5f);
        float targetZ = sway - movementInfluence * swayAmount;

        // apply rotation mượt
        Quaternion targetRot = Quaternion.Euler(0, 0, targetZ);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRot, Time.deltaTime * followSpeed);
    }
}