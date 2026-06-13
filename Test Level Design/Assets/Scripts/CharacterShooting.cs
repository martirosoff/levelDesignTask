using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private float fireRate = 0.25f; 
    [SerializeField] private LayerMask enemyLayer; 
    
    [Header("Recoil Settings")]
    [Tooltip("The 3D model of your gun. If left empty, it will recoil whatever object this script is on.")]
    [SerializeField] private Transform weaponModel;
    [Tooltip("How much the gun rotates when fired (X is up/down, Y and Z are side-to-side).")]
    [SerializeField] private Vector3 recoilRotation = new Vector3(-10f, 2f, 1f);
    [Tooltip("How fast the gun violently kicks up.")]
    [SerializeField] private float recoilSnappiness = 20f;
    [Tooltip("How fast the gun smoothly returns to its resting position.")]
    [SerializeField] private float recoilReturnSpeed = 10f;

    [Header("Visual Effects")]
    [SerializeField] private Transform firePoint; 
    [SerializeField] private GameObject muzzleFlashPrefab; 
    [SerializeField] private GameObject hitEffectPrefab; 

    private Camera mainCamera;
    private LevelManager levelManager;
    private float nextFireTime = 0f; 

    // NEW: Variables to track the mathematical spring effect
    private Vector3 currentRecoilRotation;
    private Vector3 targetRecoilRotation;
    private Quaternion startingWeaponRotation;

    private void Start()
    {
        mainCamera = Camera.main;
        levelManager = FindObjectOfType<LevelManager>();
        
        if (levelManager == null)
        {
            Debug.LogError("CharacterShooting: Could not find the LevelManager in the scene!");
        }

        // If you didn't assign a weapon model in the inspector, it targets this object
        if (weaponModel == null)
        {
            weaponModel = transform;
        }

        // Save the original rotation of the gun so it knows exactly where "Zero" is
        startingWeaponRotation = weaponModel.localRotation;
    }

    private void Update()
    {
        // ---> 1. PROCESS RECOIL FIRST <---
        // We put this at the very top of Update! This ensures the gun is always smoothly returning to 
        // its resting position, even if the player dies, pauses, or is waiting for a sector to start.
        targetRecoilRotation = Vector3.Lerp(targetRecoilRotation, Vector3.zero, recoilReturnSpeed * Time.deltaTime);
        currentRecoilRotation = Vector3.Slerp(currentRecoilRotation, targetRecoilRotation, recoilSnappiness * Time.deltaTime);
        weaponModel.localRotation = startingWeaponRotation * Quaternion.Euler(currentRecoilRotation);

        // 2. Are we in combat?
        if (levelManager != null && !levelManager.IsCombatActive())
        {
            return; 
        }

        // 3. Has enough time passed since our last shot?
        if (Time.time < nextFireTime)
        {
            return; 
        }

        // MOBILE INPUT
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
        // PC / EDITOR INPUT
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

        nextFireTime = Time.time + fireRate;

        // ---> 4. APPLY THE RECOIL <---
        // Instead of just setting the rotation, we ADD it to the target. This way, if you shoot rapidly, 
        // the gun kicks higher and higher! We also add a random side-to-side variance so it feels organic.
        targetRecoilRotation += new Vector3(recoilRotation.x, Random.Range(-recoilRotation.y, recoilRotation.y), Random.Range(-recoilRotation.z, recoilRotation.z));

        if (muzzleFlashPrefab != null && firePoint != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation, firePoint);
            Destroy(flash, 0.1f); 
        }

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, enemyLayer))
        {
            if (hitEffectPrefab != null)
            {
                GameObject impact = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impact, 2f);
            }

            EnemyAI enemy = hit.collider.GetComponentInParent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(50f);
                return; 
            }

            ExplosiveBarrel barrel = hit.collider.GetComponentInParent<ExplosiveBarrel>();
            if (barrel != null)
            {
                barrel.TakeDamage(100f); 
            }
        }
    }
}