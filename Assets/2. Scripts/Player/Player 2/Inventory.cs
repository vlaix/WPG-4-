using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    // Singleton pattern for easy access from anywhere
    public static Inventory Instance { get; private set; }

    // Dictionary to store items and their quantities
    private Dictionary<string, int> items = new Dictionary<string, int>();

    // Event that fires when inventory changes (for UI updates)
    public delegate void OnInventoryChanged();
    public event OnInventoryChanged onInventoryChanged;

    private void Awake()
    {
        // Singleton setup (removed DontDestroyOnLoad to fix instantiation issues)
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Add item to inventory
    public void AddItem(string itemName, int quantity = 1)
    {
        if (items.ContainsKey(itemName))
        {
            items[itemName] += quantity;
        }
        else
        {
            items.Add(itemName, quantity);
        }

        Debug.Log($"Added {quantity}x {itemName} to inventory");
        
        // Notify UI to update
        onInventoryChanged?.Invoke();
    }

    // Get quantity of specific item
    public int GetItemCount(string itemName)
    {
        if (items.ContainsKey(itemName))
        {
            return items[itemName];
        }
        return 0;
    }

    // Get all items in inventory
    public Dictionary<string, int> GetAllItems()
    {
        return new Dictionary<string, int>(items);
    }

    // Check if inventory has specific item
    public bool HasItem(string itemName)
    {
        return items.ContainsKey(itemName);
    }

    // Clear entire inventory
    public void ClearInventory()
    {
        items.Clear();
        onInventoryChanged?.Invoke();
    }
}