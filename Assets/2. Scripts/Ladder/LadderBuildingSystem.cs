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

    // Runtime data
    private List<RuntimeLadderResourceRequirement> runtimeResources = new List<RuntimeLadderResourceRequirement>();
    private PlayerInput playerControls;
    private Transform playerTransform;
    private LadderBuildState currentState = LadderBuildState.Blueprint;
    private AudioSource audioSource;
    private GameObject uiInstance;
    private float buildProgress = 0f;
    private float buildTimer = 0f;
    private bool isPlayerInRange = false;
    private bool isBuilding = false;
    private bool isBuildButtonPressed = false;

    // Properties
    public LadderBuildState State => currentState;
    public float BuildProgress => buildProgress;
    public bool IsCompleted => currentState == LadderBuildState.Completed;
    public string LadderName => ladderData != null ? ladderData.ladderName : "Unknown Ladder";
    public List<RuntimeLadderResourceRequirement> RuntimeResources => runtimeResources;

    private void Awake()
    {
        playerControls = new PlayerInput();
    }

    private void OnEnable()
    {
        playerControls.Player2Movement.Enable();
        playerControls.Player2Movement.Build.performed += OnBuildPressed;
        playerControls.Player2Movement.Build.canceled += OnBuildReleased;
    }

    private void OnDisable()
    {
        playerControls.Player2Movement.Build.performed -= OnBuildPressed;
        playerControls.Player2Movement.Build.canceled -= OnBuildReleased;
        playerControls.Player2Movement.Disable();
    }

    private void Start()
    {
        ValidateLadderData();
        InitializeRuntimeResources();
        InitializeLadder();
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

        // Set initial blueprint color
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
        if (ladderData == null || playerTransform == null || IsCompleted) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        isPlayerInRange = distance <= ladderData.buildRange;

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
        if (ladderData == null || !isPlayerInRange || IsCompleted || playerTransform == null)
        {
            if (isBuilding)
            {
                StopBuilding();
            }
            return;
        }

        // Check if build button is being held
        if (isBuildButtonPressed)
        {
            if (!isBuilding)
            {
                StartBuilding();
            }

            // Continue building
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
        if (currentState == LadderBuildState.Blueprint)
        {
            currentState = LadderBuildState.Building;
        }

        isBuilding = true;

        // Play building sound
        if (audioSource != null && ladderData != null && ladderData.buildingSound != null)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = ladderData.buildingSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }

        if (showDebugLogs)
        {
            Debug.Log($"🔨 Started building '{LadderName}'");
        }
    }

    private void StopBuilding()
    {
        isBuilding = false;

        // Stop building sound
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        if (showDebugLogs)
        {
            Debug.Log($"⏸️ Stopped building '{LadderName}'");
        }
    }

    private void ProcessBuilding()
    {
        if (Inventory.Instance == null)
        {
            Debug.LogWarning("⚠️ Inventory.Instance is NULL!");
            return;
        }

        buildTimer += Time.deltaTime;

        // Calculate berapa resource yang harus dipasang berdasarkan build speed
        float resourcesPerSecond = ladderData.buildSpeed;
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
                        Debug.Log($"🔧 Installing {intToConsume}x {req.resourceName} ({req.currentAmount}/{req.totalRequired})");
                    }
                }
            }
            else
            {
                // Tidak punya resource ini
                if (showDebugLogs)
                {
                    Debug.Log($"⚠️ Out of {req.resourceName}! Need {req.totalRequired - req.currentAmount} more.");
                }
            }
        }

        // Update progress
        UpdateBuildProgress();

        // Check if complete
        if (buildProgress >= 1f)
        {
            CompleteLadder();
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
        if (ladderData == null || ladderRenderer == null) return;

        switch (currentState)
        {
            case LadderBuildState.Blueprint:
                SetLadderColor(ladderData.blueprintColor);
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

        // Stop building sound
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Play complete sound
        if (audioSource != null && ladderData != null && ladderData.completeSound != null)
        {
            audioSource.PlayOneShot(ladderData.completeSound);
        }

        // Enable collider (can climb now!)
        if (ladderCollider != null)
        {
            ladderCollider.enabled = true;

            if (showDebugLogs)
            {
                Debug.Log($"✅ '{LadderName}' collider ENABLED - Can climb now!");
            }
        }

        // Set ladder tag (untuk LadderClimber detect)
        if (!string.IsNullOrEmpty(ladderData.ladderTag))
        {
            gameObject.tag = ladderData.ladderTag;
        }

        // Set final color
        if (ladderData != null)
        {
            SetLadderColor(ladderData.completedColor);
        }

        // Hide UI
        HideBuildUI();

        if (showDebugLogs)
        {
            Debug.Log($"🎉 '{LadderName}' COMPLETED!");
        }
    }

    private void ShowBuildUI()
    {
        if (buildUI != null && uiInstance == null)
        {
            uiInstance = Instantiate(buildUI, transform.position + Vector3.up * 2f, Quaternion.identity);
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