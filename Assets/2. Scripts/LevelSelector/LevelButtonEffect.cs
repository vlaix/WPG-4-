using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// TAMBAHKAN ISelectHandler dan IDeselectHandler di sini
public class LevelButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    private TextMeshProUGUI btnText;
    private Button btn;

    private Color normalColor;
    private Color hoverColor;
    private Color lockedColor = Color.white;

    void Awake()
    {
        btnText = GetComponentInChildren<TextMeshProUGUI>();
        btn = GetComponent<Button>();

        ColorUtility.TryParseHtmlString("#CCCCCC", out normalColor);
        ColorUtility.TryParseHtmlString("#A9A7A7", out hoverColor);
    }

    // --- UNTUK MOUSE (Hover) ---
    public void OnPointerEnter(PointerEventData eventData) => HandleHighlight();
    public void OnPointerExit(PointerEventData eventData) => HandleDehighlight();

    // --- UNTUK CONTROLLER / KEYBOARD (Selection) ---
    public void OnSelect(BaseEventData eventData) => HandleHighlight();
    public void OnDeselect(BaseEventData eventData) => HandleDehighlight();

    // Fungsi pusat untuk mengubah warna saat disorot
    private void HandleHighlight()
    {
        if (btn.interactable && btnText != null)
        {
            btnText.color = hoverColor;
        }
    }

    // Fungsi pusat untuk mengembalikan warna saat sorotan hilang
    private void HandleDehighlight()
    {
        if (btnText != null)
        {
            btnText.color = btn.interactable ? normalColor : lockedColor;
        }
    }

    public void SetupInitialVisual(bool isUnlocked)
    {
        if (btnText == null) btnText = GetComponentInChildren<TextMeshProUGUI>();
        btnText.color = isUnlocked ? normalColor : lockedColor;
    }
}