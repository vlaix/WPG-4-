using UnityEngine;
using UnityEngine.UI;

public class WinCondition : MonoBehaviour
{
    [SerializeField] private Image menangbox;
    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;

    private int jumlahPemainDiArea = 0; // Penghitung pemain

    private void Start()
    {
        menangbox.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Cek apakah yang masuk salah satu dari kedua player
        if (other.gameObject == player1 || other.gameObject == player2)
        {
            jumlahPemainDiArea++;
            CekKemenangan();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Jika salah satu player keluar dari area, kurangi hitungan
        if (other.gameObject == player1 || other.gameObject == player2)
        {
            jumlahPemainDiArea--;
        }
    }

    private void CekKemenangan()
    {
        // Menang HANYA JIKA kedua pemain ada di dalam area
        if (jumlahPemainDiArea >= 2)
        {
            Debug.Log("Kedua pemain sudah sampai! Menang!");
            Time.timeScale = 0.0f;
            menangbox.gameObject.SetActive(true);
        }
    }
}