using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BridgeBlueprintUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Panel yang berisi semua UI elements")]
    public GameObject uiPanel;

    [Tooltip("Text untuk nama blueprint")]
    public TextMeshProUGUI blueprintNameText;

    [Tooltip("Text untuk progress percentage")]
    public TextMeshProUGUI progressText;

    [Tooltip("Progress bar fill image")]
    public Image progressBarFill;

    [Tooltip("Text untuk resource requirements")]
    public TextMeshProUGUI requirementsText;

    [Tooltip("Text untuk interaction prompt")]
    public TextMeshProUGUI interactionPromptText;

    [Header("Settings")]
    [Tooltip("Update interval in seconds")]
    public float updateInterval = 0.1f;

    private BridgeBlueprintData blueprint;
    private float updateTimer = 0f;

    private void Start()
    {
        // Find blueprint this UI belongs to
        blueprint = GetComponentInParent<BridgeBlueprintData>();

        if (blueprint == null)
        {
            Debug.LogError("BridgeBlueprintUI tidak menemukan BridgeBlueprintData parent!");
            return;
        }

        UpdateUI();
    }

    private void Update()
    {
        if (blueprint == null) return;

        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            UpdateUI();
        }

        // Always face camera
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180, 0); // Flip to face camera
        }
    }

    /// <summary>
    /// Update all UI elements
    /// </summary>
    private void UpdateUI()
    {
        if (blueprint.IsCompleted)
        {
            ShowCompletedState();
            return;
        }

        // Update blueprint name
        if (blueprintNameText != null)
        {
            blueprintNameText.text = blueprint.blueprintName;
        }

        // Update progress
        float progress = blueprint.BuildProgress;

        if (progressText != null)
        {
            progressText.text = $"{Mathf.FloorToInt(progress * 100)}%";
        }

        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = progress;
        }

        // Update requirements
        if (requirementsText != null)
        {
            requirementsText.text = GetRequirementsText();
        }

        // Update interaction prompt
        if (interactionPromptText != null)
        {
            interactionPromptText.text = $"Press [{blueprint.interactKey}] to Build";
        }
    }

    /// <summary>
    /// Get formatted requirements text
    /// </summary>
    private string GetRequirementsText()
    {
        string text = "Requirements:\n";

        foreach (var req in blueprint.requiredResources)
        {
            bool hasEnough = req.currentAmount >= req.requiredAmount;
            string checkmark = hasEnough ? "✓" : "○";

            text += $"{checkmark} {req.resourceName}: {req.currentAmount}/{req.requiredAmount}\n";
        }

        return text;
    }

    /// <summary>
    /// Show completed state
    /// </summary>
    private void ShowCompletedState()
    {
        if (blueprintNameText != null)
        {
            blueprintNameText.text = $"{blueprint.blueprintName} - COMPLETED";
        }

        if (progressText != null)
        {
            progressText.text = "100%";
        }

        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = 1f;
        }

        if (requirementsText != null)
        {
            requirementsText.text = "All requirements met!";
        }

        if (interactionPromptText != null)
        {
            interactionPromptText.text = "";
        }

        // Optional: Hide UI after completion
        // Invoke("HideUI", 2f);
    }

    /// <summary>
    /// Hide UI panel
    /// </summary>
    private void HideUI()
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}