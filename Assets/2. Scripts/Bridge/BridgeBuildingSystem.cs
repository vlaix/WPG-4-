using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public enum BridgeBuildState
{
    Blueprint,      // Belum mulai build (warna wireframe)
    Building,       // Sedang di-build (progress)
    Completed       // Sudah selesai (warna final)
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
    [Tooltip("Drag BridgeData ScriptableObject here")]
    public BridgeData bridgeData;

    [Header("Runtime References")]
    [Tooltip("Renderer jembatan (auto-find jika kosong)")]
    public Renderer bridgeRenderer;

    [Tooltip("Collider jembatan (auto-find jika kosong)")]
    public Collider bridgeCollider;

    [Header("UI (Optional)")]
    [Tooltip("UI prefab yang muncul saat player di radius")]
    public GameObject buildUI;

    [Header("Debug")]
    [Tooltip("Show debug logs")]
    public bool showDebugLogs = true;

    // Runtime data
    private List<RuntimeResourceRequirement> runtimeResources = new List<RuntimeResourceRequirement>();
    private PlayerInput playerControls;
    private Transform playerTransform;
    private BridgeBuildState currentState = BridgeBuildState.Blueprint;
    private AudioSource audioSource;
    private GameObject uiInstance;
    private float buildProgress = 0f;
    private float buildTimer = 0f;
    private bool isPlayerInRange = false;
    private bool isBuilding = false;
    private bool isBuildButtonPressed = false;

    // Properties
    public BridgeBuildState State => currentState;
    public float BuildProgress => buildProgress;
    public bool IsCompleted => currentState == BridgeBuildState.Completed;
    public string BridgeName => bridgeData != null ? bridgeData.bridgeName : "Unknown Bridge";
    public List<RuntimeResourceRequirement> RuntimeResources => runtimeResources;

    private void Awake()
    {
        // Initialize input system
        playerControls = new PlayerInput();
    }

    private void OnEnable()
    {
        // Enable Player2Movement action map
        playerControls.Player2Movement.Enable();

        // Subscribe to Build action
        playerControls.Player2Movement.Build.performed += OnBuildPressed;
        playerControls.Player2Movement.Build.canceled += OnBuildReleased;
    }

    private void OnDisable()
    {
        // Unsubscribe from Build action
        playerControls.Player2Movement.Build.performed -= OnBuildPressed;
        playerControls.Player2Movement.Build.canceled -= OnBuildReleased;

        // Disable Player2Movement action map
        playerControls.Player2Movement.Disable();
    }

    private void Start()
    {
        ValidateBridgeData();
        InitializeRuntimeResources();
        InitializeBridge();
        SetupAudio();
        FindPlayer();
    }

    private void Update()
    {
        CheckPlayerProximity();
        HandleBuildInput();
        UpdateVisual();
    }

    private void OnBuildPressed(InputAction.CallbackContext context)
    {
        isBuildButtonPressed = true;
    }

    private void OnBuildReleased(InputAction.CallbackContext context)
    {
        isBuildButtonPressed = false;
    }

    private void ValidateBridgeData()
    {
        if (bridgeData == null)
        {
            Debug.LogError($"❌ BridgeData is NULL! Please assign a BridgeData ScriptableObject to {gameObject.name}");
            enabled = false;
        }
    }

    private void InitializeRuntimeResources()
    {
        if (bridgeData == null) return;

        runtimeResources.Clear();

        foreach (var req in bridgeData.requiredResources)
        {
            runtimeResources.Add(new RuntimeResourceRequirement(req.resourceName, req.amount));
        }

        if (showDebugLogs)
        {
            Debug.Log($"📋 '{BridgeName}' requires:");
            foreach (var req in runtimeResources)
            {
                Debug.Log($"  • {req.resourceName}: {req.totalRequired}");
            }
        }
    }

    private void InitializeBridge()
    {
        if (bridgeData == null) return;

        // Auto-find renderer
        if (bridgeRenderer == null)
        {
            bridgeRenderer = GetComponentInChildren<Renderer>();
        }

        // Set initial blueprint color
        if (bridgeRenderer != null)
        {
            SetBridgeColor(bridgeData.blueprintColor);
        }

        // Auto-find or setup collider
        if (bridgeCollider == null)
        {
            bridgeCollider = GetComponent<BoxCollider>();

            if (bridgeCollider == null)
            {
                // Create box collider dari data
                BoxCollider boxCol = gameObject.AddComponent<BoxCollider>();
                boxCol.size = bridgeData.colliderSize;
                boxCol.center = bridgeData.colliderCenter;
                bridgeCollider = boxCol;

                if (showDebugLogs)
                {
                    Debug.Log($"✅ Auto-created Box Collider for '{BridgeName}'");
                }
            }
        }

        // Disable collider saat blueprint
        if (bridgeCollider != null)
        {
            bridgeCollider.enabled = false;

            if (showDebugLogs)
            {
                Debug.Log($"🔧 '{BridgeName}': Collider DISABLED - Player akan jatuh!");
            }
        }

        currentState = BridgeBuildState.Blueprint;

        if (showDebugLogs)
        {
            Debug.Log($"🏗️ '{BridgeName}' initialized as Blueprint");
        }
    }

    private void SetupAudio()
    {
        if (bridgeData == null) return;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (bridgeData.buildingSound != null || bridgeData.completeSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
    }

    private void FindPlayer()
    {
        if (bridgeData == null) return;

        GameObject player = GameObject.FindGameObjectWithTag(bridgeData.requiredPlayerTag);
        if (player != null)
        {
            playerTransform = player.transform;

            if (showDebugLogs)
            {
                Debug.Log($"✅ Found player with tag '{bridgeData.requiredPlayerTag}' for '{BridgeName}'");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ Player with tag '{bridgeData.requiredPlayerTag}' not found for '{BridgeName}'!");
        }
    }

    private void CheckPlayerProximity()
    {
        if (bridgeData == null || playerTransform == null || IsCompleted) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        isPlayerInRange = distance <= bridgeData.buildRange;

        // Show/hide UI
        if (isPlayerInRange && !IsCompleted)
        {
            ShowBuildUI();
        }
        else
        {
            HideBuildUI();
        }
    }

    private void HandleBuildInput()
    {
        if (bridgeData == null || !isPlayerInRange || IsCompleted || playerTransform == null)
        {
            if (isBuilding)
            {
                StopBuilding();
            }
            return;
        }

        // HOLD Build button untuk build
        if (isBuildButtonPressed)
        {
            if (!isBuilding)
            {
                StartBuilding();
            }

            ProcessBuilding();
        }
        else
        {
            if (isBuilding)
            {
                StopBuilding();
            }
        }
    }

    private void StartBuilding()
    {
        isBuilding = true;
        currentState = BridgeBuildState.Building;

        // Play building sound loop
        if (audioSource != null && bridgeData.buildingSound != null)
        {
            audioSource.clip = bridgeData.buildingSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        if (showDebugLogs)
        {
            Debug.Log($"🔨 Mulai membangun '{BridgeName}'...");
        }
    }

    private void StopBuilding()
    {
        isBuilding = false;

        // Stop building sound
        if (audioSource != null && audioSource.isPlaying && audioSource.clip == bridgeData.buildingSound)
        {
            audioSource.Stop();
        }

        if (showDebugLogs)
        {
            Debug.Log($"⏸️ Berhenti membangun '{BridgeName}'");
        }
    }

    private void ProcessBuilding()
    {
        if (bridgeData == null || Inventory.Instance == null)
        {
            if (Inventory.Instance == null)
            {
                Debug.LogError("❌ Inventory tidak ditemukan!");
            }
            StopBuilding();
            return;
        }

        // Increment build timer
        buildTimer += Time.deltaTime;

        // Calculate berapa resource yang harus dipasang berdasarkan build speed
        float resourcesPerSecond = bridgeData.buildSpeed;
        float resourcesThisFrame = resourcesPerSecond * Time.deltaTime;

        // Try consume resources
        bool anyResourceConsumed = false;

        foreach (var req in runtimeResources)
        {
            if (req.currentAmount >= req.totalRequired)
            {
                continue; // Resource ini sudah penuh
            }

            // Check inventory
            int available = Inventory.Instance.GetItemCount(req.resourceName);

            if (available > 0)
            {
                // Calculate berapa yang bisa dipasang
                float needed = req.totalRequired - req.currentAmount;
                float toConsume = Mathf.Min(resourcesThisFrame, needed);

                // Consume dari inventory (integer)
                int intToConsume = Mathf.CeilToInt(toConsume);

                if (Inventory.Instance.RemoveItem(req.resourceName, intToConsume))
                {
                    req.currentAmount += intToConsume;
                    anyResourceConsumed = true;

                    if (showDebugLogs)
                    {
                        Debug.Log($"🔧 Memasang {intToConsume}x {req.resourceName} ({req.currentAmount}/{req.totalRequired})");
                    }
                }
            }
            else
            {
                // Tidak punya resource ini
                if (showDebugLogs)
                {
                    Debug.Log($"⚠️ Tidak punya {req.resourceName}! Butuh {req.totalRequired - req.currentAmount} lagi.");
                }
            }
        }

        // Update progress
        UpdateBuildProgress();

        // Check if complete
        if (buildProgress >= 1f)
        {
            CompleteBridge();
        }

        // Stop building jika tidak ada resource yang ter-consume
        if (!anyResourceConsumed)
        {
            StopBuilding();
        }
    }

    private void UpdateBuildProgress()
    {
        int totalRequired = 0;
        int totalCurrent = 0;

        foreach (var req in runtimeResources)
        {
            totalRequired += req.totalRequired;
            totalCurrent += req.currentAmount;
        }

        buildProgress = totalRequired > 0 ? (float)totalCurrent / totalRequired : 0f;
    }

    private void UpdateVisual()
    {
        if (bridgeData == null || bridgeRenderer == null) return;

        switch (currentState)
        {
            case BridgeBuildState.Blueprint:
                SetBridgeColor(bridgeData.blueprintColor);
                break;

            case BridgeBuildState.Building:
                // Lerp color based on progress
                Color buildColor = Color.Lerp(bridgeData.blueprintColor, bridgeData.buildingColor, buildProgress);
                SetBridgeColor(buildColor);
                break;

            case BridgeBuildState.Completed:
                SetBridgeColor(bridgeData.completedColor);
                break;
        }
    }

    private void SetBridgeColor(Color color)
    {
        if (bridgeRenderer != null)
        {
            // Set color di material
            bridgeRenderer.material.color = color;

            // Enable/disable transparency based on alpha
            if (color.a < 1f)
            {
                // Transparent mode
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
                // Opaque mode
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

    private void CompleteBridge()
    {
        currentState = BridgeBuildState.Completed;
        buildProgress = 1f;
        isBuilding = false;

        // Stop building sound
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Play complete sound
        if (audioSource != null && bridgeData != null && bridgeData.completeSound != null)
        {
            audioSource.PlayOneShot(bridgeData.completeSound);
        }

        // Enable collider
        if (bridgeCollider != null)
        {
            bridgeCollider.enabled = true;

            if (showDebugLogs)
            {
                Debug.Log($"✅ '{BridgeName}' collider ENABLED - Player bisa lewat!");
            }
        }

        // Set final color
        if (bridgeData != null)
        {
            SetBridgeColor(bridgeData.completedColor);
        }

        // Hide UI
        HideBuildUI();

        if (showDebugLogs)
        {
            Debug.Log($"🎉 '{BridgeName}' SELESAI DIBANGUN!");
        }
    }

    private void ShowBuildUI()
    {
        if (buildUI != null && uiInstance == null)
        {
            uiInstance = Instantiate(buildUI, transform.position + Vector3.up * 2f, Quaternion.identity);
            uiInstance.transform.SetParent(transform);

            // Pass reference ke UI script jika ada
            var uiScript = uiInstance.GetComponent<BridgeBuildUI>();
            if (uiScript != null)
            {
                uiScript.SetBridge(this);
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
        if (bridgeData == null) return "No Bridge Data";

        if (IsCompleted)
        {
            return $"{BridgeName}\nCOMPLETED";
        }

        string info = $"{BridgeName}\n";
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
        if (bridgeData == null) return;

        // Build range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, bridgeData.buildRange);

        // Collider preview
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(bridgeData.colliderCenter, bridgeData.colliderSize);

        // Progress indicator
        if (Application.isPlaying && buildProgress > 0f && buildProgress < 1f)
        {
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f * buildProgress);
        }
    }
}