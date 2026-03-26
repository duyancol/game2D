using UnityEngine;

public class CharacterVisualRefs : MonoBehaviour
{
    [Header("Body Parts")]
    public Transform body;
    public Transform head;
    public Transform footL;
    public Transform footR;

    [Header("Arm")]
    public Transform armRightRoot;
    public Transform armWeaponRoot;
    public SpriteRenderer weaponRenderer;
    [Header("Weapon")]
    public Transform weaponPivot;
    public ArmWeaponController armController;
}