using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public enum LadderBuildState
{
    Blueprint,      // Belum mulai build (warna wireframe)
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

    [Header("Debug")]
    [Tooltip("Show debug logs")]
    public bool showDebugLogs = true;

    // --- TAMBAHAN UNTUK HOLOGRAM ---
    [Header("Hologram Settings")]
    [SerializeField] private Color hologramColor = new Color(0f, 0.8f, 1f, 1f); // Cyan default
    [SerializeField] private float hologramPulseSpeed = 2f;
    [SerializeField] private float hologramMinAlpha = 0.15f;
    [SerializeField] private float hologramMaxAlpha = 0.55f;
    // -------------------------------

    // Runtime data
    private List<RuntimeLadderResourceRequirement> runtimeResources = new List<RuntimeLadderResourceRequirement>();
    private Animator playerAnimator;
    private Transform playerTransform;
    private LadderBuildState currentState = LadderBuildState.Blueprint;
    private AudioSource audioSource;
    private GameObject uiInstance;
    public float buildProgress = 0f;
    private float buildTimer = 0f;
    private bool isPlayerInRange = false;
    private bool isBuilding = false;
    private bool isBuildButtonPressed = false;

    // Properties
    public LadderBuildState State => currentState;
    public bool IsCompleted => currentState == LadderBuildState.Completed;
    public string LadderName => ladderData != null ? ladderData.ladderName : "Unknown Ladder";
    public List<RuntimeLadderResourceRequirement> RuntimeResources => runtimeResources;

    public void AssignPlayer(Transform player)
    {
        playerTransform = player;
        playerAnimator = player.GetComponent<Animator>();
        if (showDebugLogs) Debug.Log($" '{LadderName}' terhubung ke {player.name}");
    }

    public void OnBuild(InputAction.CallbackContext context)
    {
        if (playerTransform == null || IsCompleted) return;

        if (context.performed) isBuildButtonPressed = true;
        else if (context.canceled) isBuildButtonPressed = false;
    }

    private void Start()
    {
        ValidateLadderData();
        InitializeRuntimeResources();
        InitializeLadder();
        SetupAudio();
        // Jika playerTransform sudah di-assign lewat Inspector
        if (playerTransform != null)
            playerAnimator = playerTransform.GetComponent<Animator>();
    }

    private void Update()
    {
        CheckPlayerProximity();
        HandleBuildProcess();
        UpdateVisual();
    }



    private void ValidateLadderData()
    {
        if (ladderData == null)
        {
            Debug.LogError($"❌ LadderData is NULL! Please assign a LadderData ScriptableObject to {gameObject.name}");
            enabled = false;
        }
    }

    private void InitializeRuntimeResources()
    {
        if (ladderData == null) return;

        runtimeResources.Clear();

        foreach (var req in ladderData.requiredResources)
        {
            runtimeResources.Add(new RuntimeLadderResourceRequirement(req.resourceName, req.amount));
        }

        if (showDebugLogs)
        {
            Debug.Log($"🪜 '{LadderName}' requires:");
            foreach (var req in runtimeResources)
            {
                Debug.Log($"  • {req.resourceName}: {req.totalRequired}");
            }
        }
    }

    private void InitializeLadder()
    {
        if (ladderData == null) return;

        // Auto-find renderer
        if (ladderRenderer == null)
        {
            ladderRenderer = GetComponentInChildren<Renderer>();
        }

        // Set initial blueprint color (TIDAK DIUBAH AGAR AMAN)
        if (ladderRenderer != null)
        {
            SetLadderColor(ladderData.blueprintColor);
        }

        // Auto-find or setup collider
        if (ladderCollider == null)
        {
            ladderCollider = GetComponent<BoxCollider>();

            if (ladderCollider == null)
            {
                // Create box collider dari data
                BoxCollider boxCol = gameObject.AddComponent<BoxCollider>();
                boxCol.size = ladderData.colliderSize;
                boxCol.center = ladderData.colliderCenter;
                boxCol.isTrigger = true; // PENTING: trigger untuk climbing!
                ladderCollider = boxCol;

                if (showDebugLogs)
                {
                    Debug.Log($"✅ Auto-created Box Collider for '{LadderName}'");
                }
            }
        }

        // Disable collider saat blueprint (cannot climb yet)
        if (ladderCollider != null)
        {
            ladderCollider.enabled = false;

            if (showDebugLogs)
            {
                Debug.Log($"🔧 '{LadderName}': Collider DISABLED - Cannot climb yet!");
            }
        }

        currentState = LadderBuildState.Blueprint;

        if (showDebugLogs)
        {
            Debug.Log($"🪜 '{LadderName}' initialized as Blueprint");
        }
    }

    private void SetupAudio()
    {
        if (ladderData == null) return;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (ladderData.buildingSound != null || ladderData.completeSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
    }

    private void FindPlayer()
    {
        if (ladderData == null) return;

        GameObject player = GameObject.FindGameObjectWithTag(ladderData.requiredPlayerTag);
        if (player != null)
        {
            playerTransform = player.transform;

            if (showDebugLogs)
            {
                Debug.Log($"✅ Found player with tag '{ladderData.requiredPlayerTag}' for '{LadderName}'");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ Player with tag '{ladderData.requiredPlayerTag}' not found for '{LadderName}'!");
        }
    }

    private void CheckPlayerProximity()
    {
        if (ladderData == null || playerTransform == null || IsCompleted)
        {
            isPlayerInRange = false;
            HideBuildUI();
            return;
        }

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        isPlayerInRange = distance <= ladderData.buildRange;

        if (isPlayerInRange) ShowBuildUI();
        else HideBuildUI();
    }

    private void HandleBuildProcess()
    {
        // Jika player menjauh atau tombol dilepas, stop building
        if (!isPlayerInRange || !isBuildButtonPressed || IsCompleted || playerTransform == null)
        {
            if (isBuilding) StopBuilding();
            return;
        }

        if (!isBuilding) StartBuilding();
        ProcessBuilding();
    }

    private void StartBuilding()
    {
        if (currentState == LadderBuildState.Blueprint) currentState = LadderBuildState.Building;
        isBuilding = true;

        // Trigger build animation pada player
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("Build");
        }

        if (audioSource != null && ladderData.buildingSound != null)
        {
            audioSource.clip = ladderData.buildingSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        if (showDebugLogs)
        {
            Debug.Log($"🔨 Started building '{LadderName}'");
        }
    }

    private void StopBuilding()
    {
        isBuilding = false;
        isBuildButtonPressed = false; // Reset state tombol
        if (audioSource != null) audioSource.Stop();

        if (showDebugLogs)
        {
            Debug.Log($"⏸️ Stopped building '{LadderName}'");
        }
    }

    private void ProcessBuilding()
    {
        if (Inventory.Instance == null) return;

        float resourcesThisFrame = ladderData.buildSpeed * Time.deltaTime;
        bool anyResourceConsumed = false;

        foreach (var req in runtimeResources)
        {
            if (req.currentAmount >= req.totalRequired) continue;

            int available = Inventory.Instance.GetItemCount(req.resourceName);
            if (available > 0)
            {
                float needed = req.totalRequired - req.currentAmount;
                int toConsume = Mathf.CeilToInt(Mathf.Min(resourcesThisFrame, needed));

                if (Inventory.Instance.RemoveItem(req.resourceName, toConsume))
                {
                    req.currentAmount += toConsume;
                    anyResourceConsumed = true;
                }
            }
        }

        UpdateBuildProgress();

        if (buildProgress >= 1f) CompleteLadder();
        else if (!anyResourceConsumed) StopBuilding();
    }

    private void UpdateBuildProgress()
    {
        int totalRequired = 0, totalCurrent = 0;
        foreach (var req in runtimeResources)
        {
            totalRequired += req.totalRequired;
            totalCurrent += req.currentAmount;
        }
        buildProgress = totalRequired > 0 ? (float)totalCurrent / totalRequired : 0f;
    }

    private void UpdateVisual()
    {
        if (ladderData == null || ladderRenderer == null) return;

        switch (currentState)
        {
            case LadderBuildState.Blueprint:
                // KUNCI PERUBAHAN: Memanggil Hologram Effect
                ApplyHologramEffect();
                break;

            case LadderBuildState.Building:
                // Lerp color based on progress
                Color buildColor = Color.Lerp(ladderData.blueprintColor, ladderData.buildingColor, buildProgress);
                SetLadderColor(buildColor);
                break;

            case LadderBuildState.Completed:
                SetLadderColor(ladderData.completedColor);
                break;
        }
    }

    // --- FUNGSI BARU KHUSUS VISUAL HOLOGRAM ---
    private void ApplyHologramEffect()
    {
        if (ladderRenderer == null) return;

        float pulse = Mathf.Sin(Time.time * hologramPulseSpeed) * 0.5f + 0.5f;
        float alpha = Mathf.Lerp(hologramMinAlpha, hologramMaxAlpha, pulse);

        Color hColor = new Color(hologramColor.r, hologramColor.g, hologramColor.b, alpha);

        Material mat = ladderRenderer.material;
        mat.color = hColor;

        // Set transparent mode
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;

        // Emission glow biar kelihatan hologram
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(hologramColor.r, hologramColor.g, hologramColor.b) * (pulse * 1.5f));
    }
    // ------------------------------------------

    private void SetLadderColor(Color color)
    {
        if (ladderRenderer != null)
        {
            // Set color di material
            ladderRenderer.material.color = color;

            // Enable/disable transparency based on alpha
            if (color.a < 1f)
            {
                // Transparent mode
                ladderRenderer.material.SetFloat("_Mode", 3);
                ladderRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                ladderRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                ladderRenderer.material.SetInt("_ZWrite", 0);
                ladderRenderer.material.DisableKeyword("_ALPHATEST_ON");
                ladderRenderer.material.EnableKeyword("_ALPHABLEND_ON");
                ladderRenderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                ladderRenderer.material.renderQueue = 3000;
            }
            else
            {
                // Opaque mode
                ladderRenderer.material.SetFloat("_Mode", 0);
                ladderRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                ladderRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                ladderRenderer.material.SetInt("_ZWrite", 1);
                ladderRenderer.material.DisableKeyword("_ALPHATEST_ON");
                ladderRenderer.material.DisableKeyword("_ALPHABLEND_ON");
                ladderRenderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                ladderRenderer.material.renderQueue = -1;
            }
        }
    }

    private void CompleteLadder()
    {
        currentState = LadderBuildState.Completed;
        buildProgress = 1f;
        isBuilding = false;

        if (audioSource != null)
        {
            audioSource.Stop();
            if (ladderData.completeSound != null) audioSource.PlayOneShot(ladderData.completeSound);
        }

        if (ladderCollider != null) ladderCollider.enabled = true;
        if (!string.IsNullOrEmpty(ladderData.ladderTag)) gameObject.tag = ladderData.ladderTag;

        HideBuildUI();
    }

    private void ShowBuildUI()
    {
        if (buildUI != null && uiInstance == null)
        {
            // 1. Tambahkan tinggi ubah nilai f nya
            Vector3 spawnPos = transform.position + (Vector3.up * 4.5f);

            // 2. Tarik sedikit UI-nya ke arah luar/kamera agar tidak tenggelam di dalam mesh tangga
            if (Camera.main != null)
            {
                Vector3 arahKeKamera = (Camera.main.transform.position - spawnPos).normalized;
                // Tarik sejauh f
                spawnPos += arahKeKamera * 1f;
            }

            uiInstance = Instantiate(buildUI, spawnPos, Quaternion.identity);
            uiInstance.transform.SetParent(transform);

            // Pass reference ke UI script jika ada
            var uiScript = uiInstance.GetComponent<LadderBuildUI>();
            if (uiScript != null)
            {
                uiScript.SetLadder(this);
            }
        }
    }

    private void HideBuildUI()
    {
        if (uiInstance != null)
        {
            Destroy(uiInstance);
            uiInstance = null;
        }
    }

    public string GetInfoText()
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

        // Progress indicator
        if (Application.isPlaying && buildProgress > 0f && buildProgress < 1f)
        {
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f * buildProgress);
        }
    }
}