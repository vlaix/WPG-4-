using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject panelPause;    // Induk Panel Pause
    [SerializeField] private TextMeshProUGUI pauseTxt;  // Text Judul (misal: "PAUSED")
    [SerializeField] private GameObject btnNextLevel;   // Tombol yang ingin di-SetActive(false)
    [SerializeField] private GameObject firstButtonPause; // Tombol pertama untuk fokus controller

    private bool isPaused = false;

    void Awake()
    {
        // Buat Instance agar bisa dipanggil script lain
        Instance = this;
    }
    void Start()
    {
        // Pastikan awal game panel mati
        if (panelPause != null) panelPause.SetActive(false);
    }

    // Fungsi ini dipanggil dari Player Input (misal tombol Esc/Start)
    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            DoPause();
        }
        else
        {
            DoResume();
        }
    }

    private void DoPause()
    {
        // 1. Set Active Panel
        panelPause.SetActive(true);

        // 2. Set Text
        if (pauseTxt != null) pauseTxt.text = "GAME PAUSED";

        // 3. GameObject SetActive False (Tombol yang tidak terpakai)
        if (btnNextLevel != null) btnNextLevel.SetActive(false);
        firstButtonPause.SetActive(true);

        // 4. Set First Button (Untuk Controller)
        if (firstButtonPause != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstButtonPause);
        }

        // Hentikan waktu game
        Time.timeScale = 0f;
    }

    public void DoResume()
    {
        isPaused = false;
        panelPause.SetActive(false);
        btnNextLevel.SetActive(true);
        firstButtonPause.SetActive(false);

        // Kembalikan waktu game
        Time.timeScale = 1f;

        // Bersihkan seleksi agar tidak mengganggu gameplay
        EventSystem.current.SetSelectedGameObject(btnNextLevel);
    }
}