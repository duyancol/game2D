
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skills/Ultimate Fire Bird Skill")]
public class UltimateFireBirdSkill : WeaponSkill
{
    [Header("Bird Prefab (has FireBirdProjectile)")]
    public FireBirdProjectile birdPrefab;

    [Header("Charge VFX (trên nhân vật)")]
    public GameObject auraPrefab;
    public Vector3 auraLocalOffset = new Vector3(0f, 0.15f, 0f);
    public float auraLife = 1.0f;
    public float arcHeight1 = 1.5f;
    public float arcHeight2 = 4.0f;
    [Header("Ultimate Splash (POW)")]
    public GameObject ultimatePrefab;
    public Vector3 ultimateLocalOffset = new Vector3(0f, 1.0f, 0f);
    public float ultimateLife = 0.6f;

    [Header("Charge Timing")]
    public float chargeDelay = 0.6f;

    [Header("Bird Config")]
    public bool rotateToVelocity = true;
    public float spriteAngleOffset = 0f;

    [Header("2-Step Flight (tay -> ra trước -> bay lên trời)")]
    public Vector3 handLocalOffset = new Vector3(0.35f, 0.15f, 0f); // lệch ra phía tay (tùy nhân vật)
    public float outDistance = 2.8f;     // bay ra trước bao xa
    public float outTime = 0.18f;        // thời gian bay ra
    public float riseHeight = 7.0f;      // bay ngược lên trời cao bao nhiêu
    public float riseTime = 0.35f;       // thời gian bay lên
    public float riseForward = 0.8f;     // vừa bay lên vừa “lệch” nhẹ theo hướng bắn (cho đẹp)

    [Header("Impact VFX")]
    public GameObject impactVfx;
    public float impactLife = 0.8f;

    [Header("Damage (Base + Scaling)")]
    public int damage = 40;
    public DamageType damageType = DamageType.Physical;
    public float atkScale = 1.2f;
    public float smptScale = 0f;
    public bool canCrit = true;

    [Header("AOE")]
    public float radius = 2.2f;
    public LayerMask hitMask;

    protected override void OnUse(SkillContext ctx)
    {
        if (!birdPrefab || ctx.owner == null) return;

        var runner = ctx.owner.GetComponent<SkillCoroutineRunner>();
        if (runner == null) runner = ctx.owner.gameObject.AddComponent<SkillCoroutineRunner>();

        runner.Run(UltRoutine(ctx));
    }

    IEnumerator UltRoutine(SkillContext ctx)
    {
        // 1) tụ khí
        var playerMove = ctx.owner.GetComponent<PlayerMove2D>();
        if (playerMove != null)
        {
            playerMove.SetSkillVisual();
        }

        var ownerGo = ctx.owner.gameObject;
        var anim = ctx.owner.transform.Find("Visual_Skill")?.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetBool("Ultimate", true);
        }
        SpawnOnOwner(ctx, auraPrefab, auraLocalOffset, auraLife);
        SpawnOnOwner(ctx, ultimatePrefab, ultimateLocalOffset, ultimateLife);

        yield return new WaitForSeconds(Mathf.Max(0f, chargeDelay));
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 aimDir = (mouseWorld - ctx.owner.position).normalized;

        // 2) lấy vị trí tay (nếu có weaponPivot thì ngon nhất)
        Vector3 handPos = ctx.owner.position +
     new Vector3(
         handLocalOffset.x * Mathf.Sign(aimDir.x),
         handLocalOffset.y,
         0f
     );
        handPos.z = 0f;

        // hướng bắn theo chuột
        // 🔥 dùng cùng hướng với skill ChargeArrow

        //if (aimDir.sqrMagnitude < 0.0001f)
        //    aimDir = Vector2.right;

        aimDir.Normalize();

        // 🔥 Lật đúng object có Animator
        Transform visual = ctx.owner.transform.Find("Visual_Skill");
        float sign = aimDir.x >= 0 ? 1f : -1f;

        if (visual != null)
        {
            visual.localScale = new Vector3(
                Mathf.Abs(visual.localScale.x) * sign,
                visual.localScale.y,
                visual.localScale.z
            );
        }

        if (aimDir.sqrMagnitude < 0.0001f)
            aimDir = Vector2.right;

        aimDir.Normalize();

        // 3) điểm “bay ra trước”
        Vector3 outPoint = handPos + (Vector3)(aimDir * outDistance);
        outPoint.z = 0f;

        // 4) điểm “bay ngược lên trời”
        Vector3 skyPoint = outPoint
                           + Vector3.up * riseHeight
                           + (Vector3)(aimDir * riseForward);
        Debug.Log("HandPos: " + handPos);
        Debug.Log("SkyPoint: " + skyPoint);
        skyPoint.z = 0f;

        // 5) spawn chim ngay tay
        float startAngle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        var bird = Instantiate(birdPrefab, handPos, Quaternion.Euler(0, 0, startAngle));

        // === config bird ===
        bird.rotateToVelocity = rotateToVelocity;
        bird.spriteAngleOffset = spriteAngleOffset;

        bird.impactVfx = impactVfx;
        bird.impactLife = impactLife;

        bird.radius = radius;

        // === owner + scaled damage ===
        bird.owner = ownerGo;

        var stats = ownerGo.GetComponent<PlayerStatsMono>();
        bird.InitScaled(
            stats,
            damage,
            atkScale,
            smptScale,
            damageType,
            canCrit,
            hitMask,
            ownerGo
        );

        // 6) launch 2-step: tay -> outPoint (impact/damage) -> skyPoint (bay lên rồi biến mất)
        // ví dụ:
       bird.LaunchLateRise(skyPoint, riseTime, riseHeight, risePower: 8f, destroyAtEnd: true);


        float totalSkillTime = chargeDelay + riseTime + 0.2f;
        yield return new WaitForSeconds(totalSkillTime);

        if (playerMove != null)
        {
            playerMove.SetNormalVisual();
        }



    }

    void SpawnOnOwner(SkillContext ctx, GameObject prefab, Vector3 localOffset, float life)
    {
        if (!prefab || ctx.owner == null) return;

        Vector3 pos = ctx.owner.position + localOffset;
        pos.z = 0f;

        var go = Instantiate(prefab, pos, Quaternion.identity);
        go.transform.SetParent(ctx.owner, worldPositionStays: true);

        if (life > 0f) Destroy(go, life);
    }
}
