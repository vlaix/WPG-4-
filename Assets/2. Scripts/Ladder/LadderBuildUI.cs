using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LadderBuildUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI ladderNameText;

    // PERHATIKAN: Ini sudah saya ganti jadi Slider
    public Slider buildProgressSlider;

    public TextMeshProUGUI requirementsText;
    public TextMeshProUGUI actionText;
    public GameObject resourceIconObject;

    private LadderBuildingSystem ladder;

    public void SetLadder(LadderBuildingSystem ladderSystem)
    {
        ladder = ladderSystem;
    }

    private void Update()
    {
        if (ladder == null) return;

        // UI selalu menghadap kamera
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180, 0);
        }

        UpdateUIDisplay();
    }

    private void UpdateUIDisplay()
    {
        if (ladderNameText != null) ladderNameText.text = ladder.LadderName;

        // Update Slider Bar (0% - 100%)
        if (buildProgressSlider != null)
        {
            buildProgressSlider.value = ladder.BuildProgress;
        }

        if (actionText != null)
        {
            actionText.text = ladder.State == LadderBuildState.Building ? "BUILDING..." : "HOLD [E] TO BUILD";
        }

        if (requirementsText != null) requirementsText.text = GetRequirementsText();
        if (resourceIconObject != null) resourceIconObject.SetActive(true);
    }

    private string GetRequirementsText()
    {
        string text = "";

        foreach (var req in ladder.RuntimeResources)
        {
            int current = Inventory.Instance != null ? Inventory.Instance.GetItemCount(req.resourceName) : 0;

            // Jika sedang building, tampilkan max requirement (karena scrap sudah dipotong dari inventory)
            if (ladder.State == LadderBuildState.Building) current = req.totalRequired;

            text += $"{current}/{req.totalRequired} {req.resourceName}\n";
        }

        return text;
    }
}