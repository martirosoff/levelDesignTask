using UnityEngine;

public class CharacterShooting : MonoBehaviour
{
    private Camera mainCamera;
    [SerializeField] private LayerMask enemyLayer; // Allows you to select the enemy layer

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (mainCamera == null) return;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            
            // Added Mathf.Infinity and enemyLayer here
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, enemyLayer))
            {
                EnemyAI enemy = hit.collider.GetComponentInParent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.TakeDamage(50f);
                    Debug.Log("Hit");
                }
            }
        }
    }
}