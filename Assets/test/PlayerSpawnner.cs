using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Daftar Prefab Karakter")]
    public GameObject player1Prefab;
    public GameObject player2Prefab;

    private PlayerInputManager manager;
    public CameraFollow gameCamera;

    void Awake()
    {
        manager = GetComponent<PlayerInputManager>();

        // Set prefab pertama saat game mulai (Player 1)
        manager.playerPrefab = player1Prefab;
    }

    // Fungsi ini akan dipanggil otomatis setiap kali Player bergabung
    public void OnPlayerJoined(UnityEngine.InputSystem.PlayerInput playerInput)
    {
        Debug.Log("Player " + playerInput.playerIndex + " Joined!");
        BridgeBuildingSystem[] allBridges = FindObjectsByType<BridgeBuildingSystem>(FindObjectsSortMode.None);
        LadderBuildingSystem[] allLadders = FindObjectsByType<LadderBuildingSystem>(FindObjectsSortMode.None);
        // Setelah Player 1 masuk, ganti prefab untuk Player berikutnya
        if (playerInput.playerIndex == 0)
        {
            manager.playerPrefab = player2Prefab;
        }

        if (gameCamera != null)
        {
            gameCamera.AddTarget(playerInput.transform);
        }

        Debug.Log($"Player {playerInput.playerIndex} ditambahkan ke kamera.");
        foreach (var bridge in allBridges)
        {
            if (playerInput.CompareTag(bridge.bridgeData.requiredPlayerTag))
            {
                // Misalnya fungsi untuk menghubungkan player ke jembatan
                bridge.AssignPlayer(playerInput.transform);
                playerInput.actions["Build"].performed += bridge.OnBuild;
                playerInput.actions["Build"].canceled += bridge.OnBuild;
            }
        }
        foreach (var ladder in allLadders)
        {
            if (playerInput.CompareTag(ladder.ladderData.requiredPlayerTag))
            {
                ladder.AssignPlayer(playerInput.transform);
                // Daftarkan event OnBuild secara dinamis
                playerInput.actions["Build"].performed += ladder.OnBuild;
                playerInput.actions["Build"].canceled += ladder.OnBuild;
            }
        }
        // Opsional: Beri warna atau posisi spawn yang berbeda
        SetSpawnPosition(playerInput);
    }

    private void SetSpawnPosition(UnityEngine.InputSystem.PlayerInput player)
    {
        // Contoh sederhana memisahkan posisi P1 dan P2
        Vector3 spawnPos = (player.playerIndex == 0) ? new Vector3(-2, 1, 0) : new Vector3(2, 1, 0);
        player.transform.position = spawnPos;
    }
}