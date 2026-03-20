
//using System.Collections;
//using UnityEngine;

//[CreateAssetMenu(menuName = "Game/Skills/Ultimate Charge Arrow Skill")]
//public class UltimateChargeArrowSkill : WeaponSkill, IChargeSkill
//{
//    [Header("Charge")]
//    public float chargeTime = 4f;

//    [Header("Combat (Base + Scaling)")]
//    public int damage = 180;
//    public DamageType damageType = DamageType.Physical;
//    public float atkScale = 1.4f;
//    public float smptScale = 0f;
//    public bool canCrit = true;

//    [Header("Charge VFX at bow muzzle")]
//    public GameObject chargeVfxPrefab;
//    public Vector3 vfxLocalOffset = Vector3.zero;
//    public float chargeVfxAngleOffset = 0f;
//    public bool rotateChargeVfxToMouse = true;

//    [Header("Charge Aura (on owner)")]
//    public GameObject auraPrefab;
//    public Vector3 auraLocalOffset = new Vector3(0f, 0.15f, 0f);

//    [Header("Ultimate POW (on success)")]
//    public GameObject ultPowPrefab;
//    public Vector3 ultPowLocalOffset = new Vector3(0f, 1.0f, 0f);
//    public float ultPowLife = 0.6f;

//    [Header("Timing")]
//    public float fireDelayAfterPow = 0.12f;

//    [Header("Charge Bar")]
//    public ChargeBarWorld chargeBarPrefab;
//    public Vector3 barWorldOffset = new Vector3(0f, 1.2f, 0f);

//    [Header("Projectile")]
//    public GameObject projectilePrefab;
//    public float projectileSpeed = 16f;
//    public float projectileLife = 1.2f;
//    public float spawnForward = 1.2f;
//    public bool rotateProjectileToDir = true;
//    public float projectileSpriteAngleOffset = 0f;

//    float t;
//    bool charging;

//    GameObject chargeVfxInstance;
//    GameObject auraInstance;
//    ChargeBarWorld barInstance;

//    PlayerMove2D cachedMove;
//    Animator cachedAnimator;

//    bool IsFullyCharged => (chargeTime <= 0.01f) || (t >= chargeTime - 0.0001f);

//    public void ChargeBegin(SkillContext ctx)
//    {
//        if (charging) return;
//        if (ctx.owner == null || ctx.weaponPivot == null) return;

//        charging = true;
//        t = 0f;

//        var owner = ctx.owner;

//        cachedMove = owner.GetComponent<PlayerMove2D>();
//       // cachedAnimator = owner.GetComponentInChildren<Animator>();
//         cachedAnimator = ctx.owner.transform.Find("Visual_Skill")?.GetComponent<Animator>();

//        if (cachedMove != null)
//            cachedMove.SetSkillVisual();

//        if (cachedAnimator != null)
//            cachedAnimator.SetBool("E_AL", true);

//        // Flip hướng nhân vật theo chuột
//        Vector2 ownerPos = owner.position;
//        Vector2 dir = (Vector2)ctx.mouseWorld - ownerPos;

//        Transform graphics = owner.Find("Graphics");
//        float scaleX = dir.x > 0 ? 0.6f : -0.6f;

//        if (graphics != null)
//            graphics.localScale = new Vector3(scaleX, 0.6f, 0.6f);
//        else
//            owner.localScale = new Vector3(scaleX, 0.6f, 0.6f);

//        // Spawn VFX
//        if (chargeVfxPrefab != null)
//        {
//            Vector3 p = ctx.weaponPivot.TransformPoint(vfxLocalOffset);
//            p.z = 0f;

//            chargeVfxInstance = Instantiate(chargeVfxPrefab, p, Quaternion.identity);
//            chargeVfxInstance.transform.SetParent(ctx.weaponPivot, true);
//            UpdateChargeVfxTransform(ctx);
//        }

//        // Aura
//        if (auraPrefab != null)
//        {
//            Vector3 p = owner.position + auraLocalOffset;
//            p.z = 0f;

//            auraInstance = Instantiate(auraPrefab, p, Quaternion.identity);
//            auraInstance.transform.SetParent(owner, true);
//        }

//        // Charge bar
//        if (chargeBarPrefab != null)
//        {
//            barInstance = Instantiate(chargeBarPrefab, owner.position + barWorldOffset, Quaternion.identity);
//            barInstance.followTarget = owner;
//            barInstance.worldOffset = barWorldOffset;
//            barInstance.Set01(0f);
//        }
//    }

//    public void ChargeTick(SkillContext ctx, float dt)
//    {
//        if (!charging) return;
//        if (ctx.owner == null || ctx.weaponPivot == null) { Cancel(); return; }

//        t += dt;

//        float ratio = chargeTime <= 0.01f ? 1f : Mathf.Clamp01(t / chargeTime);

//        if (barInstance != null)
//            barInstance.Set01(ratio);

//        UpdateChargeVfxTransform(ctx);

//        if (ratio >= 1f)
//        {
//            charging = false;
//            EndChargeVisuals();
//            RunRoutine(ctx, PowThenFireRoutine(ctx));
//        }
//    }

//    public void ChargeEnd(SkillContext ctx, bool released)
//    {
//        if (!charging) return;

//        if (released && !IsFullyCharged)
//        {
//            Cancel();
//            return;
//        }

//        if (released && IsFullyCharged)
//        {
//            charging = false;
//            EndChargeVisuals();
//            RunRoutine(ctx, PowThenFireRoutine(ctx));
//            return;
//        }

//        Cancel();
//    }

//    IEnumerator PowThenFireRoutine(SkillContext ctx)
//    {
//        SpawnUltPow(ctx);

//        if (fireDelayAfterPow > 0f)
//            yield return new WaitForSeconds(fireDelayAfterPow);

//        Fire(ctx);

//        // Trả lại trạng thái bình thường
//        if (cachedAnimator != null)
//            cachedAnimator.SetBool("E_AL", false);

//        if (cachedMove != null)
//            cachedMove.SetNormalVisual();
//    }

//    void SpawnUltPow(SkillContext ctx)
//    {
//        if (ultPowPrefab == null || ctx.owner == null) return;

//        Vector3 p = ctx.owner.position + ultPowLocalOffset;
//        p.z = 0f;

//        var go = Instantiate(ultPowPrefab, p, Quaternion.identity);
//        go.transform.SetParent(ctx.owner, true);

//        if (ultPowLife > 0f)
//            Destroy(go, ultPowLife);
//    }

//    void Fire(SkillContext ctx)
//    {
//        if (projectilePrefab == null || ctx.weaponPivot == null) return;

//        Vector2 dir = (Vector2)(ctx.mouseWorld - ctx.weaponPivot.position);
//        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
//        dir.Normalize();

//        Vector3 pos = ctx.weaponPivot.position + (Vector3)(dir * spawnForward);
//        pos.z = 0f;

//        Quaternion rot = Quaternion.identity;
//        if (rotateProjectileToDir)
//        {
//            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + projectileSpriteAngleOffset;
//            rot = Quaternion.Euler(0f, 0f, angle);
//        }

//        var go = Instantiate(projectilePrefab, pos, rot);

//        var dmgComp = go.GetComponent<ProjectileDamage>();
//        if (dmgComp != null)
//        {
//            var ownerGo = ctx.owner != null ? ctx.owner.gameObject : null;
//            var stats = ownerGo != null ? ownerGo.GetComponent<PlayerStatsMono>() : null;

//            dmgComp.InitScaled(
//                stats,
//                damage,
//                atkScale,
//                smptScale,
//                damageType,
//                canCrit,
//                ownerGo
//            );
//        }

//        var rb = go.GetComponent<Rigidbody2D>();
//        if (rb != null)
//        {
//            rb.gravityScale = 0f;
//            rb.linearVelocity = dir * projectileSpeed;
//        }

//        Destroy(go, projectileLife);
//    }

//    void Cancel()
//    {
//        charging = false;
//        t = 0f;

//        EndChargeVisuals();

//        if (cachedAnimator != null)
//            cachedAnimator.SetBool("E_AL", false);

//        if (cachedMove != null)
//            cachedMove.SetNormalVisual();
//    }

//    void EndChargeVisuals()
//    {
//        if (chargeVfxInstance != null) Destroy(chargeVfxInstance);
//        if (auraInstance != null) Destroy(auraInstance);
//        if (barInstance != null) Destroy(barInstance.gameObject);

//        chargeVfxInstance = null;
//        auraInstance = null;
//        barInstance = null;
//    }

//    void UpdateChargeVfxTransform(SkillContext ctx)
//    {
//        if (chargeVfxInstance == null) return;

//        Vector3 p = ctx.weaponPivot.TransformPoint(vfxLocalOffset);
//        p.z = 0f;
//        chargeVfxInstance.transform.position = p;

//        if (!rotateChargeVfxToMouse) return;

//        Vector2 dir = (Vector2)(ctx.mouseWorld - ctx.weaponPivot.position);
//        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
//        dir.Normalize();

//        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + chargeVfxAngleOffset;
//        chargeVfxInstance.transform.rotation = Quaternion.Euler(0f, 0f, angle);
//    }

//    void RunRoutine(SkillContext ctx, IEnumerator routine)
//    {
//        if (ctx.owner == null) return;

//        var runner = ctx.owner.GetComponent<SkillCoroutineRunner>();
//        if (runner == null)
//            runner = ctx.owner.gameObject.AddComponent<SkillCoroutineRunner>();

//        runner.Run(routine);
//    }

//    protected override void OnUse(SkillContext ctx) { }
//}

using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skills/Ultimate Charge Arrow Skill")]
public class UltimateChargeArrowSkill : WeaponSkill, IChargeSkill
{
    [Header("Charge")]
    public float chargeTime = 4f;

    [Header("Combat (Base + Scaling)")]
    public int damage = 180;
    public DamageType damageType = DamageType.Physical;
    public float atkScale = 1.4f;
    public float smptScale = 0f;
    public bool canCrit = true;

    [Header("Charge VFX at bow muzzle")]
    public GameObject chargeVfxPrefab;
    public Vector3 vfxLocalOffset = Vector3.zero;
    public float chargeVfxAngleOffset = 0f;
    public bool rotateChargeVfxToMouse = true;

    [Header("Charge Aura (on owner)")]
    public GameObject auraPrefab;
    public Vector3 auraLocalOffset = new Vector3(0f, 0.15f, 0f);

    [Header("Ultimate POW (on success)")]
    public GameObject ultPowPrefab;
    public Vector3 ultPowLocalOffset = new Vector3(0f, 1.0f, 0f);
    public float ultPowLife = 0.6f;

    [Header("Timing")]
    public float fireDelayAfterPow = 0.12f;

    [Header("Charge Bar")]
    public ChargeBarWorld chargeBarPrefab;
    public Vector3 barWorldOffset = new Vector3(0f, 1.2f, 0f);

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 16f;
    public float projectileLife = 1.2f;
    public float spawnForward = 1.2f;
    public bool rotateProjectileToDir = true;
    public float projectileSpriteAngleOffset = 0f;

    float t;
    bool charging;

    GameObject chargeVfxInstance;
    GameObject auraInstance;
    ChargeBarWorld barInstance;

    PlayerMove2D cachedMove;
    Animator cachedAnimator;
    SkillAimButton aimButton;
    bool IsFullyCharged => (chargeTime <= 0.01f) || (t >= chargeTime - 0.0001f);

    public void ChargeBegin(SkillContext ctx)
    {
        if (charging) return;
        if (ctx.owner == null || ctx.weaponPivot == null) return;

        charging = true;
        t = 0f;

        var owner = ctx.owner;

        cachedMove = owner.GetComponent<PlayerMove2D>();
        // cachedAnimator = owner.GetComponentInChildren<Animator>();
        cachedAnimator = ctx.owner.transform.Find("Visual_Skill")?.GetComponent<Animator>();

        if (cachedMove != null)
            cachedMove.SetSkillVisual();

        if (cachedAnimator != null)
            cachedAnimator.SetBool("E_AL", true);



        Vector2 dir = ctx.aimDirection;
             Debug.Log("Bắn theo hướng debug : " + dir);


        //Transform graphics = owner.Find("Graphics");

        //if (graphics != null)
        //{
        //    float sign = dir.x >= 0 ? 1f : -1f;
        //    graphics.localScale = new Vector3(
        //        Mathf.Abs(graphics.localScale.x) * sign,
        //        graphics.localScale.y,
        //        graphics.localScale.z
        //    );
        //}
        //else
        //{
        //    float sign = dir.x >= 0 ? 1f : -1f;
        //    owner.localScale = new Vector3(
        //        Mathf.Abs(owner.localScale.x) * sign,
        //        owner.localScale.y,
        //        owner.localScale.z
        //    );
        //}
       
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.right;

        dir.Normalize();

        // 🔥 LẬT ĐÚNG VISUAL_SKILL
        Transform visual = owner.transform.Find("Visual_Skill");
        float sign = dir.x >= 0 ? 1f : -1f;

        if (visual != null)
        {
            visual.localScale = new Vector3(
                Mathf.Abs(visual.localScale.x) * sign,
                visual.localScale.y,
                visual.localScale.z
            );
        }
        // Spawn VFX
        if (chargeVfxPrefab != null)
        {
            Vector3 p = ctx.weaponPivot.TransformPoint(vfxLocalOffset);
            p.z = 0f;

            chargeVfxInstance = Instantiate(chargeVfxPrefab, p, Quaternion.identity);
            chargeVfxInstance.transform.SetParent(ctx.weaponPivot, true);
            UpdateChargeVfxTransform(ctx);
        }

        // Aura
        if (auraPrefab != null)
        {
            Vector3 p = owner.position + auraLocalOffset;
            p.z = 0f;

            auraInstance = Instantiate(auraPrefab, p, Quaternion.identity);
            auraInstance.transform.SetParent(owner, true);
        }

        // Charge bar
        if (chargeBarPrefab != null)
        {
            barInstance = Instantiate(chargeBarPrefab, owner.position + barWorldOffset, Quaternion.identity);
            barInstance.followTarget = owner;
            barInstance.worldOffset = barWorldOffset;
            barInstance.Set01(0f);
        }
    }

    //public void ChargeTick(SkillContext ctx, float dt)
    //{
    //    if (!charging) return;
    //    if (ctx.owner == null || ctx.weaponPivot == null) { Cancel(); return; }

    //    t += dt;

    //    float ratio = chargeTime <= 0.01f ? 1f : Mathf.Clamp01(t / chargeTime);

    //    if (barInstance != null)
    //        barInstance.Set01(ratio);

    //    UpdateChargeVfxTransform(ctx);

    //    if (ratio >= 1f)
    //    {
    //        charging = false;
    //        EndChargeVisuals();
    //        RunRoutine(ctx, PowThenFireRoutine(ctx));
    //    }
    //}
    public void ChargeTick(SkillContext ctx, float dt)
    {
        if (!charging) return;
        if (ctx.owner == null || ctx.weaponPivot == null) { Cancel(); return; }

        t += dt;

        Vector2 dir = ctx.aimDirection;

        //// 🔥 UPDATE HƯỚNG MỖI FRAME
        //Transform graphics = ctx.owner.Find("Graphics");
        //float sign = dir.x >= 0 ? 1f : -1f;

        //if (graphics != null)
        //{
        //    graphics.localScale = new Vector3(
        //        Mathf.Abs(graphics.localScale.x) * -sign,
        //        graphics.localScale.y,
        //        graphics.localScale.z
        //    );
        //}
        //else
        //{
        //    ctx.owner.localScale = new Vector3(
        //        Mathf.Abs(ctx.owner.localScale.x) * -sign,
        //        ctx.owner.localScale.y,
        //        ctx.owner.localScale.z
        //    );
        //}
        //Vector2 dir = ctx.aimDirection;
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.right;

        dir.Normalize();

        Transform visual = ctx.owner.transform.Find("Visual_Skill");
        float sign = dir.x >= 0 ? 1f : -1f;

        if (visual != null)
        {
            visual.localScale = new Vector3(
                Mathf.Abs(visual.localScale.x) * sign,
                visual.localScale.y,
                visual.localScale.z
            );
        }
        float ratio = chargeTime <= 0.01f ? 1f : Mathf.Clamp01(t / chargeTime);

        if (barInstance != null)
            barInstance.Set01(ratio);

        UpdateChargeVfxTransform(ctx);

        if (ratio >= 1f)
        {
            charging = false;
            EndChargeVisuals();
            RunRoutine(ctx, PowThenFireRoutine(ctx));
        }
    }
    public void ChargeEnd(SkillContext ctx, bool released)
    {
        if (!charging) return;

        if (released && !IsFullyCharged)
        {
            Cancel();
            return;
        }

        if (released && IsFullyCharged)
        {
            charging = false;
            EndChargeVisuals();
            RunRoutine(ctx, PowThenFireRoutine(ctx));
            return;
        }

        Cancel();
    }

    IEnumerator PowThenFireRoutine(SkillContext ctx)
    {
        SpawnUltPow(ctx);

        if (fireDelayAfterPow > 0f)
            yield return new WaitForSeconds(fireDelayAfterPow);

        Fire(ctx);

        // Trả lại trạng thái bình thường
        if (cachedAnimator != null)
            cachedAnimator.SetBool("E_AL", false);

        if (cachedMove != null)
            cachedMove.SetNormalVisual();
    }

    void SpawnUltPow(SkillContext ctx)
    {
        if (ultPowPrefab == null || ctx.owner == null) return;

        Vector3 p = ctx.owner.position + ultPowLocalOffset;
        p.z = 0f;

        var go = Instantiate(ultPowPrefab, p, Quaternion.identity);
        go.transform.SetParent(ctx.owner, true);

        if (ultPowLife > 0f)
            Destroy(go, ultPowLife);
    }

    void Fire(SkillContext ctx)
    {
        if (projectilePrefab == null || ctx.weaponPivot == null) return;

        Vector2 dir = ctx.aimDirection;

        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.right;

        dir.Normalize();
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
        dir.Normalize();

        Vector3 pos = ctx.weaponPivot.position + (Vector3)(dir * spawnForward);
        pos.z = 0f;

        Quaternion rot = Quaternion.identity;
        if (rotateProjectileToDir)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + projectileSpriteAngleOffset;
            rot = Quaternion.Euler(0f, 0f, angle);
        }

        var go = Instantiate(projectilePrefab, pos, rot);

        var dmgComp = go.GetComponent<ProjectileDamage>();
        if (dmgComp != null)
        {
            var ownerGo = ctx.owner != null ? ctx.owner.gameObject : null;
            var stats = ownerGo != null ? ownerGo.GetComponent<PlayerStatsMono>() : null;

            dmgComp.InitScaled(
                stats,
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
            rb.linearVelocity = dir * projectileSpeed;
        }

        Destroy(go, projectileLife);
    }

    void Cancel()
    {
        charging = false;
        t = 0f;

        EndChargeVisuals();

        if (cachedAnimator != null)
            cachedAnimator.SetBool("E_AL", false);

        if (cachedMove != null)
            cachedMove.SetNormalVisual();
    }

    void EndChargeVisuals()
    {
        if (chargeVfxInstance != null) Destroy(chargeVfxInstance);
        if (auraInstance != null) Destroy(auraInstance);
        if (barInstance != null) Destroy(barInstance.gameObject);

        chargeVfxInstance = null;
        auraInstance = null;
        barInstance = null;
    }

    void UpdateChargeVfxTransform(SkillContext ctx)
    {
        if (chargeVfxInstance == null) return;

        Vector3 p = ctx.weaponPivot.TransformPoint(vfxLocalOffset);
        p.z = 0f;
        chargeVfxInstance.transform.position = p;

        if (!rotateChargeVfxToMouse) return;

        Vector2 dir = ctx.aimDirection;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
        dir.Normalize();

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + chargeVfxAngleOffset;
        chargeVfxInstance.transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void RunRoutine(SkillContext ctx, IEnumerator routine)
    {
        if (ctx.owner == null) return;

        var runner = ctx.owner.GetComponent<SkillCoroutineRunner>();
        if (runner == null)
            runner = ctx.owner.gameObject.AddComponent<SkillCoroutineRunner>();

        runner.Run(routine);
    }

    protected override void OnUse(SkillContext ctx) { }
}