using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShield : MonoBehaviour
{
    [Header("Shield Data")]
    [Tooltip("Drag ShieldData ScriptableObject here")]
    public ShieldData shieldData;

    [Header("Debug")]
    public bool showDebugLogs = true;

    // Private variables
    private PlayerInput playerControls;
    private GameObject shieldInstance;
    private Renderer shieldRenderer;
    private AudioSource audioSource;
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
    private void Awake()
    {
        playerControls = new PlayerInput();
    }

    private void OnEnable()
    {
        playerControls.Player2Movement.Enable();
        playerControls.Player2Movement.ActivateShield.performed += OnActivateShieldPressed;
    }

    private void OnDisable()
    {
        playerControls.Player2Movement.ActivateShield.performed -= OnActivateShieldPressed;
        playerControls.Player2Movement.Disable();
    }

    private void OnActivateShieldPressed(InputAction.CallbackContext context)
    {
        if (isShieldActive)
        {
            DeactivateShield();
        }
        else
        {
            TryActivateShield();
        }
    }
    // ---------------------------

    private void Start()
    {
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
            if (showDebugLogs)
            {
                Debug.Log($"? Shield on cooldown! Wait {cooldownTimer:F1}s");
            }
            return;
        }

        // Check if already active
        if (isShieldActive)
        {
            if (showDebugLogs)
            {
                Debug.Log("??? Shield already active!");
            }
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
            if (showDebugLogs)
            {
                Debug.Log($"? Not enough {shieldData.requiredResource}! Need {shieldData.resourceCost}, have {available}");
            }
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

        // Spawn shield visual
        if (shieldData.shieldPrefab != null)
        {
            shieldInstance = Instantiate(shieldData.shieldPrefab, transform);
            shieldInstance.transform.localPosition = shieldData.shieldOffset;
            shieldInstance.transform.localScale = shieldData.shieldScale;

            // Memanggil ShieldCollision dari objek yang di-spawn
            ShieldCollision shieldCol = shieldInstance.GetComponent<ShieldCollision>();
            if (shieldCol == null)
            {
                shieldCol = shieldInstance.AddComponent<ShieldCollision>();
            }
            shieldCol.parentShield = this;

            shieldRenderer = shieldInstance.GetComponentInChildren<Renderer>();
            if (shieldRenderer != null)
            {
                Material mat = shieldRenderer.material;
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
        else
        {
            Debug.LogError("? Shield Prefab is NULL! Assign a shield prefab in ShieldData.");
        }

        // Play activate sound
        if (audioSource != null && shieldData.activateSound != null)
        {
            audioSource.PlayOneShot(shieldData.activateSound);
        }

        if (showDebugLogs)
        {
            Debug.Log($"??? Shield activated! Health: {currentShieldHealth}");
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

        // Spawn hit effect
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
        if (showDebugLogs)
        {
            Debug.Log("?? Shield broken!");
        }

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

        // Destroy shield visual
        if (shieldInstance != null)
        {
            Destroy(shieldInstance);
            shieldInstance = null;
        }

        if (showDebugLogs)
        {
            Debug.Log("??? Shield deactivated");
        }
    }

    private void UpdateShieldVisual()
    {
        if (!isShieldActive || shieldRenderer == null || shieldData == null) return;

        // Lerp color based on health percentage
        float healthPercent = ShieldHealthPercent;
        Color targetColor = Color.Lerp(shieldData.lowHealthColor, shieldData.fullHealthColor, healthPercent);
        shieldRenderer.material.color = targetColor;
    }

    public string GetShieldInfo()
    {
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