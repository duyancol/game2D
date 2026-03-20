//using UnityEngine;
//using TMPro;

//public class DamagePopup : MonoBehaviour
//{
//    [Header("Refs")]
//    public TMP_Text text;
//    public SpriteRenderer iconRenderer;

//    [Header("Motion")]
//    public float floatSpeed = 1.4f;
//    public float lifeTime = 0.7f;
//    public float randomX = 0.25f;

//    [Header("Text Color")]
//    public Color normalColor = Color.white;
//    public Color critColor = Color.red;

//    [Header("Icons")]
//    public Sprite normalIcon;
//    public Sprite critIcon;

//    [Header("Scale")]
//    public float normalScale = 1f;
//    public float critScale = 1.25f;

//    [Header("Crit Text")]
//    public float critFontScale = 1.15f;
//    public float critExtraPadding = 12f; // tăng bề ngang rect để không cắt số

//    Vector3 _vel;

//    float _baseFontSize;
//    RectTransform _rt;
//    Vector2 _baseSizeDelta;
//    bool _baseAutoSize;
//    float _baseOutline;

//    void Awake()
//    {
//        if (text == null) text = GetComponentInChildren<TMP_Text>();

//        _rt = text != null ? text.rectTransform : null;

//        if (text != null)
//        {
//            _baseFontSize = text.fontSize;
//            _baseAutoSize = text.enableAutoSizing;
//            _baseOutline = text.outlineWidth;
//        }

//        if (_rt != null) _baseSizeDelta = _rt.sizeDelta;

//        _vel = new Vector3(Random.Range(-randomX, randomX), 1f, 0f) * floatSpeed;
//    }

//    public void Setup(int amount, bool isCrit)
//    {
//        if (text == null) return;

//        // luôn reset về base trước
//        text.enableAutoSizing = _baseAutoSize;
//        text.fontSize = _baseFontSize;
//        text.outlineWidth = _baseOutline;
//        if (_rt != null) _rt.sizeDelta = _baseSizeDelta;

//        text.text = amount.ToString();

//        if (isCrit)
//        {
//            text.color = critColor;
//            text.fontStyle = FontStyles.Bold | FontStyles.Italic;
//            text.fontWeight = FontWeight.Black;

//            text.outlineWidth = 0.18f;
//            text.outlineColor = Color.black;

//            // tăng size chữ
//            text.fontSize = _baseFontSize * critFontScale;

//            // tăng bề ngang rect để không bị cắt chữ số
//            if (_rt != null)
//                _rt.sizeDelta = new Vector2(_baseSizeDelta.x + critExtraPadding, _baseSizeDelta.y);

//            // (tùy chọn) ép TMP cập nhật mesh ngay
//            text.ForceMeshUpdate();

//            if (iconRenderer && critIcon)
//            {
//                iconRenderer.sprite = critIcon;
//                iconRenderer.transform.localScale = Vector3.one * critScale;
//                iconRenderer.color = Color.white;
//            }
//        }
//        else
//        {
//            text.color = normalColor;
//            text.fontStyle = FontStyles.Normal;
//            text.fontWeight = FontWeight.Regular;
//            text.outlineWidth = 0f;

//            if (iconRenderer && normalIcon)
//            {
//                iconRenderer.sprite = normalIcon;
//                iconRenderer.transform.localScale = Vector3.one * normalScale;
//                iconRenderer.color = Color.white;
//            }
//        }
//    }

//    void Update()
//    {
//        transform.position += _vel * Time.deltaTime;

//        lifeTime -= Time.deltaTime;
//        if (lifeTime <= 0f) Destroy(gameObject);
//    }
//}
using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [Header("Refs")]
    public TMP_Text text;
    public SpriteRenderer iconRenderer;

    [Header("Motion")]
    public float floatSpeed = 1.6f;
    public float lifeTime = 0.8f;
    public float randomX = 0.3f;

    [Header("Fade")]
    public float fadeSpeed = 2f;

    [Header("Text Color")]
    public Color normalColor = Color.white;
    public Color critColor = Color.red;

    [Header("Icons")]
    public Sprite normalIcon;
    public Sprite critIcon;

    [Header("Scale")]
    public float normalScale = 1f;
    public float critScale = 1.35f;

    [Header("Crit Text")]
    public float critFontScale = 1.2f;
    public float critExtraPadding = 12f;

    Vector3 _vel;
    float _timer;

    float _baseFontSize;
    RectTransform _rt;
    Vector2 _baseSizeDelta;
    bool _baseAutoSize;
    float _baseOutline;

    void Awake()
    {
        if (text == null) text = GetComponentInChildren<TMP_Text>();

        _rt = text != null ? text.rectTransform : null;

        if (text != null)
        {
            _baseFontSize = text.fontSize;
            _baseAutoSize = text.enableAutoSizing;
            _baseOutline = text.outlineWidth;
        }

        if (_rt != null) _baseSizeDelta = _rt.sizeDelta;

        // random hướng bay
        _vel = new Vector3(Random.Range(-randomX, randomX), 1f, 0f) * floatSpeed;

        // xoay nhẹ cho tự nhiên
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(-10f, 10f));
    }

    public void Setup(int amount, bool isCrit)
    {
        if (text == null) return;

        text.enableAutoSizing = _baseAutoSize;
        text.fontSize = _baseFontSize;
        text.outlineWidth = _baseOutline;

        if (_rt != null) _rt.sizeDelta = _baseSizeDelta;

        text.text = amount.ToString();

        if (isCrit)
        {
            text.color = critColor;
            text.fontStyle = FontStyles.Bold | FontStyles.Italic;
            text.fontWeight = FontWeight.Black;

            text.outlineWidth = 0.18f;
            text.outlineColor = Color.black;

            text.fontSize = _baseFontSize * critFontScale;

            if (_rt != null)
                _rt.sizeDelta = new Vector2(_baseSizeDelta.x + critExtraPadding, _baseSizeDelta.y);

            text.ForceMeshUpdate();

            transform.localScale = Vector3.one * critScale;

            if (iconRenderer && critIcon)
            {
                iconRenderer.sprite = critIcon;
                iconRenderer.transform.localScale = Vector3.one * critScale;
            }
        }
        else
        {
            text.color = normalColor;
            text.fontStyle = FontStyles.Normal;
            text.fontWeight = FontWeight.Regular;

            transform.localScale = Vector3.one * normalScale;

            if (iconRenderer && normalIcon)
            {
                iconRenderer.sprite = normalIcon;
                iconRenderer.transform.localScale = Vector3.one * normalScale;
            }
        }
    }

    void Update()
    {
        _timer += Time.deltaTime;

        // bay lên + chậm dần
        transform.position += _vel * Time.deltaTime;
        _vel *= 0.95f;

        // scale nhỏ dần khi gần biến mất
        transform.localScale *= 0.995f;

        // fade out
        if (text != null)
        {
            Color c = text.color;
            c.a -= fadeSpeed * Time.deltaTime;
            text.color = c;
        }

        if (_timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }
}