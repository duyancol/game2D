using UnityEngine;

public class BossStunHit : MonoBehaviour
{
    public bool IsStunned => _timer > 0f;

    float _timer;

    void Update()
    {
        if (_timer > 0f) _timer -= Time.deltaTime;
    }

    public void Stun(float time)
    {
        if (time <= 0f) return;
        _timer = Mathf.Max(_timer, time); // bị hit liên tục thì gia hạn stun
    }
}
