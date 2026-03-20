using UnityEngine;

public class Parallax : MonoBehaviour
{
    public float parallaxSpeed;
    private Transform cameraTransform;
    private Vector3 previousCameraPosition;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        previousCameraPosition = cameraTransform.position;
    }

    void LateUpdate()
    {
        Vector3 delta = cameraTransform.position - previousCameraPosition;
        transform.position += new Vector3(delta.x * parallaxSpeed, delta.y * parallaxSpeed, 0);
        previousCameraPosition = cameraTransform.position;
    }
}