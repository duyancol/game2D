
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    PlayerStatsMono stats;

    [Header("Invincible")]
    public float invincibleTime = 0.25f;
    float invTimer;

    [Header("Events")]
    public UnityEvent<int, int> onHpChanged;
    public UnityEvent onDied;

    void Awake()
    {
        stats = GetComponent<PlayerStatsMono>();

        if (stats == null)
        {
            Debug.LogError("PlayerStatsMono not found!");
            return;
        }

        stats.hp = stats.maxHP;
        onHpChanged?.Invoke(stats.hp, stats.maxHP);
    }

    void Update()
    {
        if (invTimer > 0f)
            invTimer -= Time.deltaTime;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        if (invTimer > 0f) return;

        invTimer = invincibleTime;

        stats.hp = Mathf.Clamp(stats.hp - amount, 0, stats.maxHP);
        onHpChanged?.Invoke(stats.hp, stats.maxHP);

        if (stats.hp <= 0)
            onDied?.Invoke();
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        stats.hp = Mathf.Clamp(stats.hp + amount, 0, stats.maxHP);
        onHpChanged?.Invoke(stats.hp, stats.maxHP);
    }

}
