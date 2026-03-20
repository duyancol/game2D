using UnityEngine;

public class ArmAimFlip : MonoBehaviour
{
    public Transform armPivot; // KÉO ArmShoulderPivot vào đây
    public Camera cam;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (armPivot == null) armPivot = transform;
    }

    void Update()
    {
        Vector3 mouse = cam.ScreenToWorldPoint(Input.mousePosition);
        mouse.z = 0;

        Vector2 dir = (mouse - armPivot.position);
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        armPivot.rotation = Quaternion.Euler(0, 0, ang);

        // (Optional) flip cả cụm arms theo hướng trái/phải
        // nếu m đang flip bằng scale thì làm ở object "arms" hoặc "ArmShoulderPivot"
    }
}
