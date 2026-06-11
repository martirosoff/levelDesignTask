using UnityEngine;
using UnityEngine.EventSystems; 

public class CharacterShooting : MonoBehaviour
{
    private Camera mainCamera;
    [SerializeField] private LayerMask enemyLayer; 
    
    // ---> THIS IS THE LINE YOUR SCRIPT WAS MISSING <---
    // It declares the variable so the whole script can see it!
    private LevelManager levelManager;

    private void Start()
    {
        mainCamera = Camera.main;
        
        // Find the Level Manager in the scene when the game starts
        levelManager = FindObjectOfType<LevelManager>();
        
        if (levelManager == null)
        {
            Debug.LogError("CharacterShooting: Could not find the LevelManager in the scene!");
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 1. Are we moving? If so, lock the gun.
            if (levelManager != null && !levelManager.IsCombatActive())
            {
                return; 
            }

            // 2. Are we clicking on a UI menu? If so, lock the gun.
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return; 
            }

            // Safety check for the camera
            if (mainCamera == null) return;

            // 3. Fire the Raycast!
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, enemyLayer))
            {
                // Did we hit an enemy?
                EnemyAI enemy = hit.collider.GetComponentInParent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.TakeDamage(100f);
                    return; // Stop looking, we already hit an enemy
                }

                // Did we hit an explosive barrel?
                ExplosiveBarrel barrel = hit.collider.GetComponentInParent<ExplosiveBarrel>();
                if (barrel != null)
                {
                    barrel.TakeDamage(100f); 
                }
            }
        }
    }
}