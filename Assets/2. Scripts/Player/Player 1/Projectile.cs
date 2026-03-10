using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float damage = 1f;
    [SerializeField] private LayerMask hitLayers;

    [Header("Audio Settings")]
    [Tooltip("Suara saat peluru keluar")]
    [SerializeField] private AudioClip shootSound;

    [Tooltip("Suara saat mengenai musuh")]
    [SerializeField] private AudioClip hitSound;

    [Tooltip("Volume suara (Bisa diisi 1 atau lebih jika masih kurang keras)")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    private GameObject player;

    void Start()
    {
        // 1. MAIN-KAN SFX TEMBAKAN (Mode 2D agar sangat jelas dan keras)
        PlaySound2D(shootSound);
    }

    public void SetPlayer(GameObject playerRef)
    {
        player = playerRef;
    }

    void OnTriggerEnter(Collider other)
    {
        // Jangan bertabrakan dengan player
        if (other.CompareTag("Player") || other.CompareTag("Player2"))
            return;

        HandleHit(other.gameObject);

        if (other.CompareTag("Enemy"))
        {
            // 2. MAIN-KAN SFX HIT (Mode 2D) sebelum peluru hancur
            PlaySound2D(hitSound);

            other.GetComponent<EnemyBehavior>().TakeDamage(damage, transform.position);
            Destroy(gameObject);
        }
    }

    void HandleHit(GameObject hitObject)
    {
        Debug.Log($"Projectile hit: {hitObject.name}");
    }

    // --- FUNGSI KHUSUS PEMUTAR SUARA 2D ---
    private void PlaySound2D(AudioClip clip)
    {
        if (clip == null) return;

        // Buat GameObject sementara khusus untuk menjadi speaker
        GameObject audioObj = new GameObject("TempAudio_" + clip.name);
        AudioSource source = audioObj.AddComponent<AudioSource>();

        source.clip = clip;
        source.volume = volume;

        // KUNCI UTAMA: Ubah ke 0 agar jadi suara 2D (Tidak terpengaruh jarak kamera)
        source.spatialBlend = 0f;

        source.Play();

        // Hancurkan speaker sementara ini setelah durasi lagunya selesai
        Destroy(audioObj, clip.length);
    }
}