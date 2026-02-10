using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("Scrap Display")]
    [Tooltip("Drag the TextMeshProUGUI that shows scrap count here")]
    public TextMeshProUGUI scrapText;

    private void Start()
    {
        Debug.Log("InventoryUI Started!");
        if (Inventory.Instance == null)
        {
            Debug.LogError("Inventory.Instance is NULL! Make sure InventoryManager exists in scene!");
            return;
        }
        if (scrapText == null)
        {
            Debug.LogError("scrapText is not assigned! Drag a TextMeshProUGUI to this field in Inspector!");
            return;
        }

        // Subscribe to inventory change event
        Inventory.Instance.onInventoryChanged += UpdateScrapDisplay;
        
        // Update display immediately
        UpdateScrapDisplay();
    }

    private void OnDestroy()
    {
        // Unsubscribe when destroyed to prevent memory leaks
        if (Inventory.Instance != null)
        {
            Inventory.Instance.onInventoryChanged -= UpdateScrapDisplay;
        }
    }

    private void UpdateScrapDisplay()
    {
        if (Inventory.Instance != null && scrapText != null)
        {
            int scrapCount = Inventory.Instance.GetItemCount("Scrap");
            scrapText.text = scrapCount.ToString();
            Debug.Log($"Scrap UI Updated: {scrapCount}");
        }
    }
}