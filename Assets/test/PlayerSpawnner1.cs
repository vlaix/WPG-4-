using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawner1 : MonoBehaviour
{
    [Header("Daftar Prefab Karakter")]
    public GameObject player1Prefab; // Karakter untuk Sisi Kiri
    public GameObject player2Prefab; // Karakter untuk Sisi Kanan

    private PlayerInputManager manager;
    public CameraFollow gameCamera;

    void Awake()
    {
        manager = GetComponent<PlayerInputManager>();

        // KUNCI UTAMA: Kita mematikan auto-join karena kita mau spawn manual 
        // berdasarkan data dari Lobby.
        manager.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;
    }

    void Start()
    {
        // 1. Spawn Player pertama (P0 dari Lobby)
        if (GameData.Instance.p0Connected)
            SpawnFromLobby(0, GameData.Instance.p0Side);

        // 2. Spawn Player kedua (P1 dari Lobby)
        if (GameData.Instance.p1Connected)
            SpawnFromLobby(1, GameData.Instance.p1Side);
    }

    void SpawnFromLobby(int playerIndex, int side)
    {
        // Pilih prefab: -1 (Kiri) pakai Prefab1, 1 (Kanan) pakai Prefab2
        GameObject chosenPrefab = (side == -1) ? player1Prefab : player2Prefab;

        // Ambil device yang benar dari Input System agar tidak tertukar
        // (Mengambil device yang join saat di lobby)
        InputDevice userDevice = (playerIndex == 0) ? GameData.Instance.p0Device : GameData.Instance.p1Device;
        if (userDevice == null)
        {
            Debug.LogError($"Device untuk Player {playerIndex} tidak ditemukan di GameData!");
            return;
        }
        // Spawn player dengan device tersebut
        UnityEngine.InputSystem.PlayerInput pi = UnityEngine.InputSystem.PlayerInput.Instantiate(chosenPrefab, playerIndex, pairWithDevice: userDevice);

        // Panggil fungsi inisialisasi (Logika Bridge/Ladder kamu)
        InitializePlayerSystems(pi);
    }

    public void InitializePlayerSystems(UnityEngine.InputSystem.PlayerInput playerInput)
    {
        Debug.Log("Initializing Systems for Player " + playerInput.playerIndex);

        // --- Tetap Gunakan Logika Kamu Sebelumnya ---
        BridgeBuildingSystem[] allBridges = FindObjectsByType<BridgeBuildingSystem>(FindObjectsSortMode.None);
        LadderBuildingSystem[] allLadders = FindObjectsByType<LadderBuildingSystem>(FindObjectsSortMode.None);

        if (gameCamera != null)
        {
            gameCamera.AddTarget(playerInput.transform);
        }

        foreach (var bridge in allBridges)
        {
            if (playerInput.CompareTag(bridge.bridgeData.requiredPlayerTag))
            {
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
                playerInput.actions["Build"].performed += ladder.OnBuild;
                playerInput.actions["Build"].canceled += ladder.OnBuild;
            }
        }

        SetSpawnPosition(playerInput);
    }

    private void SetSpawnPosition(UnityEngine.InputSystem.PlayerInput player)
    {
        // Gunakan side dari GameData untuk menentukan posisi spawn fisik
        int side = (player.playerIndex == 0) ? GameData.Instance.p0Side : GameData.Instance.p1Side;

        Vector3 spawnPos = (side == -1) ? new Vector3(-2, 1, 0) : new Vector3(2, 1, 0);
        player.transform.position = spawnPos;
    }
}