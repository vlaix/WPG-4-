using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] Health Health;
    [SerializeField] EnemyData archer;

    void Start()
    {
        Destroy(gameObject, lifeTime);
        Health = Health.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Player2"))
        {
            Health.Hurt(archer.damage);
            
            Destroy(gameObject); 
        }
    }
}