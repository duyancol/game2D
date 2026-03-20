//using UnityEngine;

//[CreateAssetMenu(menuName = "Game/Skills/Shoot Cube Bolt")]
//public class ShootCubeBoltSkill : WeaponSkill
//{
//    public GameObject projectilePrefab;
//    public float speed = 12f;
//    public float lifeTime = 1.2f;
//    [Header("Combat")]
//    public int damage = 40;
//    protected override void OnUse(SkillContext ctx)
//    {
//        if (projectilePrefab == null || ctx.weaponPivot == null) return;

//        Vector3 pos = ctx.weaponPivot.position;
//        Vector2 dir = (ctx.mouseWorld - pos).normalized;

//        // đẩy đạn ra trước mũi tay
//        pos += (Vector3)(dir * 2f);


//        var go = Instantiate(projectilePrefab, pos, Quaternion.identity);

//        Debug.Log("SPAWNED = " + go.name + " at " + pos);
//        // dame
//        var dmg = go.GetComponent<ProjectileDamage>();
//        if (dmg != null)
//            dmg.Init(damage, ctx.owner != null ? ctx.owner.gameObject : null);

//        var rb = go.GetComponent<Rigidbody2D>();
//        if (rb != null) rb.linearVelocity = dir * speed;

//        Destroy(go, lifeTime);
//    }


//}
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skills/Shoot Cube Bolt")]
public class ShootCubeBoltSkill : WeaponSkill
{
    [Header("Prefabs")]
    public GameObject projectilePrefab;

    [Header("Shoot")]
    public float speed = 12f;
    public float lifeTime = 1.2f;
    public float muzzleOffset = 2f;
    public float aimAngleOffset = 0f;

    [Header("Combat")]
    public int damage = 40;                     // base damage của skill
    public DamageType damageType = DamageType.Physical;
    public float atkScale = 1.0f;               // scale theo atk
    public float smptScale = 0f;
    public bool canCrit = true;

    [Header("Anti self-hit")]
    public float ignoreOwnerCollisionTime = 0.08f;

    protected override void OnUse(SkillContext ctx)
    {
        if (projectilePrefab == null || ctx.weaponPivot == null) return;

        Vector3 pivotPos = ctx.weaponPivot.position;

        // ===== AIM =====
        Vector2 dir = (ctx.mouseWorld - pivotPos);
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.right;
        dir.Normalize();

        Vector3 spawnPos = pivotPos + (Vector3)(dir * muzzleOffset);
        spawnPos.z = 0f;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + aimAngleOffset;
        Quaternion rot = Quaternion.Euler(0f, 0f, angle);

        // ===== SPAWN =====
        var go = Instantiate(projectilePrefab, spawnPos, rot);
        Debug.Log($"[CubeBolt] SPAWNED {go.name} at {spawnPos}");

        // ===== DAMAGE (theo ATK nhân vật) =====
        var dmgComp = go.GetComponent<ProjectileDamage>();
        if (dmgComp != null)
        {
            var ownerGo = ctx.owner != null ? ctx.owner.gameObject : null;
            var attackerStats = ownerGo != null
                ? ownerGo.GetComponent<PlayerStatsMono>()
                : null;

            dmgComp.InitScaled(
                attackerStats,
                damage,        // base damage
                atkScale,
                smptScale,
                damageType,
                canCrit,
                ownerGo
            );
        }

        // ===== MOVE =====
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = dir * speed;
        }

        // ===== ANTI SELF HIT =====
        IgnoreOwnerCollision(ctx.owner, go, ignoreOwnerCollisionTime);

        Destroy(go, lifeTime);
    }

    // ---- copy y chang từ skill Light Arrow ----
    static void IgnoreOwnerCollision(Transform owner, GameObject projectile, float restoreAfter)
    {
        if (owner == null || projectile == null) return;

        var ownerCols = owner.GetComponentsInChildren<Collider2D>();
        var projCols = projectile.GetComponentsInChildren<Collider2D>();

        foreach (var oc in ownerCols)
            foreach (var pc in projCols)
                Physics2D.IgnoreCollision(oc, pc, true);

        if (restoreAfter > 0f)
        {
            var runner = owner.GetComponent<SkillCoroutineRunner>();
            if (runner == null)
                runner = owner.gameObject.AddComponent<SkillCoroutineRunner>();

            runner.Run(RestoreCollision(ownerCols, projCols, restoreAfter));
        }
    }

    //static System.Collections.IEnumerator RestoreCollision(
    //    Collider2D[] ownerCols,
    //    Collider2D[] projCols,
    //    float t
    //)
    //{
    //    yield return new WaitForSeconds(t);

    //    foreach (var oc in ownerCols)
    //        foreach (var pc in projCols)
    //            Physics2D.IgnoreCollision(oc, pc, false);
    //}
    static System.Collections.IEnumerator RestoreCollision(
    Collider2D[] ownerCols,
    Collider2D[] projCols,
    float t
)
    {
        yield return new WaitForSeconds(t);

        foreach (var oc in ownerCols)
        {
            if (!oc) continue;

            foreach (var pc in projCols)
            {
                if (!pc) continue;

                Physics2D.IgnoreCollision(oc, pc, false);
            }
        }
    }
}
