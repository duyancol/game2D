
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDeathKnockback : MonoBehaviour
{
    [Header("Optional Facing Root (để lấy hướng nhìn)")]
    public Transform flipRoot;         // kéo Visual/body của player (cái bạn flip X) vào đây
    public int facingSign = 1;         // nếu flipRoot bị ngược thì đổi 1 <-> -1

    [Header("Game Over")]
    public GameOverUI gameOverUI;      // kéo GameOverUI object vào đây

    [Header("Disable When Dead (Player)")]
    public MonoBehaviour[] disableOnDeath;
    public Collider2D[] disableColliders;

    [Header("Hide UI On Dead")]
    public GameObject playerHealthUIRoot;   // kéo Canvas/PlayerHealthUI vào đây (hoặc cả Canvas)

    [Header("Stop Enemies On Dead")]
    public bool disableEnemyScripts = true; // tắt script AI/skill trên enemy
    public bool stopEnemyRigidbodies = true;// dừng rb của enemy

    [Header("Knockback")]
    public float knockbackX = 7.5f;
    public float knockbackY = 9.0f;
    public float deathFreezeTime = 0.9f;

    [Header("Visual")]
    public GameObject deathVfxPrefab;
    public float deathVfxLife = 1.2f;
    public bool hideSpriteAfter = true;

    Rigidbody2D rb;
    SpriteRenderer[] srs;
    bool dead;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        srs = GetComponentsInChildren<SpriteRenderer>();
        if (!flipRoot) flipRoot = transform; // fallback
    }

    public void PlayDeath()
    {
        if (dead) return;
        dead = true;

        // 0) Ẩn UI thanh máu
        if (playerHealthUIRoot) playerHealthUIRoot.SetActive(false);

        // 1) tắt điều khiển player
        if (disableOnDeath != null)
            foreach (var mb in disableOnDeath)
                if (mb) mb.enabled = false;

        // 2) dừng quái/boss để nó không đánh nữa + xoá đạn đang bay
        StopAllEnemies();

        // 3) đảm bảo có vật lý để bay + rơi
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.freezeRotation = true;

        // ===== TỰ QUYẾT HƯỚNG VĂNG =====
        int dir = GetFacingDir();

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(knockbackX * dir, knockbackY), ForceMode2D.Impulse);

        // VFX
        if (deathVfxPrefab)
        {
            var vfx = Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, deathVfxLife);
        }

        // 4) HIỆN GAME OVER UI
        if (gameOverUI) gameOverUI.Show();

        StartCoroutine(DeathRoutine());
    }

    void StopAllEnemies()
    {
        // ===== Unity 6: thay FindObjectsOfType bằng FindObjectsByType =====
        var all = Object.FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (var mb in all)
        {
            if (mb == null) continue;

            // tránh tắt script của chính Player
            if (mb.transform != null && mb.transform.IsChildOf(transform)) continue;

            // Chỉ tắt các script boss/skill của bạn
            bool isBossScript =
                (mb is BossShooter) ||
                (mb is BossChaseStab) ||
                (mb is BossMoveAndIdle2D) ||
                (mb is BossIdleMotion);

            if (!isBossScript) continue;

            if (disableEnemyScripts) mb.enabled = false;

            if (stopEnemyRigidbodies)
            {
                var rb2 = mb.GetComponent<Rigidbody2D>();
                if (rb2)
                {
                    rb2.linearVelocity = Vector2.zero;
                    rb2.simulated = false; // khoá physics để không đẩy/không trôi
                }
            }
        }

        // Xoá toàn bộ projectile/bullet đang bay (để không còn bị trừ máu sau khi chết)
        // LƯU Ý: tag "Projectile" phải tồn tại và các đạn phải gắn đúng tag này.
        GameObject[] bullets = null;
        try
        {
            bullets = GameObject.FindGameObjectsWithTag("Projectile");
        }
        catch
        {
            // nếu chưa tạo tag Projectile thì bỏ qua
        }

        if (bullets != null)
        {
            for (int i = 0; i < bullets.Length; i++)
                if (bullets[i]) Destroy(bullets[i]);
        }
    }

    int GetFacingDir()
    {
        // 1) ưu tiên hướng đang chạy (velocity)
        float vx = rb.linearVelocity.x;
        if (Mathf.Abs(vx) > 0.05f) return vx >= 0 ? 1 : -1;

        // 2) lấy theo flipRoot scale.x (hướng nhìn)
        if (flipRoot != null)
        {
            float sx = flipRoot.localScale.x * facingSign;
            if (Mathf.Abs(sx) > 0.0001f) return sx >= 0 ? 1 : -1;
        }

        return 1;
    }

    IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(deathFreezeTime);

        if (disableColliders != null)
            foreach (var c in disableColliders)
                if (c) c.enabled = false;

        if (hideSpriteAfter)
            foreach (var sr in srs)
                if (sr) sr.enabled = false;
    }
}
