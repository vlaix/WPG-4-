using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class LevelButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TextMeshProUGUI btnText;
    private Button btn;

    // Warna sesuai permintaanmu
    private Color normalColor; 
    private Color hoverColor;  
    private Color lockedColor = Color.white; // Warna Locked: Putih

    void Awake()
    {
        btnText = GetComponentInChildren<TextMeshProUGUI>();
        btn = GetComponent<Button>();

        // Konversi Hex ke Color Unity
        ColorUtility.TryParseHtmlString("#CCCCCC", out normalColor); // Abu-abu Normal
        ColorUtility.TryParseHtmlString("#A9A7A7", out hoverColor);  // Abu-abu Hover
    }

    // Saat mouse masuk (Hover)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (btn.interactable && btnText != null)
        {
            btnText.color = hoverColor;
        }
    }

    // Saat mouse keluar
    public void OnPointerExit(PointerEventData eventData)
    {
        if (btnText != null)
        {
            // Jika tombol aktif kembali ke normal, jika mati tetap warna locked
            btnText.color = btn.interactable ? normalColor : lockedColor;
        }
    }

    // Fungsi inisialisasi awal yang dipanggil oleh LevelSelector
    public void SetupInitialVisual(bool isUnlocked)
    {
        if (btnText == null) btnText = GetComponentInChildren<TextMeshProUGUI>();
        
        // Memastikan warna awal benar saat pertama kali muncul
        btnText.color = isUnlocked ? normalColor : lockedColor;
    }
}