using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class TutorialPanelController : MonoBehaviour
{
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private float animDuration = 0.3f;

    private InputAction tipsAction;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Coroutine currentAnim;

    // Posisi slide (panel geser dari bawah)
    private Vector2 hiddenPos;
    private Vector2 shownPos;

    void Awake()
    {
        var uiMap = inputActions.FindActionMap("UI");
        tipsAction = uiMap.FindAction("Tips");

        canvasGroup = tutorialPanel.GetComponent<CanvasGroup>();
        rectTransform = tutorialPanel.GetComponent<RectTransform>();

        // Simpan posisi shown dulu, lalu hitung hidden (geser ke bawah 100px)
        shownPos = rectTransform.anchoredPosition;
        hiddenPos = shownPos + new Vector2(0, -300f);   // dari kanan

        // Set initial state: hidden
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        rectTransform.anchoredPosition = hiddenPos;
    }

    void OnEnable()
    {
        tipsAction.Enable();
        tipsAction.performed += OnShowTips;
        tipsAction.canceled  += OnHideTips;
    }

    void OnDisable()
    {
        tipsAction.performed -= OnShowTips;
        tipsAction.canceled  -= OnHideTips;
        tipsAction.Disable();
    }

    private void OnShowTips(InputAction.CallbackContext ctx) => PlayAnim(true);
    private void OnHideTips(InputAction.CallbackContext ctx) => PlayAnim(false);

    private void PlayAnim(bool show)
    {
        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(AnimatePanel(show));
    }

    private IEnumerator AnimatePanel(bool show)
    {
        float startAlpha = canvasGroup.alpha;
        float targetAlpha = show ? 1f : 0f;

        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 targetPos = show ? shownPos : hiddenPos;

        float elapsed = 0f;
        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / animDuration); // easing

            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);

            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        rectTransform.anchoredPosition = targetPos;

        // Kalau udah selesai hide, matiin interaksi
        canvasGroup.interactable = show;
        canvasGroup.blocksRaycasts = show;
    }
}