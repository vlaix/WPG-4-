using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] private int MAXHP;
    [SerializeField] private Image HPBar;
    [SerializeField] private TextMeshProUGUI HPtxt;
    [SerializeField] private Image BoxKalah;
    [SerializeField] private Button ButtonRestart;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip deathSFX; 

    private int currentHP;
    public static Health Instance;

    void Awake()
    {
        Instance = this;

        // Otomatis mencari AudioSource di objek ini jika belum dimasukkan di Inspector
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void Start()
    {
        currentHP = MAXHP;
        UpdateUI();
        BoxKalah.gameObject.SetActive(false);
    }

    void Update()
    {
        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Hurt(int damage)
    {
        // Langsung kurangi HP karena perlindungan shield sudah ditangani oleh peluru/musuh
        currentHP = currentHP - damage;
        UpdateUI();
        Debug.Log($"Player took {damage} damage! HP: {currentHP}/{MAXHP}");
    }

    private void UpdateUI()
    {
        int Display = Mathf.Max(0, currentHP);

        HPtxt.SetText(Display + "/" + MAXHP);
        HPBar.fillAmount = (float)currentHP / MAXHP;
    }

    private void Die()
    {
        // Mainkan SFX Mati
        if (audioSource != null && deathSFX != null)
        {
            audioSource.PlayOneShot(deathSFX);
        }

        BoxKalah.gameObject.SetActive(true);
        ButtonRestart.interactable = true; // Penulisan yang lebih rapi tanpa GetComponent berulang

        Debug.Log("Player died!");
        Time.timeScale = 0;
        currentHP = 1; // Mencegah fungsi Die() terpanggil berulang kali
    }
}