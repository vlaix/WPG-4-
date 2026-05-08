using UnityEngine;

public class AmmoCollectible : MonoBehaviour
{
    [SerializeField] private int ammoAmount = 5; // Jumlah peluru yang didapat

    private void OnTriggerEnter(Collider other)
    {
        // Cek apakah yang menyentuh adalah Player 1 atau Player 2
        if (other.CompareTag("Player"))
        {
            // Cari script Wshoot di anak-anak player (karena biasanya senjata ada di tangan)
            Wshoot weapon = other.GetComponentInChildren<Wshoot>();

            if (weapon != null)
            {
                weapon.AddAmmo(ammoAmount);
                Destroy(gameObject); // Hapus item ammo setelah diambil
            }
        }
    }
}