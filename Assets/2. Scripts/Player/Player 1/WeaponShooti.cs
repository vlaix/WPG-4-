using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponShoot : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float projectileLifetime = 5f;

    [Header("Shooting Settings")]
    [SerializeField] private float fireRate = 0.5f;

    [Header("Player Reference")]
    [SerializeField] private GameObject player;

    // Reference to your PlayerControl
    private PlayerControl playerControls;
    private float nextFireTime = 0f;

    void Awake()
    {
        // Create instance of your PlayerControl
        playerControls = new PlayerControl();
    }

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (firePoint == null)
        {
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.SetParent(transform);
            firePointObj.transform.localPosition = new Vector3(0, 0, 1f);
            firePoint = firePointObj.transform;
        }
    }

    void OnEnable()
    {
        // Enable the Player1Movement action map
        playerControls.Player1Movement.Enable();

        // Subscribe to the shooting action
        playerControls.Player1Movement.shooting.performed += OnShoot;
    }

    void OnDisable()
    {
        // Unsubscribe and disable
        playerControls.Player1Movement.shooting.performed -= OnShoot;
        playerControls.Player1Movement.Disable();
    }

    private void OnShoot(InputAction.CallbackContext context)
    {
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("Projectile prefab not assigned!");
            return;
        }

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = firePoint.forward * projectileSpeed;
        }

        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.SetPlayer(player);
        }

        Destroy(projectile, projectileLifetime);
    }
}