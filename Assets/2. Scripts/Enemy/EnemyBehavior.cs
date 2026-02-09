using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField] private Transform Player;
    [SerializeField] private float speed;
    [SerializeField] private GameObject loot;
    public float healthpoint;

    void Start()
    {
        healthpoint = 3f;

        GameObject playerObj = GameObject.FindWithTag("Player");
    
        if (playerObj != null)
        {
            Player = playerObj.transform;
        }
    }

    void Update()
    {
        Vector3 PlayerPosition = new Vector3(Player.position.x, transform.position.y, Player.position.z);

        if (healthpoint != 0f) {
            transform.position = Vector3.MoveTowards(transform.position, PlayerPosition, speed * Time.deltaTime);
        }
    }

    public void TakeDamage(float damage)
    {
        healthpoint -= damage;

        if(healthpoint <= 0) {
            Die();
        }
    }

    private void Die()
    {
        SpawnLoot();
        Destroy(gameObject);
    }

    void SpawnLoot()
    {
        Instantiate(loot, transform.position, Quaternion.identity);
    }
}