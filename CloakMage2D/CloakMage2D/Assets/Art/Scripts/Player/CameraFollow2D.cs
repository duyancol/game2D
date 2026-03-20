using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Follow")]
    public Transform target;              // kéo Player vào đây
    public Vector3 offset = new Vector3(0, 0, -10);
    public float followSmooth = 10f;

    [Header("Zoom (Orthographic Size)")]
    public Camera cam;
    public float zoomSmooth = 10f;
    public float zoomSize = 4.5f;         // muốn zoom gần thì giảm số này
    public float minZoom = 3f;
    public float maxZoom = 10f;

    void Reset()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (!target) return;
        if (!cam) cam = GetComponent<Camera>();

        // Follow mượt
        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSmooth * Time.deltaTime);

        // Zoom mượt
        if (cam.orthographic)
        {
            float z = Mathf.Clamp(zoomSize, minZoom, maxZoom);
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, z, zoomSmooth * Time.deltaTime);
        }
    }
}
