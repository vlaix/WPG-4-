using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [Header("UI Display")]
    public TextMeshProUGUI txtKiri;
    public TextMeshProUGUI txtTengah;
    public TextMeshProUGUI txtKanan;
    public Button btnStart;

    void Start()
    {
        // Reset data jika perlu setiap balik ke lobby
        GameData.Instance.p0Connected = false;
        GameData.Instance.p1Connected = false;
        GameData.Instance.p0Side = 0;
        GameData.Instance.p1Side = 0;

        UpdateUI();
    }

    public void OnPlayerJoined(UnityEngine.InputSystem.PlayerInput input)
    {
        if (input.playerIndex == 0)
        {
            GameData.Instance.p0Connected = true;
            GameData.Instance.p0Device = input.devices[0]; // Catat device P1
        }
        else if (input.playerIndex == 1)
        {
            GameData.Instance.p1Connected = true;
            GameData.Instance.p1Device = input.devices[0]; // Catat device P2
        }

        UpdateUI();
    }

    public void RequestMove(int pIndex, int direction)
    {
        // Validasi: Jika ingin ke Kiri/Kanan, cek apakah sudah ada orang lain di sana
        bool isTaken = false;
        if (direction != 0) // Jika bukan ke tengah
        {
            isTaken = (pIndex == 0) ? (GameData.Instance.p1Side == direction)
                                     : (GameData.Instance.p0Side == direction);
        }

        if (!isTaken)
        {
            if (pIndex == 0) GameData.Instance.p0Side = direction;
            else GameData.Instance.p1Side = direction;
            UpdateUI();
        }
    }

    public void UpdateUI()
    {
        // Cek apakah ada setidaknya satu player yang sudah join
        bool anyPlayerJoined = GameData.Instance.p0Connected || GameData.Instance.p1Connected;

        // Update Teks Kiri & Kanan
        txtKiri.text = GetNamesAt(-1);
        txtKanan.text = GetNamesAt(1);

        // Update Teks Tengah dengan instruksi jika belum ada yang join atau slot kosong
        if (!anyPlayerJoined)
        {
            txtTengah.text = "<color=yellow>PRESS SOUTH/ENTER TO JOIN</color>";
        }
        else
        {
            // Jika sudah ada yang join, tampilkan daftar yang di tengah
            string joinedList = GetNamesAt(0);

            // Jika kolom tengah kosong tapi masih ada slot player (max 2), 
            // tambahkan instruksi untuk player kedua
            if (joinedList == "---" && (!GameData.Instance.p0Connected || !GameData.Instance.p1Connected))
            {
                txtTengah.text = "CONNECTED:\n---\n<size=80%><color=#AAAAAA>Press South/Enter to join P2</color></size>";
            }
            else
            {
                txtTengah.text = "CONNECTED:\n" + joinedList;
            }
        }

        // Logika tombol Start tetap sama
        bool p0Ready = GameData.Instance.p0Connected && GameData.Instance.p0Side != 0;
        bool p1Ready = GameData.Instance.p1Connected && GameData.Instance.p1Side != 0;
        btnStart.interactable = (p0Ready && p1Ready && GameData.Instance.p0Side != GameData.Instance.p1Side);
    }

    string GetNamesAt(int side)
    {
        string s = "";
        // Cek P0: Harus Connected DAN berada di sisi yang dicari
        if (GameData.Instance.p0Connected && GameData.Instance.p0Side == side)
            s += "PLAYER 1\n";

        // Cek P1: Harus Connected DAN berada di sisi yang dicari
        if (GameData.Instance.p1Connected && GameData.Instance.p1Side == side)
            s += "PLAYER 2\n";

        return string.IsNullOrEmpty(s) ? "---" : s;
    }

    public void LaunchGame() => SceneManager.LoadScene("LVL 1(1)");
}