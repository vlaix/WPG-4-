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

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }
}