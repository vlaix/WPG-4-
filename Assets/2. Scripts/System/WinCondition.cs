using UnityEngine;
using UnityEngine.UI;

public class WinCondition : MonoBehaviour
{
    [SerializeField] private Image menangbox;

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
        }
    }
}