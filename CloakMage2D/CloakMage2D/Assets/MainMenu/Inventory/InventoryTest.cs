////using UnityEngine;

////public class InventoryTest : MonoBehaviour
////{
////    public ItemSO testItem;

////    void Start()
////    {
////        InventorySystem.I.Add(testItem, 1);
////    }
////}
using UnityEngine;

public class InventoryTest : MonoBehaviour
{
    public ItemSO testWeaponItem;

    void Start()
    {
        if (testWeaponItem != null)
            InventorySystem.I.Add(testWeaponItem, 1);
    }
}
//using UnityEngine;
//using System.Collections.Generic;

//public class InventoryTest : MonoBehaviour
//{
//    public List<ItemSO> testItems = new List<ItemSO>();

//    void Start()
//    {
//        if (InventorySystem.I == null) return;

//        foreach (var item in testItems)
//        {
//            if (item != null)
//                InventorySystem.I.Add(item);
//        }
//    }
//}
