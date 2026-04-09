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
    private GameObject shieldInstanceP2; // Visual Shield untuk Player 2
    private GameObject shieldInstanceP1; // Visual Shield untuk Player 1
    private Renderer shieldRendererP2;
    private Renderer shieldRendererP1;
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
            if (showDebugLogs) Debug.Log("Shield sedang aktif! Tidak bisa dimatikan");
            return;
        }

        // Jika shield BELUM AKTIF, coba nyalakan
        TryActivateShield();
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void Update()
    {
        if (isShieldActive)
        {
            if (shieldData.duration > 0f)
            {
                shieldDurationTimer -= Time.deltaTime;
                if (shieldDurationTimer <= 0f)
                {
                    DeactivateShield();
                }
            }
            UpdateShieldVisual();
        }
        else
        {
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
            }
        }
    }

    private void TryActivateShield()
    {
        if (!CanActivateShield)
        {
            if (showDebugLogs) Debug.Log("Shield is on cooldown!");
            return;
        }

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
            shieldInstanceP2 = Instantiate(shieldData.shieldPrefab, transform.position, Quaternion.identity, transform);
            shieldRendererP2 = shieldInstanceP2.GetComponentInChildren<Renderer>();

            ShieldCollision colP2 = shieldInstanceP2.GetComponent<ShieldCollision>();
            if (colP2 == null) colP2 = shieldInstanceP2.AddComponent<ShieldCollision>();
            colP2.parentShield = this;
        }

        // 2. SPAWN SHIELD UNTUK PLAYER 1
        // Mencari Player 1 berdasarkan Tag "Player"
        GameObject player1 = GameObject.FindGameObjectWithTag("Player");
        if (player1 != null && shieldData.shieldPrefab != null)
        {
            shieldInstanceP1 = Instantiate(shieldData.shieldPrefab, player1.transform.position, Quaternion.identity, player1.transform);
            shieldRendererP1 = shieldInstanceP1.GetComponentInChildren<Renderer>();

            ShieldCollision colP1 = shieldInstanceP1.GetComponent<ShieldCollision>();
            if (colP1 == null) colP1 = shieldInstanceP1.AddComponent<ShieldCollision>();

            // KUNCI UTAMA: Shield P1 juga mengirim deteksi tembakan (Damage) ke script P2 ini!
            colP1.parentShield = this;
        }
        else
        {
            if (showDebugLogs) Debug.LogWarning("Player 1 tidak ditemukan! Pastikan Player 1 memiliki Tag 'Player'");
        }

        if (audioSource != null && shieldData.activateSound != null)
        {
            audioSource.PlayOneShot(shieldData.activateSound);
        }

        if (showDebugLogs) Debug.Log("Shield Activated for BOTH Players!");
    }

    public bool TakeDamage(float damage)
    {
        if (!isShieldActive) return false;

        currentShieldHealth -= damage;

        if (audioSource != null && shieldData.hitSound != null)
        {
            audioSource.PlayOneShot(shieldData.hitSound);
        }

        if (shieldData.hitEffect != null)
        {
            Instantiate(shieldData.hitEffect, transform.position, Quaternion.identity);
        }

        if (showDebugLogs) Debug.Log($"Shield took {damage} damage! HP: {currentShieldHealth}");

        if (currentShieldHealth <= 0)
        {
            BreakShield();
        }

        return true;
    }

    private void BreakShield()
    {
        if (audioSource != null && shieldData.breakSound != null)
        {
            audioSource.PlayOneShot(shieldData.breakSound);
        }

        if (shieldData.breakEffect != null)
        {
            Instantiate(shieldData.breakEffect, transform.position, Quaternion.identity);
        }

        DeactivateShield();
    }

    private void DeactivateShield()
    {
        isShieldActive = false;
        cooldownTimer = shieldData.cooldown;

        // Hancurkan visual Shield Player 2
        if (shieldInstanceP2 != null)
        {
            Destroy(shieldInstanceP2);
            shieldInstanceP2 = null;
        }

        // Hancurkan visual Shield Player 1
        if (shieldInstanceP1 != null)
        {
            Destroy(shieldInstanceP1);
            shieldInstanceP1 = null;
        }

        if (showDebugLogs) Debug.Log("Shield Deactivated");
    }

    private void UpdateShieldVisual()
    {
        if (!isShieldActive || shieldData == null) return;

        float healthPercent = ShieldHealthPercent;
        Color targetColor = Color.Lerp(shieldData.lowHealthColor, shieldData.fullHealthColor, healthPercent);

        // Update warna kedua shield secara bersamaan agar terlihat sinkron
        if (shieldRendererP2 != null) shieldRendererP2.material.color = targetColor;
        if (shieldRendererP1 != null) shieldRendererP1.material.color = targetColor;
    }
}