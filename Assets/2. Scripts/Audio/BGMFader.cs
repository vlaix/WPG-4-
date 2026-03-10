using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class BGMFader : MonoBehaviour
{
    [Header("Fade Settings")]
    [Tooltip("Volume maksimal BGM (0 sampai 1)")]
    [Range(0f, 1f)]
    public float targetVolume = 0.5f;

    [Tooltip("Berapa detik waktu yang dibutuhkan sampai volume maksimal")]
    public float fadeDuration = 3f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Pastikan lagu mulai dari volume 0
        audioSource.volume = 0f;
    }

    private void Start()
    {
        // Mulai mainkan lagu dan jalankan efek Fade-In
        audioSource.Play();
        StartCoroutine(FadeInProcess());
    }

    private IEnumerator FadeInProcess()
    {
        float currentTime = 0f;

        // Proses menaikkan volume secara perlahan
        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            // Lerp digunakan untuk menghitung transisi nilai dari 0 ke targetVolume dengan mulus
            audioSource.volume = Mathf.Lerp(0f, targetVolume, currentTime / fadeDuration);
            yield return null; // Tunggu ke frame berikutnya
        }

        // Pastikan di akhir transisi, volumenya pas di target
        audioSource.volume = targetVolume;
    }
}