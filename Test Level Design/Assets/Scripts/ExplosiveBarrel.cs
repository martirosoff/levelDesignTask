using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 7f;
    [SerializeField] private float explosionForce = 15f;
    [SerializeField] private float explosionDamage = 1000f; 

    [Header("Effects (Optional)")]
    [SerializeField] private GameObject explosionEffectPrefab;

    private bool hasExploded = false;

    public void TakeDamage(float amount)
    {
        if (!hasExploded) 
        {
            Explode();
        }
    }

    private void Explode()
    {
        hasExploded = true;

        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearbyObject in colliders)
        {
            // 1. Check if the object is an Enemy. If it is, kill it!
            EnemyAI enemy = nearbyObject.GetComponentInParent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(explosionDamage); 
            }

            // 2. Find any Rigidbody in the blast
            Rigidbody rb = nearbyObject.GetComponentInParent<Rigidbody>();
            if (rb != null)
            {
                // ---> NEW SAFETY CHECK HERE <---
                // If this Rigidbody belongs to the player (or its parent is tagged Player), 
                // skip it entirely so they don't fly off the rails!
                if (rb.CompareTag("Player") || nearbyObject.CompareTag("Player"))
                {
                    continue; 
                }

                // Everything else (enemies and environment props) gets blasted:
                rb.isKinematic = false;
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 1f, ForceMode.Impulse);
            }
        }

        Destroy(gameObject);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}