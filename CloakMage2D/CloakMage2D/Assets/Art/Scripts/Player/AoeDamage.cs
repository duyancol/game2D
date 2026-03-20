using UnityEngine;

public class UltimateDamage : MonoBehaviour
{
    public int damage;
    public float radius;
    public LayerMask hitMask;
    public GameObject owner;

    public void Init(int dmg, float r, LayerMask mask, GameObject ownerGo)
    {
        damage = dmg;
        radius = r;
        hitMask = mask;
        owner = ownerGo;
    }

    public void ApplyDamage()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, radius, hitMask);

        foreach (var h in hits)
        {
            if (owner != null && h.transform.IsChildOf(owner.transform))
                continue;

            var hp = h.GetComponentInParent<BossHealth>();
            if (hp != null)
                hp.TakeDamage(damage);
        }
    }
}
