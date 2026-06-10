using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [System.Serializable]
    public class SpaceSector
    {
        public string sectorName = "Sector";
        
        [Tooltip("The sequential list of points the player follows to reach this sector. The last point is the combat station.")]
        public List<Transform> movementPath = new List<Transform>(); 
        
        public GameObject sectorEnemyContainer; 
    }

    [Header("Level Setup")]
    [SerializeField] private List<SpaceSector> sectors = new List<SpaceSector>();
    [SerializeField] private Transform playerTransform;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 2f;

    private int currentSectorIndex = 0;
    private int currentPathPointIndex = 0; // NEW: Tracks the current node in the sector's path
    private int remainingEnemies;
    private bool isMovingToSector = false;

    private void Start()
    {
        if (playerTransform == null && Camera.main != null)
        {
            playerTransform = Camera.main.transform;
        }

        if (sectors.Count > 0)
        {
            InitializeSector(0);
        }
        else
        {
            Debug.LogError("No space sectors defined in the LevelManager!");
        }
    }

    private void Update()
    {
        if (isMovingToSector)
        {
            ExecuteMovement();
        }
    }

    private void InitializeSector(int index)
    {
        currentSectorIndex = index;
        currentPathPointIndex = 0; // NEW: Reset path progress back to the first point
        
        // Safety check: Make sure the sector actually has at least one movement point
        if (sectors[index].movementPath == null || sectors[index].movementPath.Count == 0)
        {
            Debug.LogError($"Sector {index} ({sectors[index].sectorName}) has no movement points assigned!");
            return;
        }

        isMovingToSector = true;
        
        if (sectors[index].sectorEnemyContainer != null)
        {
            remainingEnemies = sectors[index].sectorEnemyContainer.transform.childCount;
        }
        else
        {
            remainingEnemies = 0;
        }
        
        Debug.Log($"Advancing to {sectors[index].sectorName}. Traveling along path...");
    }

    private void ExecuteMovement()
    {
        // NEW: Get the exact target point we are currently heading towards in the list
        List<Transform> activePath = sectors[currentSectorIndex].movementPath;
        Transform targetWaypoint = activePath[currentPathPointIndex];

        if (targetWaypoint == null)
        {
            Debug.LogError($"Sector {currentSectorIndex} is missing waypoint index {currentPathPointIndex}!");
            isMovingToSector = false;
            return;
        }

        // Lerp position
        playerTransform.position = Vector3.MoveTowards(
            playerTransform.position, 
            targetWaypoint.position, 
            moveSpeed * Time.deltaTime
        );

        // Lerp rotation
        playerTransform.rotation = Quaternion.Slerp(
            playerTransform.rotation, 
            targetWaypoint.rotation, 
            rotationSpeed * Time.deltaTime
        );

        // Check if player has arrived at this specific path point
        if (Vector3.Distance(playerTransform.position, targetWaypoint.position) < 0.01f &&
            Quaternion.Angle(playerTransform.rotation, targetWaypoint.rotation) < 1f)
        {
            // Snap cleanly to prevent subtle drifting
            playerTransform.position = targetWaypoint.position;
            playerTransform.rotation = targetWaypoint.rotation;
            
            // NEW: Check if there are more points left in this sector's path
            if (currentPathPointIndex < activePath.Count - 1)
            {
                currentPathPointIndex++; // Target the next point on the next frame
                Debug.Log($"Reached point {currentPathPointIndex - 1}. Moving to point {currentPathPointIndex}.");
            }
            else
            {
                // We reached the final point of the path! Stop moving and start combat.
                isMovingToSector = false;
                OnArrivedAtSector();
            }
        }
    }

    private void OnArrivedAtSector()
    {
        Debug.Log($"Arrived at {sectors[currentSectorIndex].sectorName} final station. Combat active.");
        
        GameObject container = sectors[currentSectorIndex].sectorEnemyContainer;
        if (container != null)
        {
            EnemyAI[] sectorEnemies = container.GetComponentsInChildren<EnemyAI>();
            foreach (EnemyAI enemy in sectorEnemies)
            {
                enemy.WakeUp();
            }
        }
    }

    public void RegisterEnemyDeath()
    {
        if (isMovingToSector) return; 

        remainingEnemies--;
        Debug.Log($"Enemy killed! Remaining in sector: {remainingEnemies}");

        if (remainingEnemies <= 0)
        {
            EvaluateProgress();
        }
    }

    private void EvaluateProgress()
    {
        int nextSector = currentSectorIndex + 1;

        if (nextSector < sectors.Count)
        {
            InitializeSector(nextSector);
        }
        else
        {
            Debug.Log("Level Completed!");
        }
    }

    public bool IsCombatActive()
    {
        return !isMovingToSector;
    }
}