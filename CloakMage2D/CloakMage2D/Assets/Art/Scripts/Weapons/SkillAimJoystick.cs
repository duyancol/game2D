using UnityEngine;
using UnityEngine.EventSystems;

public class SkillAimJoystick : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform background;
    public RectTransform handle;
    public float maxRadius = 120f;

    private Vector2 inputDir;

    public Vector2 Direction => inputDir;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData); // xử lý luôn khi chạm
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        Debug.Log("Dragging...");
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        localPoint = Vector2.ClampMagnitude(localPoint, maxRadius);

        handle.anchoredPosition = localPoint;

        // 🔥 CÁI M QUÊN LÀ DÒNG NÀY
        inputDir = localPoint / maxRadius;

        Debug.Log(inputDir);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputDir = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }
}