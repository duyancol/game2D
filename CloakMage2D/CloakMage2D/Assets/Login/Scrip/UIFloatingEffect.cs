using UnityEngine;

public class UIFloatingEffect : MonoBehaviour
{
    public float floatSpeed = 1.5f;
    public float floatAmount = 10f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float newY = Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.localPosition = startPos + new Vector3(0, newY, 0);
    }
}
