using UnityEngine;

public class CollectLoots : MonoBehaviour
{
    [Header("Collection Settings")]
    [Tooltip("Range untuk mengumpulkan loot secara otomatis")]
    public float collectionRange = 2f;

    [Tooltip("Layer mask untuk loot objects (opsional)")]
    public LayerMask lootLayer;

    [Header("Audio & Effects")]
    [Tooltip("Sound effect saat mengambil loot")]
    public AudioClip pickupSound;

    [Tooltip("Particle effect saat mengambil loot")]
    public GameObject pickupEffect;

    [Header("Debug")]
    [Tooltip("Show collection range in Scene view")]
    public bool showDebugGizmos = true;

    private Transform playerTransform;
    private AudioSource audioSource;

    private void Start()
    {
        playerTransform = transform;

        // Setup audio source jika ada pickup sound
        if (pickupSound != null)
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
        }

        // Validasi apakah ini Player 2
        if (!gameObject.CompareTag("Player2"))
        {
            Debug.LogWarning("CollectLoots script sebaiknya dipasang di GameObject dengan tag 'Player2'!");
        }

        // Cek apakah Inventory Instance ada
        if (Inventory.Instance == null)
        {
            Debug.LogError("Inventory Instance tidak ditemukan! Pastikan ada GameObject dengan script Inventory di scene!");
        }
    }

    private void Update()
    {
        CheckForNearbyLoots();
    }

    /// <summary>
    /// Cek loot yang ada di sekitar player dan ambil jika dalam jangkauan
    /// </summary>
    private void CheckForNearbyLoots()
    {
        // Cari semua GameObject dengan tag "Loot" di scene
        GameObject[] loots = GameObject.FindGameObjectsWithTag("Loot");

        foreach (GameObject lootObject in loots)
        {
            if (lootObject == null) continue;

            float distance = Vector3.Distance(playerTransform.position, lootObject.transform.position);

            // Jika loot dalam jangkauan, ambil
            if (distance <= collectionRange)
            {
                CollectLoot(lootObject);
            }
        }
    }

    /// <summary>
    /// Mengumpulkan loot dan menambahkannya ke inventory
    /// </summary>
    /// <param name="lootObject">GameObject loot yang akan diambil</param>
    private void CollectLoot(GameObject lootObject)
    {
        // Ambil komponen Loot dari object
        Loot loot = lootObject.GetComponent<Loot>();

        if (loot != null)
        {
            // Tambahkan ke inventory
            if (Inventory.Instance != null)
            {
                Inventory.Instance.AddItem(loot.itemName, loot.quantity);
                Debug.Log($"Player 2 mengambil {loot.quantity}x {loot.itemName}");
            }

            // Play sound effect
            PlayPickupSound();

            // Spawn particle effect
            SpawnPickupEffect(lootObject.transform.position);
        }
        else
        {
            // Jika tidak ada komponen Loot, gunakan default
            if (Inventory.Instance != null)
            {
                Inventory.Instance.AddItem("Scrap", 1);
                Debug.Log("Player 2 mengambil 1x Scrap (default)");
            }

            PlayPickupSound();
            SpawnPickupEffect(lootObject.transform.position);
        }

        // Hancurkan loot object
        Destroy(lootObject);
    }

    /// <summary>
    /// Play sound effect saat mengambil loot
    /// </summary>
    private void PlayPickupSound()
    {
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
    }

    /// <summary>
    /// Spawn particle effect di posisi loot
    /// </summary>
    /// <param name="position">Posisi untuk spawn effect</param>
    private void SpawnPickupEffect(Vector3 position)
    {
        if (pickupEffect != null)
        {
            GameObject effect = Instantiate(pickupEffect, position, Quaternion.identity);
            // Auto destroy effect setelah 2 detik
            Destroy(effect, 2f);
        }
    }

    /// <summary>
    /// Alternatif: Collection menggunakan Trigger Collision
    /// Pastikan loot object memiliki Collider dengan "Is Trigger" enabled
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Loot"))
        {
            CollectLoot(other.gameObject);
        }
    }

    /// <summary>
    /// Untuk game 2D menggunakan 2D colliders
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Loot"))
        {
            CollectLoot(other.gameObject);
        }
    }

    /// <summary>
    /// Visualisasi collection range di Scene view
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (showDebugGizmos)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f); // Hijau transparan
            Gizmos.DrawSphere(transform.position, collectionRange);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, collectionRange);
        }
    }
}