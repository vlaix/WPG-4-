using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Penting untuk fokus controller

public class Health : MonoBehaviour
{
    [SerializeField] private int MAXHP;
    [SerializeField] private Image HPBar;
    [SerializeField] private TextMeshProUGUI HPtxt;

    [Header("Lose UI Settings")]
    [SerializeField] private GameObject panelKalah; // Objek Induk Panel
    [SerializeField] private TextMeshProUGUI statusText; // Text di dalam panel
    [SerializeField] private Button firstButtonKalah; // Button pertama untuk difokuskan controller
    [SerializeField] private GameObject nextmati;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip deathSFX;

    private int currentHP;
    public static Health Instance;

    void Awake()
    {
        Instance = this;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        currentHP = MAXHP;
        UpdateUI();

        // Pastikan panel mati saat awal game
        if (panelKalah != null) panelKalah.SetActive(false);
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
        currentHP -= damage;
        UpdateUI();
    }

    private void UpdateUI()
    {
        int displayHP = Mathf.Max(0, currentHP);
        HPtxt.SetText(displayHP + "/" + MAXHP);
        HPBar.fillAmount = (float)currentHP / MAXHP;
    }

    private void Die()
    {
        if (audioSource != null && deathSFX != null && currentHP <= 0)
        {
            audioSource.PlayOneShot(deathSFX);
        }

        // TAMPILKAN PANEL KALAH
        if (panelKalah != null)
        {
            panelKalah.SetActive(true);
            nextmati.SetActive(false);
            // Ganti Text di dalam panel sesuai keinginan
            if (statusText != null)
            {
                statusText.text = "LEVEL GAGAL";
            }

            // AGAR CONTROLLER BISA NAVIGASI:
            if (firstButtonKalah != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(firstButtonKalah.gameObject);
            }
        }

        Time.timeScale = 0;
        currentHP = 1; // Mencegah looping Die()
    }
}