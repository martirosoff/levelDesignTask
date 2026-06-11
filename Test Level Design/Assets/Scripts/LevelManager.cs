using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 
// NEW: Required to talk to TextMeshPro UI elements
using TMPro; 

public class LevelManager : MonoBehaviour
{
    [System.Serializable]
    public class SpaceSector
    {
        public string sectorName = "Sector";
        [Tooltip("The sequential list of points the player follows to reach this sector.")]
        public List<Transform> movementPath = new List<Transform>(); 
        public GameObject sectorEnemyContainer; 
    }

    [Header("UI Settings")]
    [Tooltip("Drag your TextMeshPro UI element here to display the level name.")]
    [SerializeField] private TextMeshProUGUI levelNameText; // NEW: The UI reference

    [Header("Level Setup")]
    [SerializeField] private List<SpaceSector> sectors = new List<SpaceSector>();
    [SerializeField] private Transform playerTransform;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 2f;

    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName;

    private int currentSectorIndex = 0;
    private int currentPathPointIndex = 0; 
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
        currentPathPointIndex = 0; 
        
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
        
        // NEW: Update the UI Text to show the Scene Name and the Sector Name!
        if (levelNameText != null)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            string currentSector = sectors[index].sectorName;
            levelNameText.text = $"{currentScene} - {currentSector}";
        }
        
        Debug.Log($"Advancing to {sectors[index].sectorName}. Traveling along path...");
    }

    private void ExecuteMovement()
    {
        List<Transform> activePath = sectors[currentSectorIndex].movementPath;
        Transform targetWaypoint = activePath[currentPathPointIndex];

        if (targetWaypoint == null) return;

        playerTransform.position = Vector3.MoveTowards(
            playerTransform.position, 
            targetWaypoint.position, 
            moveSpeed * Time.deltaTime
        );

        playerTransform.rotation = Quaternion.Slerp(
            playerTransform.rotation, 
            targetWaypoint.rotation, 
            rotationSpeed * Time.deltaTime
        );

        if (Vector3.Distance(playerTransform.position, targetWaypoint.position) < 0.01f &&
            Quaternion.Angle(playerTransform.rotation, targetWaypoint.rotation) < 1f)
        {
            playerTransform.position = targetWaypoint.position;
            playerTransform.rotation = targetWaypoint.rotation;
            
            if (currentPathPointIndex < activePath.Count - 1)
            {
                currentPathPointIndex++; 
            }
            else
            {
                isMovingToSector = false;
                OnArrivedAtSector();
            }
        }
    }

    private void OnArrivedAtSector()
    {
        GameObject container = sectors[currentSectorIndex].sectorEnemyContainer;
        if (container != null)
        {
            EnemyAI[] sectorEnemies = container.GetComponentsInChildren<EnemyAI>();
            foreach (EnemyAI enemy in sectorEnemies)
            {
                enemy.WakeUp();
            }
        }
        else 
        {
            LoadNextLevel(); 
        }
    }

    public void RegisterEnemyDeath()
    {
        if (isMovingToSector) return; 

        remainingEnemies--;

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
            LoadNextLevel(); 
        }
    }

    private void LoadNextLevel()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
        }
    }

    public bool IsCombatActive()
    {
        return !isMovingToSector;
    }
}