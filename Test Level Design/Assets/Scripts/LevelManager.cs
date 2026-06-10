using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [System.Serializable]
    public class SpaceSector
    {
        public string sectorName = "Sector";
        public Transform playerWaypoint; // Position and rotation the player moves to
        public int enemyCount;            // Total enemies to kill in this wave
    }

    [Header("Level Setup")]
    [SerializeField] private List<SpaceSector> sectors = new List<SpaceSector>();
    [SerializeField] private Transform playerTransform;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 2f;

    private int currentSectorIndex = 0;
    private int remainingEnemies;
    private bool isMovingToSector = false;

    private void Start()
    {
        // Default to Main Camera if player transform isn't manually assigned
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
            Debug.LogError("No space sectors defined in the RailShooterManager!");
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
        remainingEnemies = sectors[index].enemyCount;
        isMovingToSector = true;
        
        Debug.Log($"Advancing to {sectors[index].sectorName}. Get ready!");
    }

    private void ExecuteMovement()
    {
        Transform targetWaypoint = sectors[currentSectorIndex].playerWaypoint;

        if (targetWaypoint == null)
        {
            Debug.LogError($"Sector {currentSectorIndex} is missing a player waypoint assignment!");
            isMovingToSector = false;
            return;
        }

        // Lerp position
        playerTransform.position = Vector3.MoveTowards(
            playerTransform.position, 
            targetWaypoint.position, 
            moveSpeed * Time.deltaTime
        );

        // Lerp rotation (important if sectors face different directions)
        playerTransform.rotation = Quaternion.Slerp(
            playerTransform.rotation, 
            targetWaypoint.rotation, 
            rotationSpeed * Time.deltaTime
        );

        // Check if player has arrived at the sector checkpoint
        if (Vector3.Distance(playerTransform.position, targetWaypoint.position) < 0.01f &&
            Quaternion.Angle(playerTransform.rotation, targetWaypoint.rotation) < 1f)
        {
            // Snap cleanly to prevent subtle drifting
            playerTransform.position = targetWaypoint.position;
            playerTransform.rotation = targetWaypoint.rotation;
            
            isMovingToSector = false;
            OnArrivedAtSector();
        }
    }

    private void OnArrivedAtSector()
    {
        Debug.Log($"Arrived at {sectors[currentSectorIndex].sectorName}. Combat active.");
        // OPTIONAL: Trigger your enemy spawning system here if you don't want enemies present before arrival
    }

    /// <summary>
    /// Call this method from your Enemy Health script whenever an enemy is destroyed.
    /// </summary>
    public void RegisterEnemyDeath()
    {
        if (isMovingToSector) return; // Prevent accidental triggers mid-transit

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

}
