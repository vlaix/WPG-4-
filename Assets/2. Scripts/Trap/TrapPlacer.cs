using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class TrapPlacer : MonoBehaviour
{
    [Header("Trap Data")]
    [Tooltip("Drag TrapData ScriptableObject here")]
    public TrapData trapData;

    [Header("Placement Settings")]
    [Tooltip("Offset posisi trap dari player (forward)")]
    public float placementDistance = 2f;

    [Header("Debug")]
    public bool showDebugLogs = true;

    // Private variables
    private PlayerInput playerControls;
    private float placementCooldownTimer = 0f;
    private List<GameObject> activeTraps = new List<GameObject>();
    private AudioSource audioSource;

    // Properties
    public bool CanPlaceTrap => placementCooldownTimer <= 0f;
    public int ActiveTrapCount => activeTraps.Count;

    private void Awake()
    {
        playerControls = new PlayerInput();
    }

    private void OnEnable()
    {
        playerControls.Player2Movement.Enable();
        playerControls.Player2Movement.PlaceTrap.performed += OnPlaceTrapPressed;
    }

    private void OnDisable()
    {
        playerControls.Player2Movement.PlaceTrap.performed -= OnPlaceTrapPressed;
        playerControls.Player2Movement.Disable();
    }

    private void Start()
    {
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (trapData == null)
        {
            Debug.LogWarning("?? TrapData is not assigned! Assign a TrapData ScriptableObject.");
        }
    }

    private void Update()
    {
        // Update cooldown timer
        if (placementCooldownTimer > 0f)
        {
            placementCooldownTimer -= Time.deltaTime;
        }

        // Clean up destroyed traps from list
        activeTraps.RemoveAll(trap => trap == null);
    }

    private void OnPlaceTrapPressed(InputAction.CallbackContext context)
    {
        TryPlaceTrap();
    }

    public void TryPlaceTrap()
    {
        if (trapData == null)
        {
            Debug.LogError("? TrapData is NULL!");
            return;
        }

        // Check cooldown
        if (!CanPlaceTrap)
        {
            if (showDebugLogs)
            {
                Debug.Log($"? Trap on cooldown! Wait {placementCooldownTimer:F1}s");
            }
            return;
        }

        // Check max active traps
        if (trapData.maxActiveTraps > 0 && ActiveTrapCount >= trapData.maxActiveTraps)
        {
            if (showDebugLogs)
            {
                Debug.Log($"?? Max traps reached! ({ActiveTrapCount}/{trapData.maxActiveTraps})");
            }
            return;
        }

        // Check resource cost
        if (Inventory.Instance == null)
        {
            Debug.LogError("? Inventory not found!");
            return;
        }

        int available = Inventory.Instance.GetItemCount(trapData.requiredResource);
        if (available < trapData.resourceCost)
        {
            if (showDebugLogs)
            {
                Debug.Log($"? Not enough {trapData.requiredResource}! Need {trapData.resourceCost}, have {available}");
            }
            return;
        }

        // Consume resources
        if (!Inventory.Instance.RemoveItem(trapData.requiredResource, trapData.resourceCost))
        {
            Debug.LogError("? Failed to remove resources from inventory!");
            return;
        }

        // Place trap
        PlaceTrap();
    }

    private void PlaceTrap()
    {
        // Calculate placement position (in front of player)
        Vector3 placementPos = transform.position + transform.forward * placementDistance;
        placementPos.y = transform.position.y; // Keep same Y level

        // Spawn trap
        GameObject trap = Instantiate(trapData.trapPrefab, placementPos, Quaternion.identity);

        // Set trap data
        BearTrap trapScript = trap.GetComponent<BearTrap>();
        if (trapScript != null)
        {
            trapScript.trapData = trapData;
        }

        // Add to active traps list
        activeTraps.Add(trap);

        // Start cooldown
        placementCooldownTimer = trapData.placementCooldown;

        // Play sound
        if (audioSource != null && trapData.triggerSound != null)
        {
            audioSource.PlayOneShot(trapData.triggerSound);
        }

        if (showDebugLogs)
        {
            Debug.Log($"?? Placed '{trapData.trapName}' trap! ({ActiveTrapCount}/{trapData.maxActiveTraps})");
        }
    }

    public string GetTrapInfo()
    {
        if (trapData == null) return "No Trap Data";

        string info = $"{trapData.trapName}\n";
        info += $"Cost: {trapData.resourceCost} {trapData.requiredResource}\n";
        info += $"Active: {ActiveTrapCount}";

        if (trapData.maxActiveTraps > 0)
        {
            info += $"/{trapData.maxActiveTraps}";
        }

        info += "\n";

        if (!CanPlaceTrap)
        {
            info += $"Cooldown: {placementCooldownTimer:F1}s";
        }
        else
        {
            info += "Ready to place!";
        }

        return info;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw placement position preview
        Vector3 placementPos = transform.position + transform.forward * placementDistance;
        Gizmos.color = CanPlaceTrap ? Color.green : Color.red;
        Gizmos.DrawWireSphere(placementPos, 0.5f);

        if (trapData != null)
        {
            Gizmos.DrawWireSphere(placementPos, trapData.triggerRadius);
        }
    }
}