using UnityEngine;
using TMPro;
using UnityEngine.UI;

// Pastikan nama class ini SAMA PERSIS dengan nama file-nya.
// Jika nama filemu BridgeBluepritUI.cs, ubah kata BridgeBuildUI di bawah ini menjadi BridgeBluepritUI.
public class BridgeBuildUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI bridgeNameText;
    public Slider buildProgressSlider;
    public TextMeshProUGUI requirementsText;
    public TextMeshProUGUI actionText;
    public GameObject resourceIcon;

    private BridgeBuildingSystem bridge;

    public void SetBridge(BridgeBuildingSystem system)
    {
        bridge = system;
    }

    private void Update()
    {
        if (bridge == null) return;

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
        if (bridgeNameText != null) bridgeNameText.text = bridge.BridgeName;

        // Tampilkan Progress Building di Slider (0% ke 100%)
        if (buildProgressSlider != null)
            buildProgressSlider.value = bridge.BuildProgress;

        if (actionText != null)
            actionText.text = bridge.State == BridgeBuildState.Building ? "BUILDING..." : "HOLD [E] TO BUILD";

        if (requirementsText != null) requirementsText.text = GetRequirements();
        if (resourceIcon != null) resourceIcon.SetActive(true);
    }

    private string GetRequirements()
    {
        string t = "";
        foreach (var req in bridge.RuntimeResources)
        {
            int current = Inventory.Instance != null ? Inventory.Instance.GetItemCount(req.resourceName) : 0;

            // Jika sedang building, tampilkan angka yang dibutuhkan (karena scrap sudah ditarik)
            if (bridge.State == BridgeBuildState.Building) current = req.totalRequired;

            t += $"{current}/{req.totalRequired} {req.resourceName}\n";
        }
        return t;
    }
}