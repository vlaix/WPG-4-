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

    // --- DIKEMBALIKAN: Pengaturan Efek Hologram ---
    [Header("Hologram Settings")]
    [SerializeField] private Color hologramColor = new Color(0f, 0.8f, 1f, 1f);
    [SerializeField] private float hologramPulseSpeed = 2f;
    [SerializeField] private float hologramMinAlpha = 0.15f;
    [SerializeField] private float hologramMaxAlpha = 0.55f;

    private List<RuntimeResourceRequirement> runtimeResources = new List<RuntimeResourceRequirement>();
    private Transform playerTransform;
    private BridgeBuildState currentState = BridgeBuildState.Blueprint;
    private AudioSource audioSource;
    private GameObject uiInstance;

    private float currentHoldTimer = 0f;
    private bool isHolding = false;
    private bool resourcesConsumed = false;
    private bool isPlayerInRange = false;

    private Vector3 buildStartPosition;

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

    public void AssignPlayer(Transform player)
    {
        playerTransform = player;
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

    private void StartHolding()
    {
        if (currentState != BridgeBuildState.Blueprint || !isPlayerInRange) return;

        if (HasResources())
        {
            isHolding = true;
            currentState = BridgeBuildState.Building;

            // Simpan posisi kaki player saat ini
            buildStartPosition = playerTransform.position;

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

        // Jika player bergeser lebih dari 0.1 unit dari titik awal, batalkan build
        if (Vector3.Distance(playerTransform.position, buildStartPosition) > 0.1f)
        {
            if (showDebugLogs) Debug.Log("Build batal karena player bergerak dari tempat!");
            StopHolding();
            return;
        }

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
        resourcesConsumed = false;

        if (audioSource != null)
        {
            audioSource.Stop();
            if (bridgeData.completeSound != null) audioSource.PlayOneShot(bridgeData.completeSound);
        }

        if (bridgeCollider != null) bridgeCollider.enabled = true;

        HideUI();
    }

    private void RefundResources()
    {
        if (Inventory.Instance == null) return;
        foreach (var req in runtimeResources) Inventory.Instance.AddItem(req.resourceName, req.totalRequired);
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
        foreach (var req in runtimeResources) Inventory.Instance.RemoveItem(req.resourceName, req.totalRequired);
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

        // --- DIKEMBALIKAN: Memanggil fungsi SetBridgeColor agar transparansi nyala ---
        if (bridgeRenderer != null) SetBridgeColor(bridgeData.blueprintColor);
    }

    private void SetupAudio()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void CheckPlayerProximity()
    {
        if (IsCompleted) return;

        if (playerTransform == null) return;
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        isPlayerInRange = dist <= bridgeData.buildRange;

        if ((isPlayerInRange || isHolding) && uiInstance == null) ShowUI();
        else if (!isPlayerInRange && !isHolding && uiInstance != null) HideUI();
    }

    // --- DIKEMBALIKAN: Fungsi UpdateVisual dan Hologram secara lengkap ---
    private void UpdateVisual()
    {
        if (bridgeRenderer == null) return;

        switch (currentState)
        {
            case BridgeBuildState.Blueprint:
                ApplyHologramEffect();
                break;
            case BridgeBuildState.Building:
                Color buildColor = Color.Lerp(bridgeData.blueprintColor, bridgeData.buildingColor, BuildProgress);
                SetBridgeColor(buildColor);
                break;
            case BridgeBuildState.Completed:
                SetBridgeColor(bridgeData.completedColor);
                break;
        }
    }

    private void ApplyHologramEffect()
    {
        if (bridgeRenderer == null) return;

        float pulse = Mathf.Sin(Time.time * hologramPulseSpeed) * 0.5f + 0.5f;
        float alpha = Mathf.Lerp(hologramMinAlpha, hologramMaxAlpha, pulse);
        Color hColor = new Color(hologramColor.r, hologramColor.g, hologramColor.b, alpha);

        Material mat = bridgeRenderer.material;
        mat.color = hColor;
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(hologramColor.r, hologramColor.g, hologramColor.b) * (pulse * 1.5f));
    }

    private void SetBridgeColor(Color color)
    {
        if (bridgeRenderer != null)
        {
            bridgeRenderer.material.color = color;
            if (color.a < 1f)
            {
                bridgeRenderer.material.SetFloat("_Mode", 3);
                bridgeRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                bridgeRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                bridgeRenderer.material.SetInt("_ZWrite", 0);
                bridgeRenderer.material.DisableKeyword("_ALPHATEST_ON");
                bridgeRenderer.material.EnableKeyword("_ALPHABLEND_ON");
                bridgeRenderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                bridgeRenderer.material.renderQueue = 3000;
            }
            else
            {
                bridgeRenderer.material.SetFloat("_Mode", 0);
                bridgeRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                bridgeRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                bridgeRenderer.material.SetInt("_ZWrite", 1);
                bridgeRenderer.material.DisableKeyword("_ALPHATEST_ON");
                bridgeRenderer.material.DisableKeyword("_ALPHABLEND_ON");
                bridgeRenderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                bridgeRenderer.material.renderQueue = -1;
            }
        }
    }

    private void ShowUI()
    {
        if (buildUI == null) return;
        uiInstance = Instantiate(buildUI, transform.position + Vector3.up * 4f, Quaternion.identity, transform);
        var uiScript = uiInstance.GetComponent<BridgeBuildUI>();
        if (uiScript != null) uiScript.SetBridge(this);
    }

    private void HideUI() { if (uiInstance != null) Destroy(uiInstance); }
}