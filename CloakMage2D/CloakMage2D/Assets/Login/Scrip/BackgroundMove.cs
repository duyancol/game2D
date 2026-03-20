using UnityEngine;

public class BackgroundMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float moveAmount = 20f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float newX = Mathf.Sin(Time.time * moveSpeed) * moveAmount;
        transform.localPosition = startPos + new Vector3(newX, 0, 0);
    }
}
