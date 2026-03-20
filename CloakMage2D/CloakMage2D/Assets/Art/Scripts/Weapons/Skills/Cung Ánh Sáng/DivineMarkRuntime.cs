////using UnityEngine;
////using TMPro; // nếu dùng TMP
////public class DivineMarkRuntime : MonoBehaviour
////{
////    public DivineMarkPassive sourcePassive;

////    int currentStack = 0;
////    public TextMeshProUGUI stackText; // kéo thả trong Inspector
////    public void Init(DivineMarkPassive passive)
////    {
////        sourcePassive = passive;
////    }

////    public void OnHit(Vector3 hitPosition, BossHealth target)
////    {
////        if (sourcePassive == null) return;

////        currentStack++;

////        if (currentStack >= sourcePassive.maxStack)
////        {
////            TriggerMark(hitPosition, target);
////            currentStack = 0;
////        }
////        Debug.Log("Hit registered");
////    }

////    void TriggerMark(Vector3 pos, BossHealth target)
////    {
////        var stats = GetComponent<PlayerStatsMono>();
////        if (stats == null) return;

////        // ===== BONUS DAMAGE =====
////        int bonusDamage = Mathf.RoundToInt(stats.atk * sourcePassive.bonusAtkScale);

////        bool isCrit;
////        int finalDamage = DamageCore.Compute(
////            stats,
////            target.GetComponent<PlayerStatsMono>(),
////            bonusDamage,
////            0f,
////            0f,
////            DamageType.Physical,
////            true,
////            out isCrit
////        );

////        target.TakeDamage(finalDamage);

////        DamagePopupSpawner.I?.Show(
////            target.transform.position,
////            finalDamage,
////            isCrit
////        );

////        // ===== MINI RAIN =====
////        if (sourcePassive.miniRainPrefab != null)
////        {
////            Vector3 spawnPos = pos + Vector3.up * sourcePassive.spawnHeight;
////            var arrow = Instantiate(sourcePassive.miniRainPrefab, spawnPos, Quaternion.identity);

////            arrow.owner = gameObject;
////            arrow.LaunchTo(pos);
////        }
////    }
////}
//using UnityEngine;
//using TMPro;

//public class DivineMarkRuntime : MonoBehaviour
//{
//    public DivineMarkPassive sourcePassive;

//    int currentStack = 0;

//    [Header("UI")]
//    public TextMeshProUGUI stackText; // kéo trong Inspector

//    public void Init(DivineMarkPassive passive)
//    {
//        sourcePassive = passive;
//        currentStack = 0;
//        UpdateUI();
//    }

//    public void OnHit(Vector3 hitPosition, BossHealth target)
//    {
//        if (sourcePassive == null || target == null)
//            return;

//        currentStack++;

//        UpdateUI(); // 🔥 cập nhật UI ngay khi cộng stack

//        if (currentStack >= sourcePassive.maxStack)
//        {
//            TriggerMark(hitPosition, target);
//            currentStack = 0;

//            UpdateUI(); // 🔥 reset UI về 0
//        }

//        Debug.Log($"Divine Mark Stack: {currentStack}");
//    }

//    void UpdateUI()
//    {
//        if (stackText == null || sourcePassive == null)
//            return;

//        stackText.text = $"Divine Mark: {currentStack}/{sourcePassive.maxStack}";
//    }

//    void TriggerMark(Vector3 pos, BossHealth target)
//    {
//        var stats = GetComponent<PlayerStatsMono>();
//        if (stats == null) return;

//        var targetStats = target.GetComponent<PlayerStatsMono>();

//        int bonusDamage = Mathf.RoundToInt(stats.atk * sourcePassive.bonusAtkScale);

//        bool isCrit;
//        int finalDamage = DamageCore.Compute(
//            stats,
//            targetStats,
//            bonusDamage,
//            0f,
//            0f,
//            DamageType.Physical,
//            true,
//            out isCrit
//        );

//        target.TakeDamage(finalDamage);

//        DamagePopupSpawner.I?.Show(
//            target.transform.position,
//            finalDamage,
//            isCrit
//        );

//        // ===== MINI RAIN =====
//        if (sourcePassive.miniRainPrefab != null)
//        {
//            Vector3 spawnPos = pos + Vector3.up * sourcePassive.spawnHeight;
//            var arrow = Instantiate(sourcePassive.miniRainPrefab, spawnPos, Quaternion.identity);

//            arrow.owner = gameObject;

//            // ⚠ nếu không muốn mini rain kích lại passive:
//            // arrow.triggerPassive = false;

//            arrow.LaunchTo(pos);
//        }
//    }
//}
using UnityEngine;
using System;

public class DivineMarkRuntime : MonoBehaviour
{
    public DivineMarkPassive sourcePassive;

    int currentStack = 0;

    // Event cho UI nghe
    public event Action<int, int> OnStackChanged;

    public void Init(DivineMarkPassive passive)
    {
        sourcePassive = passive;
        currentStack = 0;
        NotifyStackChanged();
    }

    public void OnHit(Vector3 hitPosition, BossHealth target)
    {
        if (sourcePassive == null || target == null)
            return;

        currentStack++;
        NotifyStackChanged();

        if (currentStack >= sourcePassive.maxStack)
        {
            TriggerMark(hitPosition, target);
            currentStack = 0;
            NotifyStackChanged();
        }
    }

    void NotifyStackChanged()
    {
        if (sourcePassive == null) return;
        OnStackChanged?.Invoke(currentStack, sourcePassive.maxStack);
    }

    void TriggerMark(Vector3 pos, BossHealth target)
    {
        var stats = GetComponent<PlayerStatsMono>();
        if (stats == null) return;

        var targetStats = target.GetComponent<PlayerStatsMono>();

        int bonusDamage = Mathf.RoundToInt(stats.atk * sourcePassive.bonusAtkScale);

        bool isCrit;
        int finalDamage = DamageCore.Compute(
            stats,
            targetStats,
            bonusDamage,
            0f,
            0f,
            DamageType.Physical,
            true,
            out isCrit
        );

        target.TakeDamage(finalDamage);

        DamagePopupSpawner.I?.Show(
            target.transform.position,
            finalDamage,
            isCrit
        );

        // MINI RAIN
        if (sourcePassive.miniRainPrefab != null)
        {
            Vector3 spawnPos = pos + Vector3.up * sourcePassive.spawnHeight;
            var arrow = Instantiate(sourcePassive.miniRainPrefab, spawnPos, Quaternion.identity);

            arrow.owner = gameObject;
            arrow.LaunchTo(pos);
        }
    }
}