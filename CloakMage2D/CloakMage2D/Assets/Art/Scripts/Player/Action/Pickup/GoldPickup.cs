////using UnityEngine;

////public class GoldPickup : MonoBehaviour
////{
////    public int amount = 10;

////    void OnTriggerEnter2D(Collider2D other)
////    {
////        if (other.CompareTag("Player"))
////        {
////            GameManager.Instance.AddGold(amount);
////            Destroy(gameObject);
////        }
////    }
////}
//using UnityEngine;

//public class GoldPickup : MonoBehaviour
//{
//    public int amount = 10;

//    void OnTriggerEnter2D(Collider2D other)
//    {
//        if (other.CompareTag("Player"))
//        {
//            CurrencyAPI api = FindObjectOfType<CurrencyAPI>();

//            if (api != null)
//            {
//                api.AddGold(GameManager.Instance.playerData.id, amount);
//            }

//            Destroy(gameObject);
//        }
//    }
//}
using UnityEngine;

public class GoldPickup : MonoBehaviour
{
    public int amount = 10;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CurrencySync.Instance.AddPendingGold(amount);
            Destroy(gameObject);
        }
    }
}