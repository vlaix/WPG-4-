using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelSelector : MonoBehaviour
{
    [Header("UI References")]
    public GameObject levelButtonPrefab; // Prefab tombol level
    public Transform levelPanel;         // Panel dengan Grid Layout Group

    [Header("Level Settings")]
    public int totalLevels = 20;

    void Start()
    {
        // Mengambil data level mana yang terakhir terbuka (default level 1)
        int levelReached = PlayerPrefs.GetInt("levelReached", 1);
        for (int i = 1; i <= totalLevels; i++)
        {
            // Buat tombol
            GameObject button = Instantiate(levelButtonPrefab, levelPanel);
            
            // Set teks angka level
            TextMeshProUGUI btnText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.text = i.ToString();

            Button btnComponent = button.GetComponent<Button>();
            int levelIndex = i; // Local variable untuk closure

            // Logika Lock/Unlock
            if (i > levelReached)
            {
                // Jika level belum terbuka, buat tombol tidak bisa diklik
                btnComponent.interactable = false;
                // Opsional: Ubah warna atau tambah ikon gembok di sini
            }
            else
            {
                // Jika level terbuka, tambahkan fungsi pindah scene
                btnComponent.onClick.AddListener(() => LoadLevel("LVL " + levelIndex));
            }
        }
    }

    void LoadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void BackMenu(){
        SceneManager.LoadScene("MainMenu");
    }

}
