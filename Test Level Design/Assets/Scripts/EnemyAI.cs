using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [SerializeField] private float health = 100f;
    private NavMeshAgent agent;
    private Transform player;
    private LevelManager levelManager;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        // Find the player (assumes player tag is "Player")
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // Find the LevelManager in the scene
        levelManager = FindObjectOfType<LevelManager>();
    }

    private void Update()
    {
        // Move towards player if assigned
        if (player != null)
        {
            agent.SetDestination(player.position);
        }
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
        // Notify the manager
        if (levelManager != null)
        {
            levelManager.RegisterEnemyDeath();
        }

        // Remove from scene
        Destroy(gameObject);
    }
}