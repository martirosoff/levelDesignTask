using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [SerializeField] private float health = 100f;
    
    [Header("Attack Settings")]
    [Tooltip("How close the enemy needs to be to kill the player.")]
    [SerializeField] private float attackRange = 1.5f; 
    
    private NavMeshAgent agent;
    private Transform player;
    private LevelManager levelManager;
    
    private bool isAwake = false; 
    private bool hasAttacked = false; // Prevents the enemy from killing you 50 times in one frame

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        levelManager = FindObjectOfType<LevelManager>();
    }

    private void Update()
    {
        // Only chase if awake, the player exists, we are on the NavMesh, and we haven't attacked yet
        if (isAwake && player != null && agent.isOnNavMesh && !hasAttacked)
        {
            agent.SetDestination(player.position);

            // Check the distance between the enemy and the player
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            if (distanceToPlayer <= attackRange)
            {
                AttackPlayer();
            }
        }
    }

    private void AttackPlayer()
    {
        hasAttacked = true;
        agent.isStopped = true; // Stop the enemy in its tracks
        
        if (levelManager != null)
        {
            levelManager.GameOver(); // Tell the manager the player is dead
        }
    }

    public void WakeUp()
    {
        isAwake = true;
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
        if (levelManager != null)
        {
            levelManager.RegisterEnemyDeath();
        }

        if (agent != null)
        {
            agent.enabled = false;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        rb.isKinematic = false;
        rb.useGravity = true;

        rb.AddForce(-transform.forward * 5f, ForceMode.Impulse); 
        rb.AddTorque(transform.right * 5f, ForceMode.Impulse);   

        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        this.enabled = false;
        Destroy(gameObject, 5f);
    }
}