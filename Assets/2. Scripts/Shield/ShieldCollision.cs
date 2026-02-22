using UnityEngine;

/// <summary>
/// Script ini attach OTOMATIS ke shield sphere instance
/// Fungsi: Detect collision dengan enemy & bullet, forward damage ke PlayerShield
/// </summary>
public class ShieldCollision : MonoBehaviour
{
    [HideInInspector]
    public PlayerShield parentShield;

    // Timer agar saat musuh menempel, HP shield tidak langsung habis dalam sekejap
    private float damageCooldown = 0f;

    private void Start()
    {
        // Pastikan ada collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            // Auto add sphere collider
            SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.isTrigger = true;
            sphereCol.radius = 1f;
            Debug.Log("✅ Auto-added SphereCollider to shield");
        }
        else
        {
            // Set ke trigger
            col.isTrigger = true;
        }
    }

    private void Update()
    {
        // Kurangi cooldown timer setiap frame
        if (damageCooldown > 0f)
        {
            damageCooldown -= Time.deltaTime;
        }
    }

    // 1. DETEKSI PELURU MASUK (Tetap pakai OnTriggerEnter)
    private void OnTriggerEnter(Collider other)
    {
        if (parentShield == null || !parentShield.IsShieldActive) return;

        // Check if enemy bullet
        if (other.CompareTag("EnemyBullet"))
        {
            float damage = 10f;
            parentShield.TakeDamage(damage);
            Destroy(other.gameObject);
            Debug.Log($"Shield blocked bullet! Damage: {damage}");
        }
    }

    // 2. DETEKSI MUSUH MENABRAK (Pakai OnTriggerStay agar bisa didorong keluar)
    private void OnTriggerStay(Collider other)
    {
        if (parentShield == null || !parentShield.IsShieldActive) return;

        // Check if enemy direct collision
        if (other.CompareTag("Enemy"))
        {
            // --- A. EFEK FORCEFIELD MENDORONG MUSUH KELUAR ---
            // Cari tahu arah dari pusat shield ke arah musuh
            Vector3 pushDirection = (other.transform.position - transform.position).normalized;
            // Nol-kan sumbu Y agar musuh tidak ikut terdorong terbang ke atas
            pushDirection.y = 0;

            // Dorong musuh menjauh. (Angka 5f adalah kekuatan dorongan)
            // Jika musuh data.speed nya kencang, kamu bisa naikkan angka 5f ini jadi 8f atau 10f.
            other.transform.position += pushDirection * 5f * Time.deltaTime;


            // --- B. DAMAGE KE SHIELD ---
            // Hanya kurangi HP shield kalau cooldown sudah 0 (setiap 1 detik)
            if (damageCooldown <= 0f)
            {
                float damage = 15f;
                parentShield.TakeDamage(damage);

                // Reset cooldown agar 1 detik ke depan shield tidak kena damage lagi dari tabrakan ini
                damageCooldown = 1f;
                Debug.Log($"🛡️ Shield menahan musuh! Damage: {damage}");
            }
        }
    }
}