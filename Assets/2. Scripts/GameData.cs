using UnityEngine;
using UnityEngine.InputSystem; // Tambahkan ini

public class GameData : MonoBehaviour
{
    public static GameData Instance;

    public int p0Side = 0;
    public int p1Side = 0;
    public bool p0Connected = false;
    public bool p1Connected = false;

    // SIMPAN DEVICE DI SINI
    public InputDevice p0Device;
    public InputDevice p1Device;

    public string selectedLevelName = "LVL 1";
    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }
    public void ResetLobbyData()
    {
        p0Connected = false;
        p1Connected = false;
        p0Side = 0;
        p1Side = 0;
        p0Device = null;
        p1Device = null;
        // Jangan reset selectedLevelName jika kamu masih butuh data levelnya
        Debug.Log("GameData telah di-reset untuk Lobby baru.");
    }
}