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
        if (levelManager != null)
        {
            levelManager.RegisterEnemyDeath();
        }
        Destroy(gameObject);
    }
}