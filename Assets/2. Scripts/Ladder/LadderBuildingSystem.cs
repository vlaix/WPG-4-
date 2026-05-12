using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public enum LadderBuildState
{
    Blueprint,
    Building,
    Completed
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
    public LadderData ladderData;

    [Header("Runtime References")]
    public Renderer ladderRenderer;
    public Collider ladderCollider;

    [Header("UI (Optional)")]
    public GameObject buildUI;

    [Header("Hold Settings")]
    [Tooltip("Waktu yang dibutuhkan untuk menahan build (detik)")]
    public float holdDuration = 5f;

    [Header("Debug")]
    public bool showDebugLogs = true;

    [Header("Hologram Settings")]
    [SerializeField] private Color hologramColor = new Color(0f, 0.8f, 1f, 1f);
    [SerializeField] private float hologramPulseSpeed = 2f;
    [SerializeField] private float hologramMinAlpha = 0.15f;
    [SerializeField] private float hologramMaxAlpha = 0.55f;

    private List<RuntimeLadderResourceRequirement> runtimeResources = new List<RuntimeLadderResourceRequirement>();
    private Transform playerTransform;
    private LadderBuildState currentState = LadderBuildState.Blueprint;
    private AudioSource audioSource;
    private GameObject uiInstance;

    private float currentHoldTimer = 0f;
    private bool isHolding = false;
    private bool resourcesConsumed = false;
    private bool isPlayerInRange = false;

    // --- TAMBAHAN BARU: Menyimpan posisi awal player ---
    private Vector3 buildStartPosition;

    public LadderBuildState State => currentState;
    public float BuildProgress => Mathf.Clamp01(currentHoldTimer / holdDuration);
    public bool IsCompleted => currentState == LadderBuildState.Completed;
    public string LadderName => ladderData != null ? ladderData.ladderName : "Unknown Ladder";
    public List<RuntimeLadderResourceRequirement> RuntimeResources => runtimeResources;

    private void Start()
    {
        InitializeLadder();
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
        if (context.performed) StartHolding();
        else if (context.canceled) StopHolding();
    }

    private void StartHolding()
    {
        if (currentState != LadderBuildState.Blueprint || !isPlayerInRange) return;

        if (HasResources())
        {
            isHolding = true;
            currentState = LadderBuildState.Building;

            // Simpan posisi kaki player saat ini
            buildStartPosition = playerTransform.position;

            ConsumeResources();
            resourcesConsumed = true;

            if (playerTransform != null)
            {
                Animator anim = playerTransform.GetComponent<Animator>();
                if (anim != null) anim.SetTrigger("Build");
            }

            if (audioSource != null && ladderData.buildingSound != null)
            {
                audioSource.clip = ladderData.buildingSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
    }

    private void StopHolding()
    {
        if (IsCompleted) return;
        if (isHolding && currentHoldTimer < holdDuration) CancelBuild();
        isHolding = false;
    }

    private void CancelBuild()
    {
        if (showDebugLogs) Debug.Log("Build Tangga Dibatalkan! Mengembalikan Scrap...");

        if (resourcesConsumed)
        {
            RefundResources();
            resourcesConsumed = false;
        }

        currentHoldTimer = 0f;
        currentState = LadderBuildState.Blueprint;

        if (audioSource != null) audioSource.Stop();
    }

    private void HandleBuilding()
    {
        if (!isHolding || IsCompleted) return;

        // --- TAMBAHAN BARU: Cek apakah player bergerak ---
        // Jika player bergeser lebih dari 0.1 unit dari titik awal, batalkan build
        if (Vector3.Distance(playerTransform.position, buildStartPosition) > 0.1f)
        {
            if (showDebugLogs) Debug.Log("Build batal karena player bergerak dari tempat!");
            StopHolding(); // Memanggil fungsi batal
            return;
        }

        currentHoldTimer += Time.deltaTime;
        if (currentHoldTimer >= holdDuration) CompleteBuild();
    }

    private void CompleteBuild()
    {
        currentState = LadderBuildState.Completed;
        isHolding = false;
        resourcesConsumed = false;

        if (audioSource != null)
        {
            audioSource.Stop();
            if (ladderData.completeSound != null) audioSource.PlayOneShot(ladderData.completeSound);
        }

        if (ladderCollider != null) ladderCollider.enabled = true;
        if (!string.IsNullOrEmpty(ladderData.ladderTag)) gameObject.tag = ladderData.ladderTag;

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
        if (ladderData == null) return;
        runtimeResources.Clear();
        foreach (var req in ladderData.requiredResources)
            runtimeResources.Add(new RuntimeLadderResourceRequirement(req.resourceName, req.amount));
    }

    private void InitializeLadder()
    {
        if (ladderData == null) return;

        if (ladderRenderer == null) ladderRenderer = GetComponentInChildren<Renderer>();
        if (ladderRenderer != null) SetLadderColor(ladderData.blueprintColor);

        if (ladderCollider == null)
        {
            ladderCollider = GetComponent<BoxCollider>();
            if (ladderCollider == null)
            {
                BoxCollider boxCol = gameObject.AddComponent<BoxCollider>();
                boxCol.size = ladderData.triggerColliderSize;
                boxCol.center = ladderData.triggerColliderCenter;
                boxCol.isTrigger = true;
                ladderCollider = boxCol;
            }
        }

        if (ladderCollider != null) ladderCollider.enabled = false;
        currentState = LadderBuildState.Blueprint;
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
        isPlayerInRange = dist <= ladderData.buildRange;

        if ((isPlayerInRange || isHolding) && uiInstance == null) ShowUI();
        else if (!isPlayerInRange && !isHolding && uiInstance != null) HideUI();
    }

    private void UpdateVisual()
    {
        if (ladderRenderer == null) return;

        switch (currentState)
        {
            case LadderBuildState.Blueprint:
                ApplyHologramEffect();
                break;
            case LadderBuildState.Building:
                Color buildColor = Color.Lerp(ladderData.blueprintColor, ladderData.buildingColor, BuildProgress);
                SetLadderColor(buildColor);
                break;
            case LadderBuildState.Completed:
                SetLadderColor(ladderData.completedColor);
                break;
        }
    }

    private void ApplyHologramEffect()
    {
        if (ladderRenderer == null) return;

        float pulse = Mathf.Sin(Time.time * hologramPulseSpeed) * 0.5f + 0.5f;
        float alpha = Mathf.Lerp(hologramMinAlpha, hologramMaxAlpha, pulse);
        Color hColor = new Color(hologramColor.r, hologramColor.g, hologramColor.b, alpha);

        Material mat = ladderRenderer.material;
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

    private void SetLadderColor(Color color)
    {
        if (ladderRenderer != null)
        {
            ladderRenderer.material.color = color;
            if (color.a < 1f)
            {
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

    private void ShowUI()
    {
        if (buildUI == null) return;

        Vector3 spawnPos = transform.position + (Vector3.up * 4.5f);
        if (Camera.main != null)
        {
            Vector3 arahKeKamera = (Camera.main.transform.position - spawnPos).normalized;
            spawnPos += arahKeKamera * 1f;
        }

        uiInstance = Instantiate(buildUI, spawnPos, Quaternion.identity, transform);
        var uiScript = uiInstance.GetComponent<LadderBuildUI>();
        if (uiScript != null) uiScript.SetLadder(this);
    }

    private void HideUI() { if (uiInstance != null) Destroy(uiInstance); }
}