using UnityEngine;

//BulletImpactusing UnityEngine;

public class BulletImpactNear : MonoBehaviour
{
    public GameObject impactPrefab;
    public float impactLife = 0.25f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;
        if (impactPrefab != null)
        {
            GameObject fx = Instantiate(
                impactPrefab,
                transform.position,
                Quaternion.identity
            );
            Destroy(fx, impactLife);
        }

        //Destroy(gameObject);
    }
}
