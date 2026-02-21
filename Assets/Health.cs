using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] private int MAXHP;
    [SerializeField] private Image HPBar;
    [SerializeField] private TextMeshProUGUI HPtxt;
    private int currentHP;
    public static Health Instance;

    void Awake() {
        Instance = this;
    }

    void Start()
    {
        currentHP = MAXHP;
        UpdateUI();
    }

    void Update()
    {
        if(currentHP <= 0) {
            Die();
        }
    }

    public void Hurt(int damage) {
        currentHP = currentHP-damage;
        UpdateUI();
    }

    private void UpdateUI() {
        int Display = Mathf.Max(0, currentHP);

        HPtxt.SetText(Display + "/" + MAXHP);
        HPBar.fillAmount = (float)currentHP / MAXHP;
    }

    private void Die() {
        Time.timeScale = 0;
    }
}
