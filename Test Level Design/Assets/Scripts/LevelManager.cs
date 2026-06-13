using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 
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
    [SerializeField] private TextMeshProUGUI levelNameText; 
    
    // NEW: A slot for the start button so we can hide it once clicked
    [Tooltip("Drag your Start Button GameObject here.")]
    [SerializeField] private GameObject startButtonContainer; 
    [SerializeField] private GameObject startBackgroundImageContainer; 

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
    private bool hasLevelStarted = false; // NEW: Tracks if the button was pressed
    private bool isGameOver = false; // NEW: Tracks if the player is dead
    
    private void Start()
    {
        if (playerTransform == null && Camera.main != null)
        {
            playerTransform = Camera.main.transform;
        }

        if (sectors.Count == 0)
        {
            Debug.LogError("No space sectors defined in the LevelManager!");
        }

        // Optional: Show a "Ready" message before the button is pressed
        if (levelNameText != null)
        {
            levelNameText.text = $"{SceneManager.GetActiveScene().name} - Ready";
        }
        
        // Notice we REMOVED InitializeSector(0) from here! 
        // We now wait for the button click.
    }

    private void Update()
    {
        if (isMovingToSector)
        {
            ExecuteMovement();
        }
    }

    // NEW: The method the UI Button will call to kick off the game
    public void StartLevel()
    {
        // Prevent double-clicking
        if (hasLevelStarted) return; 
        
        hasLevelStarted = true;

        // Hide the button from the screen
        if (startButtonContainer != null)
        {
            startButtonContainer.SetActive(false);
            startBackgroundImageContainer.SetActive(false);
        }

        // Begin the first sector!
        if (sectors.Count > 0)
        {
            InitializeSector(0);
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
        // Combat is only active if the level started, we aren't moving, AND we aren't dead!
        return hasLevelStarted && !isMovingToSector && !isGameOver;
    }

    // This is called by the enemy when it reaches the player
    public void GameOver()
    {
        if (isGameOver) return; // Prevent multiple enemies from triggering this at once
        
        isGameOver = true;
        Debug.Log("Player Killed! Game Over.");

        // Update the UI so the player knows they died
        if (levelNameText != null)
        {
            levelNameText.text = "<color=red>GAME OVER</color>";
        }

        // Wait 2 seconds, then execute the RestartLevel method
        Invoke(nameof(RestartLevel), 2f);
    }

    private void RestartLevel()
    {
        // Load the exact same scene we are currently in
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}