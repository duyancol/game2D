//using UnityEngine;
//using UnityEngine.EventSystems;

//public class SkillAimButton : MonoBehaviour,
//    IPointerDownHandler,
//    IDragHandler,
//    IPointerUpHandler
//{
//    public GameObject aimCircle;
//    public Transform arrow;
//    public Transform player;
//    public LineRenderer line;

//    private Vector2 currentDirection;
//    public float maxDistance = 3f;
//    public WeaponController weaponController;
//    public void OnPointerDown(PointerEventData eventData)
//    {
//        aimCircle.SetActive(true);
//        line.enabled = true;

//        aimCircle.transform.position = player.position;
//    }

//    public void OnDrag(PointerEventData eventData)
//    {
//        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(eventData.position);
//        mouseWorld.z = 0;

//        Vector2 direction = mouseWorld - player.position;

//        if (direction.sqrMagnitude < 0.001f)
//            return;

//        currentDirection = direction.normalized;

//        // 🔥 QUAN TRỌNG: truyền qua WeaponController
//        weaponController.SetMobileAimDirection(currentDirection);

//        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
//        arrow.rotation = Quaternion.Euler(0, 0, angle);

//        line.SetPosition(0, player.position);
//        line.SetPosition(1, player.position + (Vector3)(currentDirection * maxDistance));
//    }

//    public void OnPointerUp(PointerEventData eventData)
//    {
//        Shoot(currentDirection);

//        weaponController.ClearMobileAim(); // reset direction

//        aimCircle.SetActive(false);
//        line.enabled = false;
//    }

//   public void Shoot(Vector2 dir)
//    {
//        Debug.Log("Bắn theo hướng: " + dir);
//    }
//}
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillAimButton : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    public GameObject aimCircle;   // dùng cho Area
    public Transform arrow;        // dùng cho Directional
    public Transform player;
    public LineRenderer line;
    public GameObject bgaimCircle;
    public float maxDistance = 12f;

    private Vector2 currentDirection;
    private Vector3 currentAreaPos;

    public WeaponController weaponController;

    WeaponSkill CurrentSkill =>
        weaponController.CurrentWeapon.ultimateSkill;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (CurrentSkill == null) return;

        if (CurrentSkill.castType == SkillCastType.Directional)
        {
            line.enabled = true;
            arrow.gameObject.SetActive(true);
            bgaimCircle.gameObject.SetActive(true);
        }
        else if (CurrentSkill.castType == SkillCastType.AreaTarget)
        {
            bgaimCircle.gameObject.SetActive(true);
            aimCircle.SetActive(true);

        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (CurrentSkill == null) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(eventData.position);
        mouseWorld.z = 0;

        // ==========================
        // 🎯 DIRECTIONAL SKILL
        // ==========================
        if (CurrentSkill.castType == SkillCastType.Directional)
        {
            Vector2 direction = mouseWorld - player.position;

            if (direction.sqrMagnitude < 0.001f)
                return;

            currentDirection = direction.normalized;

            weaponController.SetMobileAimDirection(currentDirection);

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrow.rotation = Quaternion.Euler(0, 0, angle);

            line.SetPosition(0, player.position);
            line.SetPosition(1, player.position + (Vector3)(currentDirection * maxDistance));
        }

        // ==========================
        // 🔵 AREA TARGET SKILL
        // ==========================
        else if (CurrentSkill.castType == SkillCastType.AreaTarget)
        {
            Vector3 offset = mouseWorld - player.position;

            if (offset.magnitude > maxDistance)
                offset = offset.normalized * maxDistance;

            currentAreaPos = player.position + offset;

            aimCircle.transform.position = currentAreaPos;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (CurrentSkill == null) return;

        SkillContext ctx = weaponController.BuildContext();

        // 🎯 Directional cast
        if (CurrentSkill.castType == SkillCastType.Directional)
        {
            ctx.aimDirection = currentDirection;
            CurrentSkill.TryUse(ctx);

            weaponController.ClearMobileAim();
        }

        // 🔵 Area cast
        else if (CurrentSkill.castType == SkillCastType.AreaTarget)
        {
            ctx.mouseWorld = currentAreaPos;
            CurrentSkill.TryUse(ctx);
        }

        aimCircle.SetActive(false);
        line.enabled = false;
        arrow.gameObject.SetActive(false);
        bgaimCircle.gameObject.SetActive(false);
    }
}