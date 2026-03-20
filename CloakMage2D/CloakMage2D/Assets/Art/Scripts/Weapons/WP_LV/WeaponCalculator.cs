using UnityEngine;

public static class WeaponCalculator
{
    public static StatBonus GetFinalStat(WeaponInstance weapon)
    {
        StatBonus baseStat = weapon.profile.bonus;

        float percent = (1f + weapon.enhanceLevel * 0.1f)/2;

        StatBonus final = new StatBonus();

        final.addAtk = Mathf.RoundToInt(baseStat.addAtk * percent);
        final.addMaxHP = Mathf.RoundToInt(baseStat.addMaxHP * percent);
        final.addSmpt = Mathf.RoundToInt(baseStat.addSmpt * percent);

        final.addCritChance = baseStat.addCritChance * percent;
        final.addCritDamage = baseStat.addCritDamage * percent;

        final.addDef = Mathf.RoundToInt(baseStat.addDef * percent);
        final.addMdef = Mathf.RoundToInt(baseStat.addMdef * percent);

        return final;
    }
    public static StatBonus GetBaseStatByLevel(WeaponInstance weapon)
    {
        StatBonus baseStat = weapon.profile.bonus;

        int lv = weapon.level;
        float levelScale = 1f + lv * 0.02f; 
        float percent = (1f + weapon.level * 0.02f)/2;

        StatBonus result = new StatBonus();

        result.addAtk = Mathf.RoundToInt(baseStat.addAtk * levelScale);
        result.addMaxHP = Mathf.RoundToInt(baseStat.addMaxHP * levelScale);
        result.addSmpt = Mathf.RoundToInt(baseStat.addSmpt * levelScale);

        result.addDef = Mathf.RoundToInt(baseStat.addDef * levelScale);
        result.addMdef = Mathf.RoundToInt(baseStat.addMdef * levelScale);

        // crit KHÔNG scale mạnh
        result.addCritChance = baseStat.addCritChance * percent ;
        result.addCritDamage = baseStat.addCritDamage * percent;


        return result;
    }
}