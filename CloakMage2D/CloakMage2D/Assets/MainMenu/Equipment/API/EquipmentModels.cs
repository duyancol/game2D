using System;
using UnityEngine;

[Serializable]
public class EquipmentList
{
    public EquipmentResponse[] items;
}

[Serializable]
public class EquipmentResponse
{
    public int id;
    public string itemId;
    public string quality;
    public string mainStat;
    public int mainValue;
    public string subStatsJson;
    public bool equipped;
}

[Serializable]
public class SubStat
{
    public string type;
    public int value;
}

public static class JsonHelper
{
    public static System.Collections.Generic.List<T> FromJson<T>(string json)
    {
        string wrapped = "{\"items\":" + json + "}";
        Wrapper<T> wrapper =
            JsonUtility.FromJson<Wrapper<T>>(wrapped);

        return new System.Collections.Generic.List<T>(wrapper.items);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] items;
    }
}
