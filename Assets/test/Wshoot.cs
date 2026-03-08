using UnityEngine;
using UnityEngine.InputSystem; // Wajib ada

public partial class Wshoot : MonoBehaviour
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

    private float nextFireTime = 0f;

    void Start()
    {
        // Jika player tidak diisi di Inspector, anggap objek ini adalah bagian dari Player
        if (player == null)
        {
            player = this.gameObject;
        }

        // Setup Firepoint otomatis jika kosong
        if (firePoint == null)
        {
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.SetParent(transform);
            firePointObj.transform.localPosition = new Vector3(0, 0, 1f);
            firePoint = firePointObj.transform;
        }
    }

    // --- INI FUNGSI CALLBACK UTAMA ---
    // Hubungkan ini ke Player Input Component di Inspector
    public void OnFire(InputAction.CallbackContext context)
    {
        // Kita gunakan context.performed agar tembakan terjadi saat tombol ditekan penuh
        if (context.performed && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("Projectile prefab belum dipasang!");
            return;
        }

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Gunakan velocity (linearVelocity untuk Unity versi terbaru)
            rb.linearVelocity = firePoint.forward * projectileSpeed;
        }

        // Jika ada script Projectile untuk identifikasi siapa penembaknya
        if (projectile.TryGetComponent<Projectile>(out Projectile projScript))
        {
            projScript.SetPlayer(player);
        }

        Destroy(projectile, projectileLifetime);
    }
}