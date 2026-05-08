using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // Wajib untuk mengakses komponen Image

public partial class Wshoot : MonoBehaviour
{
    [Header("Ammo Settings")]
    public int currentAmmo = 10;
    public int maxAmmo = 20;

    [Header("UI Indicator")]
    [SerializeField] private Image emptyAmmoWarning; // Tarik Image Warning (misal icon peluru silang) ke sini

    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float projectileLifetime = 5f;

    [Header("Shooting Settings")]
    [SerializeField] private float fireRate = 0.5f;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float knockbackJumpForce = 5f;
    [SerializeField] private ForceMode knockbackForceMode = ForceMode.Impulse;

    [Header("Player Reference")]
    [SerializeField] private GameObject player;
    [SerializeField] private Animator animator;

    private float nextFireTime = 0f;

    void Start()
    {
        if (player == null) player = this.gameObject;
        if (animator == null) animator = player.GetComponent<Animator>();

        if (firePoint == null)
        {
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.SetParent(transform);
            firePointObj.transform.localPosition = new Vector3(0, 0, 1f);
            firePoint = firePointObj.transform;
        }

        // Jalankan pengecekan UI di awal
        UpdateAmmoIndicator();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
            else
            {
                Debug.Log("Ammo Kosong!");
                // Opsional: Mainkan suara 'klik' kosong di sini
            }
        }
    }

    void Shoot()
    {
        if (projectilePrefab == null) return;

        currentAmmo--;
        UpdateAmmoIndicator(); // Update indikator setelah menembak

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = firePoint.forward * projectileSpeed;
        }

        if (projectile.TryGetComponent<Projectile>(out Projectile projScript))
        {
            projScript.SetPlayer(player);
        }

        Destroy(projectile, projectileLifetime);
        if (animator != null) animator.SetTrigger("Shoot");

        ApplyKnockback();
    }

    void ApplyKnockback()
    {
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            Vector3 knockbackDir = -firePoint.forward;
            Vector3 totalForce = knockbackDir * knockbackForce + Vector3.up * knockbackJumpForce;
            playerRb.AddForce(totalForce, knockbackForceMode);
        }
    }

    public void AddAmmo(int amount)
    {
        currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo);
        UpdateAmmoIndicator(); // Update indikator setelah diisi Builder
    }

    // --- LOGIKA INDIKATOR GAMBAR ---
    private void UpdateAmmoIndicator()
    {
        if (emptyAmmoWarning != null)
        {
            // Jika peluru 0, maka Image AKTIF. Jika masih ada, Image MATI.
            emptyAmmoWarning.gameObject.SetActive(currentAmmo <= 0);
        }
    }
}