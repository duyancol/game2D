using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [Header("Rotate Settings")]
    public float rotateSpeed = 180f; // độ / giây
    public bool clockwise = true;    // quay theo chiều kim đồng hồ

    void Update()
    {
        float direction = clockwise ? -1f : 1f;
        transform.Rotate(0f, 0f, rotateSpeed * direction * Time.deltaTime);
    }
}
