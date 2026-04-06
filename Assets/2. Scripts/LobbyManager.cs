using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [Header("UI Slots (Bingkai di Hierarchy)")]
    public Image slotKiri;
    public Image slotKanan;
    public Image slotTengahP1; // Slot khusus P1 saat di tengah
    public Image slotTengahP2; // Slot khusus P2 saat di tengah
    public Image slotInstruksi; // Slot khusus gambar "Press to Join"

    [Header("Sprites Asset (Foto)")]
    public Sprite spriteP1;
    public Sprite spriteP2;
    public Sprite spriteEmpty;    // Gambar "---"
    public Sprite spritePressJoin;

    [Header("Controls")]
    public Button btnStart;

    void Start()
    {
        if (GameData.Instance != null)
        {
            GameData.Instance.ResetLobbyData();
        }

        // Pastikan TimeScale kembali ke 1 (karena biasanya Win UI me-pause game)
        Time.timeScale = 1f;

        UpdateUI();
    }

    public void OnPlayerJoined(UnityEngine.InputSystem.PlayerInput input)
    {
        if (input.playerIndex == 0)
        {
            GameData.Instance.p0Connected = true;
            GameData.Instance.p0Device = input.devices[0];
        }
        else if (input.playerIndex == 1)
        {
            GameData.Instance.p1Connected = true;
            GameData.Instance.p1Device = input.devices[0];
        }
        UpdateUI();
    }

    public void RequestMove(int pIndex, int direction)
    {
        bool isTaken = false;
        if (direction != 0)
        {
            isTaken = (pIndex == 0) ? (GameData.Instance.p1Side == direction)
                                     : (GameData.Instance.p0Side == direction);
        }

        if (!isTaken)
        {
            if (pIndex == 0) GameData.Instance.p0Side = direction;
            else if (pIndex == 1) GameData.Instance.p1Side = direction;
            UpdateUI();
        }
    }

    public void UpdateUI()
    {
        // 1. Slot Kiri & Kanan (Sama seperti sebelumnya)
        slotKiri.sprite = GetSpriteForSide(-1);
        slotKanan.sprite = GetSpriteForSide(1);

        // 2. Slot Tengah P1 (Hanya muncul jika P1 terhubung dan di posisi 0)
        bool p0InCenter = GameData.Instance.p0Connected && GameData.Instance.p0Side == 0;
        slotTengahP1.sprite = p0InCenter ? spriteP1 : spriteEmpty;
        slotTengahP1.gameObject.SetActive(p0InCenter);

        // 3. Slot Tengah P2 (Hanya muncul jika P2 terhubung dan di posisi 0)
        bool p1InCenter = GameData.Instance.p1Connected && GameData.Instance.p1Side == 0;
        slotTengahP2.sprite = p1InCenter ? spriteP2 : spriteEmpty;
        slotTengahP2.gameObject.SetActive(p1InCenter);

        // 4. Slot Instruksi (Muncul jika ada slot player yang masih bisa join)
        bool allJoined = GameData.Instance.p0Connected && GameData.Instance.p1Connected;
        slotInstruksi.sprite = spritePressJoin;
        slotInstruksi.gameObject.SetActive(true);
        // Opsional: Instruksi hanya muncul jika tidak ada p1/p2 yang menghalangi di tengah
        // slotInstruksi.gameObject.SetActive(!allJoined && !p0InCenter && !p1InCenter);

        // 5. Logika Button Start
        bool p0Ready = GameData.Instance.p0Connected && GameData.Instance.p0Side != 0;
        bool p1Ready = GameData.Instance.p1Connected && GameData.Instance.p1Side != 0;
        btnStart.interactable = (p0Ready && p1Ready && GameData.Instance.p0Side != GameData.Instance.p1Side);
    }

    Sprite GetSpriteForSide(int side)
    {
        if (GameData.Instance.p0Connected && GameData.Instance.p0Side == side) return spriteP1;
        if (GameData.Instance.p1Connected && GameData.Instance.p1Side == side) return spriteP2;
        return spriteEmpty;
    }

    public void LaunchGame()
    {
        // Ambil nama level yang tadi dipilih di Level Selector
        string targetLevel = GameData.Instance.selectedLevelName;

        Debug.Log($"Semua Ready! Memulai level: {targetLevel}");

        // Gunakan LoadingScreen sesuai standar kodinganmu
        LoadingScreen.LoadScene(targetLevel);
    }
}