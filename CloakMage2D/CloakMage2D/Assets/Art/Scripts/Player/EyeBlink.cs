using UnityEngine;
using System.Collections;

public class EyeBlink : MonoBehaviour
{
    [Header("Eye Sprites")]
    public Sprite eyesOpen;
    public Sprite eyesClosed;

    [Header("Blink Timing")]
    public float minBlinkInterval = 2.5f; // thời gian chờ tối thiểu
    public float maxBlinkInterval = 5f;   // thời gian chờ tối đa
    public float blinkDuration = 0.12f;   // thời gian nhắm mắt

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = eyesOpen;
    }

    void Start()
    {
        StartCoroutine(BlinkLoop());
    }

    IEnumerator BlinkLoop()
    {
        while (true)
        {
            // Đợi ngẫu nhiên
            float waitTime = Random.Range(minBlinkInterval, maxBlinkInterval);
            yield return new WaitForSeconds(waitTime);

            // Nhắm mắt
            sr.sprite = eyesClosed;
            yield return new WaitForSeconds(blinkDuration);

            // Mở mắt
            sr.sprite = eyesOpen;
        }
    }
}
