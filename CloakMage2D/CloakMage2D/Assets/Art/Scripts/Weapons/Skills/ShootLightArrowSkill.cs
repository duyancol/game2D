
//using System.Collections;
//using UnityEngine;

//[CreateAssetMenu(menuName = "Game/Skills/Shoot Light Arrow")]
//public class ShootLightArrowSkill : WeaponSkill
//{
//    [Header("Prefabs")]
//    public GameObject lightArrowPrefab;     // Sprite + Rigidbody2D + Collider2D + ProjectileDamage
//    public GameObject muzzleFlashPrefab;    // optional

//    [Header("Shoot")]
//    public float speed = 16f;
//    public float lifeTime = 1.2f;
//    public float muzzleOffset = 0.8f;
//    public float aimAngleOffset = 0f;

//    [Header("Combat")]
//    public int damage = 100;
//    public DamageType damageType = DamageType.Physical;
//    public float atkScale = 1.1f;
//    public float smptScale = 0f;
//    public bool canCrit = true;

//    [Header("Anti self-hit")]
//    public float ignoreOwnerCollisionTime = 0.08f;

//    [Header("Muzzle Flash")]
//    public float muzzleFlashLife = 0.25f;

//    protected override void OnUse(SkillContext ctx)
//    {
//        var playerMove = ctx.owner.GetComponent<PlayerMove2D>();
//        if (playerMove != null)
//        {
//            playerMove.SetSkillVisual();
//        }
//        var anim = ctx.owner.transform.Find("Visual_Skill")?.GetComponent<Animator>();
//        if (anim != null)
//        {
//            anim.SetBool("Q_AL", true);
//        }
//        if (lightArrowPrefab == null || ctx.weaponPivot == null) return;

//        Vector3 pivotPos = ctx.weaponPivot.position;

//        Vector2 dir = (ctx.mouseWorld - pivotPos);
//        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
//        dir.Normalize();

//        Vector3 spawnPos = pivotPos + (Vector3)(dir * muzzleOffset);
//        spawnPos.z = 0f;

//        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + aimAngleOffset;
//        Quaternion rot = Quaternion.Euler(0f, 0f, angle);

//        // muzzle flash (optional)
//        if (muzzleFlashPrefab != null)
//        {
//            var mf = Object.Instantiate(muzzleFlashPrefab, spawnPos, rot);
//            if (muzzleFlashLife > 0f) Object.Destroy(mf, muzzleFlashLife);
//        }

//        // spawn projectile
//        var go = Object.Instantiate(lightArrowPrefab, spawnPos, rot);

//        // ===== DAMAGE (giống skill thường) =====
//        var dmgComp = go.GetComponent<ProjectileDamage>();
//        if (dmgComp != null)
//        {
//            var ownerGo = ctx.owner != null ? ctx.owner.gameObject : null;
//            var attackerStats = ownerGo != null ? ownerGo.GetComponent<PlayerStatsMono>() : null;

//            dmgComp.InitScaled(
//                attackerStats,
//                damage,         // baseDamage của skill
//                atkScale,
//                smptScale,
//                damageType,
//                canCrit,
//                ownerGo
//            );
//            Debug.Log($"[SKILL] damage={damage} owner={(ctx.owner ? ctx.owner.name : "null")} " +
//          $"atk={(ctx.owner ? ctx.owner.GetComponent<PlayerStatsMono>()?.atk : 0)}");

//        }


//        // ===== MOVE =====
//        var rb = go.GetComponent<Rigidbody2D>();
//        if (rb != null)
//        {
//            rb.gravityScale = 0f;
//            rb.linearVelocity = dir * speed;
//        }

//        // tránh tự dính collider của player ngay lúc bắn
//        IgnoreOwnerCollision(ctx.owner, go, ignoreOwnerCollisionTime);
//        if (playerMove != null)
//        {
//            playerMove.SetNormalVisual();
//        }
//        Object.Destroy(go, lifeTime);
//    }

//    static void IgnoreOwnerCollision(Transform owner, GameObject projectile, float restoreAfter)
//    {
//        if (owner == null || projectile == null) return;

//        var ownerCols = owner.GetComponentsInChildren<Collider2D>();
//        var projCols = projectile.GetComponentsInChildren<Collider2D>();
//        if (ownerCols == null || projCols == null) return;

//        foreach (var oc in ownerCols)
//        {
//            if (oc == null) continue;
//            foreach (var pc in projCols)
//            {
//                if (pc == null) continue;
//                Physics2D.IgnoreCollision(oc, pc, true);
//            }
//        }

//        if (restoreAfter > 0f)
//        {
//            // chạy coroutine bằng runner gắn lên owner (đúng chuẩn, không bị null)
//            var runner = owner.GetComponent<SkillCoroutineRunner>();
//            if (runner == null) runner = owner.gameObject.AddComponent<SkillCoroutineRunner>();
//            runner.Run(RestoreCollision(ownerCols, projCols, restoreAfter));
//        }
//    }

//    static IEnumerator RestoreCollision(Collider2D[] ownerCols, Collider2D[] projCols, float t)
//    {
//        yield return new WaitForSeconds(t);

//        foreach (var oc in ownerCols)
//        {
//            if (oc == null) continue;
//            foreach (var pc in projCols)
//            {
//                if (pc == null) continue;
//                Physics2D.IgnoreCollision(oc, pc, false);
//            }
//        }
//    }
//}
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skills/Shoot Light Arrow")]
public class ShootLightArrowSkill : WeaponSkill
{
    int shootCount = 0;
   
    [Header("Triple Shot")]
    public float spreadAngle = 15f;
    public int multiArrowCount = 5;
    [Header("Prefabs")]
    public GameObject lightArrowPrefab;
    public GameObject muzzleFlashPrefab;

    [Header("Shoot")]
    public float speed = 16f;
    public float lifeTime = 1.2f;
    public float muzzleOffset = 0.8f;
    public float aimAngleOffset = 0f;

    [Header("Combat")]
    public int damage = 100;
    public DamageType damageType = DamageType.Physical;
    public float atkScale = 1.1f;
    public float smptScale = 0f;
    public bool canCrit = true;

    [Header("Anti self-hit")]
    public float ignoreOwnerCollisionTime = 0.08f;

    [Header("Muzzle Flash")]
    public float muzzleFlashLife = 0.25f;

    [Header("Animation")]
    public float skillAnimDuration = 0.6f; // phải khớp với độ dài animation clip

    //protected override void OnUse(SkillContext ctx)
    //{

    //    if (lightArrowPrefab == null || ctx.weaponPivot == null)
    //        return;

    //    var owner = ctx.owner;
    //    var playerMove = owner.GetComponent<PlayerMove2D>();
    //    // ===== THÊM ĐOẠN NÀY Ở ĐÂY =====
    //    Vector2 ownerPos = new Vector2(owner.position.x, owner.position.y);
    //    Vector2 dir = (Vector2)ctx.mouseWorld - ownerPos;

    //    // nếu có object Graphics thì nên flip nó thay vì root
    //    Transform graphics = ctx.owner.Find("Graphics");
    //    float sign = dir.x >= 0 ? 1f : -1f;

    //    if (graphics != null)
    //    {
    //        //graphics.localScale = new Vector3(
    //        //    Mathf.Abs(graphics.localScale.x) * sign,
    //        //    graphics.localScale.y,
    //        //    graphics.localScale.z
    //        //);

    //    }
    //    else
    //    {
    //        ctx.owner.localScale = new Vector3(
    //            Mathf.Abs(ctx.owner.localScale.x) * sign,
    //            ctx.owner.localScale.y,
    //            ctx.owner.localScale.z
    //        );

    //        playerMove.visualNormal.transform.localScale = playerMove.visualSkill.transform.localScale;
    //    }

    //    // ===== HẾT ĐOẠN THÊM =====
    //    if (playerMove != null)
    //        playerMove.SetSkillVisual();

    //    var anim = owner.GetComponentInChildren<Animator>();
    //    if (anim != null)
    //        anim.SetTrigger("Q_AL");

    //    // chạy coroutine delay spawn
    //    var runner = owner.GetComponent<SkillCoroutineRunner>();
    //    if (runner == null)
    //        runner = owner.gameObject.AddComponent<SkillCoroutineRunner>();

    //    runner.Run(DelayedShoot(ctx, playerMove));
    //}
    protected override void OnUse(SkillContext ctx)
    {
        if (lightArrowPrefab == null || ctx.weaponPivot == null)
            return;

        var owner = ctx.owner;
        var playerMove = owner.GetComponent<PlayerMove2D>();

        // ===== TÍNH HƯỚNG GIỐNG FIREBIRD =====
        Vector3 mouseWorld = ctx.mouseWorld;
        mouseWorld.z = 0f;

        Vector2 aimDir = (mouseWorld - owner.position);
        if (aimDir.sqrMagnitude < 0.0001f)
            aimDir = Vector2.right;

        aimDir.Normalize();

        // ===== LẬT ĐÚNG VISUAL_SKILL =====
        Transform visual = owner.transform.Find("Visual_Skill");
        float sign = aimDir.x >= 0 ? 1f : -1f;

        if (visual != null)
        {
            visual.localScale = new Vector3(
                Mathf.Abs(visual.localScale.x) * sign,
                visual.localScale.y,
                visual.localScale.z
            );
        }

        if (playerMove != null)
            playerMove.SetSkillVisual();

        var anim = owner.GetComponentInChildren<Animator>();
        if (anim != null)
            anim.SetTrigger("Q_AL");
        // ===== DELAY SPAWN =====
        var runner = owner.GetComponent<SkillCoroutineRunner>();
        if (runner == null)
            runner = owner.gameObject.AddComponent<SkillCoroutineRunner>();

        runner.Run(DelayedShoot(ctx, playerMove));
    }
    IEnumerator DelayedShoot(SkillContext ctx, PlayerMove2D playerMove)
    {
        yield return new WaitForSeconds(skillAnimDuration);

        var owner = ctx.owner;

        Vector3 pivotPos = ctx.weaponPivot.position;

        Vector2 dir = (ctx.mouseWorld - pivotPos);
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.right;

        dir.Normalize();

        Vector3 spawnPos = pivotPos + (Vector3)(dir * muzzleOffset);
        spawnPos.z = 0f;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + aimAngleOffset;
        Quaternion rot = Quaternion.Euler(0f, 0f, angle);

        // muzzle flash
        if (muzzleFlashPrefab != null)
        {
            var mf = Object.Instantiate(muzzleFlashPrefab, spawnPos, rot);
            if (muzzleFlashLife > 0f)
                Object.Destroy(mf, muzzleFlashLife);
        }

        // spawn projectile
        //var go = Object.Instantiate(lightArrowPrefab, spawnPos, rot);

        //var dmgComp = go.GetComponent<ProjectileDamage>();
        //if (dmgComp != null)
        //{
        //    var ownerGo = owner.gameObject;
        //    var attackerStats = ownerGo.GetComponent<PlayerStatsMono>();

        //    dmgComp.InitScaled(
        //        attackerStats,
        //        damage,
        //        atkScale,
        //        smptScale,
        //        damageType,
        //        canCrit,
        //        ownerGo
        //    );
        //}

        //var rb = go.GetComponent<Rigidbody2D>();
        //if (rb != null)
        //{
        //    rb.gravityScale = 0f;
        //    rb.linearVelocity = dir * speed;
        //}

        //IgnoreOwnerCollision(owner, go, ignoreOwnerCollisionTime);

        //Object.Destroy(go, lifeTime);
        shootCount++;

        int arrowCount = 1;

        if (shootCount >= 3)
        {
            arrowCount = multiArrowCount;
            shootCount = 0;
        }

        for (int i = 0; i < arrowCount; i++)
        {
            float offset = 0;

            if (arrowCount > 1)
            {
                float startAngle = -spreadAngle * (arrowCount - 1) * 0.5f;
                offset = startAngle + spreadAngle * i;
            }

             angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + aimAngleOffset + offset;
             rot = Quaternion.Euler(0f, 0f, angle);

            Vector2 newDir = Quaternion.Euler(0, 0, offset) * dir;

            Vector3 spawnOffset = (Vector3)newDir * 0.15f * i;
            var go = Object.Instantiate(lightArrowPrefab, spawnPos + spawnOffset, rot);

            var dmgComp = go.GetComponent<ProjectileDamage>();
            if (dmgComp != null)
            {
                var ownerGo = owner.gameObject;
                var attackerStats = ownerGo.GetComponent<PlayerStatsMono>();

                dmgComp.InitScaled(
                    attackerStats,
                    damage,
                    atkScale,
                    smptScale,
                    damageType,
                    canCrit,
                    ownerGo
                );
            }

            var rb = go.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearVelocity = newDir * speed;
            }

            IgnoreOwnerCollision(owner, go, ignoreOwnerCollisionTime);

            Object.Destroy(go, lifeTime);
        }

        if (playerMove != null)
            playerMove.SetNormalVisual();
    }

    static void IgnoreOwnerCollision(Transform owner, GameObject projectile, float restoreAfter)
    {
        if (owner == null || projectile == null)
            return;

        var ownerCols = owner.GetComponentsInChildren<Collider2D>();
        var projCols = projectile.GetComponentsInChildren<Collider2D>();

        foreach (var oc in ownerCols)
        {
            if (oc == null) continue;
            foreach (var pc in projCols)
            {
                if (pc == null) continue;
                Physics2D.IgnoreCollision(oc, pc, true);
            }
        }

        if (restoreAfter > 0f)
        {
            var runner = owner.GetComponent<SkillCoroutineRunner>();
            if (runner == null)
                runner = owner.gameObject.AddComponent<SkillCoroutineRunner>();

            runner.Run(RestoreCollision(ownerCols, projCols, restoreAfter));
        }
    }

    static IEnumerator RestoreCollision(Collider2D[] ownerCols, Collider2D[] projCols, float t)
    {
        yield return new WaitForSeconds(t);

        foreach (var oc in ownerCols)
        {
            if (oc == null) continue;
            foreach (var pc in projCols)
            {
                if (pc == null) continue;
                Physics2D.IgnoreCollision(oc, pc, false);
            }
        }
    }
}