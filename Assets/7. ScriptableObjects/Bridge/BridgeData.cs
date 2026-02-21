using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BridgeResourceRequirement
{
    public string resourceName;     // Nama resource (e.g., "Scrap", "Wood", "Stone")
    public int amount;              // Jumlah yang dibutuhkan
}

[CreateAssetMenu(fileName = "BridgeData", menuName = "Scriptable Objects/BridgeData")]
public class BridgeData : ScriptableObject
{
    [Header("Bridge Info")]
    [Tooltip("Nama jembatan ini")]
    public string bridgeName = "Wooden Bridge";

    [TextArea(2, 4)]
    [Tooltip("Deskripsi jembatan (optional)")]
    public string description = "A simple wooden bridge";

    [Header("Resource Requirements")]
    [Tooltip("List resource yang dibutuhkan untuk membangun")]
    public List<BridgeResourceRequirement> requiredResources = new List<BridgeResourceRequirement>();

    [Header("Build Settings")]
    [Tooltip("Kecepatan build (resource per detik)")]
    [Range(0.5f, 10f)]
    public float buildSpeed = 2f;

    [Tooltip("Jarak player untuk bisa build")]
    [Range(1f, 10f)]
    public float buildRange = 3f;

    [Header("Visual Settings")]
    [Tooltip("Prefab jembatan (akan di-instantiate)")]
    public GameObject bridgePrefab;

    [Tooltip("Warna saat blueprint (belum dibangun)")]
    public Color blueprintColor = new Color(1f, 1f, 1f, 0.3f);

    [Tooltip("Warna saat building (sedang dibangun)")]
    public Color buildingColor = new Color(1f, 0.8f, 0f, 0.7f);

    [Tooltip("Warna saat completed (sudah selesai)")]
    public Color completedColor = new Color(0.6f, 0.4f, 0.2f, 1f);

    [Header("Audio")]
    [Tooltip("Sound saat building (loop)")]
    public AudioClip buildingSound;

    [Tooltip("Sound saat complete")]
    public AudioClip completeSound;

    [Header("Collision Settings")]
    [Tooltip("Ukuran collider jembatan (X, Y, Z)")]
    public Vector3 colliderSize = new Vector3(2f, 1f, 10f);

    [Tooltip("Center offset collider")]
    public Vector3 colliderCenter = new Vector3(0f, 0.5f, 0f);

    [Header("Advanced")]
    [Tooltip("Tag yang dibutuhkan untuk build (e.g., 'Player2')")]
    public string requiredPlayerTag = "Player2";

    /// <summary>
    /// Get total semua resource yang dibutuhkan
    /// </summary>
    public int GetTotalResourcesRequired()
    {
        int total = 0;
        foreach (var req in requiredResources)
        {
            total += req.amount;
        }
        return total;
    }

    /// <summary>
    /// Check apakah ada resource dengan nama tertentu
    /// </summary>
    public bool HasResource(string resourceName)
    {
        foreach (var req in requiredResources)
        {
            if (req.resourceName == resourceName)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Get amount resource tertentu
    /// </summary>
    public int GetResourceAmount(string resourceName)
    {
        foreach (var req in requiredResources)
        {
            if (req.resourceName == resourceName)
            {
                return req.amount;
            }
        }
        return 0;
    }
}