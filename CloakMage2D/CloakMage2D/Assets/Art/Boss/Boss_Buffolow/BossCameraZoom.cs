
using Unity.Cinemachine;
using UnityEngine;

public class BossCameraZoom : MonoBehaviour
{
    public CinemachineCamera vcam;

    public float normalSize = 5f;
    public float bossSize = 8f;
    public float zoomSpeed = 3f;

    float targetSize;

    void Start()
    {
        targetSize = normalSize;
    }

    void Update()
    {
        var lens = vcam.Lens;
        lens.OrthographicSize = Mathf.Lerp(
            lens.OrthographicSize,
            targetSize,
            Time.deltaTime * zoomSpeed
        );
        vcam.Lens = lens;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            targetSize = bossSize;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            targetSize = normalSize;
    }
}