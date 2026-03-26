using UnityEngine;

public class PlayerCharacterController : MonoBehaviour
{
    [Header("Visual Root")]
    public Transform visualRoot; // kéo object Visual vào đây

    private GameObject currentVisual;

    [Header("Current Character")]
    public CharacterProfile currentCharacter;

    // 🔥 HÀM M ĐANG THIẾU NẰM Ở ĐÂY
    public void ApplyCharacter(CharacterProfile data)
    {
        if (data == null)
        {
            Debug.LogWarning("CharacterProfile is NULL");
            return;
        }

        currentCharacter = data;

        // ❌ Xoá nhân vật cũ
        if (currentVisual != null)
        {
            Destroy(currentVisual);
        }

        // ✅ Spawn nhân vật mới
        currentVisual = Instantiate(data.visualPrefab, visualRoot);

        // Reset transform cho đúng vị trí
        //currentVisual = Instantiate(data.visualPrefab, visualRoot);

        currentVisual.transform.localPosition = Vector3.zero;
        currentVisual.transform.localRotation = Quaternion.identity;
        currentVisual.transform.localScale = Vector3.one;

        // 🔥 FIX MẤT TAY CHÂN Ở ĐÂY
        var refs = currentVisual.GetComponent<CharacterVisualRefs>();

        // ===== MOVE =====
        var move = GetComponent<PlayerMove2D>();
        move.body = refs.body;
        move.head = refs.head;
        move.footL = refs.footL;
        move.footR = refs.footR;
        move.armRightRoot = refs.armRightRoot;
        move.armWeaponRoot = refs.armWeaponRoot;

        // ===== WEAPON =====
        var weapon = GetComponent<WeaponController>();

        weapon.weaponPivot = refs.weaponPivot;
        weapon.armController = refs.armController;
        // 🔥 RE-EQUIP để gắn lại weapon vào tay mới
        weapon.weaponRenderer = refs.weaponRenderer; // 🔥 QUAN TRỌNG NHẤT
        weapon.EquipByProfile(weapon.CurrentWeapon);
        Debug.Log("Switched to: " + data.characterName);
    }
}