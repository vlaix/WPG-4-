using UnityEngine;
using TMPro; // Wajib untuk TextMeshPro

public class AmmoUI : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI ammoText;

    [Header("Player Search")]
    [SerializeField] private string playerTag = "Player"; // Sesuaikan dengan tag player kamu

    private Wshoot playerWeapon;

    void Start()
    {
        // Jika belum ditarik manual di Inspector, coba cari komponen teks di objek ini
        if (ammoText == null) ammoText = GetComponent<TextMeshProUGUI>();

        FindWeaponReference();
    }

    void Update()
    {
        // Jika referensi hilang (misal ganti scene atau player baru spawn)
        if (playerWeapon == null)
        {
            FindWeaponReference();
            return;
        }

        // Update Teks
        UpdateAmmoDisplay();
    }

    private void FindWeaponReference()
    {
        // Cari objek dengan tag Player
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObj != null)
        {
            // Ambil script Wshoot yang ada di player atau anak-anaknya (senjata)
            playerWeapon = playerObj.GetComponentInChildren<Wshoot>();
        }
    }

    private void UpdateAmmoDisplay()
    {
        if (playerWeapon != null && ammoText != null)
        {
            // Menampilkan format: "Ammo: 10 / 20"
            ammoText.SetText(""+playerWeapon.currentAmmo);

            // Opsional: Beri warna merah jika peluru habis
            ammoText.color = (playerWeapon.currentAmmo <= 0) ? Color.red : Color.white;
        }
    }
}