using UnityEngine;

public class RotateAura : MonoBehaviour
{
    public float speed = 120f;

    void Update()
    {
        transform.Rotate(0, 0, speed * Time.deltaTime);
    }
}