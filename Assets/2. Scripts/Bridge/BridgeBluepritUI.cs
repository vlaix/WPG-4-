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

    // --- TAMBAHAN BARU: Referensi untuk menyembunyikan ikon ---
    [Tooltip("Masukkan GameObject gambar ikon/lingkaran kayu ke sini")]
    public GameObject resourceIconObject;

    // (Opsional) Kalau kamu mau menyembunyikan bar merahnya juga saat countdown
    [Tooltip("Masukkan GameObject Progress Bar ke sini (Opsional)")]
    public GameObject progressBarObject;

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

        if (bridgeNameText != null)
        {
            bridgeNameText.text = bridge.BridgeName;
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

        // Update requirements & action text
        if (bridge.IsCompleted)
        {
            // --- UI SAAT SELESAI (TIMER SAJA) ---
            if (requirementsText != null)
            {
                requirementsText.text = $"Countdown: {Mathf.CeilToInt(bridge.currentTimer)}s";
            }

            if (actionText != null)
            {
                actionText.text = "";
            }

            // Sembunyikan ikon resource
            if (resourceIconObject != null)
            {
                resourceIconObject.SetActive(false);
            }

            // Sembunyikan progress bar (opsional)
            if (progressBarObject != null)
            {
                progressBarObject.SetActive(false);
            }
        }
        else
        {
            // --- UI SAAT BLUEPRINT / BUILDING ---
            if (requirementsText != null)
            {
                requirementsText.text = GetRequirementsText();
            }

            if (actionText != null)
            {
                actionText.text = "Hold [Build] to Build";
            }

            // Tampilkan kembali ikon resource
            if (resourceIconObject != null)
            {
                resourceIconObject.SetActive(true);
            }

            // Tampilkan kembali progress bar (opsional)
            if (progressBarObject != null)
            {
                progressBarObject.SetActive(true);
            }
        }
    }

    private string GetRequirementsText()
    {
        string text = "";

        foreach (var req in bridge.RuntimeResources)
        {
            bool hasEnough = req.currentAmount >= req.totalRequired;
            string checkmark = hasEnough ? "✓" : "○";

            // Show current inventory count
            int inInventory = Inventory.Instance != null ?
                             Inventory.Instance.GetItemCount(req.resourceName) : 0;

            text += $"{inInventory}/{req.totalRequired}";

            text += "\n";
        }

        return text;
    }
}