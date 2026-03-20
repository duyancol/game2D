////using UnityEngine;

////public class BossShooter : MonoBehaviour
////{
////    [Header("Refs")]
////    public Transform weaponPoint;
////    public Transform target;
////    public GameObject projectilePrefab;

////    [Header("Range")]
////    public float shootRangeX = 12f; // tính theo ngang kiểu gunny

////    [Header("Timing")]
////    public float shootInterval = 1.2f;
////    float nextShootTime;

////    [Header("Gunny Feel")]
////    public float extraPeakHeight = 2.5f;     // cao hơn => vòng cung cao
////    public float projectileGravityScale = 1.2f;
////    public float projectileLifeTime = 5f;

////    void Update()
////    {
////        if (!weaponPoint || !target || !projectilePrefab) return;

////        float dx = Mathf.Abs(target.position.x - weaponPoint.position.x);
////        if (dx > shootRangeX) return;

////        if (Time.time >= nextShootTime)
////        {
////            ShootArcGuaranteed();
////            nextShootTime = Time.time + shootInterval;
////        }
////        Debug.DrawLine(weaponPoint.position, target.position, Color.red);

////    }

////    void ShootArcGuaranteed()
////    {
////        var go = Instantiate(projectilePrefab, weaponPoint.position, Quaternion.identity);
////        var rb = go.GetComponent<Rigidbody2D>();
////        if (!rb)
////        {
////            Debug.LogWarning("Projectile prefab thiếu Rigidbody2D!");
////            Destroy(go);
////            return;
////        }

////        // IMPORTANT: đạn phải không drag
////        rb.linearDamping = 0f;
////        rb.angularDamping = 0f;

////        rb.gravityScale = projectileGravityScale;

////        Vector2 start = weaponPoint.position;
////        Vector2 end = target.position;


////        Debug.Log($"start={start} end={end} dx={(end.x - start.x):F2} dy={(end.y - start.y):F2} extraPeak={extraPeakHeight}");

////        if (!TryGetVelocityByPeakHeight(start, end, rb.gravityScale, extraPeakHeight, out Vector2 v0))
////        {
////            // fallback
////            rb.gravityScale = 0f;
////            rb.linearVelocity = (end - start).normalized * 10f;
////        }
////        else
////        {
////            rb.linearVelocity = v0;
////        }

////        Destroy(go, projectileLifeTime);
////    }

////    bool TryGetVelocityByPeakHeight(Vector2 start, Vector2 end, float gravityScale, float extraPeak, out Vector2 v0)
////    {
////        v0 = Vector2.zero;

////        float gWorld = Physics2D.gravity.y; // thường -9.81
////        float g = Mathf.Abs(gWorld) * Mathf.Max(0.0001f, gravityScale);

////        float peakY = Mathf.Max(start.y, end.y) + Mathf.Max(0.1f, extraPeak);

////        float hUp = peakY - start.y; // độ cao đi lên
////        float hDown = peakY - end.y; // độ cao rơi xuống

////        // vy ban đầu để lên đúng đỉnh
////        float vy0 = Mathf.Sqrt(2f * g * hUp);

////        // thời gian lên đỉnh: tUp = vy0 / g
////        float tUp = vy0 / g;

////        // thời gian rơi từ đỉnh xuống end: tDown = sqrt(2*hDown/g)
////        float tDown = Mathf.Sqrt(2f * hDown / g);

////        float tTotal = tUp + tDown;
////        if (tTotal <= 0.001f) return false;

////        float vx0 = (end.x - start.x) / tTotal;

////        v0 = new Vector2(vx0, vy0);
////        return true;
////    }


////    void OnDrawGizmosSelected()
////    {
////        if (!weaponPoint) return;
////        Gizmos.color = Color.yellow;
////        Gizmos.DrawWireSphere(weaponPoint.position, shootRangeX);
////    }
////}
//using System.Collections;
//using UnityEngine;

//public class BossShooter : MonoBehaviour
//{
//    [Header("Refs")]
//    public Transform weaponPoint;      // điểm spawn đạn (WeaponPoint hoặc SpearTip)
//    public Transform target;
//    public GameObject projectilePrefab;

//    [Header("Spear Root (Transform sẽ bị quật)")]
//    public Transform spearRoot;        // KÉO Feet (cây giáo) vào đây

//    [Header("Range")]
//    public float shootRangeX = 12f;

//    [Header("Timing")]
//    public float shootInterval = 1.2f;
//    float nextShootTime;

//    [Header("Gunny Feel")]
//    public float extraPeakHeight = 2.5f;
//    public float projectileGravityScale = 1.2f;
//    public float projectileLifeTime = 5f;

//    [Header("Spear Swing (Quật giáo khi bắn)")]
//    public bool swingOnShoot = true;
//    public float swingAngle = 120f;
//    public float swingDuration = 0.10f;
//    public float swingOvershoot = 0.25f;

//    Coroutine swingCR;
//    Quaternion spearStartRot;

//    void Awake()
//    {
//        if (!spearRoot) spearRoot = weaponPoint; // fallback
//        if (spearRoot) spearStartRot = spearRoot.localRotation;
//    }

//    void Update()
//    {
//        if (!weaponPoint || !target || !projectilePrefab) return;

//        float dx = Mathf.Abs(target.position.x - weaponPoint.position.x);
//        if (dx > shootRangeX) return;

//        if (Time.time >= nextShootTime)
//        {
//            if (swingOnShoot && spearRoot != null)
//            {
//                if (swingCR != null) StopCoroutine(swingCR);
//                swingCR = StartCoroutine(SwingSpear());
//            }

//            ShootArcGuaranteed();
//            nextShootTime = Time.time + shootInterval;
//        }
//    }

//    IEnumerator SwingSpear()
//    {
//        if (!spearRoot) yield break;

//        // reset gốc mỗi lần bắn để không lệch dần
//        spearStartRot = spearRoot.localRotation;

//        // hướng quật theo target (bên phải quật 1 chiều, bên trái đảo chiều)
//        float dirSign = (target.position.x >= spearRoot.position.x) ? -1f : 1f;
//        float a = swingAngle * dirSign;

//        float t = 0f;
//        // quật ra
//        while (t < swingDuration)
//        {
//            t += Time.deltaTime;
//            float p = Mathf.Clamp01(t / swingDuration);
//            float eased = Mathf.SmoothStep(0f, 1f, p);
//            spearRoot.localRotation = spearStartRot * Quaternion.Euler(0, 0, a * eased);
//            yield return null;
//        }

//        // giật ngược + về
//        float backDur = swingDuration * 0.9f;
//        float backT = 0f;
//        float backA = -a * swingOvershoot;

//        while (backT < backDur)
//        {
//            backT += Time.deltaTime;
//            float p = Mathf.Clamp01(backT / backDur);
//            float eased = Mathf.SmoothStep(0f, 1f, p);

//            float k = Mathf.Sin(p * Mathf.PI); // 0->1->0
//            float current = Mathf.Lerp(a, 0f, eased) + backA * k;

//            spearRoot.localRotation = spearStartRot * Quaternion.Euler(0, 0, current);
//            yield return null;
//        }

//        spearRoot.localRotation = spearStartRot;
//        swingCR = null;
//    }

//    void ShootArcGuaranteed()
//    {
//        var go = Instantiate(projectilePrefab, weaponPoint.position, Quaternion.identity);
//        var rb = go.GetComponent<Rigidbody2D>();
//        if (!rb)
//        {
//            Debug.LogWarning("Projectile prefab thiếu Rigidbody2D!");
//            Destroy(go);
//            return;
//        }

//        rb.linearDamping = 0f;
//        rb.angularDamping = 0f;
//        rb.gravityScale = projectileGravityScale;

//        Vector2 start = weaponPoint.position;
//        Vector2 end = target.position;

//        if (!TryGetVelocityByPeakHeight(start, end, rb.gravityScale, extraPeakHeight, out Vector2 v0))
//        {
//            rb.gravityScale = 0f;
//            rb.linearVelocity = (end - start).normalized * 10f;
//        }
//        else rb.linearVelocity = v0;

//        Destroy(go, projectileLifeTime);
//    }

//    bool TryGetVelocityByPeakHeight(Vector2 start, Vector2 end, float gravityScale, float extraPeak, out Vector2 v0)
//    {
//        v0 = Vector2.zero;

//        float gWorld = Physics2D.gravity.y;
//        float g = Mathf.Abs(gWorld) * Mathf.Max(0.0001f, gravityScale);

//        float peakY = Mathf.Max(start.y, end.y) + Mathf.Max(0.1f, extraPeak);

//        float hUp = peakY - start.y;
//        float hDown = peakY - end.y;

//        float vy0 = Mathf.Sqrt(2f * g * hUp);
//        float tUp = vy0 / g;
//        float tDown = Mathf.Sqrt(2f * hDown / g);

//        float tTotal = tUp + tDown;
//        if (tTotal <= 0.001f) return false;

//        float vx0 = (end.x - start.x) / tTotal;
//        v0 = new Vector2(vx0, vy0);
//        return true;
//    }
//}
using System.Collections;
using UnityEngine;

public class BossShooter : MonoBehaviour
{
    [Header("Refs")]
    public Transform weaponPoint;
    public Transform target;
    public GameObject projectilePrefab;

    [Header("Spear Root (Transform sẽ bị quật)")]
    public Transform spearRoot;

    [Header("Damage (Projectile)")]
    public int damage = 15;          // <<< set damage ở đây
    public LayerMask hitMask;        // tick layer Player
    public bool destroyOnHit = true;

    [Header("Range")]
    public float shootRangeX = 12f;

    [Header("Timing")]
    public float shootInterval = 1.2f;
    float nextShootTime;

    [Header("Gunny Feel")]
    public float extraPeakHeight = 2.5f;
    public float projectileGravityScale = 1.2f;
    public float projectileLifeTime = 5f;

    [Header("Spear Swing (Quật giáo khi bắn)")]
    public bool swingOnShoot = true;
    public float swingAngle = 120f;
    public float swingDuration = 0.10f;
    public float swingOvershoot = 0.25f;
    BossStun stun;

    Coroutine swingCR;
    Quaternion spearStartRot;

    void Awake()
    {
        if (!spearRoot) spearRoot = weaponPoint;
        if (spearRoot) spearStartRot = spearRoot.localRotation;
        stun = GetComponent<BossStun>();

    }

    void Update()

    {
        if (!weaponPoint || !target || !projectilePrefab) return;
        if (stun != null && stun.IsStunned)
        {
            // reset nhịp: bị hit thì delay lại lần bắn kế tiếp
            nextShootTime = Time.time + shootInterval;
            return;
        }

        float dx = Mathf.Abs(target.position.x - weaponPoint.position.x);
        if (dx > shootRangeX) return;

        if (Time.time >= nextShootTime)
        {
            if (swingOnShoot && spearRoot != null)
            {
                if (swingCR != null) StopCoroutine(swingCR);
                swingCR = StartCoroutine(SwingSpear());
            }

            ShootArcGuaranteed();
            nextShootTime = Time.time + shootInterval;
        }

    }

    IEnumerator SwingSpear()
    {
        if (!spearRoot) yield break;

        spearStartRot = spearRoot.localRotation;

        float dirSign = (target.position.x >= spearRoot.position.x) ? -1f : 1f;
        float a = swingAngle * dirSign;

        float t = 0f;
        while (t < swingDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / swingDuration);
            float eased = Mathf.SmoothStep(0f, 1f, p);
            spearRoot.localRotation = spearStartRot * Quaternion.Euler(0, 0, a * eased);
            yield return null;
        }

        float backDur = swingDuration * 0.9f;
        float backT = 0f;
        float backA = -a * swingOvershoot;

        while (backT < backDur)
        {
            backT += Time.deltaTime;
            float p = Mathf.Clamp01(backT / backDur);
            float eased = Mathf.SmoothStep(0f, 1f, p);

            float k = Mathf.Sin(p * Mathf.PI);
            float current = Mathf.Lerp(a, 0f, eased) + backA * k;

            spearRoot.localRotation = spearStartRot * Quaternion.Euler(0, 0, current);
            yield return null;
        }

        spearRoot.localRotation = spearStartRot;
        swingCR = null;
    }

    void ShootArcGuaranteed()
    {
        var go = Instantiate(projectilePrefab, weaponPoint.position, Quaternion.identity);

        var rb = go.GetComponent<Rigidbody2D>();
        if (!rb)
        {
            Debug.LogWarning("Projectile prefab thiếu Rigidbody2D!");
            Destroy(go);
            return;
        }

        // ✅ đảm bảo có collider trigger để nhận hit
        var col = go.GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
        else Debug.LogWarning("Projectile prefab thiếu Collider2D (để trừ máu).");

        // ✅ gắn damage script cho projectile
        var dmg = go.GetComponent<ProjectileDamage2D>();
        if (dmg == null) dmg = go.AddComponent<ProjectileDamage2D>();
        dmg.damage = damage;
        dmg.hitMask = hitMask;
        dmg.destroyOnHit = destroyOnHit;

        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.gravityScale = projectileGravityScale;

        Vector2 start = weaponPoint.position;
        Vector2 end = target.position;

        if (!TryGetVelocityByPeakHeight(start, end, rb.gravityScale, extraPeakHeight, out Vector2 v0))
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = (end - start).normalized * 10f;
        }
        else rb.linearVelocity = v0;

        Destroy(go, projectileLifeTime);
    }

    bool TryGetVelocityByPeakHeight(Vector2 start, Vector2 end, float gravityScale, float extraPeak, out Vector2 v0)
    {
        v0 = Vector2.zero;

        float gWorld = Physics2D.gravity.y;
        float g = Mathf.Abs(gWorld) * Mathf.Max(0.0001f, gravityScale);

        float peakY = Mathf.Max(start.y, end.y) + Mathf.Max(0.1f, extraPeak);

        float hUp = peakY - start.y;
        float hDown = peakY - end.y;

        float vy0 = Mathf.Sqrt(2f * g * hUp);
        float tUp = vy0 / g;
        float tDown = Mathf.Sqrt(2f * hDown / g);

        float tTotal = tUp + tDown;
        if (tTotal <= 0.001f) return false;

        float vx0 = (end.x - start.x) / tTotal;
        v0 = new Vector2(vx0, vy0);
        return true;
    }
}

/// Projectile damage (đơn giản, chắc ăn)
public class ProjectileDamage2D : MonoBehaviour
{
    public int damage = 10;
    public LayerMask hitMask;
    public bool destroyOnHit = true;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & hitMask) == 0) return;

        // ưu tiên PlayerHealth (cho chắc)
        var ph = other.GetComponentInParent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(damage);
            if (destroyOnHit) Destroy(gameObject);
            return;
        }

        // fallback interface nếu bạn dùng chung
        var d = other.GetComponentInParent<IDamageable>();
        if (d != null)
        {
            d.TakeDamage(damage);
            if (destroyOnHit) Destroy(gameObject);
        }
    }
}
