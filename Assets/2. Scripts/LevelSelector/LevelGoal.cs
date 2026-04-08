using UnityEngine;

public class LevelGoal : MonoBehaviour
{
    // Hanya menggunakan field dari script LevelGoal lama
    [SerializeField] private GameObject levelCompleteUI;
    private LevelManager levelManager;

    // Menggunakan logika boolean dari WinCondition
    private bool player1Masuk = false;
    private bool player2Masuk = false;

    private void Start()
    {
        levelManager = FindObjectOfType<LevelManager>();

        if (levelCompleteUI != null)
        {
            levelCompleteUI.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Cek tag masing-masing player yang masuk ke area goal
        if (other.CompareTag("Player")) player1Masuk = true;
        if (other.CompareTag("Player2")) player2Masuk = true;

        CekKemenangan();
    }

    private void OnTriggerExit(Collider other)
    {
        // Jika player keluar dari area sebelum player satunya sampai, batalkan status masuknya
        if (other.CompareTag("Player")) player1Masuk = false;
        if (other.CompareTag("Player2")) player2Masuk = false;
    }

    private void CekKemenangan()
    {
        // Level complete hanya akan tereksekusi jika KEDUA player sudah ada di dalam trigger
        if (player1Masuk && player2Masuk)
        {
            Debug.Log("Level Complete! Kedua pemain sudah sampai di titik akhir!");

            // Beri tahu LevelManager bahwa level selesai
            // (Biasanya logika buka gembok level selanjutnya dieksekusi di dalam LevelManager ini)
            if (levelManager != null)
            {
                levelManager.CompleteLevel();
            }

            // Tampilkan UI kemenangan
            if (levelCompleteUI != null)
            {
                levelCompleteUI.SetActive(true);
            }

            // Pause game
            Time.timeScale = 0f;
        }
    }

    // Fungsi ini saya biarkan di sini barangkali LevelManager milikmu belum menangani PlayerPrefs.
    // Namun idealnya, logika penyimpanan (Save) diletakkan di dalam LevelManager.CompleteLevel()
    public void UnlockNextLevel(int currentLevel)
    {
        int levelReached = PlayerPrefs.GetInt("levelReached", 1);

        // Jika level yang baru diselesaikan adalah level tertinggi yang pernah dicapai
        if (currentLevel == levelReached)
        {
            PlayerPrefs.SetInt("levelReached", levelReached + 1);
            PlayerPrefs.Save(); 
        }
    }
}