using UnityEngine;
using System.Collections.Generic;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    public List<ItemSO> items;

    Dictionary<string, ItemSO> _map;

    void Awake()
    {
        Instance = this;

        _map = new Dictionary<string, ItemSO>();

        foreach (var item in items)
        {
            _map[item.itemId] = item;
        }
    }

    public ItemSO GetItem(string id)
    {
        if (_map.ContainsKey(id))
            return _map[id];

        return null;
    }
}