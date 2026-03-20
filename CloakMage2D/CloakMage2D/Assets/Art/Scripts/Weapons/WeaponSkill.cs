
using UnityEngine;
public enum SkillCastType
{
    Directional,   // kéo định hướng
    AreaTarget     // chọn vùng
}
public abstract class WeaponSkill : ScriptableObject
{
    [Header("Info")]
    public string skillName;
    [Header("Cast Type")]
    public SkillCastType castType;
    [Header("UI")]
    public Sprite skillIcon;                 // ✅ kéo icon skill vào đây

    [Header("Cooldown")]
    [Tooltip("Cooldown giây")]
    public float cooldown = 0.2f;

    private float nextTime;
    public virtual void OnEquip(SkillContext ctx) { }
    public virtual void OnUnequip(SkillContext ctx) { }
    public bool CanUse() => Time.time >= nextTime;
    public virtual void ActivateDirectional(SkillContext ctx, Vector2 dir) { }

    public virtual void ActivateAtPosition(SkillContext ctx, Vector3 pos) { }
    public void ResetCooldown()
    {
        nextTime = 0f;
    }

    // ✅ cho UI hỏi còn bao nhiêu giây
    public float GetRemain()
    {
        return Mathf.Max(0f, nextTime - Time.time);
    }

    // ✅ cho UI lấy fill 0..1 (1 = đang cooldown đầy, 0 = ready)
    public float GetCooldownFill01()
    {
        if (cooldown <= 0f) return 0f;
        return Mathf.Clamp01(GetRemain() / cooldown);
    }

    public void TryUse(SkillContext ctx)
    {
       // Debug.Log($"TryUse {name} | time={Time.time:F2} nextTime={nextTime:F2} canUse={CanUse()}");

        if (!CanUse()) return;

        nextTime = Time.time + cooldown;   // ✅ set cooldown ở đây
        OnUse(ctx);                        // cast skill
    }

    protected abstract void OnUse(SkillContext ctx);
}

public struct SkillContext
{
    public Transform owner;          // Player
    public Transform weaponPivot;    // WeaponPivot
    public Transform arm;            // ArmFront
    public Vector3 mouseWorld;
    public Vector2 aimDirection;
    public StatBonus weaponStat;
}
