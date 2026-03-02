using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private EnemyData archer;

    void Start()
    {
        // Peluru otomatis hancur setelah beberapa detik
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Cek apakah peluru mengenai Player
        if (other.CompareTag("Player") || other.CompareTag("Player2"))
        {
            // 1. CEK SHIELD: Cari script PlayerShield dari badan player yang tertembak
            PlayerShield shield = other.GetComponent<PlayerShield>();

            // Jika shield ADA dan sedang AKTIF
            if (shield != null && shield.IsShieldActive)
            {
                // Shield yang menyerap damage
                shield.TakeDamage(archer.damage);

                // Peluru hancur karena membentur shield
                Destroy(gameObject);

                // RETURN sangat penting agar kode di bawahnya (kurangin darah) tidak dieksekusi!
                return;
            }

            // 2. JIKA SHIELD TIDAK ADA / SUDAH HANCUR
            if (Health.Instance != null)
            {
                // Baru kurangi darah utama player
                Health.Instance.Hurt(archer.damage);
            }

            // Peluru hancur karena mengenai badan player
            Destroy(gameObject);
        }
    }
}