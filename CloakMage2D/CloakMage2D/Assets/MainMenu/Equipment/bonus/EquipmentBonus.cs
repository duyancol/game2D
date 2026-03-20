[System.Serializable]
public class EquipmentBonus
{
    public EquipSlot slot;

    // 🔹 MAIN STAT (flat)
    public int flatHP;
    public float flatAtk;
    public float flatSmpt;
    public float flatCritChance;
    public float flatCritDamage;
    public float flatDef;
    public float flatMdef;

    // 🔹 SUB STAT (%)
    public float percentHP;
    public float percentAtk;
    public float percentSmpt;
    public float percentCritChance;
    public float percentCritDamage;
    public float percentDef;
    public float percentMdef;
}
