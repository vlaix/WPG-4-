using UnityEngine;

public class EnemyStun : MonoBehaviour
{
    private float stunTimer = 0f;
    private bool isStunned = false;
    private EnemyBehavior enemyBehavior;
    private Renderer enemyRenderer;
    private Color originalColor;

    public bool IsStunned => isStunned;

    private void Start()
    {
        enemyBehavior = GetComponent<EnemyBehavior>();
        enemyRenderer = GetComponentInChildren<Renderer>();

        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
        }
    }

    private void Update()
    {
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;

            if (stunTimer <= 0f)
            {
                UnStun();
            }
        }
    }

    public void Stun(float duration)
    {
        isStunned = true;
        stunTimer = duration;

        // Disable enemy movement (stop chasing player)
        if (enemyBehavior != null)
        {
            enemyBehavior.enabled = false;
        }

        // Visual feedback - change color to indicate stun
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = new Color(0.5f, 0.5f, 1f); // Blue tint
        }

        Debug.Log($"? Enemy stunned for {duration} seconds!");
    }

    private void UnStun()
    {
        isStunned = false;

        // Re-enable enemy movement
        if (enemyBehavior != null)
        {
            enemyBehavior.enabled = true;
        }

        // Restore original color
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = originalColor;
        }

        Debug.Log("? Enemy unstunned!");
    }
}