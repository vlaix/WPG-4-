using UnityEngine;
using TMPro;

/// <summary>
/// SUPER SIMPLE BUILD UI - Cuma requirements doang!
/// </summary>
public class SimpleBuildUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text untuk list requirements")]
    public TextMeshProUGUI requirementsText;

    [Header("Settings")]
    [Tooltip("Seberapa sering UI update (detik)")]
    public float updateInterval = 0.2f;

    [Tooltip("UI akan selalu menghadap camera")]
    public bool faceCamera = true;

    // Private
    private IBuildable buildable;
    private float updateTimer = 0f;

    public void SetBuildable(IBuildable buildableObject)
    {
        buildable = buildableObject;
        UpdateUI();
    }

    private void Update()
    {
        if (buildable == null) return;

        // Face camera
        if (faceCamera && Camera.main != null)
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
        if (buildable == null || requirementsText == null) return;

        // Hide UI if completed
        if (buildable.IsCompleted())
        {
            gameObject.SetActive(false);
            return;
        }

        // Update requirements
        requirementsText.text = GetRequirementsText();
    }

    private string GetRequirementsText()
    {
        string text = "";

        var requirements = buildable.GetRequirements();

        foreach (var req in requirements)
        {
            // Check apakah requirement sudah terpenuhi
            bool hasEnough = req.currentAmount >= req.totalRequired;

            // Icon: ✓ atau ○
            string icon = hasEnough ? "✓" : "○";

            // Color
            string colorTag = hasEnough ? "<color=green>" : "<color=yellow>";

            // Inventory count
            int inInventory = Inventory.Instance != null ?
                             Inventory.Instance.GetItemCount(req.resourceName) : 0;

            // Format text
            text += $"{colorTag}{icon} {req.resourceName}: {req.currentAmount}/{req.totalRequired}";

            // Show inventory count jika belum cukup
            if (!hasEnough && inInventory > 0)
            {
                text += $" <color=white>(Have: {inInventory})</color>";
            }
            else if (!hasEnough && inInventory == 0)
            {
                text += $" <color=red>(Need more!)</color>";
            }

            text += "</color>\n";
        }

        return text;
    }
}

// Interface tetap sama
public interface IBuildable
{
    string GetStructureName();
    float GetBuildProgress();
    bool IsCompleted();
    System.Collections.Generic.List<BuildRequirement> GetRequirements();
}

[System.Serializable]
public class BuildRequirement
{
    public string resourceName;
    public int totalRequired;
    public int currentAmount;

    public BuildRequirement(string name, int total, int current)
    {
        resourceName = name;
        totalRequired = total;
        currentAmount = current;
    }
}