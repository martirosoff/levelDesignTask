using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI; // Required to control the health bar UI

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float health = 100f;
    [Tooltip("Drag your World Space Canvas Health Bar prefab here. All enemies will now use it!")]
    [SerializeField] private GameObject healthBarPrefab; 

    [Header("Attack Settings")]
    [Tooltip("How close the enemy needs to be to kill the player.")]
    [SerializeField] private float attackRange = 1.5f; 

    private NavMeshAgent agent;
    private Transform player;
    private LevelManager levelManager;
    
    private bool isAwake = false; 
    private bool hasAttacked = false; 
    private float maxHealth;
    
    private GameObject activeHealthBar;
    private Image healthBarFill;

    private void Start()
    {
        maxHealth = health;
        agent = GetComponent<NavMeshAgent>();
        
        // Find the player in the scene
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // Find the level manager in the scene
        levelManager = FindObjectOfType<LevelManager>();

        // UPDATED: Spawns the health bar for ALL enemies if the prefab is assigned
        if (healthBarPrefab != null)
        {
            // Spawn the health bar 2 units above the enemy's head, and make it a child of this enemy
            activeHealthBar = Instantiate(healthBarPrefab, transform.position + Vector3.up * 2f, Quaternion.identity, transform);
            
            // Bulletproof lookup: Look through all images in the spawned UI to find the one named "Fill"
            Image[] images = activeHealthBar.GetComponentsInChildren<Image>();
            foreach (Image img in images)
            {
                if (img.gameObject.name.ToLower().Contains("fill"))
                {
                    healthBarFill = img;
                    break;
                }
            }
            
            // Fallback: If no image was explicitly named "Fill", just grab the last one found
            if (healthBarFill == null && images.Length > 0)
            {
                healthBarFill = images[images.Length - 1];
            }
        }
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

        // Force the health bar to always face directly at the camera
        if (activeHealthBar != null && Camera.main != null)
        {
            activeHealthBar.transform.LookAt(Camera.main.transform);
            activeHealthBar.transform.Rotate(0, 180, 0); 
        }
    }

    private void AttackPlayer()
    {
        hasAttacked = true;
        
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true; // Stop moving immediately
        }
        
        if (levelManager != null)
        {
            levelManager.GameOver(); // Kill the player and restart level
        }
    }

    // Called by LevelManager to make enemies start chasing
    public void WakeUp()
    {
        isAwake = true;
    }

    public void TakeDamage(float amount)
    {
        health -= amount;

        // Update their health bar slider fill
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = health / maxHealth;
        }

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // NEW: Disable the healthbar immediately so it doesn't float in the air while the body falls
        if (activeHealthBar != null)
        {
            activeHealthBar.SetActive(false);
        }

        // 1. Tell the LevelManager an enemy died so it can track progress
        if (levelManager != null)
        {
            levelManager.RegisterEnemyDeath();
        }

        // 2. Shut off the AI pathfinding so they stop chasing
        if (agent != null)
        {
            agent.enabled = false;
        }

        // 3. Turn on Physics Rigidbody to make them fall/fly back like a ragdoll
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        rb.isKinematic = false;
        rb.useGravity = true;

        // Apply a satisfying backward push and spin on death
        rb.AddForce(-transform.forward * 5f, ForceMode.Impulse); 
        rb.AddTorque(transform.right * 5f, ForceMode.Impulse);   

        // 4. Change layer so the player's gun raycast passes right through their dead body
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        
        // 5. Turn off this script component so Update loops stop running
        this.enabled = false;
        
        // 6. Clean up the dead body asset from the game after 5 seconds
        Destroy(gameObject, 5f);
    }
}