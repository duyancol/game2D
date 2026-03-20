using System.Collections.Generic;

[System.Serializable]
public class EquipmentInstance
{
    public EquipmentProfile profile;

    public StatEntry mainStat;
    public List<StatEntry> subStats = new List<StatEntry>();
}
