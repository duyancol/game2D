using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;

    public Joystick moveJoystick;              // joystick di chuyển
    public SkillAimJoystick skillJoystick;     // joystick skill

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Move();
        RotateBySkillJoystick();
    }

    void Move()
    {
        float horizontal = moveJoystick.Horizontal;
        float vertical = moveJoystick.Vertical;

        Vector2 move = new Vector2(horizontal, vertical);
        rb.linearVelocity = move * speed;
    }

    void RotateBySkillJoystick()
    {
        Vector2 dir = skillJoystick.Direction;

        if (dir != Vector2.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        Debug.Log(skillJoystick.Direction);
    }

    public void CastSkill()
    {
        Vector2 dir = skillJoystick.Direction;

        if (dir == Vector2.zero) return;

        Debug.Log("Bắn skill theo hướng: " + dir);
    }
}