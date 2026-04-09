using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public enum LadderBuildState
{
    Blueprint,      // Belum mulai build (warna hologram)
    Building,       // Sedang di-build (progress)
    Completed       // Sudah selesai (warna final, bisa climb)
}

[System.Serializable]
public class RuntimeLadderResourceRequirement
{
    public string resourceName;
    public int totalRequired;
    public int currentAmount;

    public RuntimeLadderResourceRequirement(string name, int required)
    {
        resourceName = name;
        totalRequired = required;
        currentAmount = 0;
    }
}

public class LadderBuildingSystem : MonoBehaviour
{
    [Header("Ladder Data")]
    [Tooltip("Drag LadderData ScriptableObject here")]
    public LadderData ladderData;

    [Header("Runtime References")]
    [Tooltip("Renderer ladder (auto-find jika kosong)")]
    public Renderer ladderRenderer;

    [Tooltip("Collider ladder (auto-find jika kosong)")]
    public Collider ladderCollider;

    [Header("UI (Optional)")]
    [Tooltip("UI prefab yang muncul saat player di radius")]
    public GameObject buildUI;

    [Header("Hologram Settings")]
    [SerializeField] private Color hologramColor = new Color(0f, 0.8f, 1f, 1f); // Cyan
    [SerializeField] private Color hologramValidColor = new Color(0f, 1f, 0f, 1f); // Hijau
    [SerializeField] private Color hologramInvalidColor = new Color(1f, 0f, 0f, 1f); // Merah
    [SerializeField] private float hologramPulseSpeed = 2f;
    [SerializeField] private float hologramMinAlpha = 0.15f;
    [SerializeField] private float hologramMaxAlpha = 0.55f;

    [Header("Debug")]
    [Tooltip("Show debug logs")]
    public bool showDebugLogs = true;

    // Runtime data
    private List<RuntimeLadderResourceRequirement> runtimeResources = new List<RuntimeLadderResourceRequirement>();
    public List<RuntimeLadderResourceRequirement> RuntimeResources => runtimeResources;
    private LadderBuildState currentState = LadderBuildState.Blueprint;
    public LadderBuildState CurrentState => currentState;

    public float buildProgress { get; private set; } = 0f;

    private bool isPlayerInRange = false;
    private bool isBuilding = false;

    // Transform player yang sedang berinteraksi
    private Transform activePlayer;

    // State properties
    public bool IsCompleted => currentState == LadderBuildState.Completed;
    public string LadderName => ladderData != null ? ladderData.ladderName : "Unknown Ladder";

    // Material caching untuk Hologram
    private Material originalMaterial;
    private Material hologramMaterial;

    private void Start()
    {
        if (ladderData == null)
        {
            Debug.LogError($"[Ladder] LadderData missing on {gameObject.name}!");
            return;
        }

        // Initialize runtime requirements
        foreach (var req in ladderData.requiredResources)
        {
            runtimeResources.Add(new RuntimeLadderResourceRequirement(req.resourceName, req.amount));
        }

        // Setup Renderer
        if (ladderRenderer == null) ladderRenderer = GetComponentInChildren<Renderer>();

        // Setup Hologram Materials
        if (ladderRenderer != null)
        {
            // Simpan material asli untuk dipakai saat sedang dibangun dan selesai
            originalMaterial = new Material(ladderRenderer.material);

            // Buat material hologram khusus (transparan)
            hologramMaterial = new Material(ladderRenderer.material);
            SetMaterialTransparent(hologramMaterial);
        }

        // Setup Collider
        if (ladderCollider == null)
        {
            ladderCollider = GetComponent<BoxCollider>();
            if (ladderCollider == null)
            {
                BoxCollider boxCol = gameObject.AddComponent<BoxCollider>();
                boxCol.center = ladderData.colliderCenter;
                boxCol.size = ladderData.colliderSize;
                boxCol.isTrigger = true;
                ladderCollider = boxCol;
                if (showDebugLogs) Debug.Log($"[Ladder] Auto-added BoxCollider to {gameObject.name}");
            }
        }
        else
        {
            ladderCollider.isTrigger = true;
        }

        UpdateState(LadderBuildState.Blueprint);
    }

    private void Update()
    {
        if (IsCompleted) return;

        CheckPlayerDistance();

        // Update warna hologram (pulse animation) jika dalam mode Blueprint
        if (currentState == LadderBuildState.Blueprint)
        {
            UpdateHologramVisual();
        }

        if (isBuilding && isPlayerInRange)
        {
            ProcessBuilding();
        }
    }

    public void AssignPlayer(Transform playerTransform)
    {
        activePlayer = playerTransform;
        if (showDebugLogs) Debug.Log($"[Ladder] Player assigned: {playerTransform.name}");
    }

    private void CheckPlayerDistance()
    {
        if (activePlayer == null) return;

        float distance = Vector3.Distance(transform.position, activePlayer.position);
        bool wasInRange = isPlayerInRange;
        isPlayerInRange = distance <= ladderData.buildRange;

        if (isPlayerInRange && !wasInRange)
        {
            if (showDebugLogs) Debug.Log($"[Ladder] Player entered build range: {gameObject.name}");
            if (buildUI != null && !IsCompleted) buildUI.SetActive(true);
        }
        else if (!isPlayerInRange && wasInRange)
        {
            if (showDebugLogs) Debug.Log($"[Ladder] Player left build range: {gameObject.name}");
            if (buildUI != null) buildUI.SetActive(false);
            isBuilding = false; // Batal build kalau ngejauh
        }
    }

    public void OnBuild(InputAction.CallbackContext context)
    {
        if (IsCompleted || !isPlayerInRange) return;

        if (context.performed)
        {
            if (showDebugLogs) Debug.Log("[Ladder] Build button PRESSED");
            isBuilding = true;
        }
        else if (context.canceled)
        {
            if (showDebugLogs) Debug.Log("[Ladder] Build button RELEASED");
            isBuilding = false;
        }
    }

    private void ProcessBuilding()
    {
        if (currentState == LadderBuildState.Blueprint)
        {
            if (TryConsumeResources())
            {
                if (showDebugLogs) Debug.Log("[Ladder] Blueprint started building!");
                UpdateState(LadderBuildState.Building);
            }
            else
            {
                isBuilding = false; // Stop mencoba build
            }
        }
        else if (currentState == LadderBuildState.Building)
        {
            buildProgress += ladderData.buildSpeed * Time.deltaTime / 100f; // Scale to 0-1
            buildProgress = Mathf.Clamp01(buildProgress);

            if (buildProgress >= 1f)
            {
                if (showDebugLogs) Debug.Log($"[Ladder] {ladderData.ladderName} COMPLETED!");
                UpdateState(LadderBuildState.Completed);
                isBuilding = false;
            }
        }
    }

    private bool TryConsumeResources()
    {
        if (Inventory.Instance == null)
        {
            Debug.LogError("[Ladder] No Inventory found in scene!");
            return false;
        }

        // 1. Cek dulu semua cukup nggak
        foreach (var req in runtimeResources)
        {
            if (Inventory.Instance.GetItemCount(req.resourceName) < req.totalRequired)
            {
                if (showDebugLogs) Debug.Log($"[Ladder] Not enough {req.resourceName}. Need {req.totalRequired}");
                return false;
            }
        }

        // 2. Kalau cukup semua, baru consume (kurangi dari inventory)
        foreach (var req in runtimeResources)
        {
            Inventory.Instance.RemoveItem(req.resourceName, req.totalRequired);
            req.currentAmount = req.totalRequired; // Catat bahwa resource sudah terpenuhi
        }

        return true;
    }

    private void UpdateState(LadderBuildState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case LadderBuildState.Blueprint:
                if (ladderCollider != null) ladderCollider.enabled = false;
                if (ladderRenderer != null && hologramMaterial != null)
                {
                    ladderRenderer.material = hologramMaterial;
                    UpdateHologramVisual(); // Set warna awal
                }
                if (ladderCollider != null) ladderCollider.tag = "Untagged";
                break;

            case LadderBuildState.Building:
                if (ladderCollider != null) ladderCollider.enabled = false;
                if (ladderRenderer != null && originalMaterial != null)
                {
                    ladderRenderer.material = originalMaterial;
                    SetMaterialOpaque(ladderRenderer.material);
                    ladderRenderer.material.color = ladderData.buildingColor; // Warna solid dari LadderData
                }
                break;

            case LadderBuildState.Completed:
                if (ladderCollider != null)
                {
                    ladderCollider.enabled = true;
                    ladderCollider.tag = ladderData.ladderTag;
                }
                if (ladderRenderer != null && originalMaterial != null)
                {
                    ladderRenderer.material = originalMaterial;
                    SetMaterialOpaque(ladderRenderer.material);
                    ladderRenderer.material.color = ladderData.completedColor; // Warna final dari LadderData
                }

                // Hide UI
                if (buildUI != null) buildUI.SetActive(false);
                break;
        }
    }

    // --- BAGIAN LOGIKA HOLOGRAM ---

    private void UpdateHologramVisual()
    {
        if (hologramMaterial == null || ladderRenderer == null) return;

        // Jika player jauh, warna cyan standar
        if (!isPlayerInRange)
        {
            SetHologramColor(hologramColor);
            return;
        }

        // Jika player dekat, cek isi tasnya
        bool hasAllResources = true;
        if (Inventory.Instance != null)
        {
            foreach (var req in runtimeResources)
            {
                if (Inventory.Instance.GetItemCount(req.resourceName) < req.totalRequired)
                {
                    hasAllResources = false;
                    break;
                }
            }
        }

        // Ganti warna sesuai ketersediaan resource
        if (hasAllResources)
        {
            SetHologramColor(hologramValidColor); // Hijau (Bisa dibangun)
        }
        else
        {
            SetHologramColor(hologramInvalidColor); // Merah (Resource kurang)
        }
    }

    private void SetHologramColor(Color baseColor)
    {
        if (hologramMaterial != null)
        {
            // Logika "Pulse" persis seperti di BridgeBuildingSystem
            float pulse = Mathf.Sin(Time.time * hologramPulseSpeed) * 0.5f + 0.5f;
            float alpha = Mathf.Lerp(hologramMinAlpha, hologramMaxAlpha, pulse);

            Color hColor = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            hologramMaterial.color = hColor;

            // Emission glow biar keliatan hologram
            hologramMaterial.EnableKeyword("_EMISSION");
            hologramMaterial.SetColor("_EmissionColor", new Color(baseColor.r, baseColor.g, baseColor.b) * (pulse * 1.5f));
        }
    }

    // Script ajaib dari Unity untuk mengubah Opaque jadi Transparent lewat kode
    private void SetMaterialTransparent(Material mat)
    {
        mat.SetFloat("_Mode", 3); // 3 = Transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }

    private void SetMaterialOpaque(Material mat)
    {
        mat.SetFloat("_Mode", 0); // 0 = Opaque
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        mat.SetInt("_ZWrite", 1);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = -1;
    }

    public string GetBuildInfo()
    {
        if (ladderData == null) return "No Ladder Data";

        if (IsCompleted)
        {
            return $"{LadderName}\nCOMPLETED - Can Climb!";
        }

        string info = $"{LadderName}\n";
        info += $"Progress: {Mathf.FloorToInt(buildProgress * 100)}%\n\n";

        info += "Required:\n";
        foreach (var req in runtimeResources)
        {
            info += $"• {req.resourceName}: {req.currentAmount}/{req.totalRequired}\n";
        }

        if (isPlayerInRange)
        {
            if (isBuilding)
            {
                info += "\n[Holding Build] Building...";
            }
            else
            {
                info += "\nHold [Build] to Build";
            }
        }

        return info;
    }

    private void OnDrawGizmosSelected()
    {
        if (ladderData == null) return;

        // Build range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, ladderData.buildRange);

        // Collider preview
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(ladderData.colliderCenter, ladderData.colliderSize);
    }
}