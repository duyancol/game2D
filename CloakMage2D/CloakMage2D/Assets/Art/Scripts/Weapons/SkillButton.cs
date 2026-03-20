using UnityEngine;
using UnityEngine.EventSystems;

public class SkillButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public SkillAimJoystick aimJoystick;
    public PlayerController player;
    public CanvasGroup joystickCanvasGroup;
    public GameObject skillJoystick;

    
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("SKILL BUTTON DOWN");

        skillJoystick.SetActive(true);

        ExecuteEvents.Execute(
            skillJoystick,
            eventData,
            ExecuteEvents.pointerDownHandler
        );
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        skillJoystick.SetActive(false);
    }
    void Start()
    {
        joystickCanvasGroup.alpha = 1;
    }
}