using UnityEngine;
using System.Collections.Generic;

public enum BlueprintVisualMode
{
    Wireframe,      // Visible dengan material wireframe/ghost (seperti The Forest)
    Invisible,      // Completely invisible, hanya muncul saat dibangun
    SemiTransparent // Semi-transparent tapi tetap kelihatan
}

public enum CollisionMode
{
    WholeStructure,  // Satu collider untuk seluruh jembatan (simple)
    PerPart         // Collider per part (progressive collision - advanced)
}

[System.Serializable]
public class ResourceRequirement
{
    public string resourceName;  // Nama item yang dibutuhkan (e.g., "Scrap", "Wood", "Plank")
    public int requiredAmount;   // Jumlah yang dibutuhkan
    public int currentAmount;    // Jumlah yang sudah dipasang
}

public class BridgeBlueprintData : MonoBehaviour
{
    [Header("Bridge Info")]
    [Tooltip("Nama blueprint ini")]
    public string blueprintName = "Wooden Bridge";

    [Header("Resource Requirements")]
    [Tooltip("Daftar resource yang dibutuhkan untuk membangun")]
    public List<ResourceRequirement> requiredResources = new List<ResourceRequirement>();

    [Header("Bridge Parts")]
    [Tooltip("Drag semua bagian jembatan yang akan muncul bertahap (urut dari bawah ke atas)")]
    public List<GameObject> bridgeParts = new List<GameObject>();

    [Header("Interaction Settings")]
    [Tooltip("Jarak player harus berada untuk bisa interact")]
    public float interactionRange = 3f;

    [Tooltip("Key untuk interact (default: E)")]
    public KeyCode interactKey = KeyCode.E;

    [Header("Visual Settings")]
    [Tooltip("Material untuk blueprint (wireframe/ghost material)")]
    public Material blueprintMaterial;

    [Tooltip("Material normal untuk jembatan yang sudah selesai")]
    public Material completedMaterial;

    [Tooltip("Mode tampilan blueprint saat belum selesai")]
    public BlueprintVisualMode visualMode = BlueprintVisualMode.Wireframe;

    [Header("Collision Settings")]
    [Tooltip("UNCHECKED = Player BISA JATUH (seperti The Forest) | CHECKED = Player TIDAK bisa jatuh")]
    public bool hasColliderWhenIncomplete = false;

    [Tooltip("Mode collision: WholeStructure (1 collider) atau PerPart (collider tiap part)")]
    public CollisionMode collisionMode = CollisionMode.WholeStructure;

    [Tooltip("Collider utama jembatan (untuk WholeStructure mode)")]
    public Collider bridgeCollider;

    [Header("Audio")]
    [Tooltip("Sound effect saat memasang resource")]
    public AudioClip buildSound;

    [Tooltip("Sound effect saat jembatan selesai dibangun")]
    public AudioClip completeSound;

    [Header("UI")]
    [Tooltip("Prefab UI yang muncul saat player dekat (menampilkan progress)")]
    public GameObject interactionUI;

    private GameObject uiInstance;
    private Transform player2Transform;
    private bool isCompleted = false;
    private AudioSource audioSource;

    // Properties untuk cek status
    public bool IsCompleted => isCompleted;
    public float BuildProgress => GetBuildProgress();

    private void Start()
    {
        InitializeBridge();
        SetupAudio();
        FindPlayer2();
    }

    private void Update()
    {
        if (isCompleted) return;

        CheckPlayerProximity();
        HandleInteraction();
    }

    /// <summary>
    /// Initialize bridge - setup visual mode dan collider
    /// </summary>
    private void InitializeBridge()
    {
        // Setup initial visual based on mode
        foreach (GameObject part in bridgeParts)
        {
            if (part != null)
            {
                Renderer renderer = part.GetComponent<Renderer>();
                if (renderer != null)
                {
                    switch (visualMode)
                    {
                        case BlueprintVisualMode.Wireframe:
                            // Wireframe/ghost material (seperti The Forest)
                            if (blueprintMaterial != null)
                            {
                                renderer.material = blueprintMaterial;
                            }
                            break;

                        case BlueprintVisualMode.Invisible:
                            // Completely invisible - disable renderer
                            renderer.enabled = false;
                            break;

                        case BlueprintVisualMode.SemiTransparent:
                            // Semi-transparent
                            if (blueprintMaterial != null)
                            {
                                renderer.material = blueprintMaterial;
                            }
                            Color color = renderer.material.color;
                            color.a = 0.3f;
                            renderer.material.color = color;
                            break;
                    }
                }

                // Setup collider per part jika mode PerPart
                if (collisionMode == CollisionMode.PerPart)
                {
                    Collider partCollider = part.GetComponent<Collider>();
                    if (partCollider != null)
                    {
                        // Disable semua part colliders di awal
                        partCollider.enabled = false;
                    }
                }
            }
        }

        // Setup main collider untuk WholeStructure mode
        if (collisionMode == CollisionMode.WholeStructure)
        {
            if (bridgeCollider == null)
            {
                bridgeCollider = GetComponent<Collider>();
            }

            if (bridgeCollider != null)
            {
                // Set collider based on setting
                bridgeCollider.enabled = hasColliderWhenIncomplete;

                if (hasColliderWhenIncomplete)
                {
                    Debug.Log($"⚠️ Blueprint '{blueprintName}': Collider AKTIF - Player TIDAK bisa jatuh");
                }
                else
                {
                    Debug.Log($"✅ Blueprint '{blueprintName}': Collider NONAKTIF - Player BISA jatuh sampai selesai!");
                }
            }
            else
            {
                Debug.LogWarning($"Blueprint '{blueprintName}': Tidak ada collider! Tambahkan Box Collider atau Mesh Collider!");
            }
        }

        Debug.Log($"Blueprint '{blueprintName}' initialized with {bridgeParts.Count} parts (Visual: {visualMode}, Collision: {collisionMode})");
    }

    /// <summary>
    /// Setup audio source
    /// </summary>
    private void SetupAudio()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (buildSound != null || completeSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    /// <summary>
    /// Find Player 2 in scene
    /// </summary>
    private void FindPlayer2()
    {
        GameObject player2 = GameObject.FindGameObjectWithTag("Player2");
        if (player2 != null)
        {
            player2Transform = player2.transform;
        }
        else
        {
            Debug.LogWarning("Player2 tidak ditemukan! Pastikan ada GameObject dengan tag 'Player2'");
        }
    }

    /// <summary>
    /// Check apakah player 2 dekat dengan blueprint
    /// </summary>
    private void CheckPlayerProximity()
    {
        if (player2Transform == null) return;

        float distance = Vector3.Distance(transform.position, player2Transform.position);

        if (distance <= interactionRange)
        {
            ShowInteractionUI();
        }
        else
        {
            HideInteractionUI();
        }
    }

    /// <summary>
    /// Handle input interaction dari player
    /// </summary>
    private void HandleInteraction()
    {
        if (player2Transform == null) return;

        float distance = Vector3.Distance(transform.position, player2Transform.position);

        if (distance <= interactionRange && Input.GetKeyDown(interactKey))
        {
            TryBuild();
        }
    }

    /// <summary>
    /// Coba build - ambil resource dari inventory dan pasang ke jembatan
    /// </summary>
    public void TryBuild()
    {
        if (isCompleted)
        {
            Debug.Log("Jembatan sudah selesai dibangun!");
            return;
        }

        if (Inventory.Instance == null)
        {
            Debug.LogError("Inventory Instance tidak ditemukan!");
            return;
        }

        // Cari resource yang masih butuh
        foreach (ResourceRequirement req in requiredResources)
        {
            if (req.currentAmount < req.requiredAmount)
            {
                // Cek apakah player punya resource ini di inventory
                int availableAmount = Inventory.Instance.GetItemCount(req.resourceName);

                if (availableAmount > 0)
                {
                    // Ambil 1 resource dari inventory
                    bool removed = Inventory.Instance.RemoveItem(req.resourceName, 1);

                    if (removed)
                    {
                        req.currentAmount++;

                        Debug.Log($"🔨 Memasang {req.resourceName} ({req.currentAmount}/{req.requiredAmount})");

                        // Update visual progress
                        UpdateBridgeVisual();

                        // Play build sound
                        PlayBuildSound();

                        // Cek apakah sudah selesai
                        if (CheckIfCompleted())
                        {
                            CompleteBridge();
                        }
                    }

                    return; // Hanya pasang 1 resource per interact
                }
                else
                {
                    Debug.Log($"❌ Tidak punya {req.resourceName}! Butuh {req.requiredAmount - req.currentAmount} lagi.");
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Update visual jembatan sesuai progress
    /// </summary>
    private void UpdateBridgeVisual()
    {
        float progress = GetBuildProgress();
        int partsToShow = Mathf.FloorToInt(bridgeParts.Count * progress);

        for (int i = 0; i < bridgeParts.Count; i++)
        {
            if (bridgeParts[i] != null)
            {
                Renderer renderer = bridgeParts[i].GetComponent<Renderer>();
                if (renderer != null)
                {
                    if (i < partsToShow)
                    {
                        // Part sudah dibangun - tampilkan dengan material normal
                        renderer.enabled = true; // Enable renderer jika mode invisible

                        if (completedMaterial != null)
                        {
                            renderer.material = completedMaterial;
                        }

                        // Set fully opaque
                        Color color = renderer.material.color;
                        color.a = 1f;
                        renderer.material.color = color;

                        // Enable collider untuk part ini (PerPart mode)
                        if (collisionMode == CollisionMode.PerPart)
                        {
                            Collider partCollider = bridgeParts[i].GetComponent<Collider>();
                            if (partCollider != null)
                            {
                                partCollider.enabled = true;
                            }
                        }
                    }
                    else
                    {
                        // Part belum dibangun - sesuai visual mode
                        switch (visualMode)
                        {
                            case BlueprintVisualMode.Wireframe:
                            case BlueprintVisualMode.SemiTransparent:
                                renderer.enabled = true;
                                if (blueprintMaterial != null)
                                {
                                    renderer.material = blueprintMaterial;
                                }
                                Color color = renderer.material.color;
                                color.a = 0.3f;
                                renderer.material.color = color;
                                break;

                            case BlueprintVisualMode.Invisible:
                                renderer.enabled = false;
                                break;
                        }

                        // Disable collider untuk part ini (PerPart mode)
                        if (collisionMode == CollisionMode.PerPart)
                        {
                            Collider partCollider = bridgeParts[i].GetComponent<Collider>();
                            if (partCollider != null)
                            {
                                partCollider.enabled = false;
                            }
                        }
                    }
                }
            }
        }

        Debug.Log($"📊 Build Progress: {Mathf.FloorToInt(progress * 100)}% ({partsToShow}/{bridgeParts.Count} parts)");
    }

    /// <summary>
    /// Hitung progress building (0.0 - 1.0)
    /// </summary>
    private float GetBuildProgress()
    {
        int totalRequired = 0;
        int totalCurrent = 0;

        foreach (ResourceRequirement req in requiredResources)
        {
            totalRequired += req.requiredAmount;
            totalCurrent += req.currentAmount;
        }

        return totalRequired > 0 ? (float)totalCurrent / totalRequired : 0f;
    }

    /// <summary>
    /// Check apakah semua resource sudah terpenuhi
    /// </summary>
    private bool CheckIfCompleted()
    {
        foreach (ResourceRequirement req in requiredResources)
        {
            if (req.currentAmount < req.requiredAmount)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Jembatan selesai dibangun
    /// </summary>
    private void CompleteBridge()
    {
        isCompleted = true;
        Debug.Log($"🎉 '{blueprintName}' SELESAI DIBANGUN!");

        // Set semua parts ke material completed dan visible
        foreach (GameObject part in bridgeParts)
        {
            if (part != null)
            {
                Renderer renderer = part.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.enabled = true; // Enable jika ada yang disabled

                    if (completedMaterial != null)
                    {
                        renderer.material = completedMaterial;
                    }

                    Color color = renderer.material.color;
                    color.a = 1f;
                    renderer.material.color = color;
                }

                // Enable semua part colliders (PerPart mode)
                if (collisionMode == CollisionMode.PerPart)
                {
                    Collider partCollider = part.GetComponent<Collider>();
                    if (partCollider != null)
                    {
                        partCollider.enabled = true;
                    }
                }
            }
        }

        // ENABLE main collider (WholeStructure mode)
        if (collisionMode == CollisionMode.WholeStructure && bridgeCollider != null)
        {
            bridgeCollider.enabled = true;
            Debug.Log("✅ Bridge collider ENABLED - Player sekarang bisa melewati jembatan!");
        }

        // Play complete sound
        if (audioSource != null && completeSound != null)
        {
            audioSource.PlayOneShot(completeSound);
        }

        // Hide UI
        HideInteractionUI();
    }

    /// <summary>
    /// Play sound saat memasang resource
    /// </summary>
    private void PlayBuildSound()
    {
        if (audioSource != null && buildSound != null)
        {
            audioSource.PlayOneShot(buildSound);
        }
    }

    /// <summary>
    /// Show UI interaction prompt
    /// </summary>
    private void ShowInteractionUI()
    {
        if (interactionUI != null && uiInstance == null)
        {
            uiInstance = Instantiate(interactionUI, transform.position + Vector3.up * 2f, Quaternion.identity);
            uiInstance.transform.SetParent(transform);
        }
    }

    /// <summary>
    /// Hide UI interaction prompt
    /// </summary>
    private void HideInteractionUI()
    {
        if (uiInstance != null)
        {
            Destroy(uiInstance);
            uiInstance = null;
        }
    }

    /// <summary>
    /// Get info text untuk UI
    /// </summary>
    public string GetInfoText()
    {
        if (isCompleted)
        {
            return $"{blueprintName} - COMPLETED";
        }

        string info = $"{blueprintName}\n";
        info += $"Progress: {Mathf.FloorToInt(GetBuildProgress() * 100)}%\n\n";

        foreach (ResourceRequirement req in requiredResources)
        {
            info += $"{req.resourceName}: {req.currentAmount}/{req.requiredAmount}\n";
        }

        info += $"\nPress [{interactKey}] to Build";

        return info;
    }

    /// <summary>
    /// Visualize interaction range
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}