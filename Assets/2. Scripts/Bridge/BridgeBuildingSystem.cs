using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public enum BridgeBuildState
{
    Blueprint,
    Building,
    Completed
}

[System.Serializable]
public class RuntimeResourceRequirement
{
    public string resourceName;
    public int totalRequired;
    public int currentAmount;

    public RuntimeResourceRequirement(string name, int required)
    {
        resourceName = name;
        totalRequired = required;
        currentAmount = 0;
    }
}

public class BridgeBuildingSystem : MonoBehaviour
{
    [Header("Bridge Data")]
    public BridgeData bridgeData;

    [Header("Runtime References")]
    public Renderer bridgeRenderer;
    public Collider bridgeCollider;

    [Header("UI (Optional)")]
    public GameObject buildUI;

    [Header("Hold Settings")]
    [Tooltip("Waktu yang dibutuhkan untuk menahan build (detik)")]
    public float holdDuration = 5f;

    [Header("Debug")]
    public bool showDebugLogs = true;

    private List<RuntimeResourceRequirement> runtimeResources = new List<RuntimeResourceRequirement>();
    private Transform playerTransform;
    private BridgeBuildState currentState = BridgeBuildState.Blueprint;
    private AudioSource audioSource;
    private GameObject uiInstance;

    private float currentHoldTimer = 0f;
    private bool isHolding = false;
    private bool resourcesConsumed = false;
    private bool isPlayerInRange = false; // <-- Ditambahkan kembali agar tidak error

    public BridgeBuildState State => currentState;
    public float BuildProgress => Mathf.Clamp01(currentHoldTimer / holdDuration);
    public bool IsCompleted => currentState == BridgeBuildState.Completed;
    public string BridgeName => bridgeData != null ? bridgeData.bridgeName : "Unknown Bridge";
    public List<RuntimeResourceRequirement> RuntimeResources => runtimeResources;

    private void Start()
    {
        InitializeBridge();
        InitializeResources();
        SetupAudio();
    }

    private void Update()
    {
        CheckPlayerProximity();
        HandleBuilding();
        UpdateVisual();
    }

    // --- FUNGSI UNTUK INTEGRASI DENGAN PLAYER SPAWNER ---
    public void AssignPlayer(Transform player)
    {
        playerTransform = player;
        if (showDebugLogs) Debug.Log($"Player berhasil di-assign ke jembatan: {BridgeName}");
    }

    public void OnBuild(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            StartHolding();
        }
        else if (context.canceled)
        {
            StopHolding();
        }
    }
    // -----------------------------------------------------------

    private void StartHolding()
    {
        if (currentState != BridgeBuildState.Blueprint || !isPlayerInRange) return;

        if (HasResources())
        {
            isHolding = true;
            currentState = BridgeBuildState.Building;

            // Tarik resource di awal agar tidak bisa curang
            ConsumeResources();
            resourcesConsumed = true;

            if (audioSource != null && bridgeData.buildingSound != null)
            {
                audioSource.clip = bridgeData.buildingSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
    }

    private void StopHolding()
    {
        if (IsCompleted) return;

        if (isHolding && currentHoldTimer < holdDuration)
        {
            CancelBuild();
        }

        isHolding = false;
    }

    private void CancelBuild()
    {
        if (showDebugLogs) Debug.Log("Build Dibatalkan! Mengembalikan Scrap...");

        if (resourcesConsumed)
        {
            RefundResources();
            resourcesConsumed = false;
        }

        currentHoldTimer = 0f;
        currentState = BridgeBuildState.Blueprint;

        if (audioSource != null) audioSource.Stop();
    }

    private void HandleBuilding()
    {
        if (!isHolding || IsCompleted) return;

        currentHoldTimer += Time.deltaTime;

        if (currentHoldTimer >= holdDuration)
        {
            CompleteBuild();
        }
    }

    private void CompleteBuild()
    {
        currentState = BridgeBuildState.Completed;
        isHolding = false;
        resourcesConsumed = false; // Sudah jadi bangunan, tidak perlu refund lagi

        if (audioSource != null)
        {
            audioSource.Stop();
            if (bridgeData.completeSound != null) audioSource.PlayOneShot(bridgeData.completeSound);
        }

        if (bridgeCollider != null) bridgeCollider.enabled = true;

        HideUI(); // <-- Menyembunyikan UI seketika saat jembatan selesai
        if (showDebugLogs) Debug.Log($"Jembatan {BridgeName} Selesai Dibangun!");
    }

    private void RefundResources()
    {
        if (Inventory.Instance == null) return;

        foreach (var req in runtimeResources)
        {
            Inventory.Instance.AddItem(req.resourceName, req.totalRequired);
        }
    }

    private bool HasResources()
    {
        if (Inventory.Instance == null) return false;
        foreach (var req in runtimeResources)
        {
            if (Inventory.Instance.GetItemCount(req.resourceName) < req.totalRequired) return false;
        }
        return true;
    }

    private void ConsumeResources()
    {
        if (Inventory.Instance == null) return;
        foreach (var req in runtimeResources)
        {
            Inventory.Instance.RemoveItem(req.resourceName, req.totalRequired);
        }
    }

    private void InitializeResources()
    {
        if (bridgeData == null) return;
        runtimeResources.Clear();
        foreach (var req in bridgeData.requiredResources)
            runtimeResources.Add(new RuntimeResourceRequirement(req.resourceName, req.amount));
    }

    private void InitializeBridge()
    {
        if (bridgeRenderer == null) bridgeRenderer = GetComponentInChildren<Renderer>();
        if (bridgeCollider == null) bridgeCollider = GetComponent<Collider>();
        if (bridgeCollider != null) bridgeCollider.enabled = false;
        if (bridgeRenderer != null) bridgeRenderer.material.color = bridgeData.blueprintColor;
    }

    private void SetupAudio()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void CheckPlayerProximity()
    {
        if (IsCompleted) return; // <-- Mencegah UI muncul lagi kalau jembatan sudah jadi

        if (playerTransform == null) return;
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        isPlayerInRange = dist <= bridgeData.buildRange;

        if ((isPlayerInRange || isHolding) && uiInstance == null) ShowUI();
        else if (!isPlayerInRange && !isHolding && uiInstance != null) HideUI();
    }

    private void UpdateVisual()
    {
        if (bridgeRenderer == null) return;
        if (currentState == BridgeBuildState.Building)
            bridgeRenderer.material.color = Color.Lerp(bridgeData.blueprintColor, bridgeData.buildingColor, BuildProgress);
        else if (IsCompleted)
            bridgeRenderer.material.color = bridgeData.completedColor;
    }

    private void ShowUI()
    {
        if (buildUI == null) return;
        uiInstance = Instantiate(buildUI, transform.position + Vector3.up * 4f, Quaternion.identity, transform);

        // Memanggil class BridgeBluepritUI sesuai nama file/class terbarumu
        var uiScript = uiInstance.GetComponent<BridgeBuildUI>();
        if (uiScript != null) uiScript.SetBridge(this);
    }

    private void HideUI() { if (uiInstance != null) Destroy(uiInstance); }
}