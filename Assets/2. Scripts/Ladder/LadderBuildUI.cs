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

    // --- TAMBAHAN BARU: Referensi untuk menyembunyikan ikon ---
    [Tooltip("Masukkan GameObject gambar ikon/lingkaran kayu ke sini")]
    public GameObject resourceIconObject;

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
        float progress = ladder.buildProgress;

        if (progressText != null)
        {
            progressText.text = $"{Mathf.FloorToInt(progress * 100)}%";
        }

        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = progress;
        }

        // Update requirements & action text
        if (ladder.IsCompleted)
        {
            // --- UI SAAT SELESAI (TIMER SAJA) ---
            if (requirementsText != null)
            {
                requirementsText.text = $"Countdown: {Mathf.CeilToInt(ladder.currentTimer)}s";
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

            text += $"{inInventory}/{req.totalRequired}";

            text += "\n";
        }

        return text;
    }
}