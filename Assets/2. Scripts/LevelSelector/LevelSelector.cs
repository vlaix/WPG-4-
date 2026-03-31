using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class LevelSelector : MonoBehaviour
{
    [Header("UI References")]
    public GameObject levelButtonPrefab; // Prefab tombol level
    public Transform levelPanel;         // Panel dengan Grid Layout Group
    public TMP_FontAsset font;

    [Header("Level Settings")]
    public int totalLevels = 20;

    void Start()
    {
        // Mengambil data level mana yang terakhir terbuka (default level 1)
        int levelReached = PlayerPrefs.GetInt("levelReached", 1);
        GameObject firstButtonCreated = null;

        for (int i = 1; i <= totalLevels; i++)
        {
            // Buat tombol
            GameObject button = Instantiate(levelButtonPrefab, levelPanel);
            if (i == 1)
            {
                firstButtonCreated = button;
            }
            Button btnComponent = button.GetComponent<Button>();
            LevelButtonEffect effect = button.GetComponent<LevelButtonEffect>();

            // Set teks angka level
            TextMeshProUGUI btnText = button.GetComponentInChildren<TextMeshProUGUI>();
            Button buttonprop = button.GetComponent<Button>();
            btnText.font = font;

            if (btnText != null) btnText.text = i.ToString();

            int levelIndex = i; // Local variable untuk closure

            // Logika Lock/Unlock
            if (i > levelReached)
            {
                btnComponent.interactable = false;
                if (effect != null) effect.SetupInitialVisual(false); // Mode Locked
            }
            else
            {
                btnComponent.interactable = true;
                if (effect != null) effect.SetupInitialVisual(true);  // Mode Unlocked
                
                // ✅ CHANGED: Use LoadingScreen instead of direct SceneManager
                btnComponent.onClick.AddListener(() => LoadLevel(levelIndex));
            }

            if (buttonprop.interactable)
            {
                btnText.color = new Color(0.8078f, 0.8078f, 0.8078f);
            }
            else
            {
                btnText.color = new Color(1.0f, 1.0f, 1.0f);
            }
        }
        if (firstButtonCreated != null)
        {
            // 1. Bersihkan seleksi yang mungkin nyangkut
            EventSystem.current.SetSelectedGameObject(null);

            // 2. Pilih tombol pertama secara paksa
            EventSystem.current.SetSelectedGameObject(firstButtonCreated);
        }
        StartCoroutine(SelectFirstButton(firstButtonCreated));
    }

    IEnumerator SelectFirstButton(GameObject btn)
    {
        // Tunggu sampai akhir frame agar UI Mesh & Layout selesai dibuat
        yield return new WaitForEndOfFrame();

        if (btn != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(btn);
            Debug.Log("Sistem: Mencoba fokus ke " + btn.name);
        }
    }

    // ✅ UPDATED: Load level through loading screen
    void LoadLevel(int levelNumber)
    {
        string sceneName = "LVL " + levelNumber;
        if (GameData.Instance != null)
        {
            GameData.Instance.selectedLevelName = sceneName;
        }
        Debug.Log($"Level {sceneName} dipilih. Menuju Lobby...");

        // Pindah ke Scene Lobby terlebih dahulu untuk pilih karakter
        LoadingScreen.LoadScene("Lobby");
    }

    // ✅ UPDATED: Back to menu through loading screen
    public void BackMenu()
    {
        LoadingScreen.LoadScene("MainMenu");
    }
}