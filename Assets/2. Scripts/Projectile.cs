using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private LayerMask hitLayers;

    private GameObject player;

    public void SetPlayer(GameObject playerRef)
    {
        player = playerRef;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Don't collide with player
        if (collision.gameObject == player)
        {
            Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
            return;
        }

        // Handle hit logic here
        HandleHit(collision.gameObject);

        // Destroy projectile on impact
        Destroy(gameObject);
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