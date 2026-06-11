using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 7f;
    [SerializeField] private float explosionForce = 15f;
    [SerializeField] private float explosionDamage = 1000f; // Instant kill

    [Header("Effects (Optional)")]
    [SerializeField] private GameObject explosionEffectPrefab;

    private bool hasExploded = false;

    public void TakeDamage(float amount)
    {
        // Prevent the barrel from exploding multiple times in the same frame
        if (!hasExploded) 
        {
            Explode();
        }
    }

    private void Explode()
    {
        hasExploded = true;

        // 1. Play visual particle effect if you assigned one
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // 2. Find ALL physical colliders within the blast radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearbyObject in colliders)
        {
            // 3. Check if the object is an Enemy. If it is, kill it!
            EnemyAI enemy = nearbyObject.GetComponentInParent<EnemyAI>();
            if (enemy != null)
            {
                // This triggers Die(), which turns off the NavMesh and adds a Rigidbody
                enemy.TakeDamage(explosionDamage); 
            }

            // 4. Now find any Rigidbody in the blast (including the newly dead enemies!)
            Rigidbody rb = nearbyObject.GetComponentInParent<Rigidbody>();
            if (rb != null)
            {
                // Ensure they aren't kinematic so gravity and explosions affect them
                rb.isKinematic = false;

                // 5. BOOM! Apply the physical blast wave
                // The '1f' at the end is an upward modifier to toss them into the air nicely
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 1f, ForceMode.Impulse);
            }
        }

        // 6. Destroy the barrel itself
        Destroy(gameObject);
    }
    
    // Optional: Draws a red sphere in the Unity Editor so you can see the blast radius!
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}