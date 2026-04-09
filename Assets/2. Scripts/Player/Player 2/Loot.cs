using UnityEngine;

public class Loot : MonoBehaviour
{
    [Header("Loot Settings")]
    [Tooltip("Name of this loot item")]
    public string itemName = "Coin";

    [Tooltip("How many of this item to give")]
    public int quantity = 1;

    [Tooltip("Collection range - how close player needs to be")]
    public float collectionRange = 2f;

    [Header("Optional Effects")]
    [Tooltip("Sound to play when collected")]
    public AudioClip collectionSound;

    [Tooltip("Volume suara (Bisa diatur 0 sampai 1)")]
    [Range(0f, 1f)]
    public float volume = 1f;

    [Tooltip("Particle effect to spawn when collected")]
    public GameObject collectionEffect;

    private Transform player1Transform;
    private Transform player2Transform;
    private bool isCollected = false;

    private void Start()
    {
        // Find both players by their tags
        GameObject player1 = GameObject.FindGameObjectWithTag("Player");
        GameObject player2 = GameObject.FindGameObjectWithTag("Player2");

        if (player1 != null)
        {
            player1Transform = player1.transform;
        }

        if (player2 != null)
        {
            player2Transform = player2.transform;
        }

        if (player1Transform == null && player2Transform == null)
        {
            Debug.LogWarning("No players found! Make sure one player has 'Player' tag and the other has 'Player2' tag.");
        }
    }

    private void Update()
    {
        if (isCollected) return;

        // Check distance to Player 2 (can collect)
        if (player2Transform != null)
        {
            float distance2 = Vector3.Distance(transform.position, player2Transform.position);
            if (distance2 <= collectionRange)
            {
                CollectLoot();
                return;
            }
        }

        // Only Player 2 can collect loot
    }

    private void CollectLoot()
    {
        isCollected = true;

        // Add to inventory
        if (Inventory.Instance != null)
        {
            Inventory.Instance.AddItem(itemName, quantity);
        }

        //zPlay sound effect menggunakan mode 2D
        if (collectionSound != null)
        {
            PlaySound2D(collectionSound);
        }

        // Spawn particle effect
        if (collectionEffect != null)
        {
            Instantiate(collectionEffect, transform.position, Quaternion.identity);
        }

        // Destroy the loot object
        Destroy(gameObject);
    }

    // Alternative: Manual collection via trigger collision
    // Only Player2 can collect
    private void OnTriggerEnter(Collider other)
    {
        if (!isCollected && other.CompareTag("Player2"))
        {
            CollectLoot();
        }
    }

    // For 2D games - Only Player2 can collect
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isCollected && other.CompareTag("Player2"))
        {
            CollectLoot();
        }
    }

    // Visualize collection range in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectionRange);
    }

    private void PlaySound2D(AudioClip clip)
    {
        if (clip == null) return;

        // Buat GameObject sementara khusus untuk menjadi speaker
        GameObject audioObj = new GameObject("TempAudio_" + clip.name);
        AudioSource source = audioObj.AddComponent<AudioSource>();

        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = 0f;
        source.Play();

        // Hancurkan speaker sementara ini otomatis setelah lagunya selesai
        Destroy(audioObj, clip.length);
    }
}