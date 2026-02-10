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
        float randomizer;

        GameObject playerObj1 = GameObject.FindWithTag("Player");
        GameObject playerObj2 = GameObject.FindWithTag("Player2");

        randomizer = Random.Range(0f, 2f);
        Debug.Log("Randomizer = " + randomizer);

        if(randomizer < 1) {

            Player = playerObj2.transform;
        } else {
            Player = playerObj1.transform;
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