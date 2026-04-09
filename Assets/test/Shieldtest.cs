using UnityEngine;
using UnityEngine.InputSystem;

public class Shieldtest : MonoBehaviour
{
    [Header("Shield Data")]
    [Tooltip("Drag ShieldData ScriptableObject here")]
    public ShieldData shieldData;

    [Header("Debug")]
    public bool showDebugLogs = true;

    // Private variables
    private GameObject shieldInstance; // Shield untuk Player 2 (Dirinya sendiri)
    private GameObject shieldInstanceP1; // Shield untuk Player 1
    private Renderer shieldRenderer;
    private Renderer shieldRendererP1;

    private AudioSource audioSource;
    private Animator animator;
    private float currentShieldHealth;
    private float shieldDurationTimer;
    private float cooldownTimer = 0f;
    private bool isShieldActive = false;

    // Properties
    public bool IsShieldActive => isShieldActive;
    public bool CanActivateShield => cooldownTimer <= 0f;
    public float CurrentShieldHealth => currentShieldHealth;
    public float ShieldHealthPercent => shieldData != null ? currentShieldHealth / shieldData.shieldHealth : 0f;

    // --- BAGIAN INPUT SYSTEM ---

    public void OnActivateShield(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (isShieldActive)
            {
                if (showDebugLogs) Debug.Log("Shield sedang aktif! Tidak bisa dimatikan");
                return;
            }

            // Jika shield BELUM AKTIF, coba nyalakan
            TryActivateShield();
        }
    }

    private void Start()
    {
        // Setup animator
        animator = GetComponent<Animator>();

        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (shieldData == null)
        {
            Debug.LogWarning("?? ShieldData is not assigned! Assign a ShieldData ScriptableObject.");
        }
    }

    private void Update()
    {
        // Update cooldown
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        // Update shield duration
        if (isShieldActive && shieldData != null && shieldData.duration > 0f)
        {
            shieldDurationTimer -= Time.deltaTime;
            if (shieldDurationTimer <= 0f)
            {
                DeactivateShield();
            }
        }

        // Update shield visual color based on health
        UpdateShieldVisual();
    }

    public void TryActivateShield()
    {
        if (shieldData == null)
        {
            Debug.LogError("? ShieldData is NULL!");
            return;
        }

        // Check cooldown
        if (!CanActivateShield)
        {
            if (showDebugLogs) Debug.Log($"? Shield on cooldown! Wait {cooldownTimer:F1}s");
            return;
        }

        // Check if already active
        if (isShieldActive)
        {
            if (showDebugLogs) Debug.Log("??? Shield already active!");
            return;
        }

        // Check resource cost
        if (Inventory.Instance == null)
        {
            Debug.LogError("? Inventory not found!");
            return;
        }

        int available = Inventory.Instance.GetItemCount(shieldData.requiredResource);
        if (available < shieldData.resourceCost)
        {
            if (showDebugLogs) Debug.Log($"? Not enough {shieldData.requiredResource}! Need {shieldData.resourceCost}, have {available}");
            return;
        }

        // Consume resources
        if (!Inventory.Instance.RemoveItem(shieldData.requiredResource, shieldData.resourceCost))
        {
            Debug.LogError("? Failed to remove resources from inventory!");
            return;
        }

        // Activate shield
        ActivateShield();
    }

    private void ActivateShield()
    {
        isShieldActive = true;
        currentShieldHealth = shieldData.shieldHealth;
        shieldDurationTimer = shieldData.duration;

        // 1. SPAWN SHIELD UNTUK PLAYER 2 (Dirinya Sendiri)
        if (shieldData.shieldPrefab != null)
        {
            shieldInstance = Instantiate(shieldData.shieldPrefab, transform);
            SetupShieldObject(shieldInstance, out shieldRenderer);
        }
        else
        {
            Debug.LogError("? Shield Prefab is NULL! Assign a shield prefab in ShieldData.");
        }

        // 2. SPAWN SHIELD UNTUK PLAYER 1
        GameObject player1 = GameObject.FindGameObjectWithTag("Player");
        if (player1 != null && shieldData.shieldPrefab != null)
        {
            shieldInstanceP1 = Instantiate(shieldData.shieldPrefab, player1.transform);
            SetupShieldObject(shieldInstanceP1, out shieldRendererP1);
        }
        else
        {
            if (showDebugLogs) Debug.LogWarning("Player 1 tidak ditemukan! Pastikan Player 1 memiliki Tag 'Player'");
        }

        // Trigger build animation
        if (animator != null)
        {
            animator.SetTrigger("Build");
        }

        // Play activate sound
        if (audioSource != null && shieldData.activateSound != null)
        {
            audioSource.PlayOneShot(shieldData.activateSound);
        }

        if (showDebugLogs)
        {
            Debug.Log($"??? Shield activated for BOTH Players! Health: {currentShieldHealth}");
        }
    }

    // Fungsi bantuan agar kode setup material & collision tidak ditulis dua kali
    private void SetupShieldObject(GameObject instance, out Renderer r)
    {
        instance.transform.localPosition = shieldData.shieldOffset;
        instance.transform.localScale = shieldData.shieldScale;

        ShieldCollideTest shieldCol = instance.GetComponent<ShieldCollideTest>();
        if (shieldCol == null) shieldCol = instance.AddComponent<ShieldCollideTest>();
        shieldCol.parentShield = this; // Hubungkan ke script ini agar HP-nya di-share

        r = instance.GetComponentInChildren<Renderer>();
        if (r != null)
        {
            Material mat = r.material;
            mat.color = shieldData.fullHealthColor;

            // Set render mode ke transparent secara otomatis
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }
    }

    public bool TakeDamage(float damage)
    {
        if (!isShieldActive) return false;

        currentShieldHealth -= damage;

        // Play hit sound
        if (audioSource != null && shieldData.hitSound != null)
        {
            audioSource.PlayOneShot(shieldData.hitSound);
        }

        // Spawn hit effect di posisi Player 2 (bisa disesuaikan jika ingin di titik tembak)
        if (shieldData.hitEffect != null && shieldInstance != null)
        {
            Instantiate(shieldData.hitEffect, shieldInstance.transform.position, Quaternion.identity);
        }

        if (showDebugLogs)
        {
            Debug.Log($"??? Shield blocked {damage} damage! Remaining: {currentShieldHealth}/{shieldData.shieldHealth}");
        }

        // Check if shield broken
        if (currentShieldHealth <= 0f)
        {
            BreakShield();
        }

        return true; // Damage blocked
    }

    private void BreakShield()
    {
        if (showDebugLogs) Debug.Log("?? Shield broken!");

        // Play break sound
        if (audioSource != null && shieldData.breakSound != null)
        {
            audioSource.PlayOneShot(shieldData.breakSound);
        }

        // Spawn break effect
        if (shieldData.breakEffect != null && shieldInstance != null)
        {
            Instantiate(shieldData.breakEffect, shieldInstance.transform.position, Quaternion.identity);
        }

        // Deactivate shield
        DeactivateShield();

        // Start cooldown
        cooldownTimer = shieldData.cooldown;
    }

    private void DeactivateShield()
    {
        isShieldActive = false;

        // Destroy shield visual Player 2
        if (shieldInstance != null)
        {
            Destroy(shieldInstance);
            shieldInstance = null;
        }

        // Destroy shield visual Player 1
        if (shieldInstanceP1 != null)
        {
            Destroy(shieldInstanceP1);
            shieldInstanceP1 = null;
        }

        if (showDebugLogs) Debug.Log("??? Shield deactivated for BOTH players");
    }

    private void UpdateShieldVisual()
    {
        if (!isShieldActive || shieldData == null) return;

        float healthPercent = ShieldHealthPercent;
        Color targetColor = Color.Lerp(shieldData.lowHealthColor, shieldData.fullHealthColor, healthPercent);

        if (shieldRenderer != null) shieldRenderer.material.color = targetColor;
        if (shieldRendererP1 != null) shieldRendererP1.material.color = targetColor;
    }

    public string GetShieldInfo()
    {
        // ... (Fungsi GetShieldInfo dibiarkan persis sama dengan aslinya)
        if (shieldData == null) return "No Shield Data";

        string info = $"{shieldData.shieldName}\n";

        if (isShieldActive)
        {
            info += $"Health: {currentShieldHealth:F0}/{shieldData.shieldHealth}\n";
            if (shieldData.duration > 0f)
            {
                info += $"Time: {shieldDurationTimer:F1}s\n";
            }
        }
        else if (!CanActivateShield)
        {
            info += $"Cooldown: {cooldownTimer:F1}s\n";
        }
        else
        {
            info += $"Cost: {shieldData.resourceCost} {shieldData.requiredResource}\n";
            info += "Ready!\n";
        }

        return info;
    }
}