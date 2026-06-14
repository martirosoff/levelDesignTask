using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI; 

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
    
    // Tracks if the enemy is already dead to prevent explosion double-kills
    private bool isDead = false; 
    
    private GameObject activeHealthBar;
    private Image healthBarFill;

    private void Start()
    {
        maxHealth = health;
        agent = GetComponent<NavMeshAgent>();
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        levelManager = FindObjectOfType<LevelManager>();

        // Spawns the health bar for ALL enemies if the prefab is assigned
        if (healthBarPrefab != null)
        {
            activeHealthBar = Instantiate(healthBarPrefab, transform.position + Vector3.up * 2f, Quaternion.identity, transform);
            
            Image[] images = activeHealthBar.GetComponentsInChildren<Image>();
            foreach (Image img in images)
            {
                if (img.gameObject.name.ToLower().Contains("fill"))
                {
                    healthBarFill = img;
                    break;
                }
            }
            
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

    public void WakeUp()
    {
        isAwake = true;
    }

    public void TakeDamage(float amount)
    {
        // If they are already dead, ignore the explosion/bullets entirely!
        if (isDead) return; 

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
        // Lock the enemy state so they can't be killed twice by overlapping hitboxes
        isDead = true; 

        // Disable the healthbar immediately so it doesn't float over their dead body
        if (activeHealthBar != null)
        {
            activeHealthBar.SetActive(false);
        }

        // 1. Tell the LevelManager an enemy died
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