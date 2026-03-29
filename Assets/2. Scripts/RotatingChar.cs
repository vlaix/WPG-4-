using UnityEngine;

public class CharacterRotator : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Kecepatan rotasi karakter (derajat per detik)")]
    public float rotationSpeed = 50f;

    // Update dipanggil setiap frame
    void Update()
    {
        // Memutar objek pada sumbu Y (Vertical Axis) secara kontinu
        // Time.deltaTime memastikan putaran tetap smooth di FPS yang berbeda
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}