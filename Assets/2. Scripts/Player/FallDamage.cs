using UnityEngine;

public class FallDamage : MonoBehaviour
{
    [SerializeField] Health health; // Pastikan ini merujuk ke Health Manager global atau sistem health player
    [SerializeField] Transform respawn;

    private void OnTriggerEnter(Collider collision)
    {
        // Cek apakah yang masuk memiliki tag Player atau Player2
        if (collision.CompareTag("Player") || collision.CompareTag("Player2"))
        {
            // Jika script Health ada di objek player itu sendiri:
            health.Hurt(2);

            // Langsung pindahkan posisi objek yang menyentuh trigger ke tempat respawn
            collision.transform.position = respawn.position;

            Debug.Log($"{collision.gameObject.name} jatuh dan respawn!");
        }
    }
}