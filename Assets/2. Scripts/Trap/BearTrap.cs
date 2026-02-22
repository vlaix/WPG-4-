using UnityEngine;

public class BearTrap : MonoBehaviour
{
    [Header("Trap Data")]
    public TrapData trapData;

    [Header("Debug")]
    public bool showDebugLogs = true;

    // Private variables
    private bool isTriggered = false;
    private float lifetimeTimer = 0f;
    private Renderer trapRenderer;
    private AudioSource audioSource;
    private Collider trapCollider;

    // Properties
    public bool IsTriggered => isTriggered;

    private void Start()
    {
        InitializeTrap();
    }

    private void Update()
    {
        // Handle lifetime
        if (trapData != null && trapData.lifetime > 0f)
        {
            lifetimeTimer += Time.deltaTime;
            if (lifetimeTimer >= trapData.lifetime)
            {
                DestroyTrap();
            }
        }
    }

    private void InitializeTrap()
    {
        if (trapData == null)
        {
            Debug.LogError("? TrapData is NULL! Assign a TrapData ScriptableObject.");
            return;
        }

        // Setup renderer
        trapRenderer = GetComponentInChildren<Renderer>();
        if (trapRenderer != null)
        {
            trapRenderer.material.color = trapData.idleColor;
        }

        // Setup collider
        trapCollider = GetComponent<Collider>();
        if (trapCollider == null)
        {
            // Create sphere collider for trigger detection
            SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.radius = trapData.triggerRadius;
            sphereCol.isTrigger = true;
            trapCollider = sphereCol;
        }

        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (showDebugLogs)
        {
            Debug.Log($"?? '{trapData.trapName}' trap placed!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        // Check if enemy
        EnemyBehavior enemy = other.GetComponent<EnemyBehavior>();
        if (enemy != null)
        {
            TriggerTrap(enemy);
        }
    }

    private void TriggerTrap(EnemyBehavior enemy)
    {
        isTriggered = true;

        if (showDebugLogs)
        {
            Debug.Log($"?? Trap triggered on {enemy.gameObject.name}!");
        }

        // Deal damage
        if (trapData.damage > 0f)
        {
            enemy.TakeDamage(trapData.damage);
        }

        // Stun enemy
        if (trapData.stunDuration > 0f)
        {
            StunEnemy(enemy);
        }

        // Visual feedback
        if (trapRenderer != null)
        {
            trapRenderer.material.color = trapData.triggeredColor;
        }

        // Play sound
        if (audioSource != null && trapData.triggerSound != null)
        {
            audioSource.PlayOneShot(trapData.triggerSound);
        }

        // Spawn effect
        if (trapData.triggerEffect != null)
        {
            Instantiate(trapData.triggerEffect, transform.position, Quaternion.identity);
        }

        // Destroy trap after trigger
        Invoke(nameof(DestroyTrap), 0.5f);
    }

    private void StunEnemy(EnemyBehavior enemy)
    {
        // Add stun component to enemy
        EnemyStun stun = enemy.gameObject.GetComponent<EnemyStun>();
        if (stun == null)
        {
            stun = enemy.gameObject.AddComponent<EnemyStun>();
        }
        stun.Stun(trapData.stunDuration);

        if (showDebugLogs)
        {
            Debug.Log($"? Enemy stunned for {trapData.stunDuration} seconds!");
        }
    }

    private void DestroyTrap()
    {
        if (showDebugLogs)
        {
            Debug.Log($"?? Trap destroyed");
        }
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (trapData == null) return;

        // Draw trigger radius
        Gizmos.color = isTriggered ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, trapData.triggerRadius);
    }
}
