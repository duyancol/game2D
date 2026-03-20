using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SpiralProjectile : MonoBehaviour
{
    public float speed = 16f;
    public float flipAmount = 60f;    // độ nghiêng
    public float flipSpeed = 15f;
    public Transform visual;

    Rigidbody2D rb;
    

    Vector2 moveDir;
    float baseAngle;
    float time;

    public void Init(Vector2 dir, float moveSpeed)
    {
        rb = GetComponent<Rigidbody2D>();
       

        moveDir = dir.normalized;
        speed = moveSpeed;

        baseAngle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        rb.rotation = baseAngle;
    }

    void FixedUpdate()
    {
        time += Time.fixedDeltaTime;

        // bay thẳng
        rb.linearVelocity = moveDir * speed;

        // lật giả trục X
        float tilt = Mathf.Sin(time * flipSpeed) * flipAmount;
        visual.localRotation = Quaternion.Euler(tilt, 0f, 0f);
    }
}
