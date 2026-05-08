using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class BuilderAmmoCraft : MonoBehaviour
{
    [Header("Crafting Settings")]
    public string resourceName = "Scrap"; // Ganti sesuai nama resource di Inventory-mu
    public int resourceCost = 3;
    public GameObject ammoPrefab; // Prefab peluru yang bisa diambil (Collectible)
    public float dropDistance = 1.5f;

    [Header("Animation & Sound")]
    [SerializeField] private Animator animator;
    [SerializeField] private AudioClip craftSound;
    private AudioSource audioSource;

    [Header("Cooldown")]
    public float craftCooldown = 1f;
    private float cooldownTimer = 0f;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (animator == null) animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }

    // Dipanggil melalui Player Input (Sama seperti Trap)
    public void OnCraftAmmo(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            TryCraftAmmo();
        }
    }

    public void TryCraftAmmo()
    {
        // 1. Cek Cooldown
        if (cooldownTimer > 0f) return;

        // 2. Cek Inventory (Mengacu pada sistem Inventory yang kamu pakai di script Trap)
        if (Inventory.Instance == null) return;

        int available = Inventory.Instance.GetItemCount(resourceName);
        if (available < resourceCost)
        {
            Debug.Log($"Resource tidak cukup! Butuh {resourceCost} {resourceName}");
            return;
        }

        // 3. Konsumsi Resource
        if (Inventory.Instance.RemoveItem(resourceName, resourceCost))
        {
            CraftAndDrop();
        }
    }

    private void CraftAndDrop()
    {
        // Jalankan Animasi Build (Sama seperti Trap)
        if (animator != null) animator.SetTrigger("Build");

        // Hitung posisi drop di depan Builder
        Vector3 dropPos = transform.position + transform.forward * dropDistance;
        dropPos.y += 0.5f; // Sedikit di atas tanah agar jatuh

        // Spawn Ammo
        GameObject ammo = Instantiate(ammoPrefab, dropPos, Quaternion.identity);

        // Beri efek sedikit dorongan agar tidak kaku (Opsional)
        Rigidbody rb = ammo.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(transform.forward * 2f + Vector3.up * 2f, ForceMode.Impulse);
        }

        // Suara
        if (craftSound != null) audioSource.PlayOneShot(craftSound);

        // Reset Cooldown
        cooldownTimer = craftCooldown;

        Debug.Log("Ammo Berhasil Dibuat!");
    }
}