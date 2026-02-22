using UnityEngine;

[CreateAssetMenu(fileName = "TrapData", menuName = "Scriptable Objects/TrapData")]
public class TrapData : ScriptableObject
{
    [Header("Trap Info")]
    [Tooltip("Nama trap ini")]
    public string trapName = "Bear Trap";

    [TextArea(2, 3)]
    [Tooltip("Deskripsi trap")]
    public string description = "A deadly bear trap that stuns enemies";

    [Header("Trap Stats")]
    [Tooltip("Damage yang diberikan saat enemy kena trap")]
    public float damage = 10f;

    [Tooltip("Durasi stun dalam detik")]
    public float stunDuration = 3f;

    [Tooltip("Radius detection trap (berapa dekat enemy harus untuk trigger)")]
    public float triggerRadius = 1.5f;

    [Tooltip("Berapa lama trap aktif sebelum hilang (0 = unlimited)")]
    public float lifetime = 0f;

    [Header("Visual & Audio")]
    [Tooltip("Prefab trap yang akan di-spawn")]
    public GameObject trapPrefab;

    [Tooltip("Sound saat trap trigger/kena enemy")]
    public AudioClip triggerSound;

    [Tooltip("Particle effect saat trap trigger")]
    public GameObject triggerEffect;

    [Header("Placement Settings")]
    [Tooltip("Berapa detik cooldown sebelum bisa place trap lagi")]
    public float placementCooldown = 5f;

    [Tooltip("Resource yang dibutuhkan untuk place 1 trap")]
    public string requiredResource = "Scrap";

    [Tooltip("Jumlah resource yang dibutuhkan")]
    public int resourceCost = 5;

    [Tooltip("Maximum trap yang bisa aktif bersamaan (0 = unlimited)")]
    public int maxActiveTraps = 3;

    [Header("Colors")]
    [Tooltip("Warna trap saat idle (belum trigger)")]
    public Color idleColor = Color.gray;

    [Tooltip("Warna trap saat triggered")]
    public Color triggeredColor = Color.red;
}