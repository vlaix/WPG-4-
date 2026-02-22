using UnityEngine;

[CreateAssetMenu(fileName = "ShieldData", menuName = "Scriptable Objects/ShieldData")]
public class ShieldData : ScriptableObject
{
    [Header("Shield Info")]
    [Tooltip("Nama shield ini")]
    public string shieldName = "Energy Shield";

    [TextArea(2, 3)]
    [Tooltip("Deskripsi shield")]
    public string description = "A protective barrier that absorbs damage";

    [Header("Shield Stats")]
    [Tooltip("Berapa damage yang bisa diserap shield sebelum hancur")]
    public float shieldHealth = 50f;

    [Tooltip("Durasi shield aktif (dalam detik, 0 = unlimited sampai hancur)")]
    public float duration = 10f;

    [Tooltip("Cooldown sebelum bisa aktifkan shield lagi setelah hancur")]
    public float cooldown = 15f;

    [Header("Visual & Audio")]
    [Tooltip("Prefab shield visual (sphere, dome, dll)")]
    public GameObject shieldPrefab;

    [Tooltip("Sound saat shield aktif")]
    public AudioClip activateSound;

    [Tooltip("Sound saat shield kena hit")]
    public AudioClip hitSound;

    [Tooltip("Sound saat shield hancur")]
    public AudioClip breakSound;

    [Tooltip("Particle effect saat shield kena hit")]
    public GameObject hitEffect;

    [Tooltip("Particle effect saat shield hancur")]
    public GameObject breakEffect;

    [Header("Resource Cost")]
    [Tooltip("Resource yang dibutuhkan untuk aktifkan shield")]
    public string requiredResource = "Scrap";

    [Tooltip("Jumlah resource untuk aktifkan shield")]
    public int resourceCost = 10;

    [Tooltip("Apakah shield otomatis aktif saat punya resource?")]
    public bool autoActivate = false;

    [Header("Colors")]
    [Tooltip("Warna shield saat full health")]
    public Color fullHealthColor = new Color(0f, 0.5f, 1f, 0.5f); // Blue transparent

    [Tooltip("Warna shield saat hampir hancur (< 30% health)")]
    public Color lowHealthColor = new Color(1f, 0f, 0f, 0.5f); // Red transparent

    [Header("Scale & Position")]
    [Tooltip("Skala shield (size)")]
    public Vector3 shieldScale = new Vector3(2f, 2f, 2f);

    [Tooltip("Offset posisi shield dari player")]
    public Vector3 shieldOffset = Vector3.zero;
}