using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarUI : MonoBehaviour
{
    public BossHealth bossHealth;

    [Header("UI")]
    public Image fill;
    public CanvasGroup group;

    void Awake()
    {
        if (!group)
            group = GetComponent<CanvasGroup>();

        SetVisible(true);
    }

    void OnEnable()
    {
        if (bossHealth != null)
            bossHealth.onHpChanged.AddListener(OnHpChanged);
    }

    void OnDisable()
    {
        if (bossHealth != null)
            bossHealth.onHpChanged.RemoveListener(OnHpChanged);
    }

    void OnHpChanged(int hp, int maxHp)
    {
        float r = (maxHp <= 0) ? 0f : (float)hp / maxHp;

        if (fill)
            fill.fillAmount = r;

        // Nếu boss chết thì ẩn
        if (hp <= 0)
            SetVisible(false);
        else
            SetVisible(true);
    }

    void SetVisible(bool v)
    {
        if (!group) return;

        group.alpha = v ? 1f : 0f;
        group.blocksRaycasts = v;
        group.interactable = v;
    }
    public Vector3 offset = new Vector3(0, -1.5f, 0);

    void LateUpdate()
    {
        if (bossHealth == null) return;

        transform.position = bossHealth.transform.position + offset;
    }
}