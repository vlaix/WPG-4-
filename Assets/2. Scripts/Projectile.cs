using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float damage = 1f;
    [SerializeField] private LayerMask hitLayers;

    private GameObject player;

    public void SetPlayer(GameObject playerRef)
    {
        player = playerRef;
    }

    void OnTriggerEnter(Collider other)
    {
        // Don't collide with player
        if (other.CompareTag("Player"))
            return;

        // Handle hit logic here
        HandleHit(other.gameObject);

        // Destroy projectile on impact

        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyBehavior>().TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    void HandleHit(GameObject hitObject)
    {
        // Add your hit detection logic here
        Debug.Log($"Projectile hit: {hitObject.name}");

        // Example: if you have a Health script on enemies
        // Health targetHealth = hitObject.GetComponent<Health>();
        // if (targetHealth != null)
        // {
        //     targetHealth.TakeDamage(damage);
        // }
    }
}