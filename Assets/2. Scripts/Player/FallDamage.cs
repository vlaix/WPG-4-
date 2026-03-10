using UnityEngine;

public class FallDamage : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Titik lokasi player akan di-respawn")]
    [SerializeField] Transform respawn;

    [Header("Sound Effects")]
    [Tooltip("Sound yang diplay saat player jatuh")]
    [SerializeField] private AudioClip fallSound;

    [Tooltip("Sound yang diplay saat respawn")]
    [SerializeField] private AudioClip respawnSound;

    [Tooltip("Volume sound (0-1)")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.8f;

    [Header("Settings")]
    [Tooltip("Damage yang diterima saat jatuh")]
    [SerializeField] private int fallDamage = 2;

    // Audio source (optional - akan auto-create jika tidak ada)
    private AudioSource audioSource;

    private void Start()
    {
        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound (tidak terpengaruh jarak)
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        // Cek apakah yang masuk memiliki tag Player atau Player2
        if (collision.CompareTag("Player") || collision.CompareTag("Player2"))
        {
            // 1. Play fall sound
            PlayFallSound();

            // 2. Kurangi HP menggunakan sistem Health Global (Instance)
            if (Health.Instance != null)
            {
                Health.Instance.Hurt(fallDamage);
            }
            else
            {
                Debug.LogWarning("Health Instance tidak ditemukan!");
            }

            // 3. Respawn player
            if (respawn != null)
            {
                collision.transform.position = respawn.position;

                // Reset velocity (kecepatan jatuh) agar tidak terbawa saat respawn
                Rigidbody rb = collision.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                }
            }
            else
            {
                Debug.LogError("Titik Respawn belum dimasukkan di Inspector FallDamage!");
            }

            // 4. Play respawn sound
            PlayRespawnSound();

            Debug.Log($"{collision.gameObject.name} jatuh dan respawn!");
        }
    }

    private void PlayFallSound()
    {
        if (audioSource != null && fallSound != null)
        {
            audioSource.PlayOneShot(fallSound, volume);
        }
    }

    private void PlayRespawnSound()
    {
        if (audioSource != null && respawnSound != null)
        {
            // Play dengan delay kecil agar tidak bentrok dengan fall sound
            audioSource.PlayOneShot(respawnSound, volume);
        }
    }
}