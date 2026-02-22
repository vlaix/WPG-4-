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

    [Header("Shield Integration")]
    private PlayerShield playerShield;

    void Awake()
    {
        Instance = this;

        // Get PlayerShield component if exists
        playerShield = GetComponent<PlayerShield>();
    }

    void Start()
    {
        currentHP = MAXHP;
        UpdateUI();
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
        // CRITICAL: Check if shield is active FIRST
        if (playerShield != null && playerShield.IsShieldActive)
        {
            // Shield blocks damage, tidak mengenai HP
            bool damageBlocked = playerShield.TakeDamage(damage);

            if (damageBlocked)
            {
                if (playerShield.showDebugLogs)
                {
                    Debug.Log($"Shield protected player from {damage} damage!");
                }
                return; // STOP HERE - HP tidak berkurang!
            }
        }

        // Shield tidak aktif atau sudah pecah, damage HP
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
        Debug.Log("Player died!");
        Time.timeScale = 0;
    }
}