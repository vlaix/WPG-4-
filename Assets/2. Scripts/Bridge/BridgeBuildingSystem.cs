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
public class ResourceRequirement
{
    public string resourceName;     // Nama resource (e.g., "Scrap")
    public int totalRequired;       // Total yang dibutuhkan
    [HideInInspector]
    public int currentAmount;       // Jumlah yang sudah dipasang
}

public class BridgeBuildingSystem : MonoBehaviour
{
    [Header("Bridge Info")]
    public string bridgeName = "Wooden Bridge";

    [Header("Resource Requirements")]
    [Tooltip("Resource yang dibutuhkan untuk build jembatan")]
    public List<ResourceRequirement> requiredResources = new List<ResourceRequirement>();

    [Header("Building Settings")]
    [Tooltip("Berapa resource dipasang per detik saat hold E")]
    public float buildSpeed = 2f; // 2 resource per detik

    [Tooltip("Jarak player untuk bisa build")]
    public float buildRange = 3f;

    [Header("Visual Settings")]
    [Tooltip("Renderer jembatan (untuk ganti warna)")]
    public Renderer bridgeRenderer;

    [Tooltip("Warna saat blueprint (wireframe)")]
    public Color blueprintColor = new Color(1f, 1f, 1f, 0.3f);

    [Tooltip("Warna saat building (progress)")]
    public Color buildingColor = new Color(1f, 0.8f, 0f, 0.7f); // Orange

    [Tooltip("Warna saat completed")]
    public Color completedColor = new Color(0.6f, 0.4f, 0.2f, 1f); // Brown

    [Header("Collision Settings")]
    [Tooltip("Collider jembatan (disabled sampai selesai)")]
    public Collider bridgeCollider;

    [Header("Audio")]
    [Tooltip("Sound loop saat building")]
    public AudioClip buildingSound;

    [Tooltip("Sound saat complete")]
    public AudioClip completeSound;

    [Header("UI (Optional)")]
    [Tooltip("UI prefab yang muncul saat player di radius")]
    public GameObject buildUI;

    // Private variables
    private PlayerControl playerControls;
    private Transform player2Transform;
    private BridgeBuildState currentState = BridgeBuildState.Blueprint;
    private AudioSource audioSource;
    private GameObject uiInstance;
    private float buildProgress = 0f; // 0 - 1
    private float buildTimer = 0f;
    private bool isPlayerInRange = false;
    private bool isBuilding = false;
    private bool isBuildButtonPressed = false;

    // Properties
    public BridgeBuildState State => currentState;
    public float BuildProgress => buildProgress;
    public bool IsCompleted => currentState == BridgeBuildState.Completed;

    private void Awake()
    {
        // Initialize input system
        playerControls = new PlayerControl();
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
        InitializeBridge();
        SetupAudio();
        FindPlayer2();
    }

    private void Update()
    {
        CheckPlayerProximity();
        HandleBuildInput();
        UpdateVisual();
    }

    /// <summary>
    /// Input callback when Build button is pressed
    /// </summary>
    private void OnBuildPressed(InputAction.CallbackContext context)
    {
        isBuildButtonPressed = true;
    }

    /// <summary>
    /// Input callback when Build button is released
    /// </summary>
    private void OnBuildReleased(InputAction.CallbackContext context)
    {
        isBuildButtonPressed = false;
    }

    /// <summary>
    /// Initialize bridge state
    /// </summary>
    private void InitializeBridge()
    {
        // Set initial blueprint color
        if (bridgeRenderer != null)
        {
            SetBridgeColor(blueprintColor);
        }
        else
        {
            // Auto-find renderer jika tidak di-assign
            bridgeRenderer = GetComponentInChildren<Renderer>();
            if (bridgeRenderer != null)
            {
                SetBridgeColor(blueprintColor);
            }
        }

        // Disable collider saat blueprint
        if (bridgeCollider == null)
        {
            bridgeCollider = GetComponent<Collider>();
        }

        if (bridgeCollider != null)
        {
            bridgeCollider.enabled = false;
            Debug.Log($"🔧 '{bridgeName}': Collider DISABLED - Player akan jatuh!");
        }

        // Initialize resource current amounts
        foreach (var req in requiredResources)
        {
            req.currentAmount = 0;
        }

        currentState = BridgeBuildState.Blueprint;
        Debug.Log($"🏗️ '{bridgeName}' initialized as Blueprint");
    }

    /// <summary>
    /// Setup audio source
    /// </summary>
    private void SetupAudio()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (buildingSound != null || completeSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
    }

    /// <summary>
    /// Find Player 2
    /// </summary>
    private void FindPlayer2()
    {
        GameObject player2 = GameObject.FindGameObjectWithTag("Player2");
        if (player2 != null)
        {
            player2Transform = player2.transform;
        }
        else
        {
            Debug.LogWarning("⚠️ Player2 tidak ditemukan! Pastikan tag 'Player2' ada.");
        }
    }

    /// <summary>
    /// Check apakah player di radius build
    /// </summary>
    private void CheckPlayerProximity()
    {
        if (player2Transform == null || IsCompleted) return;

        float distance = Vector3.Distance(transform.position, player2Transform.position);
        isPlayerInRange = distance <= buildRange;

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

    /// <summary>
    /// Handle input untuk building (HOLD Build button)
    /// </summary>
    private void HandleBuildInput()
    {
        if (!isPlayerInRange || IsCompleted || player2Transform == null)
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

    /// <summary>
    /// Mulai building
    /// </summary>
    private void StartBuilding()
    {
        isBuilding = true;
        currentState = BridgeBuildState.Building;

        // Play building sound loop
        if (audioSource != null && buildingSound != null)
        {
            audioSource.clip = buildingSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        Debug.Log($"🔨 Mulai membangun '{bridgeName}'...");
    }

    /// <summary>
    /// Stop building
    /// </summary>
    private void StopBuilding()
    {
        isBuilding = false;

        // Stop building sound
        if (audioSource != null && audioSource.isPlaying && audioSource.clip == buildingSound)
        {
            audioSource.Stop();
        }

        Debug.Log($"⏸️ Berhenti membangun '{bridgeName}'");
    }

    /// <summary>
    /// Process building - consume resources
    /// </summary>
    private void ProcessBuilding()
    {
        if (Inventory.Instance == null)
        {
            Debug.LogError("❌ Inventory tidak ditemukan!");
            StopBuilding();
            return;
        }

        // Increment build timer
        buildTimer += Time.deltaTime;

        // Calculate berapa resource yang harus dipasang berdasarkan build speed
        float resourcesPerSecond = buildSpeed;
        float resourcesThisFrame = resourcesPerSecond * Time.deltaTime;

        // Try consume resources
        bool anyResourceConsumed = false;

        foreach (var req in requiredResources)
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

                    Debug.Log($"🔧 Memasang {intToConsume}x {req.resourceName} ({req.currentAmount}/{req.totalRequired})");
                }
            }
            else
            {
                // Tidak punya resource ini
                Debug.Log($"⚠️ Tidak punya {req.resourceName}! Butuh {req.totalRequired - req.currentAmount} lagi.");
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

    /// <summary>
    /// Update build progress (0-1)
    /// </summary>
    private void UpdateBuildProgress()
    {
        int totalRequired = 0;
        int totalCurrent = 0;

        foreach (var req in requiredResources)
        {
            totalRequired += req.totalRequired;
            totalCurrent += req.currentAmount;
        }

        buildProgress = totalRequired > 0 ? (float)totalCurrent / totalRequired : 0f;
    }

    /// <summary>
    /// Update visual based on state
    /// </summary>
    private void UpdateVisual()
    {
        if (bridgeRenderer == null) return;

        switch (currentState)
        {
            case BridgeBuildState.Blueprint:
                SetBridgeColor(blueprintColor);
                break;

            case BridgeBuildState.Building:
                // Lerp color based on progress
                Color buildColor = Color.Lerp(blueprintColor, buildingColor, buildProgress);
                SetBridgeColor(buildColor);
                break;

            case BridgeBuildState.Completed:
                SetBridgeColor(completedColor);
                break;
        }
    }

    /// <summary>
    /// Set bridge color/material
    /// </summary>
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
                bridgeRenderer.material.SetFloat("_Mode", 3); // Transparent
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
                bridgeRenderer.material.SetFloat("_Mode", 0); // Opaque
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

    /// <summary>
    /// Complete bridge
    /// </summary>
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
        if (audioSource != null && completeSound != null)
        {
            audioSource.PlayOneShot(completeSound);
        }

        // Enable collider
        if (bridgeCollider != null)
        {
            bridgeCollider.enabled = true;
            Debug.Log($"✅ '{bridgeName}' collider ENABLED - Player bisa lewat!");
        }

        // Set final color
        SetBridgeColor(completedColor);

        // Hide UI
        HideBuildUI();

        Debug.Log($"🎉 '{bridgeName}' SELESAI DIBANGUN!");
    }

    /// <summary>
    /// Show build UI
    /// </summary>
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

    /// <summary>
    /// Hide build UI
    /// </summary>
    private void HideBuildUI()
    {
        if (uiInstance != null)
        {
            Destroy(uiInstance);
            uiInstance = null;
        }
    }

    /// <summary>
    /// Get info text for UI
    /// </summary>
    public string GetInfoText()
    {
        if (IsCompleted)
        {
            return $"{bridgeName}\nCOMPLETED";
        }

        string info = $"{bridgeName}\n";
        info += $"Progress: {Mathf.FloorToInt(buildProgress * 100)}%\n\n";

        info += "Required:\n";
        foreach (var req in requiredResources)
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

    /// <summary>
    /// Visualize build range
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Build range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, buildRange);

        // Progress indicator
        if (Application.isPlaying && buildProgress > 0f && buildProgress < 1f)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f * buildProgress);
        }
    }
}