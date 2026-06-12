using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private LayerMask enemyLayer; 
    
    [Header("Visual Effects")]
    [Tooltip("The tip of the gun where the flash should appear.")]
    [SerializeField] private Transform firePoint; 
    [Tooltip("The particle effect to play when firing.")]
    [SerializeField] private GameObject muzzleFlashPrefab; 
    [Tooltip("The spark/blood effect to play where the bullet hits.")]
    [SerializeField] private GameObject hitEffectPrefab; 

    private Camera mainCamera;
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
        if (levelManager != null && !levelManager.IsCombatActive())
        {
            return; 
        }

        // MOBILE INPUT (Android / iOS)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    return; 
                }

                FireRaycast(touch.position);
            }
        }
        // PC / EDITOR INPUT (Mouse)
        else if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return; 
            }

            FireRaycast(Input.mousePosition);
        }
    }

    private void FireRaycast(Vector3 screenPosition)
    {
        if (mainCamera == null) return;

        // 1. Play Muzzle Flash immediately when the trigger is pulled
        if (muzzleFlashPrefab != null && firePoint != null)
        {
            // Spawn the flash attached to the firePoint so it moves with the gun
            GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation, firePoint);
            // Destroy the flash object after a fraction of a second so it doesn't clutter the game
            Destroy(flash, 0.15f); 
        }

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, enemyLayer))
        {
            // 2. Play Hit Effect exactly where the raycast struck the object
            if (hitEffectPrefab != null)
            {
                // Quaternion.LookRotation(hit.normal) makes the sparks fly OUTWARD from the surface we hit!
                GameObject impact = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                // Destroy the spark object after 2 seconds
                Destroy(impact, 1f);
            }

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