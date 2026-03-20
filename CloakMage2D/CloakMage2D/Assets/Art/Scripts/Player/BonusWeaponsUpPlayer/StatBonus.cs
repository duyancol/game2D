using UnityEngine;

[System.Serializable]
public struct StatBonus
{
    public int addMaxHP;

    public float addAtk;
    public float addSmpt;

    [Range(0f, 1f)] public float addCritChance; // +0.05 = +5%
    public float addCritDamage;                 // +0.2 = +20% (nếu bạn dùng kiểu cộng)

    public float addDef;
    public float addMdef;
}
