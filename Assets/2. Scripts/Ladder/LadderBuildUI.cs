using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LadderBuildUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI ladderNameText;
    public TextMeshProUGUI progressText;
    public Image progressBarFill;
    public TextMeshProUGUI requirementsText;
    public TextMeshProUGUI actionText;

    [Header("Settings")]
    public float updateInterval = 0.1f;

    private LadderBuildingSystem ladder;
    private float updateTimer = 0f;

    public void SetLadder(LadderBuildingSystem ladderSystem)
    {
        ladder = ladderSystem;
        UpdateUI();
    }

    private void Update()
    {
        if (ladder == null) return;

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
        if (ladder == null) return;

        // Update ladder name
        if (ladderNameText != null)
        {
            ladderNameText.text = ladder.LadderName;
        }

        // Update progress
        float progress = ladder.BuildProgress;

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
            if (ladder.IsCompleted)
            {
                actionText.text = "COMPLETED! Can Climb!";
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

        foreach (var req in ladder.RuntimeResources)
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