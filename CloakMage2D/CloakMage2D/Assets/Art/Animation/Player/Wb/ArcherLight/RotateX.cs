using UnityEngine;

public class RotateX : MonoBehaviour
{
    public float speed = 100f;

    void Update()
    {
        transform.Rotate(speed * Time.deltaTime, 0f, 0f);
    }
}