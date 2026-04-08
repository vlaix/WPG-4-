using UnityEngine;
using UnityEngine.UI;

public class WinCondition : MonoBehaviour
{
    [SerializeField] private GameObject menangbox;

    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;


    // Kita gunakan bool untuk mengecek apakah masing-masing player sudah sampai
    private bool player1Masuk = false;
    private bool player2Masuk = false;

    private void Start()
    {
        if (menangbox != null) menangbox.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) player1Masuk = true;
        if (other.CompareTag("Player2")) player2Masuk = true;

        CekKemenangan();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) player1Masuk = false;
        if (other.CompareTag("Player2")) player2Masuk = false;
    }

    private void CekKemenangan()
    {
        // Menang jika kedua kondisi true
        if (player1Masuk && player2Masuk)
        {
            Debug.Log("Kedua pemain sudah sampai! Menang!");
            Time.timeScale = 0.0f;

            if (menangbox != null) menangbox.gameObject.SetActive(true);
            menangbox.gameObject.SetActive(true);
        }
    }

    public void UnlockNextLevel(int currentLevel)
    {
        int levelReached = PlayerPrefs.GetInt("levelReached", 1);

        // Jika level yang baru diselesaikan adalah level tertinggi yang pernah dicapai
        if (currentLevel == levelReached)
        {
            PlayerPrefs.SetInt("levelReached", levelReached + 1);
            PlayerPrefs.Save(); // Simpan data secara permanen

        }
    }
}