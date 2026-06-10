using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [SerializeField] private float health = 100f;
    private NavMeshAgent agent;
    private Transform player;
    private LevelManager levelManager;
    
    // NEW: Controls whether this enemy is allowed to chase the player
    private bool isAwake = false; 

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        levelManager = FindObjectOfType<LevelManager>();
    }

    private void Update()
    {
        // NEW: Only move if the enemy has been awakened by the LevelManager
        if (isAwake && player != null && agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);
        }
    }

    // NEW: Public method for the LevelManager to call
    public void WakeUp()
    {
        isAwake = true;

        bool hasPlayer = (player != null);
        bool onMesh = agent.isOnNavMesh;
    
    Debug.Log($"{gameObject.name} woke up! Has Player? {hasPlayer} | Is on NavMesh? {onMesh}");
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

private void Die()
    {
        // 1. Notify the level manager that this enemy is defeated
        if (levelManager != null)
        {
            levelManager.RegisterEnemyDeath();
        }

        // 2. Turn off the NavMeshAgent! If we don't do this, it will fight the physics engine
        if (agent != null)
        {
            agent.enabled = false;
        }

        // 3. Get or Add a Rigidbody for physics
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // Ensure physics are active
        rb.isKinematic = false;
        rb.useGravity = true;

        // 4. Add a physical impulse force to knock them backward and tip them over
        rb.AddForce(-transform.forward * 5f, ForceMode.Impulse); // Push back
        rb.AddTorque(transform.right * 5f, ForceMode.Impulse);   // Tip backward

        // 5. Change the layer so the dead body doesn't block bullets meant for living enemies!
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        // 6. Disable THIS script so the enemy stops thinking/updating
        this.enabled = false;

        // 7. Destroy the body after 5 seconds to keep the level clean and performant
        Destroy(gameObject, 5f);
    }
}