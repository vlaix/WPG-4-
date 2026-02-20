using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BridgeBuildUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI bridgeNameText;
    public TextMeshProUGUI progressText;
    public Image progressBarFill;
    public TextMeshProUGUI requirementsText;
    public TextMeshProUGUI actionText;

    [Header("Settings")]
    public float updateInterval = 0.1f;

    private BridgeBuildingSystem bridge;
    private float updateTimer = 0f;

    public void SetBridge(BridgeBuildingSystem bridgeSystem)
    {
        bridge = bridgeSystem;
        UpdateUI();
    }

    private void Update()
    {
        if (bridge == null) return;

        // Always face camera
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180, 0);
        }

        // Update UI periodically
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (bridge == null) return;

        // Update bridge name
        if (bridgeNameText != null)
        {
            bridgeNameText.text = bridge.bridgeName;
        }

        // Update progress
        float progress = bridge.BuildProgress;

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

        // Update action text
        if (actionText != null)
        {
            if (bridge.IsCompleted)
            {
                actionText.text = "COMPLETED!";
            }
            else
            {
                actionText.text = "Hold [Build] to Build";
            }
        }
    }

    private string GetRequirementsText()
    {
        string text = "";

        foreach (var req in bridge.requiredResources)
        {
            bool hasEnough = req.currentAmount >= req.totalRequired;
            string checkmark = hasEnough ? "✓" : "○";

            // Show current inventory count
            int inInventory = Inventory.Instance != null ?
                             Inventory.Instance.GetItemCount(req.resourceName) : 0;

            text += $"{checkmark} {req.resourceName}: {req.currentAmount}/{req.totalRequired}";

            if (!hasEnough)
            {
                text += $" (Have: {inInventory})";
            }

            text += "\n";
        }

        return text;
    }
}