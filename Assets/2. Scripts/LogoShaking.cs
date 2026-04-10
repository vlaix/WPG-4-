using UnityEngine;

public class SmoothLogoSway : MonoBehaviour
{
    [Header("Pengaturan Goyangan")]
    [Tooltip("Seberapa cepat logo bergoyang. Semakin kecil nilainya, semakin slow.")]
    public float speed = 1.5f; 
    
    [Tooltip("Batas maksimal kemiringan logo (dalam derajat).")]
    public float maxAngle = 10f;

    // Menyimpan rotasi awal agar goyangan tetap seimbang di tengah
    private Quaternion startRotation;

    void Start()
    {
        // Simpan posisi rotasi awal saat game dimulai
        startRotation = transform.rotation;
    }

    void Update()
    {
        // Menghitung sudut rotasi dengan gelombang Sinus berdasarkan waktu
        float swayAngle = Mathf.Sin(Time.time * speed) * maxAngle;

        // Menerapkan rotasi pada sumbu Z (cocok untuk UI Image atau Sprite 2D)
        transform.rotation = startRotation * Quaternion.Euler(0f, 0f, swayAngle);
    }
}