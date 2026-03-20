using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item Effects/Heal Effect")]
public class HealEffectSO : UseEffectSO
{
    public int healAmount = 20;

    public override void Execute(GameObject user)
    {
        var health = user.GetComponent<PlayerHealth>();

        if (health != null)
        {
            health.Heal(healAmount);
        }
    }
}
