using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterShooting : MonoBehaviour
{
    private Camera mainCamera;
    [SerializeField] private LayerMask enemyLayer; 
    private LevelManager levelManager;

    private void Start()
    {
        mainCamera = Camera.main;
        levelManager = FindObjectOfType<LevelManager>();
        
        if (levelManager == null)
        {
            Debug.LogError("CharacterShooting: Could not find the LevelManager in the scene!");
        }
    }

    private void Update()
    {
        // 1. Are we moving or waiting to start? If so, lock the gun.
        if (levelManager != null && !levelManager.IsCombatActive())
        {
            return; 
        }

        // 2. MOBILE INPUT (Android / iOS)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Only fire the exact moment the finger touches the screen
            if (touch.phase == TouchPhase.Began)
            {
                // Touch-specific UI check (requires the fingerId to work!)
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    return; 
                }

                FireRaycast(touch.position);
            }
        }
        // 3. PC / EDITOR INPUT (Mouse)
        else if (Input.GetMouseButtonDown(0))
        {
            // Mouse-specific UI check
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return; 
            }

            FireRaycast(Input.mousePosition);
        }
    }

    // NEW: We moved the Raycast down here so we don't have to write it twice!
    private void FireRaycast(Vector3 screenPosition)
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, enemyLayer))
        {
            // Did we hit an enemy?
            EnemyAI enemy = hit.collider.GetComponentInParent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(100f);
                return; 
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