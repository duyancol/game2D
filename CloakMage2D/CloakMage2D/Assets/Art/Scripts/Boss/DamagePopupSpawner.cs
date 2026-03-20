//using UnityEngine;

//public class DamagePopupSpawner : MonoBehaviour
//{
//    public static DamagePopupSpawner I;

//    [Header("Prefab (World Space)")]
//    public DamagePopup popupPrefab;

//    [Header("Offset above target")]
//    public Vector3 worldOffset = new Vector3(0f, 1.0f, 0f);

//    void Awake()
//    {
//        I = this;
//    }

//    public void Show(Vector3 worldPos, int amount, bool isCrit)
//    {
//        if (popupPrefab == null) return;

//        Vector3 pos = worldPos + worldOffset;

//        // đẩy ra trước camera
//        pos.z = -5f;

//        var pop = Instantiate(popupPrefab, pos, Quaternion.identity);
//        pop.Setup(amount, isCrit);
//    }
//}
using UnityEngine;

public class DamagePopupSpawner : MonoBehaviour
{
    public static DamagePopupSpawner I;

    [Header("Prefab (World Space)")]
    public DamagePopup popupPrefab;

    [Header("Offset above target")]
    public Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);

    [Header("Random Offset")]
    public float randomX = 0.35f;
    public float randomY = 0.2f;

    void Awake()
    {
        I = this;
    }

    public void Show(Vector3 worldPos, int amount, bool isCrit)
    {
        if (popupPrefab == null) return;

        // random vị trí để không chồng nhau
        Vector3 randomOffset = new Vector3(
            Random.Range(-randomX, randomX),
            Random.Range(0f, randomY),
            0
        );

        Vector3 pos = worldPos + worldOffset + randomOffset;

        // đưa ra trước camera
        pos.z = -5f;

        var pop = Instantiate(popupPrefab, pos, Quaternion.identity);

        // xoay nhẹ cho tự nhiên
        float rot = Random.Range(-8f, 8f);
        pop.transform.rotation = Quaternion.Euler(0, 0, rot);

        pop.Setup(amount, isCrit);
    }
}