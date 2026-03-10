using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

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
        for (int i = 1; i <= totalLevels; i++)
        {
            // Buat tombol
            GameObject button = Instantiate(levelButtonPrefab, levelPanel);
            Button btnComponent = button.GetComponent<Button>();
            LevelButtonEffect effect = button.GetComponent<LevelButtonEffect>();

            // Set teks angka level
            TextMeshProUGUI btnText = button.GetComponentInChildren<TextMeshProUGUI>();
            Button buttonprop = button.GetComponent<Button>();
            btnText.font =  font;

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
                
                int levelIndexa = i;
                btnComponent.onClick.AddListener(() => LoadLevel("LVL " + levelIndexa));
            }

            if(buttonprop.interactable) {
                btnText.color = new Color(0.8078f,0.8078f,0.8078f);
            } else {
                btnText.color = new Color(1.0f, 1.0f, 1.0f);
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
