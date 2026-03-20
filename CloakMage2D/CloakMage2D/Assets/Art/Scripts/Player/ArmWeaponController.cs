
using UnityEngine;

public class ArmWeaponController : MonoBehaviour
{
    [Header("Stats")]
    public PlayerStatsMono stats;

    [Header("Spawn")]
    public Transform armHolder;
    public GameObject defaultArmObj;

    [Header("Animator")]
    public string attackTrigger = "Attack";

    [Header("Flip (NO MOUSE)")]
    [Tooltip("Object dùng để lấy hướng mặt (thường là Player/Visual). ScaleX > 0 = mặt phải, < 0 = mặt trái.")]
    public Transform faceSource;
    public bool flipByScaleX = true;

    [Header("Pivot")]
    public string pivotName = "ArmPivot";

    [Header("Idle Pose (NO MOUSE AIM)")]
    [Tooltip("Góc Z mặc định của pivot khi không aim. Nếu tay bị lệch, thử 0 / 90 / -90.")]
    public float idlePivotZ = 0f;

    GameObject currentArm;
    Animator anim;
    Transform pivot;
    Vector3 baseArmScale = Vector3.one;

    [Header("UI")]
    public WeaponPanelUI weaponPanelUI;

    void Awake()
    {
        ShowDefaultArm();

        if (stats == null)
            stats = GetComponentInParent<PlayerStatsMono>();

        // nếu không set thì lấy parent (thường controller nằm dưới Visual/arms)
        if (faceSource == null)
            faceSource = transform.root; // m có thể kéo Player/Visual vào cho chắc
    }

    void Update()
    {
        if (currentArm == null || pivot == null) return;

        // ===== FLIP theo hướng mặt (KHÔNG theo chuột) =====
        float face = GetFaceDir(); // +1 phải, -1 trái
        bool left = face < 0f;

        var s = baseArmScale;
        if (flipByScaleX)
            s.x = left ? -Mathf.Abs(s.x) : Mathf.Abs(s.x);
        else
            s.y = left ? -Mathf.Abs(s.y) : Mathf.Abs(s.y);

        currentArm.transform.localScale = s;

        // ===== GIỮ GÓC IDLE (KHÔNG AIM) =====
        pivot.localRotation = Quaternion.Euler(0f, 0f, idlePivotZ);
    }

    float GetFaceDir()
    {
        if (faceSource == null) return 1f;

        // ưu tiên scaleX local
        float sx = faceSource.localScale.x;
        if (Mathf.Abs(sx) > 0.0001f) return Mathf.Sign(sx);

        // fallback
        return 1f;
    }

    // ===== ĐỔI VŨ KHÍ =====
    //public void ApplyWeapon(WeaponProfile weapon)
    //{
    //    Debug.Log("[ArmWeaponController] ApplyWeapon = " + (weapon ? weapon.weaponName : "NULL"));
    //    Debug.Log("[ArmWeaponController] weaponPanelUI = " + weaponPanelUI);

    //    if (weaponPanelUI != null) weaponPanelUI.SetWeapon(weapon);

    // //   if (stats != null) stats.EquipWeapon(weapon);

    //    if (weapon == null || weapon.armPrefab == null)
    //    {
    //        ShowDefaultArm();
    //        return;
    //    }

    //    if (defaultArmObj != null)
    //        defaultArmObj.SetActive(false);

    //    if (currentArm != null)
    //        Destroy(currentArm);

    //    Transform parent = armHolder != null ? armHolder : transform;
    //    currentArm = Instantiate(weapon.armPrefab, parent);
    //    currentArm.transform.localPosition = Vector3.zero;
    //    currentArm.transform.localRotation = Quaternion.identity;

    //    anim = currentArm.GetComponentInChildren<Animator>();
    //    if (anim == null)
    //        Debug.LogWarning("Arm prefab KHÔNG có Animator: " + weapon.armPrefab.name);

    //    pivot = currentArm.transform.Find(pivotName);
    //    if (pivot == null) pivot = currentArm.transform;

    //    baseArmScale = currentArm.transform.localScale;
    //}
    public void ApplyWeapon(WeaponProfile weapon)
    {
        Debug.Log("[ArmWeaponController] ApplyWeapon = " + (weapon ? weapon.weaponName : "NULL"));

        if (weaponPanelUI != null)
            weaponPanelUI.SetWeapon(weapon);

        // ❌ nếu chưa có weapon thì clear
        if (weapon == null)
        {
            if (stats != null)
                stats.EquipWeapon(null);

            ShowDefaultArm();
            return;
        }

        // ✅ LẤY LEVEL TỪ WEAPON (đã load từ BE)
        int lv = weapon.enhanceLevel;

        // WeaponInstance instance = new WeaponInstance(weapon, lv);
        WeaponInstance instance = new WeaponInstance(
     weapon,
     weapon.level,
     weapon.enhanceLevel
 );
        // 🔥 GÁN VÀO PLAYER
        if (stats != null)
            stats.EquipWeapon(instance);

        // ===== SPAWN ARM =====
        if (weapon.armPrefab == null)
        {
            ShowDefaultArm();
            return;
        }

        if (defaultArmObj != null)
            defaultArmObj.SetActive(false);

        if (currentArm != null)
            Destroy(currentArm);

        Transform parent = armHolder != null ? armHolder : transform;
        currentArm = Instantiate(weapon.armPrefab, parent);
        currentArm.transform.localPosition = Vector3.zero;
        currentArm.transform.localRotation = Quaternion.identity;

        anim = currentArm.GetComponentInChildren<Animator>();
        if (anim == null)
            Debug.LogWarning("Arm prefab KHÔNG có Animator: " + weapon.armPrefab.name);

        pivot = currentArm.transform.Find(pivotName);
        if (pivot == null) pivot = currentArm.transform;

        baseArmScale = currentArm.transform.localScale;
    }
    public void UnequipWeapon()
    {
        if (weaponPanelUI != null) weaponPanelUI.SetWeapon(null);

        if (currentArm != null)
            Destroy(currentArm);

        currentArm = null;
        anim = null;
        pivot = null;

        if (stats != null) stats.EquipWeapon(null);

        ShowDefaultArm();
    }

    void ShowDefaultArm()
    {
        if (defaultArmObj != null)
            defaultArmObj.SetActive(true);
    }

    public void PlayAttack()
    {
        if (anim == null) return;
        anim.ResetTrigger(attackTrigger);
        anim.SetTrigger(attackTrigger);
    }
}
