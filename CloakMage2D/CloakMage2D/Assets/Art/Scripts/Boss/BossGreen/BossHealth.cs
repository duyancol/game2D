
////using System.Collections;
////using UnityEngine;
////using UnityEngine.Events;
////[System.Serializable]
////public class DropItem
////{
////    public ItemSO item;
////    public GameObject dropPrefab; // prefab vật rơi ra đất
////    public int minAmount = 1;
////    public int maxAmount = 1;

////    [Range(0, 100)]
////    public float dropChance = 50f;
////}
////public class BossHealth : MonoBehaviour
////{
////    public int maxHp = 200;
////    public int hp;

////    public UnityEvent<int, int> onHpChanged; // hp, maxHp

////    public Transform head;
////    public GameObject goldPrefab;
////    public DropItem[] materialDrops;
////    public float respawnTime = 60f;

////    MonoBehaviour[] behaviours;
////    SpriteRenderer[] renders;
////    Collider2D[] cols;
////    Rigidbody2D rb;
////    BossHitFX hitFx;
////    Vector3 spawnPosition;
////    BossAntChaseTrip bossAI;
////    void Awake()
////    {
////        behaviours = GetComponents<MonoBehaviour>();
////        bossAI = GetComponent<BossAntChaseTrip>();
////        hp = maxHp;

////        if (onHpChanged == null)
////            onHpChanged = new UnityEvent<int, int>();

////        hitFx = GetComponent<BossHitFX>();
////        spawnPosition = transform.position;

////        renders = GetComponentsInChildren<SpriteRenderer>();
////        cols = GetComponentsInChildren<Collider2D>();
////        rb = GetComponent<Rigidbody2D>();

////        onHpChanged.Invoke(hp, maxHp); // update lần đầu
////    }
////    void DropMaterials()
////    {
////        if (materialDrops == null) return;

////        foreach (var drop in materialDrops)
////        {
////            float roll = Random.Range(0f, 100f);

////            if (roll <= drop.dropChance)
////            {
////                int amount = Random.Range(drop.minAmount, drop.maxAmount + 1);

////                for (int i = 0; i < amount; i++)
////                {
////                    Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * 5f;
////                    GameObject obj = Instantiate(drop.dropPrefab, spawnPos, Quaternion.identity);

////                    // 🔥 THÊM FORCE VĂNG RA
////                    Rigidbody2D rbDrop = obj.GetComponent<Rigidbody2D>();
////                    if (rbDrop != null)
////                    {
////                        Vector2 dir = new Vector2(Random.Range(-1f, 1f), 1f).normalized;
////                        rbDrop.AddForce(dir * 5f + Vector2.up * 6f, ForceMode2D.Impulse);
////                    }

////                    // 🔥 TỰ HỦY SAU 30s + CHỚP
////                    StartCoroutine(DropLifeRoutine(obj));
////                }
////            }
////        }
////    }
////    public void TakeDamage(int dmg)
////    {
////        if (dmg <= 0 || hp <= 0) return;

////        hp -= dmg;
////        hp = Mathf.Clamp(hp, 0, maxHp);

////        if (hitFx != null)
////            hitFx.PlayHit();

////        onHpChanged.Invoke(hp, maxHp);

////        if (hp <= 0)
////            StartCoroutine(RespawnRoutine());
////    }

////    IEnumerator RespawnRoutine()
////    {
////        if (goldPrefab != null)
////        {
////            GameObject gold = Instantiate(goldPrefab, transform.position, Quaternion.identity);

////            Rigidbody2D rbDrop = gold.GetComponent<Rigidbody2D>();
////            if (rbDrop != null)
////            {
////                Vector2 dir = new Vector2(Random.Range(-1f, 1f), 1f).normalized;
////                rbDrop.AddForce(dir * 5f + Vector2.up * 6f, ForceMode2D.Impulse);
////            }

////            StartCoroutine(DropLifeRoutine(gold));
////        }
////        DropMaterials();
////        foreach (var b in behaviours)
////        {
////            if (b != this)
////                b.enabled = false;
////        }

////        if (rb != null)
////        {
////            rb.linearVelocity = Vector2.zero;
////            rb.angularVelocity = 0f;
////            rb.simulated = false;
////        }

////        foreach (var r in renders)
////            r.enabled = false;

////        foreach (var c in cols)
////            c.enabled = false;

////        yield return new WaitForSeconds(respawnTime);

////        hp = maxHp;
////        transform.position = spawnPosition;

////        foreach (var b in behaviours)
////        {
////            if (b != this)
////                b.enabled = true;
////        }

////        if (rb != null)
////        {
////            rb.linearVelocity = Vector2.zero;
////            rb.angularVelocity = 0f;
////            rb.simulated = true;
////        }

////        foreach (var r in renders)
////            r.enabled = true;

////        foreach (var c in cols)
////            c.enabled = true;

////        onHpChanged.Invoke(hp, maxHp); // update lại thanh máu
////    }
////    IEnumerator DropLifeRoutine(GameObject obj)
////    {
////        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();

////        float lifeTime = 30f;
////        float blinkTime = 5f;

////        // ⏳ chờ
////        yield return new WaitForSeconds(lifeTime - blinkTime);

////        // ⚡ chớp chớp
////        float t = 0;
////        while (t < blinkTime)
////        {
////            if (sr != null)
////                sr.enabled = !sr.enabled;

////            yield return new WaitForSeconds(0.2f);
////            t += 0.2f;
////        }

////        Destroy(obj);
////    }
////}

//using System.Collections;
//using UnityEngine;
//using UnityEngine.Events;
//[System.Serializable]
//public class DropItem
//{
//    public ItemSO item;
//    public GameObject dropPrefab; // prefab vật rơi ra đất
//    public int minAmount = 1;
//    public int maxAmount = 1;

//    [Range(0, 100)]
//    public float dropChance = 50f;
//}
//public class BossHealth : MonoBehaviour
//{
//    public int maxHp = 200;
//    public int hp;

//    public UnityEvent<int, int> onHpChanged; // hp, maxHp

//    public Transform head;
//    public GameObject goldPrefab;
//    public DropItem[] materialDrops;
//    public float respawnTime = 60f;

//    MonoBehaviour[] behaviours;
//    SpriteRenderer[] renders;
//    Collider2D[] cols;
//    Rigidbody2D rb;
//    BossHitFX hitFx;
//    Vector3 spawnPosition;
//    BossAntChaseTrip bossAI;
//    void Awake()
//    {
//        behaviours = GetComponents<MonoBehaviour>();
//        bossAI = GetComponent<BossAntChaseTrip>();
//        hp = maxHp;

//        if (onHpChanged == null)
//            onHpChanged = new UnityEvent<int, int>();

//        hitFx = GetComponent<BossHitFX>();
//        spawnPosition = transform.position;

//        renders = GetComponentsInChildren<SpriteRenderer>();
//        cols = GetComponentsInChildren<Collider2D>();
//        rb = GetComponent<Rigidbody2D>();

//        onHpChanged.Invoke(hp, maxHp); // update lần đầu
//    }
//    void DropMaterials()
//    {
//        if (materialDrops == null) return;

//        foreach (var drop in materialDrops)
//        {
//            float roll = Random.Range(0f, 100f);

//            if (roll <= drop.dropChance)
//            {
//                int amount = Random.Range(drop.minAmount, drop.maxAmount + 1);

//                for (int i = 0; i < amount; i++)
//                {
//                    Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * 5f;
//                    GameObject obj = Instantiate(drop.dropPrefab, spawnPos, Quaternion.identity);

//                    // 🔥 THÊM FORCE VĂNG RA
//                    Rigidbody2D rbDrop = obj.GetComponent<Rigidbody2D>();
//                    if (rbDrop != null)
//                    {
//                        Vector2 dir = new Vector2(Random.Range(-1f, 1f), 1f).normalized;
//                        rbDrop.AddForce(dir * 5f + Vector2.up * 6f, ForceMode2D.Impulse);
//                    }

//                    // 🔥 TỰ HỦY SAU 30s + CHỚP
//                    StartCoroutine(DropLifeRoutine(obj));
//                }
//            }
//        }
//    }
//    public void TakeDamage(int dmg)
//    {
//        if (dmg <= 0 || hp <= 0) return;

//        hp -= dmg;
//        hp = Mathf.Clamp(hp, 0, maxHp);

//        if (hitFx != null)
//            hitFx.PlayHit();

//        onHpChanged.Invoke(hp, maxHp);

//        if (hp <= 0)
//            StartCoroutine(RespawnRoutine());
//    }

//    IEnumerator RespawnRoutine()
//    {
//        if (goldPrefab != null)
//        {
//            GameObject gold = Instantiate(goldPrefab, transform.position, Quaternion.identity);

//            Rigidbody2D rbDrop = gold.GetComponent<Rigidbody2D>();
//            if (rbDrop != null)
//            {
//                Vector2 dir = new Vector2(Random.Range(-1f, 1f), 1f).normalized;
//                rbDrop.AddForce(dir * 5f + Vector2.up * 6f, ForceMode2D.Impulse);
//            }

//            StartCoroutine(DropLifeRoutine(gold));
//        }
//        DropMaterials();
//        foreach (var b in behaviours)
//        {
//            if (b != this)
//                b.enabled = false;
//        }

//        if (rb != null)
//        {
//            rb.linearVelocity = Vector2.zero;
//            rb.angularVelocity = 0f;
//            rb.simulated = false;
//        }

//        foreach (var r in renders)
//            r.enabled = false;

//        foreach (var c in cols)
//            c.enabled = false;

//        yield return new WaitForSeconds(respawnTime);

//        hp = maxHp;
//        transform.position = spawnPosition;

//        foreach (var b in behaviours)
//        {
//            if (b != this)
//                b.enabled = true;
//        }

//        if (rb != null)
//        {
//            rb.linearVelocity = Vector2.zero;
//            rb.angularVelocity = 0f;
//            rb.simulated = true;
//        }

//        foreach (var r in renders)
//            r.enabled = true;

//        foreach (var c in cols)
//            c.enabled = true;

//        onHpChanged.Invoke(hp, maxHp); // update lại thanh máu
//    }
//    IEnumerator DropLifeRoutine(GameObject obj)
//    {
//        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();

//        float lifeTime = 30f;
//        float blinkTime = 5f;

//        // ⏳ chờ
//        yield return new WaitForSeconds(lifeTime - blinkTime);

//        // ⚡ chớp chớp
//        float t = 0;
//        while (t < blinkTime)
//        {
//            if (sr != null)
//                sr.enabled = !sr.enabled;

//            yield return new WaitForSeconds(0.2f);
//            t += 0.2f;
//        }

//        Destroy(obj);
//    }
//}
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DropItem
{
    public ItemSO item;
    public GameObject dropPrefab;
    public int minAmount = 1;
    public int maxAmount = 1;

    [Range(0, 100)]
    public float dropChance = 50f;
}

public class BossHealth : MonoBehaviour
{
    [Header("Boss Config")]
    public string bossID; // 🔥 ID riêng cho từng boss

    public int maxHp = 200;
    public int hp;

    public UnityEvent<int, int> onHpChanged;

    public Transform head;
    public GameObject goldPrefab;
    public DropItem[] materialDrops;
    public float respawnTime = 60f;

    MonoBehaviour[] behaviours;
    SpriteRenderer[] renders;
    Collider2D[] cols;
    Rigidbody2D rb;
    BossHitFX hitFx;
    Vector3 spawnPosition;
    BossAntChaseTrip bossAI;

    // 🔥 KEY SAVE
    string GetBossKey()
    {
        return "BOSS_DEAD_TIME_" + bossID;
    }

    void Awake()
    {
        behaviours = GetComponents<MonoBehaviour>();
        bossAI = GetComponent<BossAntChaseTrip>();

        if (onHpChanged == null)
            onHpChanged = new UnityEvent<int, int>();

        hitFx = GetComponent<BossHitFX>();
        spawnPosition = transform.position;

        renders = GetComponentsInChildren<SpriteRenderer>();
        cols = GetComponentsInChildren<Collider2D>();
        rb = GetComponent<Rigidbody2D>();

        // 🔥 CHECK TIME REAL
        if (!string.IsNullOrEmpty(bossID) && PlayerPrefs.HasKey(GetBossKey()))
        {
            long deadTime = long.Parse(PlayerPrefs.GetString(GetBossKey()));
            long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            float remain = respawnTime - (now - deadTime);

            if (remain > 0)
            {
                DisableBoss();
                StartCoroutine(WaitRespawn(remain));
                return;
            }
            else
            {
                PlayerPrefs.DeleteKey(GetBossKey());
            }
        }

        hp = maxHp;
        onHpChanged.Invoke(hp, maxHp);
    }

    void DropMaterials()
    {
        if (materialDrops == null) return;

        foreach (var drop in materialDrops)
        {
            float roll = Random.Range(0f, 100f);

            if (roll <= drop.dropChance)
            {
                int amount = Random.Range(drop.minAmount, drop.maxAmount + 1);

                for (int i = 0; i < amount; i++)
                {
                    Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * 5f;
                    GameObject obj = Instantiate(drop.dropPrefab, spawnPos, Quaternion.identity);

                    Rigidbody2D rbDrop = obj.GetComponent<Rigidbody2D>();
                    if (rbDrop != null)
                    {
                        Vector2 dir = new Vector2(Random.Range(-1f, 1f), 1f).normalized;
                        rbDrop.AddForce(dir * 5f + Vector2.up * 6f, ForceMode2D.Impulse);
                    }

                    StartCoroutine(DropLifeRoutine(obj));
                }
            }
        }
    }

    public void TakeDamage(int dmg)
    {
        if (dmg <= 0 || hp <= 0) return;

        hp -= dmg;
        hp = Mathf.Clamp(hp, 0, maxHp);

        if (hitFx != null)
            hitFx.PlayHit();

        onHpChanged.Invoke(hp, maxHp);

        if (hp <= 0)
        {
            SaveDeadTime(); // 🔥 lưu thời gian chết
            StartCoroutine(RespawnRoutine());
        }
    }

    IEnumerator RespawnRoutine()
    {
        if (goldPrefab != null)
        {
            GameObject gold = Instantiate(goldPrefab, transform.position, Quaternion.identity);

            Rigidbody2D rbDrop = gold.GetComponent<Rigidbody2D>();
            if (rbDrop != null)
            {
                Vector2 dir = new Vector2(Random.Range(-1f, 1f), 1f).normalized;
                rbDrop.AddForce(dir * 5f + Vector2.up * 6f, ForceMode2D.Impulse);
            }

            StartCoroutine(DropLifeRoutine(gold));
        }

        DropMaterials();
        DisableBoss();

        yield return new WaitForSeconds(respawnTime);

        PlayerPrefs.DeleteKey(GetBossKey());

        hp = maxHp;
        transform.position = spawnPosition;

        EnableBoss();

        onHpChanged.Invoke(hp, maxHp);
    }

    // ================= REALTIME =================

    void SaveDeadTime()
    {
        if (string.IsNullOrEmpty(bossID))
        {
            Debug.LogWarning("Boss chưa có ID!");
            return;
        }

        long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        PlayerPrefs.SetString(GetBossKey(), now.ToString());
        PlayerPrefs.Save();

        Debug.Log("SAVE TIME: " + now);
    }

    IEnumerator WaitRespawn(float time)
    {
        yield return new WaitForSeconds(time);

        PlayerPrefs.DeleteKey(GetBossKey());

        hp = maxHp;
        transform.position = spawnPosition;

        EnableBoss();

        onHpChanged.Invoke(hp, maxHp);
    }

    void DisableBoss()
    {
        foreach (var b in behaviours)
        {
            if (b != this)
                b.enabled = false;
        }

        if (rb != null)
            rb.simulated = false;

        foreach (var r in renders)
            r.enabled = false;

        foreach (var c in cols)
            c.enabled = false;
    }

    void EnableBoss()
    {
        foreach (var b in behaviours)
        {
            if (b != this)
                b.enabled = true;
        }

        if (rb != null)
            rb.simulated = true;

        foreach (var r in renders)
            r.enabled = true;

        foreach (var c in cols)
            c.enabled = true;
    }

    IEnumerator DropLifeRoutine(GameObject obj)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();

        float lifeTime = 30f;
        float blinkTime = 5f;

        yield return new WaitForSeconds(lifeTime - blinkTime);

        float t = 0;
        while (t < blinkTime)
        {
            if (sr != null)
                sr.enabled = !sr.enabled;

            yield return new WaitForSeconds(0.2f);
            t += 0.2f;
        }

        Destroy(obj);
    }
}