using UnityEngine;

public class ButterflyAnimation : MonoBehaviour
{
    public float baseMoveSpeed = 2f;
    public float baseRangeX = 3f;
    public float baseFlyHeight = 0.5f;
    public float baseFlyFrequency = 2f;

    private float moveSpeed;
    private float rangeX;
    private float flyHeight;
    private float flyFrequency;

    private float minX;
    private float maxX;

    private int direction;
    private Vector3 startPos;
    private SpriteRenderer spriteRenderer;

    private float randomOffset; // lệch phase sin

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPos = transform.position;

        // Random hóa mỗi con
        moveSpeed = baseMoveSpeed * Random.Range(0.8f, 1.2f);
        rangeX = baseRangeX * Random.Range(0.7f, 1.3f);
        flyHeight = baseFlyHeight * Random.Range(0.7f, 1.3f);
        flyFrequency = baseFlyFrequency * Random.Range(0.8f, 1.2f);

        randomOffset = Random.Range(0f, 100f);

        direction = Random.value > 0.5f ? 1 : -1;

        minX = startPos.x - rangeX;
        maxX = startPos.x + rangeX;
    }

    void Update()
    {
        Move();
    }

    void Move()
    {
        Vector3 pos = transform.position;

        pos.x += direction * moveSpeed * Time.deltaTime;

        if (pos.x >= maxX)
        {
            pos.x = maxX;
            direction = -1;
            spriteRenderer.flipX = true;
        }
        else if (pos.x <= minX)
        {
            pos.x = minX;
            direction = 1;
            spriteRenderer.flipX = false;
        }

        // Quan trọng: thêm randomOffset
        pos.y = startPos.y + Mathf.Sin((Time.time + randomOffset) * flyFrequency) * flyHeight;

        transform.position = pos;
    }
}