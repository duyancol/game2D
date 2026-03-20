////using System.Collections;
////using System.Collections.Generic;
////using UnityEngine;

////[CreateAssetMenu(menuName = "Game/Skills/Shoot Green Abyss Spear")]
////public class ShootGreenAbyssSpearSkill : WeaponSkill
////{
////    [Header("Tap / Hold")]
////    public float holdThreshold = 0.22f;
////    public float stepCooldown = 0.08f;

////    [Header("Prefab (ONLY 1 spear image)")]
////    public GameObject stabPrefab;

////    [Header("Tap Stab")]
////    public float tapMuzzleOffset = 1.2f;
////    public float hitLifeTap = 0.5f;

////    [Header("Hold Throw Timing")]
////    [Tooltip("Tổng thời gian 3 mũi khi HOLD (chỉ để tham khảo nếu muốn auto chia)")]
////    public float holdTotalTime = 3f;

////    [Tooltip("Thời gian LAO/ĐÂM (start->end) của MỖI mũi khi HOLD")]
////    public float eachSpearMoveDuration = 1.2f;   // <-- tăng lên cho đâm dài hơn

////    [Tooltip("Thời gian đứng lại sau khi tới đích (tồn tại lâu hơn)")]
////    public float eachSpearLinger = 0.55f;        // <-- tăng lên cho mũi tồn tại lâu hơn

////    [Header("Hold Throw Motion")]
////    public float throwDistance = 6.5f;
////    public float fanAngle = 30f; // +30 / 0 / -30

////    [Header("Aim / Sprite Fix")]
////    public float spriteAngleOffset = 0f;

////    [Header("Combat")]
////    public int tapDamage = 35;
////    public float tapAtkScale = 1.0f;

////    public int throwDamage = 60;
////    public float throwAtkScale = 0.95f;

////    public DamageType damageType = DamageType.Physical;
////    public float smptScale = 0f;
////    public bool canCrit = true;

////    [Header("Auto collider (nếu prefab chưa có)")]
////    public Vector2 boxSize = new Vector2(2.6f, 1.2f);
////    public bool useBoxCollider = true;

////    class State { public float nextTime; public bool busy; }
////    static readonly Dictionary<int, State> states = new();

////    protected override void OnUse(SkillContext ctx)
////    {
////        if (ctx.owner == null || ctx.weaponPivot == null || stabPrefab == null) return;

////        int id = ctx.owner.GetInstanceID();
////        if (!states.TryGetValue(id, out var st)) states[id] = st = new State();

////        if (Time.time < st.nextTime) return;
////        if (st.busy) return;

////        st.nextTime = Time.time + stepCooldown;

////        var runner = ctx.owner.GetComponent<SkillCoroutineRunner>();
////        if (runner == null) runner = ctx.owner.gameObject.AddComponent<SkillCoroutineRunner>();

////        runner.Run(TapOrHoldRoutine(ctx, st));
////    }

////    IEnumerator TapOrHoldRoutine(SkillContext ctx, State st)
////    {
////        st.busy = true;
////        try
////        {
////            Transform pivot = ctx.weaponPivot;

////            Vector2 baseDir = (ctx.mouseWorld - pivot.position);
////            if (baseDir.sqrMagnitude < 0.0001f) baseDir = Vector2.right;
////            baseDir.Normalize();

////            float t = 0f;
////            while (t < holdThreshold)
////            {
////                if (Input.GetMouseButtonUp(0)) break;
////                t += Time.deltaTime;
////                yield return null;
////            }

////            bool isHold = Input.GetMouseButton(0) && t >= holdThreshold;

////            if (!isHold)
////            {
////                Vector3 tapPos = pivot.position + (Vector3)(baseDir * tapMuzzleOffset);
////                SpawnStaticHit(ctx, tapPos, baseDir, tapDamage, tapAtkScale, hitLifeTap);
////                yield break;
////            }

////            float moveDur = Mathf.Max(0.01f, eachSpearMoveDuration);
////            float linger = Mathf.Max(0f, eachSpearLinger);

////            float[] angles = { +fanAngle, 0f, -fanAngle };

////            for (int i = 0; i < angles.Length; i++)
////            {
////                Vector2 dir = Rotate(baseDir, angles[i]);
////                yield return ThrowOnce(ctx, pivot.position, dir, moveDur, linger);
////            }
////        }
////        finally
////        {
////            st.busy = false;
////        }
////    }

////    // ✅ SỬA: tách moveDuration + lingerAfter
////    IEnumerator ThrowOnce(SkillContext ctx, Vector3 startPos, Vector2 dir, float moveDuration, float lingerAfter)
////    {
////        Vector3 start = startPos; start.z = 0f;
////        Vector3 end = start + (Vector3)(dir * throwDistance); end.z = 0f;

////        var go = Instantiate(stabPrefab, start, RotationFromDir(dir));
////        ApplySortingLikeOwner(ctx, go);

////        EnsureTriggerCollider(go);
////        EnsureKinematicRB(go);

////        var dmg = go.GetComponent<ProjectileDamage>();
////        if (dmg == null) dmg = go.AddComponent<ProjectileDamage>();

////        var ownerGo = ctx.owner.gameObject;
////        var attackerStats = ownerGo.GetComponent<PlayerStatsMono>();
////        dmg.InitScaled(attackerStats, throwDamage, throwAtkScale, smptScale, damageType, canCrit, ownerGo);

////        float dur = Mathf.Max(0.01f, moveDuration);
////        float t = 0f;

////        // Move: lao/đâm chậm hơn
////        while (t < 1f)
////        {
////            t += Time.deltaTime / dur;
////            if (go == null) yield break;
////            go.transform.position = Vector3.Lerp(start, end, t);
////            yield return null;
////        }

////        // Linger: đứng lại thêm (mũi tồn tại lâu hơn)
////        float linger = Mathf.Max(0f, lingerAfter);
////        if (linger > 0f)
////            yield return new WaitForSeconds(linger);

////        if (go != null) Destroy(go);
////    }

////    void SpawnStaticHit(SkillContext ctx, Vector3 pos, Vector2 dir, int baseDmg, float atkScale, float life)
////    {
////        pos.z = 0f;
////        var go = Instantiate(stabPrefab, pos, RotationFromDir(dir));
////        ApplySortingLikeOwner(ctx, go);

////        EnsureTriggerCollider(go);
////        EnsureKinematicRB(go);

////        var dmg = go.GetComponent<ProjectileDamage>();
////        if (dmg == null) dmg = go.AddComponent<ProjectileDamage>();

////        var ownerGo = ctx.owner.gameObject;
////        var attackerStats = ownerGo.GetComponent<PlayerStatsMono>();

////        dmg.InitScaled(attackerStats, baseDmg, atkScale, smptScale, damageType, canCrit, ownerGo);

////        Destroy(go, life);
////    }

////    Quaternion RotationFromDir(Vector2 dir)
////    {
////        float z = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + spriteAngleOffset;
////        return Quaternion.Euler(0f, 0f, z);
////    }

////    void ApplySortingLikeOwner(SkillContext ctx, GameObject go)
////    {
////        var sr = go.GetComponentInChildren<SpriteRenderer>();
////        if (sr == null) return;

////        var ownerSr = ctx.owner.GetComponentInChildren<SpriteRenderer>();
////        if (ownerSr != null)
////        {
////            sr.sortingLayerID = ownerSr.sortingLayerID;
////            sr.sortingOrder = ownerSr.sortingOrder + 10;
////        }
////        else
////        {
////            sr.sortingLayerName = "VFX";
////            sr.sortingOrder = 50;
////        }
////    }

////    void EnsureTriggerCollider(GameObject go)
////    {
////        Collider2D col = go.GetComponent<Collider2D>();
////        if (col == null)
////        {
////            if (useBoxCollider)
////            {
////                var bc = go.AddComponent<BoxCollider2D>();
////                bc.size = boxSize;
////                bc.isTrigger = true;
////            }
////            else
////            {
////                var cc = go.AddComponent<CircleCollider2D>();
////                cc.radius = Mathf.Max(0.1f, boxSize.x * 0.5f);
////                cc.isTrigger = true;
////            }
////        }
////        else col.isTrigger = true;
////    }

////    void EnsureKinematicRB(GameObject go)
////    {
////        var rb = go.GetComponent<Rigidbody2D>();
////        if (rb == null) rb = go.AddComponent<Rigidbody2D>();
////        rb.gravityScale = 0f;
////        rb.bodyType = RigidbodyType2D.Kinematic;
////        rb.linearVelocity = Vector2.zero;
////    }

////    static Vector2 Rotate(Vector2 v, float deg)
////    {
////        float rad = deg * Mathf.Deg2Rad;
////        float cos = Mathf.Cos(rad);
////        float sin = Mathf.Sin(rad);
////        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos).normalized;
////    }
////}
////using System.Collections;
////using System.Collections.Generic;
////using UnityEngine;

////[CreateAssetMenu(menuName = "Game/Skills/Shoot Green Abyss Spear")]
////public class ShootGreenAbyssSpearSkill : WeaponSkill
////{
////    [Header("Tap / Hold")]
////    public float holdThreshold = 0.22f;
////    public float stepCooldown = 0.08f;

////    [Header("Prefab (ONLY 1 spear image)")]
////    public GameObject stabPrefab;

////    [Header("Tap Stab")]
////    public float tapMuzzleOffset = 1.2f;
////    public float hitLifeTap = 0.5f;

////    [Header("Hold Timing")]
////    [Tooltip("Thời gian LAO/ĐÂM (start->end) của MỖI mũi khi HOLD")]
////    public float eachSpearMoveDuration = 1.2f;

////    [Tooltip("Sau khi trúng (hoặc tới đích), mũi tồn tại thêm bao lâu rồi mới biến mất")]
////    public float eachSpearLinger = 0.55f;

////    [Header("Hold Throw Motion")]
////    public float throwDistance = 6.5f;
////    public float fanAngle = 30f; // +30 / 0 / -30

////    [Header("Aim / Sprite Fix")]
////    public float spriteAngleOffset = 0f;

////    [Header("Combat")]
////    public int tapDamage = 35;
////    public float tapAtkScale = 1.0f;

////    public int throwDamage = 60;
////    public float throwAtkScale = 0.95f;

////    public DamageType damageType = DamageType.Physical;
////    public float smptScale = 0f;
////    public bool canCrit = true;

////    [Header("Auto collider (nếu prefab chưa có)")]
////    public Vector2 boxSize = new Vector2(2.6f, 1.2f);
////    public bool useBoxCollider = true;

////    class State { public float nextTime; public bool busy; }
////    static readonly Dictionary<int, State> states = new();

////    protected override void OnUse(SkillContext ctx)
////    {
////        if (ctx.owner == null || ctx.weaponPivot == null || stabPrefab == null) return;

////        int id = ctx.owner.GetInstanceID();
////        if (!states.TryGetValue(id, out var st)) states[id] = st = new State();

////        if (Time.time < st.nextTime) return;
////        if (st.busy) return;

////        st.nextTime = Time.time + stepCooldown;

////        var runner = ctx.owner.GetComponent<SkillCoroutineRunner>();
////        if (runner == null) runner = ctx.owner.gameObject.AddComponent<SkillCoroutineRunner>();

////        runner.Run(TapOrHoldRoutine(ctx, st));
////    }

////    IEnumerator TapOrHoldRoutine(SkillContext ctx, State st)
////    {
////        st.busy = true;
////        try
////        {
////            Transform pivot = ctx.weaponPivot;

////            Vector2 baseDir = (ctx.mouseWorld - pivot.position);
////            if (baseDir.sqrMagnitude < 0.0001f) baseDir = Vector2.right;
////            baseDir.Normalize();

////            // phân biệt TAP vs HOLD
////            float holdT = 0f;
////            while (holdT < holdThreshold)
////            {
////                if (Input.GetMouseButtonUp(0)) break;
////                holdT += Time.deltaTime;
////                yield return null;
////            }

////            bool isHold = Input.GetMouseButton(0) && holdT >= holdThreshold;

////            if (!isHold)
////            {
////                Vector3 tapPos = pivot.position + (Vector3)(baseDir * tapMuzzleOffset);
////                SpawnStaticHit(ctx, tapPos, baseDir, tapDamage, tapAtkScale, hitLifeTap);
////                yield break;
////            }

////            float moveDur = Mathf.Max(0.01f, eachSpearMoveDuration);
////            float linger = Mathf.Max(0f, eachSpearLinger);

////            float[] angles = { +fanAngle, 0f, -fanAngle };

////            for (int i = 0; i < angles.Length; i++)
////            {
////                Vector2 dir = Rotate(baseDir, angles[i]);
////                yield return ThrowOnce_GhimOnHit(ctx, pivot.position, dir, moveDur, linger);
////            }
////        }
////        finally
////        {
////            st.busy = false;
////        }
////    }

////    /// <summary>
////    /// Bay từ start -> end trong moveDuration.
////    /// Nếu va chạm boss: GHIM tại chỗ, tồn tại thêm lingerAfter rồi mới biến mất.
////    /// Nếu không va chạm: tới đích, đứng lingerAfter rồi biến mất.
////    /// </summary>
////    IEnumerator ThrowOnce_GhimOnHit(SkillContext ctx, Vector3 startPos, Vector2 dir, float moveDuration, float lingerAfter)
////    {
////        Vector3 start = startPos; start.z = 0f;
////        Vector3 end = start + (Vector3)(dir * throwDistance); end.z = 0f;

////        var go = Instantiate(stabPrefab, start, RotationFromDir(dir));
////        ApplySortingLikeOwner(ctx, go);

////        EnsureTriggerCollider(go);
////        EnsureKinematicRB(go);

////        // ✅ Dùng script mới: trúng không biến mất liền
////        var stabDmg = go.GetComponent<SpearStabDamage>();
////        if (stabDmg == null) stabDmg = go.AddComponent<SpearStabDamage>();

////        var ownerGo = ctx.owner.gameObject;
////        var attackerStats = ownerGo.GetComponent<PlayerStatsMono>();

////        // Khi hit: script sẽ tự Destroy sau lingerAfter
////        stabDmg.InitScaled(
////            attackerStats,
////            throwDamage,
////            throwAtkScale,
////            smptScale,
////            damageType,
////            canCrit,
////            ownerGo,
////            destroyDelay: lingerAfter,
////            hitOnce: true
////        );

////        float dur = Mathf.Max(0.01f, moveDuration);
////        float t = 0f;

////        // Move: chậm theo dur; nếu HIT thì GHIM (dừng bay)
////        while (t < 1f)
////        {
////            if (go == null) yield break;

////            // ✅ nếu đã hit -> dừng bay (ghim)
////            if (stabDmg != null && stabDmg.HasHit) break;

////            t += Time.deltaTime / dur;
////            go.transform.position = Vector3.Lerp(start, end, t);
////            yield return null;
////        }

////        // Nếu CHƯA hit: tới đích rồi đứng lingerAfter -> destroy
////        if (go != null && (stabDmg == null || !stabDmg.HasHit))
////        {
////            if (lingerAfter > 0f) yield return new WaitForSeconds(lingerAfter);
////            if (go != null) Destroy(go);
////        }

////        // Nếu ĐÃ hit: KHÔNG destroy ở đây nữa (SpearStabDamage sẽ destroy sau lingerAfter)
////    }

////    void SpawnStaticHit(SkillContext ctx, Vector3 pos, Vector2 dir, int baseDmg, float atkScale, float life)
////    {
////        pos.z = 0f;
////        var go = Instantiate(stabPrefab, pos, RotationFromDir(dir));
////        ApplySortingLikeOwner(ctx, go);

////        EnsureTriggerCollider(go);
////        EnsureKinematicRB(go);

////        // ✅ TAP cũng dùng SpearStabDamage để không mất liền khi va chạm
////        var stabDmg = go.GetComponent<SpearStabDamage>();
////        if (stabDmg == null) stabDmg = go.AddComponent<SpearStabDamage>();

////        var ownerGo = ctx.owner.gameObject;
////        var attackerStats = ownerGo.GetComponent<PlayerStatsMono>();

////        stabDmg.InitScaled(
////            attackerStats,
////            baseDmg,
////            atkScale,
////            smptScale,
////            damageType,
////            canCrit,
////            ownerGo,
////            destroyDelay: life,
////            hitOnce: true
////        );

////        // Nếu miss thì vẫn tự huỷ sau life
////        Destroy(go, life);
////    }

////    Quaternion RotationFromDir(Vector2 dir)
////    {
////        float z = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + spriteAngleOffset;
////        return Quaternion.Euler(0f, 0f, z);
////    }

////    void ApplySortingLikeOwner(SkillContext ctx, GameObject go)
////    {
////        var sr = go.GetComponentInChildren<SpriteRenderer>();
////        if (sr == null) return;

////        var ownerSr = ctx.owner.GetComponentInChildren<SpriteRenderer>();
////        if (ownerSr != null)
////        {
////            sr.sortingLayerID = ownerSr.sortingLayerID;
////            sr.sortingOrder = ownerSr.sortingOrder + 10;
////        }
////        else
////        {
////            sr.sortingLayerName = "VFX";
////            sr.sortingOrder = 50;
////        }
////    }

////    void EnsureTriggerCollider(GameObject go)
////    {
////        Collider2D col = go.GetComponent<Collider2D>();
////        if (col == null)
////        {
////            if (useBoxCollider)
////            {
////                var bc = go.AddComponent<BoxCollider2D>();
////                bc.size = boxSize;
////                bc.isTrigger = true;
////            }
////            else
////            {
////                var cc = go.AddComponent<CircleCollider2D>();
////                cc.radius = Mathf.Max(0.1f, boxSize.x * 0.5f);
////                cc.isTrigger = true;
////            }
////        }
////        else col.isTrigger = true;
////    }

////    void EnsureKinematicRB(GameObject go)
////    {
////        var rb = go.GetComponent<Rigidbody2D>();
////        if (rb == null) rb = go.AddComponent<Rigidbody2D>();
////        rb.gravityScale = 0f;
////        rb.bodyType = RigidbodyType2D.Kinematic;
////        rb.linearVelocity = Vector2.zero;
////    }

////    static Vector2 Rotate(Vector2 v, float deg)
////    {
////        float rad = deg * Mathf.Deg2Rad;
////        float cos = Mathf.Cos(rad);
////        float sin = Mathf.Sin(rad);
////        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos).normalized;
////    }
////}
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//[CreateAssetMenu(menuName = "Game/Skills/Shoot Green Abyss Spear")]
//public class ShootGreenAbyssSpearSkill : WeaponSkill
//{
//    [Header("Tap / Hold")]
//    public float holdThreshold = 0.22f;
//    public float stepCooldown = 0.08f;

//    [Header("Prefab (ONLY 1 spear image)")]
//    public GameObject stabPrefab;

//    [Header("Tap Stab (CLICK)")]
//    [Tooltip("đẩy start ra trước tay một chút")]
//    public float tapMuzzleOffset = 0.6f;

//    [Tooltip("CLICK: mũi lao thêm bao xa (thrust)")]
//    public float tapThrustDistance = 2.2f;

//    [Tooltip("CLICK: thời gian lao (càng lớn càng chậm)")]
//    public float tapThrustDuration = 0.18f;

//    [Tooltip("CLICK: sau khi hit hoặc tới đích, tồn tại thêm bao lâu")]
//    public float hitLifeTap = 0.5f;

//    [Header("Hold Timing")]
//    [Tooltip("Thời gian LAO/ĐÂM (start->end) của MỖI mũi khi HOLD")]
//    public float eachSpearMoveDuration = 1.2f;

//    [Tooltip("Sau khi trúng (hoặc tới đích), mũi tồn tại thêm bao lâu rồi mới biến mất")]
//    public float eachSpearLinger = 0.55f;

//    [Header("Hold Throw Motion")]
//    public float throwDistance = 6.5f;
//    public float fanAngle = 30f; // +30 / 0 / -30

//    [Header("Aim / Sprite Fix")]
//    public float spriteAngleOffset = 0f;

//    [Header("Combat")]
//    public int tapDamage = 35;
//    public float tapAtkScale = 1.0f;

//    public int throwDamage = 60;
//    public float throwAtkScale = 0.95f;

//    public DamageType damageType = DamageType.Physical;
//    public float smptScale = 0f;
//    public bool canCrit = true;

//    [Header("Auto collider (nếu prefab chưa có)")]
//    public Vector2 boxSize = new Vector2(2.6f, 1.2f);
//    public bool useBoxCollider = true;

//    class State { public float nextTime; public bool busy; }
//    static readonly Dictionary<int, State> states = new();

//    protected override void OnUse(SkillContext ctx)
//    {
//        if (ctx.owner == null || ctx.weaponPivot == null || stabPrefab == null) return;

//        int id = ctx.owner.GetInstanceID();
//        if (!states.TryGetValue(id, out var st)) states[id] = st = new State();

//        if (Time.time < st.nextTime) return;
//        if (st.busy) return;

//        st.nextTime = Time.time + stepCooldown;

//        var runner = ctx.owner.GetComponent<SkillCoroutineRunner>();
//        if (runner == null) runner = ctx.owner.gameObject.AddComponent<SkillCoroutineRunner>();

//        runner.Run(TapOrHoldRoutine(ctx, st));
//    }

//    IEnumerator TapOrHoldRoutine(SkillContext ctx, State st)
//    {
//        st.busy = true;
//        try
//        {
//            Transform pivot = ctx.weaponPivot;

//            Vector2 baseDir = (ctx.mouseWorld - pivot.position);
//            if (baseDir.sqrMagnitude < 0.0001f) baseDir = Vector2.right;
//            baseDir.Normalize();

//            // phân biệt TAP vs HOLD
//            float holdT = 0f;
//            while (holdT < holdThreshold)
//            {
//                if (Input.GetMouseButtonUp(0)) break;
//                holdT += Time.deltaTime;
//                yield return null;
//            }

//            bool isHold = Input.GetMouseButton(0) && holdT >= holdThreshold;

//            if (!isHold)
//            {
//                // ✅ CLICK: mũi lao tới rồi đâm (thrust)
//                float dur = Mathf.Max(0.01f, tapThrustDuration);
//                float linger = Mathf.Max(0f, hitLifeTap);
//                float dist = Mathf.Max(0.01f, tapThrustDistance);

//                yield return TapThrust_GhimOnHit(ctx, pivot.position, baseDir, dist, dur, linger);
//                yield break;
//            }

//            // HOLD: 3 mũi
//            float moveDur = Mathf.Max(0.01f, eachSpearMoveDuration);
//            float lingerHold = Mathf.Max(0f, eachSpearLinger);

//            float[] angles = { +fanAngle, 0f, -fanAngle };
//            for (int i = 0; i < angles.Length; i++)
//            {
//                Vector2 dir = Rotate(baseDir, angles[i]);
//                yield return ThrowOnce_GhimOnHit(ctx, pivot.position, dir, moveDur, lingerHold);
//            }
//        }
//        finally
//        {
//            st.busy = false;
//        }
//    }

//    /// <summary>
//    /// CLICK: start (muzzleOffset) -> end (thrustDistance) trong tapThrustDuration
//    /// Hit thì GHIM + tồn tại lingerAfter, không biến mất liền.
//    /// </summary>
//    IEnumerator TapThrust_GhimOnHit(SkillContext ctx, Vector3 pivotPos, Vector2 dir, float thrustDistance, float moveDuration, float lingerAfter)
//    {
//        dir.Normalize();

//        Vector3 start = pivotPos + (Vector3)(dir * tapMuzzleOffset);
//        start.z = 0f;

//        Vector3 end = start + (Vector3)(dir * thrustDistance);
//        end.z = 0f;

//        var go = Instantiate(stabPrefab, start, RotationFromDir(dir));
//        ApplySortingLikeOwner(ctx, go);

//        EnsureTriggerCollider(go);
//        EnsureKinematicRB(go);

//        var stabDmg = go.GetComponent<SpearStabDamage>();
//        if (stabDmg == null) stabDmg = go.AddComponent<SpearStabDamage>();

//        var ownerGo = ctx.owner.gameObject;
//        var attackerStats = ownerGo.GetComponent<PlayerStatsMono>();

//        // ✅ hit thì tồn tại lingerAfter rồi mới mất
//        stabDmg.InitScaled(
//            attackerStats,
//            tapDamage,
//            tapAtkScale,
//            smptScale,
//            damageType,
//            canCrit,
//            ownerGo,
//            destroyDelay: lingerAfter,
//            hitOnce: true
//        );

//        float dur = Mathf.Max(0.01f, moveDuration);
//        float t = 0f;

//        while (t < 1f)
//        {
//            if (go == null) yield break;

//            // ✅ hit thì ghim (dừng thrust)
//            if (stabDmg != null && stabDmg.HasHit) break;

//            t += Time.deltaTime / dur;
//            go.transform.position = Vector3.Lerp(start, end, t);
//            yield return null;
//        }

//        // Miss: tới đích rồi đứng lingerAfter -> destroy
//        if (go != null && (stabDmg == null || !stabDmg.HasHit))
//        {
//            if (lingerAfter > 0f) yield return new WaitForSeconds(lingerAfter);
//            if (go != null) Destroy(go);
//        }
//        // Hit: SpearStabDamage sẽ tự destroy sau lingerAfter
//    }

//    /// <summary>
//    /// HOLD: Bay từ start -> end trong moveDuration.
//    /// Hit: GHIM + tồn tại lingerAfter rồi mới mất.
//    /// Miss: tới đích, đứng lingerAfter rồi mất.
//    /// </summary>
//    IEnumerator ThrowOnce_GhimOnHit(SkillContext ctx, Vector3 startPos, Vector2 dir, float moveDuration, float lingerAfter)
//    {
//        Vector3 start = startPos; start.z = 0f;
//        Vector3 end = start + (Vector3)(dir * throwDistance); end.z = 0f;

//        var go = Instantiate(stabPrefab, start, RotationFromDir(dir));
//        ApplySortingLikeOwner(ctx, go);

//        EnsureTriggerCollider(go);
//        EnsureKinematicRB(go);

//        var stabDmg = go.GetComponent<SpearStabDamage>();
//        if (stabDmg == null) stabDmg = go.AddComponent<SpearStabDamage>();

//        var ownerGo = ctx.owner.gameObject;
//        var attackerStats = ownerGo.GetComponent<PlayerStatsMono>();

//        stabDmg.InitScaled(
//            attackerStats,
//            throwDamage,
//            throwAtkScale,
//            smptScale,
//            damageType,
//            canCrit,
//            ownerGo,
//            destroyDelay: lingerAfter,
//            hitOnce: true
//        );

//        float dur = Mathf.Max(0.01f, moveDuration);
//        float t = 0f;

//        while (t < 1f)
//        {
//            if (go == null) yield break;

//            if (stabDmg != null && stabDmg.HasHit) break;

//            t += Time.deltaTime / dur;
//            go.transform.position = Vector3.Lerp(start, end, t);
//            yield return null;
//        }

//        if (go != null && (stabDmg == null || !stabDmg.HasHit))
//        {
//            if (lingerAfter > 0f) yield return new WaitForSeconds(lingerAfter);
//            if (go != null) Destroy(go);
//        }
//    }

//    Quaternion RotationFromDir(Vector2 dir)
//    {
//        float z = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + spriteAngleOffset;
//        return Quaternion.Euler(0f, 0f, z);
//    }

//    void ApplySortingLikeOwner(SkillContext ctx, GameObject go)
//    {
//        var sr = go.GetComponentInChildren<SpriteRenderer>();
//        if (sr == null) return;

//        var ownerSr = ctx.owner.GetComponentInChildren<SpriteRenderer>();
//        if (ownerSr != null)
//        {
//            sr.sortingLayerID = ownerSr.sortingLayerID;
//            sr.sortingOrder = ownerSr.sortingOrder + 10;
//        }
//        else
//        {
//            sr.sortingLayerName = "VFX";
//            sr.sortingOrder = 50;
//        }
//    }

//    void EnsureTriggerCollider(GameObject go)
//    {
//        Collider2D col = go.GetComponent<Collider2D>();
//        if (col == null)
//        {
//            if (useBoxCollider)
//            {
//                var bc = go.AddComponent<BoxCollider2D>();
//                bc.size = boxSize;
//                bc.isTrigger = true;
//            }
//            else
//            {
//                var cc = go.AddComponent<CircleCollider2D>();
//                cc.radius = Mathf.Max(0.1f, boxSize.x * 0.5f);
//                cc.isTrigger = true;
//            }
//        }
//        else col.isTrigger = true;
//    }

//    void EnsureKinematicRB(GameObject go)
//    {
//        var rb = go.GetComponent<Rigidbody2D>();
//        if (rb == null) rb = go.AddComponent<Rigidbody2D>();
//        rb.gravityScale = 0f;
//        rb.bodyType = RigidbodyType2D.Kinematic;
//        rb.linearVelocity = Vector2.zero;
//    }

//    static Vector2 Rotate(Vector2 v, float deg)
//    {
//        float rad = deg * Mathf.Deg2Rad;
//        float cos = Mathf.Cos(rad);
//        float sin = Mathf.Sin(rad);
//        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos).normalized;
//    }
//}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skills/Shoot Green Abyss Spear")]
public class ShootGreenAbyssSpearSkill : WeaponSkill
{
    [Header("Tap / Hold")]
    public float holdThreshold = 0.22f;
    public float stepCooldown = 0.08f;

    [Header("Prefab (ONLY 1 spear image)")]
    public GameObject stabPrefab;

    [Header("Tap Stab (CLICK) - Spear Thrust")]
    [Tooltip("đẩy start của mũi ra trước tay một chút")]
    public float tapMuzzleOffset = 0.6f;

    [Tooltip("CLICK: mũi lao thêm bao xa (thrust)")]
    public float tapThrustDistance = 2.2f;

    [Tooltip("CLICK: thời gian mũi lao (càng lớn càng chậm)")]
    public float tapThrustDuration = 0.18f;

    [Tooltip("CLICK: sau khi hit hoặc tới đích, tồn tại thêm bao lâu")]
    public float hitLifeTap = 0.5f;

    [Header("Tap (CLICK) - Player Dash Forward")]
    public bool dashPlayerOnTap = true;
    public float tapPlayerDashDistance = 0.6f;
    public float tapPlayerDashDuration = 0.08f;
    public LayerMask dashBlockMask; // set Ground/Wall nếu muốn chặn, để None thì vẫn dash xuyên

    [Header("Hold Timing")]
    public float eachSpearMoveDuration = 1.2f;
    public float eachSpearLinger = 0.55f;

    [Header("Hold Throw Motion")]
    public float throwDistance = 6.5f;
    public float fanAngle = 30f; // +30 / 0 / -30

    [Header("Aim / Sprite Fix")]
    public float spriteAngleOffset = 0f;

    [Header("Combat")]
    public int tapDamage = 35;
    public float tapAtkScale = 1.0f;

    public int throwDamage = 60;
    public float throwAtkScale = 0.95f;

    public DamageType damageType = DamageType.Physical;
    public float smptScale = 0f;
    public bool canCrit = true;

    [Header("Auto collider (nếu prefab chưa có)")]
    public Vector2 boxSize = new Vector2(2.6f, 1.2f);
    public bool useBoxCollider = true;

    class State { public float nextTime; public bool busy; }
    static readonly Dictionary<int, State> states = new();

    protected override void OnUse(SkillContext ctx)
    {
        if (ctx.owner == null || ctx.weaponPivot == null || stabPrefab == null) return;

        int id = ctx.owner.GetInstanceID();
        if (!states.TryGetValue(id, out var st)) states[id] = st = new State();

        if (Time.time < st.nextTime) return;
        if (st.busy) return;

        st.nextTime = Time.time + stepCooldown;

        var runner = ctx.owner.GetComponent<SkillCoroutineRunner>();
        if (runner == null) runner = ctx.owner.gameObject.AddComponent<SkillCoroutineRunner>();

        runner.Run(TapOrHoldRoutine(ctx, st));
    }

    IEnumerator TapOrHoldRoutine(SkillContext ctx, State st)
    {
        st.busy = true;
        try
        {
            Transform pivot = ctx.weaponPivot;

            Vector2 baseDir = (ctx.mouseWorld - pivot.position);
            if (baseDir.sqrMagnitude < 0.0001f) baseDir = Vector2.right;
            baseDir.Normalize();

            float holdT = 0f;
            while (holdT < holdThreshold)
            {
                if (Input.GetMouseButtonUp(0)) break;
                holdT += Time.deltaTime;
                yield return null;
            }

            bool isHold = Input.GetMouseButton(0) && holdT >= holdThreshold;

            if (!isHold)
            {
                // ✅ TAP: player dash về phía trước
                if (dashPlayerOnTap)
                    yield return DashPlayer(ctx, baseDir, tapPlayerDashDistance, tapPlayerDashDuration);

                // ✅ TAP: mũi lao tới rồi đâm (thrust)
                float dur = Mathf.Max(0.01f, tapThrustDuration);
                float linger = Mathf.Max(0f, hitLifeTap);
                float dist = Mathf.Max(0.01f, tapThrustDistance);

                // Lưu ý: pivot có thể đã dịch theo player sau dash, nên lấy lại pivot.position mới
                yield return TapThrust_GhimOnHit(ctx, ctx.weaponPivot.position, baseDir, dist, dur, linger);
                yield break;
            }

            // HOLD: 3 mũi (giữ nguyên)
            float moveDur = Mathf.Max(0.01f, eachSpearMoveDuration);
            float lingerHold = Mathf.Max(0f, eachSpearLinger);

            float[] angles = { +fanAngle, 0f, -fanAngle };
            for (int i = 0; i < angles.Length; i++)
            {
                Vector2 dir = Rotate(baseDir, angles[i]);
                yield return ThrowOnce_GhimOnHit(ctx, pivot.position, dir, moveDur, lingerHold);
            }
        }
        finally
        {
            st.busy = false;
        }
    }

    IEnumerator DashPlayer(SkillContext ctx, Vector2 dir, float distance, float duration)
    {
        if (ctx.owner == null) yield break;
        if (distance <= 0.0001f || duration <= 0.0001f) yield break;

        dir = dir.normalized;

        Transform tr = ctx.owner.transform;
        Vector3 start = tr.position;
        Vector3 target = start + (Vector3)(dir * distance);
        target.z = start.z;

        // nếu set mask thì chặn tường; nếu mask = 0 thì bỏ qua
        if (dashBlockMask.value != 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(start, dir, distance, dashBlockMask);
            if (hit.collider != null)
            {
                target = hit.point - (Vector2)dir * 0.05f;
                target.z = start.z;
            }
        }

        Rigidbody2D rb = ctx.owner.GetComponent<Rigidbody2D>();

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            Vector3 p = Vector3.Lerp(start, target, t);

            if (rb != null) rb.MovePosition(p);
            else tr.position = p;

            yield return null;
        }
    }

    IEnumerator TapThrust_GhimOnHit(SkillContext ctx, Vector3 pivotPos, Vector2 dir, float thrustDistance, float moveDuration, float lingerAfter)
    {
        dir.Normalize();

        Vector3 start = pivotPos + (Vector3)(dir * tapMuzzleOffset);
        start.z = 0f;

        Vector3 end = start + (Vector3)(dir * thrustDistance);
        end.z = 0f;

        var go = Instantiate(stabPrefab, start, RotationFromDir(dir));
        ApplySortingLikeOwner(ctx, go);

        EnsureTriggerCollider(go);
        EnsureKinematicRB(go);

        var stabDmg = go.GetComponent<SpearStabDamage>();
        if (stabDmg == null) stabDmg = go.AddComponent<SpearStabDamage>();

        var ownerGo = ctx.owner.gameObject;
        var attackerStats = ownerGo.GetComponent<PlayerStatsMono>();

        stabDmg.InitScaled(
            attackerStats,
            tapDamage,
            tapAtkScale,
            smptScale,
            damageType,
            canCrit,
            ownerGo,
            destroyDelay: lingerAfter,
            hitOnce: true
        );

        float dur = Mathf.Max(0.01f, moveDuration);
        float t = 0f;

        while (t < 1f)
        {
            if (go == null) yield break;
            if (stabDmg != null && stabDmg.HasHit) break;

            t += Time.deltaTime / dur;
            go.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        if (go != null && (stabDmg == null || !stabDmg.HasHit))
        {
            if (lingerAfter > 0f) yield return new WaitForSeconds(lingerAfter);
            if (go != null) Destroy(go);
        }
    }

    IEnumerator ThrowOnce_GhimOnHit(SkillContext ctx, Vector3 startPos, Vector2 dir, float moveDuration, float lingerAfter)
    {
        Vector3 start = startPos; start.z = 0f;
        Vector3 end = start + (Vector3)(dir * throwDistance); end.z = 0f;

        var go = Instantiate(stabPrefab, start, RotationFromDir(dir));
        ApplySortingLikeOwner(ctx, go);

        EnsureTriggerCollider(go);
        EnsureKinematicRB(go);

        var stabDmg = go.GetComponent<SpearStabDamage>();
        if (stabDmg == null) stabDmg = go.AddComponent<SpearStabDamage>();

        var ownerGo = ctx.owner.gameObject;
        var attackerStats = ownerGo.GetComponent<PlayerStatsMono>();

        stabDmg.InitScaled(
            attackerStats,
            throwDamage,
            throwAtkScale,
            smptScale,
            damageType,
            canCrit,
            ownerGo,
            destroyDelay: lingerAfter,
            hitOnce: true
        );

        float dur = Mathf.Max(0.01f, moveDuration);
        float t = 0f;

        while (t < 1f)
        {
            if (go == null) yield break;
            if (stabDmg != null && stabDmg.HasHit) break;

            t += Time.deltaTime / dur;
            go.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        if (go != null && (stabDmg == null || !stabDmg.HasHit))
        {
            if (lingerAfter > 0f) yield return new WaitForSeconds(lingerAfter);
            if (go != null) Destroy(go);
        }
    }

    Quaternion RotationFromDir(Vector2 dir)
    {
        float z = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + spriteAngleOffset;
        return Quaternion.Euler(0f, 0f, z);
    }

    void ApplySortingLikeOwner(SkillContext ctx, GameObject go)
    {
        var sr = go.GetComponentInChildren<SpriteRenderer>();
        if (sr == null) return;

        var ownerSr = ctx.owner.GetComponentInChildren<SpriteRenderer>();
        if (ownerSr != null)
        {
            sr.sortingLayerID = ownerSr.sortingLayerID;
            sr.sortingOrder = ownerSr.sortingOrder + 10;
        }
        else
        {
            sr.sortingLayerName = "VFX";
            sr.sortingOrder = 50;
        }
    }

    void EnsureTriggerCollider(GameObject go)
    {
        Collider2D col = go.GetComponent<Collider2D>();
        if (col == null)
        {
            if (useBoxCollider)
            {
                var bc = go.AddComponent<BoxCollider2D>();
                bc.size = boxSize;
                bc.isTrigger = true;
            }
            else
            {
                var cc = go.AddComponent<CircleCollider2D>();
                cc.radius = Mathf.Max(0.1f, boxSize.x * 0.5f);
                cc.isTrigger = true;
            }
        }
        else col.isTrigger = true;
    }

    void EnsureKinematicRB(GameObject go)
    {
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb == null) rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
    }

    static Vector2 Rotate(Vector2 v, float deg)
    {
        float rad = deg * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos).normalized;
    }
}
