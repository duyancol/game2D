
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBarUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    PlayerStatsMono stats;

    [Header("UI")]
    public Image fill;
    public CanvasGroup group;

    [Header("Show/Hide")]
    public bool autoHideWhenFull = true;
    public float showTimeAfterHit = 1.5f;

    float timer;

    void Awake()
    {
        if (!group) group = GetComponent<CanvasGroup>();

        if (playerHealth != null)
            stats = playerHealth.GetComponent<PlayerStatsMono>();

        SetVisible(true);
    }

    void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.onHpChanged.AddListener(OnHpChanged);
    }

    void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.onHpChanged.RemoveListener(OnHpChanged);
    }

    void Update()
    {
        if (autoHideWhenFull && stats != null)
        {
            if (timer > 0f)
                timer -= Time.deltaTime;

            bool isFull = stats.hp >= stats.maxHP;

            if (isFull && timer <= 0f)
                SetVisible(false);
        }
    }

    public void OnHpChanged(int hp, int maxHp)
    {
        float r = (maxHp <= 0) ? 0f : (float)hp / maxHp;

        if (fill)
            fill.fillAmount = r;

        timer = showTimeAfterHit;
        SetVisible(true);
    }

    void SetVisible(bool v)
    {
        if (!group) return;

        group.alpha = v ? 1f : 0f;
        group.blocksRaycasts = v;
        group.interactable = v;
    }
}
