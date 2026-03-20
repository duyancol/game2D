using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ProjectileRotateFlip2D : MonoBehaviour
{
    public bool spritePointsRight = true; // sprite gốc hướng sang phải?
    public bool spritePointsUp = false;   // sprite gốc hướng lên?

    public float minSpeed = 0.05f;

    Rigidbody2D rb;
    SpriteRenderer sr;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        Vector2 v = rb.linearVelocity;
        if (v.sqrMagnitude < minSpeed * minSpeed) return;

        // 1) Xoay theo hướng bay
        float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;

        // Nếu sprite gốc hướng lên thì trừ 90 độ để "mũi" theo velocity
        if (spritePointsUp) angle -= 90f;

        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 2) Flip cho đúng trái/phải (để mũi không bị ngược)
        // Khi bay sang trái thì lật Y (mirror ngang)
        if (sr != null)
            sr.flipY = (v.x < 0f);
    }
}
