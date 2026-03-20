using UnityEngine;

public enum DamageType { Physical, Magic, True }

public static class DamageCore
{
    static float Mitigation(float def)
    {
        def = Mathf.Max(0f, def);
        return 100f / (100f + def);
    }

    public static int Compute(
        PlayerStatsMono attacker,
        PlayerStatsMono target,
        int baseDamage,
        float atkScale,
        float smptScale,
        DamageType type,
        bool canCrit,
        out bool isCrit)
    {
        isCrit = false;

        float raw = baseDamage;
        if (attacker != null)
            raw += attacker.atk * atkScale + attacker.smpt * smptScale;

        if (canCrit && attacker != null && Random.value < attacker.critChance)
        {
            isCrit = true;
            raw *= Mathf.Max(1f, attacker.critDamage);
        }

        if (target != null)
        {
            if (type == DamageType.Physical) raw *= Mitigation(target.def);
            else if (type == DamageType.Magic) raw *= Mitigation(target.mdef);
        }

        return Mathf.Max(1, Mathf.RoundToInt(raw));
    }
}
